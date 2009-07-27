''' <summary>
''' Reads a binary file containing GSM cell IDs and Latitude/Longitude
''' Draws cells on a map (Inherits clsLayer)
''' </summary>
''' <remarks></remarks>
Public Class clsOpenCellID
    Inherits clsLayer

    Public Shared WebsiteURL As String = "http://www.opencellid.org/"
    Public Shared RawDatabaseURL As String = "http://myapp.fr/cellsIdData/cells.txt.gz"

    Private Const BinaryMagic As UInt32 = &H43454C4C 'spells "CELL"

    Public SymbolSize As Integer
    Public SymbolPen As Pen
    Public BrushLabel As SolidBrush
    Public FontLabel As Font

    Private pCells() As clsCell
    Private pLastCell As Integer = 0

    Public Sub New(ByVal aFilename As String, ByVal aMapForm As frmMap)
        MyBase.New(aFilename, aMapForm)
        SetDefaults()
        Dim lReader As New IO.BinaryReader(IO.File.OpenRead(aFilename))
        If lReader.ReadUInt32 <> BinaryMagic Then
            Throw New ApplicationException("Unknown file type '" & aFilename & "'")
        Else
            'Subtract 4 bytes for BinaryMagic
            Dim lLastCell As Integer = (FileLen(aFilename) - 4) / clsCell.NumBytes - 1
            ReDim pCells(lLastCell)
            For lIndex As Integer = 0 To lLastCell
                pCells(lIndex) = New clsCell(lReader)
                pBounds.Expand(pCells(lIndex).Latitude, pCells(lIndex).Longitude)
            Next
        End If
    End Sub

    Private Sub SetDefaults()
        ReDim pCells(0)
        Me.Bounds = New clsGPXbounds
        BrushLabel = New SolidBrush(Color.Black)
        FontLabel = New Font("Arial", 10, FontStyle.Regular)

        SymbolSize = 25
        SymbolPen = New Pen(Me.LegendColor, 4)
    End Sub

    Public Overrides Sub Render(ByVal g As Graphics, ByVal aTopLeftTile As Point, ByVal aOffsetToCenter As Point)
        If Me.Visible Then
            Dim lDrawLayer As Boolean = True
            Dim lLastCell As Integer = pCells.GetUpperBound(0)
            If pBounds IsNot Nothing Then
                With pBounds 'Skip drawing if it is not in view
                    If .minlat > MapForm.LatMax OrElse _
                        .maxlat < MapForm.LatMin OrElse _
                        .minlon > MapForm.LonMax OrElse _
                        .maxlon < MapForm.LonMin Then

                        lDrawLayer = False
                    End If
                End With
            End If
            If lDrawLayer Then
                For lIndex As Integer = 0 To lLastCell
                    If MapForm.LatLonInView(pCells(lIndex).Latitude, pCells(lIndex).Longitude) Then
                        DrawCell(g, pCells(lIndex), aTopLeftTile, aOffsetToCenter)
                    End If
                Next
            End If
        End If
    End Sub

    ''' <summary>
    ''' Draw a cell tower, return True if it was drawn
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="aCell"></param>
    ''' <param name="aTopLeftTile"></param>
    ''' <param name="aOffsetToCenter"></param>
    ''' <returns>True if waypoint was drawn, False if it was outside view</returns>
    ''' <remarks></remarks>
    Private Function DrawCell(ByVal g As Graphics, _
                                  ByVal aCell As clsCell, _
                                  ByVal aTopLeftTile As Point, _
                                  ByVal aOffsetToCenter As Point) As Boolean
        With aCell
            Dim lTileXY As Point 'Which tile this point belongs in
            Dim lTileOffset As Point 'Offset within lTileXY in pixels
            lTileXY = CalcTileXY(.Latitude, .Longitude, MapForm.Zoom, lTileOffset)
            Dim lX As Integer = (lTileXY.X - aTopLeftTile.X) * g_TileSize + aOffsetToCenter.X + lTileOffset.X
            Dim lY As Integer = (lTileXY.Y - aTopLeftTile.Y) * g_TileSize + aOffsetToCenter.Y + lTileOffset.Y
            Dim lBitmap As Drawing.Bitmap = Nothing

            If MapForm.Zoom < 10 Then
                g.DrawLine(SymbolPen, lX - 3, lY - 3, lX + 3, lY + 3)
                g.DrawLine(SymbolPen, lX - 3, lY + 3, lX + 3, lY - 3)
            Else
                g.DrawEllipse(SymbolPen, lX - 5, lY - 5, 10, 10)

                If MapForm.Zoom > 12 Then
                    Try
                        g.DrawString(.Label, FontLabel, BrushLabel, lX, lY)
                    Catch
                    End Try
                End If
            End If
        End With
    End Function

    ''' <summary>
    ''' Convert from OpenCellID CSV formatted raw list of cells into binary or text format
    ''' </summary>
    ''' <param name="aCSVFilename">File as downloaded from OpenCellID Raw Data, cells.txt.gz, uncompressed</param>
    ''' <param name="aSaveAs">File name to save binary formatted data in</param>
    ''' <param name="aSaveBinary">True to save as binary, False to save as text</param>
    ''' <param name="aMCCs">List of Mobile Country Codes to include in output</param>
    ''' <returns>True on success, False on failure</returns>
    ''' <remarks></remarks>
    Public Shared Function ConvertDatabase(ByVal aCSVFilename As String, _
                                           ByVal aSaveAs As String, _
                                           ByVal aSaveBinary As Boolean, _
                                           ByVal aMCCs() As String) As Boolean
        Try
            Dim lLatitude As Double
            Dim lLongitude As Double
            Dim lFields() As String
            Dim lAllMCCs As Boolean = aMCCs Is Nothing OrElse aMCCs.Length = 0 OrElse Not IsNumeric(aMCCs(0))

            Dim lReader As IO.StreamReader = IO.File.OpenText(aCSVFilename)
            Dim lCell As clsCell
            Dim lCells As New Generic.SortedList(Of String, clsCell)
            Dim lInputLine As String
            While Not lReader.EndOfStream
                lInputLine = lReader.ReadLine()
                lFields = lInputLine.Split(","c)
                If lAllMCCs OrElse Array.IndexOf(aMCCs, lFields(3)) > -1 Then
                    lCell = New clsCell
                    With lCell
                        If lFields(1) <> "0" AndAlso lFields(2) <> "0" _
                           AndAlso Double.TryParse(lFields(1), .Latitude) AndAlso lLatitude > -90 AndAlso lLatitude < 90 _
                           AndAlso Double.TryParse(lFields(2), .Longitude) AndAlso lLongitude > -180 AndAlso lLongitude < 180 Then
                            Try
                                .MCC = lFields(3)
                                .MNC = lFields(4)
                                .LAC = lFields(5)
                                .ID = lFields(6)
                                Dim lKey As String = Format(.MCC, "000") & "," & Format(.MNC, "0000") & "," & Format(.LAC, "000000") & "," & Format(.ID, "000000000")
                                'Debug.WriteLine(lCell.Label & "  " & lKey)
                                lCells.Add(lKey, lCell)
                            Catch
                                'IO.File.AppendAllText(.FileName & ".badbin.txt", lLine & vbCrLf)
                            End Try
                        Else
                            'IO.File.AppendAllText(.FileName & ".bad.txt", lLine & vbCrLf)
                        End If
                    End With
                End If
            End While

            If lCells.Count > 0 Then
                IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(aSaveAs))
                Dim lWriter As New IO.BinaryWriter(New IO.FileStream(aSaveAs, IO.FileMode.Create))
                If aSaveBinary Then
                    lWriter.Write(BinaryMagic)
                Else 'header designed to be compatible with http://www.opencellid.org/measure/upload
                    lWriter.Write(System.Text.Encoding.UTF8.GetBytes("mcc,mnc,lac,cellid,lat,lon" & vbLf))
                End If
                For Each lCell In lCells.Values
                    If aSaveBinary Then
                        lCell.Write(lWriter)
                    Else 'Save as text
                        With lCell
                            lWriter.Write(System.Text.Encoding.UTF8.GetBytes(.MCC & "," & .MNC & "," & .LAC & "," & .ID & "," & .Latitude & "," & .Longitude & vbLf))
                        End With
                    End If
                Next
                lWriter.Close()

                If aSaveBinary Then SaveAppSetting("OpenCellIDBinaryDatabase", aSaveAs)
                Return True
            End If
        Catch e As Exception
        End Try
        Return False
    End Function

End Class
