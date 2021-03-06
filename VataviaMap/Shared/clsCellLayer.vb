''' <summary>
''' Reads a binary file containing GSM cell IDs and Latitude/Longitude
''' Draws cells on a map (Inherits clsLayer)
''' </summary>
''' <remarks></remarks>
Public Class clsCellLayer
    Inherits clsLayer

    Private pCells As New Generic.List(Of clsCell)
    Private pLastCell As Integer = 0
    Private pCenterComputed As Boolean = False
    Private pCenterLat As Double
    Private pCenterLon As Double

    Private pGroupBy As Integer = 0 '0=no grouping, 1=MCC, 2=MCC+MNC, 3=MCC+MNC+LAC, 4=MCC+MNC+LAC+ID
    Private pGroups As Generic.Dictionary(Of String, clsCellLayer)

    ''' <summary>
    ''' Create a new cell layer from a binary file
    ''' </summary>
    ''' <param name="aBinaryFilename">name of binary file to open</param>
    ''' <param name="aMap">Map to draw this layer on, can be Nothing to open layer just for reading/writing cell information</param>
    ''' <remarks>binary file must be in format written by this class, starting with BinaryMagic then filled with clsCell.Write</remarks>
    Public Sub New(ByVal aBinaryFilename As String, ByVal aMap As ctlMap)
        MyBase.New(aBinaryFilename, aMap)
        SetDefaults()
        Me.LoadBinary(aBinaryFilename)
    End Sub

    Public Sub New(ByVal aMap As ctlMap)
        MyBase.New("", aMap)
        SetDefaults()
    End Sub

    Public Overrides Sub Clear()
        MyBase.Clear()
        SetDefaults()
    End Sub

    Public Overrides Property GroupField() As String
        Get
            Return pGroupField
        End Get
        Set(ByVal value As String)
            If pGroupField Is Nothing OrElse pGroupField <> value Then
                pGroupField = value
                Select Case pGroupField
                    Case "MobileCountryCode" : GroupBy = 1
                    Case "MobileNetworkCode" : GroupBy = 2
                    Case "LocalAreaCode" : GroupBy = 3
                    Case "CellID" : GroupBy = 4
                    Case "Filename" : GroupBy = 5
                    Case Else : GroupBy = 0
                End Select
            End If
        End Set
    End Property

    Public Property GroupBy() As Integer
        Get
            Return pGroupBy
        End Get
        Set(ByVal value As Integer)
            If value <> pGroupBy Then
                pGroupBy = value
                If pGroups Is Nothing Then
                    pGroups = New Generic.Dictionary(Of String, clsCellLayer)
                Else
                    pGroups.Clear()
                End If
                If pGroupBy > 0 Then
                    Dim lKey As String = ""
                    Dim lGroup As clsCellLayer
                    For Each lCell As clsCell In pCells
                        With lCell
                            Select Case pGroupBy
                                Case 1 : lKey = .MCC
                                Case 2 : lKey = .MCC & "." & .MNC
                                Case 3 : lKey = .MCC & "." & .MNC & "." & .LAC
                                Case 4 : lKey = .MCC & "." & .MNC & "." & .LAC & "." & .ID
                            End Select                            
                        End With
                        If pGroups.ContainsKey(lKey) Then
                            lGroup = pGroups.Item(lKey)
                        Else
                            lGroup = New clsCellLayer(Map)
                            lGroup.LegendColor = RandomRGBColor(100)
                            pGroups.Add(lKey, lGroup)
                        End If
                        lGroup.AddCell(lCell, False)
                    Next
                End If
            End If
        End Set
    End Property

#Region "Drawing Code"

    Public SymbolSize As Integer
    Public SymbolPen As Pen

    Private pFields() As String = {"MobileCountryCode", _
                                   "MobileNetworkCode", _
                                   "LocalAreaCode", _
                                   "CellID", _
                                   "Filename"}

    Private Sub SetDefaults()
        Me.Bounds = New clsGPXbounds
        BrushLabel = New SolidBrush(Color.Black)
        FontLabel = New Font("Arial", 10, FontStyle.Regular)
        SymbolSize = 25
    End Sub

    Public Overrides Function Fields() As String()
        Return pFields
    End Function

    Public Overrides Sub Render(ByVal aTileServer As clsServer, ByVal g As Graphics, ByVal aTopLeftTile As Point, ByVal aOffsetToCenter As Point)
        If Me.Visible Then
            Dim lDrawLayer As Boolean = True
            If pBounds IsNot Nothing Then
                With pBounds 'Skip drawing if it is not in view
                    If .minlat > Map.LatMax OrElse .maxlat < Map.LatMin OrElse _
                       .minlon > Map.LonMax OrElse .maxlon < Map.LonMin Then

                        lDrawLayer = False
                    End If
                End With
            End If

            If lDrawLayer Then
                If pGroups IsNot Nothing AndAlso pGroups.Count > 0 Then
                    For Each lGroup As clsLayer In pGroups.Values
                        lGroup.Render(aTileServer, g, aTopLeftTile, aOffsetToCenter)
                    Next
                Else
                    Dim lCell As clsCell = Nothing
                    If Not pCenterComputed Then
                        'pCenterLat = 0
                        'pCenterLon = 0
                        'For Each lCell In pCells
                        '    pCenterLat += lCell.Latitude
                        '    pCenterLon += lCell.Longitude
                        'Next
                        'pCenterLat /= pCells.Count
                        'pCenterLon /= pCells.Count

                        Dim lMinLat As Double = pCells(0).Latitude
                        Dim lMaxLat As Double = lMinLat
                        Dim lMinLon As Double = pCells(0).Longitude
                        Dim lMaxLon As Double = lMinLon

                        For Each lCell In pCells
                            If lCell.Latitude < lMinLat Then
                                lMinLat = lCell.Latitude
                            ElseIf lCell.Latitude > lMaxLat Then
                                lMaxLat = lCell.Latitude
                            End If
                            If lCell.Longitude < lMinLon Then
                                lMinLon = lCell.Longitude
                            ElseIf lCell.Longitude > lMaxLon Then
                                lMaxLon = lCell.Longitude
                            End If
                        Next
                        pCenterLat = (lMinLat + lMaxLat) / 2
                        pCenterLon = (lMinLon + lMaxLon) / 2

                    End If
                    SymbolPen = New Pen(Me.LegendColor, 1)

                    Dim lTileXY As Point 'Which tile this point belongs in
                    Dim lTileOffset As Point 'Offset within lTileXY in pixels
                    lTileXY = CalcTileXY(aTileServer, pCenterLat, pCenterLon, Map.Zoom, lTileOffset)
                    Dim lCenterX As Integer = (lTileXY.X - aTopLeftTile.X) * aTileServer.TileSize + aOffsetToCenter.X + lTileOffset.X
                    Dim lCenterY As Integer = (lTileXY.Y - aTopLeftTile.Y) * aTileServer.TileSize + aOffsetToCenter.Y + lTileOffset.Y

                    Dim lSkip As Integer = pCells.Count / 200
                    Dim lSkipped As Integer = 0
                    For Each lCell In pCells
                        If lSkip > lSkipped Then
                            lSkipped += 1
                        Else
                            lSkipped = 0
                            With lCell
                                lTileXY = CalcTileXY(aTileServer, .Latitude, .Longitude, Map.Zoom, lTileOffset)
                                Dim lX As Integer = (lTileXY.X - aTopLeftTile.X) * aTileServer.TileSize + aOffsetToCenter.X + lTileOffset.X
                                Dim lY As Integer = (lTileXY.Y - aTopLeftTile.Y) * aTileServer.TileSize + aOffsetToCenter.Y + lTileOffset.Y

                                g.DrawLine(SymbolPen, lX, lY, lCenterX, lCenterY)

                            End With
                        End If
                    Next
                    If Map.Zoom >= LabelMinZoom Then
                        Try
                            Dim lLabelText As String = Nothing
                            Select Case LabelField
                                Case "MobileCountryCode" : lLabelText = lCell.MCC
                                Case "MobileNetworkCode" : lLabelText = lCell.MNC
                                Case "LocalAreaCode" : lLabelText = lCell.LAC
                                Case "CellID" : lLabelText = lCell.ID
                                Case "CellLabel" : lLabelText = lCell.ToString
                                Case "Filename" : lLabelText = IO.Path.GetFileNameWithoutExtension(Filename)
                            End Select
                            If lLabelText IsNot Nothing AndAlso lLabelText.Length > 0 Then
                                g.DrawString(lLabelText, FontLabel, BrushLabel, lCenterX, lCenterY)
                            End If
                        Catch
                        End Try
                    End If
                End If
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
    Private Function DrawCell(ByVal aTileServer As clsServer, _
                              ByVal g As Graphics, _
                              ByVal aCell As clsCell, _
                              ByVal aTopLeftTile As Point, _
                              ByVal aOffsetToCenter As Point) As Boolean
        With aCell
            Dim lTileXY As Point 'Which tile this point belongs in
            Dim lTileOffset As Point 'Offset within lTileXY in pixels
            lTileXY = CalcTileXY(aTileServer, .Latitude, .Longitude, Map.Zoom, lTileOffset)
            Dim lX As Integer = (lTileXY.X - aTopLeftTile.X) * aTileServer.TileSize + aOffsetToCenter.X + lTileOffset.X
            Dim lY As Integer = (lTileXY.Y - aTopLeftTile.Y) * aTileServer.TileSize + aOffsetToCenter.Y + lTileOffset.Y

            If Map.Zoom < 16 Then
                g.DrawLine(SymbolPen, lX - 3, lY - 3, lX + 3, lY + 3)
                g.DrawLine(SymbolPen, lX - 3, lY + 3, lX + 3, lY - 3)
            Else
                g.DrawEllipse(SymbolPen, lX - 5, lY - 5, 10, 10)

                If Map.Zoom > 15 Then
                    Try
                        g.DrawString(.ToString, FontLabel, BrushLabel, lX, lY)
                    Catch
                    End Try
                End If
            End If
        End With
    End Function
#End Region 'Drawing Code

    ''' <summary>
    ''' Find the latitude and longitude of aCell and set them inside aCell
    ''' </summary>
    ''' <param name="aCell"></param>
    ''' <returns>True if location was found and set, false if location was not found in this layer</returns>
    ''' <remarks></remarks>
    <CLSCompliant(False)> _
    Public Function GetCellLocation(ByVal aCell As clsCell) As Boolean
        For Each lCell As clsCell In pCells
            If lCell.CompareTo(aCell) = 0 Then
                aCell.Latitude = lCell.Latitude
                aCell.Longitude = lCell.Longitude
                Return True
            End If
        Next
        Return False
    End Function

#Region "Binary File Handling"

    Private Const BinaryMagic As UInt32 = &H43454C4C 'Magic number written at beginning of binary file spells "CELL"

    <CLSCompliant(False)> _
    Public Sub AddCell(ByVal aCell As clsCell, ByVal aSaveNow As Boolean)
        pCells.Add(aCell)
        pBounds.Expand(aCell.Latitude, aCell.Longitude)
        pCenterComputed = False
        If aSaveNow Then
            Dim lNeedsMagic As Boolean = Not IO.File.Exists(Filename)
            If lNeedsMagic Then IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(Filename))
            Dim lWriter As New IO.BinaryWriter(New IO.FileStream(Filename, IO.FileMode.Append, IO.FileAccess.Write))
            If lNeedsMagic Then lWriter.Write(BinaryMagic)
            aCell.Write(lWriter)
            lWriter.Close()
        End If
    End Sub

    ''' <summary>
    ''' Add the contents of the specified binary file to the layer
    ''' </summary>
    ''' <param name="aBinaryFilename"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function LoadBinary(ByVal aBinaryFilename As String) As Boolean
        Dim lSuccess As Boolean = False
        Dim lReader As New IO.BinaryReader(IO.File.OpenRead(aBinaryFilename))
        If lReader.ReadUInt32 <> BinaryMagic Then
            Throw New ApplicationException("Unknown file type '" & aBinaryFilename & "'")
        Else
            'Subtract 4 bytes for BinaryMagic
            'Dim lLastCell As Integer = (FileSize(aBinaryFilename) - 4) / clsCell.NumBytes - 1
            Try
                Dim lCell As clsCell
                Do
                    lCell = New clsCell(lReader)
                    pCells.Add(lCell)
                    pBounds.Expand(lCell.Latitude, lCell.Longitude)
                    lSuccess = True
                Loop
            Catch
            End Try
        End If
        Return lSuccess
    End Function

    ''' <summary>
    ''' Save in binary formatted file for faster reading later
    ''' </summary>
    ''' <param name="aSaveAs">File name to save binary formatted data in</param>
    ''' <param name="aSplitCells">
    ''' 0 to put all cells in file aSaveAs    
    ''' 1 to put cells in separate files aSaveAs.[MobileCountryCode]
    ''' 2 to put cells in separate files aSaveAs.[MobileCountryCode].[MobileNetworkCode]
    ''' 3 to put cells in separate files aSaveAs.[MobileCountryCode].[MobileNetworkCode].[LocalAreaCode]
    ''' 4 to put cells in separate files aSaveAs.[MobileCountryCode].[MobileNetworkCode].[LocalAreaCode].[CellID]
    ''' </param>
    ''' <remarks>
    ''' Extension of ".cell" is suggested. 
    ''' When splitting, extension of aSaveAs is moved to end of file name.
    ''' </remarks>
    Public Function SaveBinary(ByVal aSaveAs As String, Optional ByVal aSplitCells As Integer = 0) As Boolean
        Try
            Dim lExtension As String = IO.Path.GetExtension(aSaveAs)
            Dim lLastFilename As String = ""
            Dim lNewFilename As String = aSaveAs
            IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(aSaveAs))
            Dim lWriter As IO.BinaryWriter = Nothing
            For Each lCell As clsCell In pCells
                With lCell
                    Select Case aSplitCells
                        Case 1 : lNewFilename = IO.Path.ChangeExtension(aSaveAs, .MCC) & lExtension
                        Case 2 : lNewFilename = IO.Path.ChangeExtension(aSaveAs, .MCC & "." & .MNC) & lExtension
                        Case 3 : lNewFilename = IO.Path.ChangeExtension(aSaveAs, .MCC & "." & .MNC & "." & .LAC) & lExtension
                        Case 4 : lNewFilename = IO.Path.ChangeExtension(aSaveAs, .ToString) & lExtension
                    End Select
                    If lNewFilename <> lLastFilename Then
                        If lWriter IsNot Nothing Then lWriter.Close()
                        Dim lNeedMagic As Boolean = Not IO.File.Exists(lNewFilename)
                        lWriter = New IO.BinaryWriter(New IO.FileStream(lNewFilename, IO.FileMode.Append))
                        If lNeedMagic Then lWriter.Write(BinaryMagic)
                        lLastFilename = lNewFilename
                    End If
                    lCell.Write(lWriter)
                End With
            Next
            If lWriter IsNot Nothing Then
                lWriter.Close()
                Filename = aSaveAs
                Return True
            End If
        Catch e As Exception
        End Try
        Return False
    End Function
