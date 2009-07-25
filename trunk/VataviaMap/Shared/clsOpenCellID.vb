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

    Public Shared Function ConvertDatabase(ByVal aDatabaseFilename As String, _
                                           ByVal aSaveAs As String, _
                                           ByVal aSaveBinary As Boolean, _
                                           ByVal aMCCs() As String) As Boolean
        Try
            Dim lLatitude As Double
            Dim lLongitude As Double
            Dim lFields() As String
            Dim lAllMCCs As Boolean = aMCCs Is Nothing OrElse aMCCs.Length = 0 OrElse Not IsNumeric(aMCCs(0))

            Dim lReader As IO.StreamReader = IO.File.OpenText(aDatabaseFilename)
            IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(aSaveAs))
            Dim lWriter As New IO.BinaryWriter(New IO.FileStream(aSaveAs, IO.FileMode.Create))
            lWriter.Write(BinaryMagic)
            'TODO: sort values in converted version for faster searching
            Dim lCell As New clsCell
            Dim lInputLine As String
            While Not lReader.EndOfStream
                lInputLine = lReader.ReadLine()
                lFields = lInputLine.Split(","c)
                If lAllMCCs OrElse Array.IndexOf(aMCCs, lFields(3)) > -1 Then
                    If lFields(1) <> "0" AndAlso lFields(2) <> "0" _
                       AndAlso Double.TryParse(lFields(1), lCell.Latitude) AndAlso lLatitude > -90 AndAlso lLatitude < 90 _
                       AndAlso Double.TryParse(lFields(2), lCell.Longitude) AndAlso lLongitude > -180 AndAlso lLongitude < 180 Then

                        If aSaveBinary Then
                            Try
                                lCell.MCC = lFields(3)
                                lCell.MNC = lFields(4)
                                lCell.LAC = lFields(5)
                                lCell.ID = lFields(6)
                                lCell.Write(lWriter)
                            Catch
                                'IO.File.AppendAllText(.FileName & ".badbin.txt", lLine & vbCrLf)
                            End Try
                        Else 'Save as text
                            IO.File.AppendAllText(aSaveAs, lInputLine & vbLf) 'lFields(5) & "," & lFields(6) & "," & lFields(1) & "," & lFields(2) & vbLf)
                        End If
                    Else
                        'IO.File.AppendAllText(.FileName & ".bad.txt", lLine & vbCrLf)
                    End If
                End If
            End While
            lWriter.Close()

            If aSaveBinary Then SaveAppSetting("OpenCellIDBinaryDatabase", aSaveAs)
            Return True
        Catch e As Exception
        End Try
        Return False
    End Function

End Class