#End Region 'Binary File Handling

    ''' <summary>
    ''' Save in binary formatted file for faster reading later
    ''' </summary>
    ''' <param name="aSaveAs">File name to save binary formatted data in</param>
    ''' <remarks></remarks>
    Public Function SaveCSV(ByVal aSaveAs As String) As Boolean
        Try
            IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(aSaveAs))
            Dim lWriter As New IO.BinaryWriter(New IO.FileStream(aSaveAs, IO.FileMode.Create))
            'header designed to be compatible with http://www.opencellid.org/measure/upload
            lWriter.Write(System.Text.Encoding.UTF8.GetBytes("mcc,mnc,lac,cellid,lat,lon" & vbLf))
            For Each lCell As clsCell In pCells
                With lCell
                    lWriter.Write(System.Text.Encoding.UTF8.GetBytes(.MCC & "," & .MNC & "," & .LAC & "," & .ID & "," & .Latitude & "," & .Longitude & vbLf))
                End With
            Next
            lWriter.Close()
            Return True
        Catch e As Exception
        End Try
        Return False
    End Function

    ''' <summary>
    ''' Convert from OpenCellID CSV formatted raw list of cells into binary or text format
    ''' </summary>
    ''' <param name="aCSVFilename">File as downloaded from OpenCellID Raw Data, cells.txt.gz, uncompressed</param>
    ''' <param name="aMCCs">List of Mobile Country Codes to include in output</param>
    ''' <returns>True on success, False on failure</returns>
    ''' <remarks></remarks>
    Public Function LoadRawCSV(ByVal aCSVFilename As String, ByVal aMCCs() As String) As Boolean
        Try
            Dim lLatitude As Double
            Dim lLongitude As Double
            Dim lFields() As String
            Dim lAllMCCs As Boolean = aMCCs Is Nothing OrElse aMCCs.Length = 0 OrElse Not IsNumeric(aMCCs(0))

            Dim lReader As IO.StreamReader = IO.File.OpenText(aCSVFilename)
            Dim lCell As clsCell
            Dim lCells As New Generic.List(Of clsCell)
            Dim lInputLine As String
            While Not lReader.EndOfStream
                lInputLine = lReader.ReadLine()
                lFields = lInputLine.Split(","c)
                If lAllMCCs OrElse Array.IndexOf(aMCCs, lFields(3)) > -1 Then
                    lCell = New clsCell
                    With lCell
                        If lFields(1) <> "0" AndAlso lFields(2) <> "0" _
                           AndAlso DoubleTryParse(lFields(1), .Latitude) AndAlso lLatitude > -90 AndAlso lLatitude < 90 _
                           AndAlso DoubleTryParse(lFields(2), .Longitude) AndAlso lLongitude > -180 AndAlso lLongitude < 180 Then
                            Try
                                .MCC = lFields(3)
                                .MNC = lFields(4)
                                .LAC = lFields(5)
                                .ID = lFields(6)
                                lCells.Add(lCell)
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
                pCells.AddRange(lCells)
                pCells.Sort()
            End If
            Return True
        Catch e As Exception
        End Try
        Return False
    End Function

    Public Function LoadGPX(ByVal aGPX As clsGPX, ByVal aMCCs() As String) As Boolean
        Try
            Dim lMCCs(-1) As UInt16

            Dim lAllMCCs As Boolean = aMCCs Is Nothing OrElse aMCCs.Length = 0 OrElse Not IsNumeric(aMCCs(0))
            If Not lAllMCCs Then
                Dim lLastMCC As Integer = aMCCs.GetUpperBound(0)
                ReDim lMCCs(lLastMCC)
                For lIndex As Integer = 0 To lLastMCC
                    lMCCs(lIndex) = UInt16.Parse(aMCCs(lIndex))
                Next
            End If

            Dim lCell As clsCell
            Dim lCells As New Generic.List(Of clsCell)

            For Each lTrack As clsGPXtrack In aGPX.trk
                For Each lTrackSegment As clsGPXtracksegment In lTrack.trkseg
                    Dim lLastX As Integer = -1
                    Dim lLastY As Integer = -1
                    For Each lTrackPoint As clsGPXwaypoint In lTrackSegment.trkpt
                        Try
                            lCell = Nothing
                            'VataviaMap at first put cell tower information in the name field, then in extension "celltower", then in extension "cellid".
                            'We check for what the latest version writes first, then if not found we check for older ways it has been written
                            lCell = clsCell.Parse(lTrackPoint.GetExtension("cellid")) 'Default order of "C.N.L.I" when using this extension
                            If lCell Is Nothing Then lCell = clsCell.Parse(lTrackPoint.GetExtension("celltower"), "I.L.C.N")
                            If lCell Is Nothing Then lCell = clsCell.Parse(lTrackPoint.name, "I.L.C.N")

                            If lCell IsNot Nothing Then
                                If lAllMCCs OrElse Array.IndexOf(lMCCs, lCell.MCC) > -1 Then
                                    lCell.Latitude = lTrackPoint.lat
                                    lCell.Longitude = lTrackPoint.lon
                                    lCells.Add(lCell)
                                End If
                            End If
                        Catch e As Exception
                        End Try
                    Next
                Next
            Next

            If lCells.Count > 0 Then
                pCells.AddRange(lCells)
                pCells.Sort()
                Return True
            End If
        Catch e As Exception
        End Try
        Return False
    End Function

End Class
