Public Class clsLayer
    Public Filename As String
    Public Visible As Boolean = True
    Public MapForm As frmMap
    Private pBounds As clsGPXbounds
    Private pLegendColor As Color = Drawing.Color.White

    Public Sub New(ByVal aFilename As String, ByVal aMapForm As frmMap)
        Filename = aFilename
        MapForm = aMapForm
    End Sub

    Public Overridable Property Bounds() As clsGPXbounds
        Get
            Return pBounds
        End Get
        Set(ByVal value As clsGPXbounds)
            pBounds = value
        End Set
    End Property

    Public Overridable Sub Clear()
        Filename = ""
    End Sub

    Public Overridable Property LegendColor() As Color
        Get
            Return pLegendColor
        End Get
        Set(ByVal value As Color)
            pLegendColor = value
        End Set
    End Property

    Public Overridable Sub Render(ByVal g As Graphics, ByVal aTopLeftTile As Point, ByVal aOffsetToCenter As Point)
    End Sub
End Class

Public Class clsBuddy
    Public Name As String
    Public LocationURL As String
    Public IconFilename As String = ""
    Public IconURL As String = ""
    Public Waypoint As clsGPXwaypoint
    Public Selected As Boolean = True

    Public Function LoadFile(ByVal aFilename As String) As Boolean
        Dim lFileContents As String = ReadTextFile(aFilename)
        If lFileContents.IndexOf("<?xml") = 0 Then
            Return LoadLatitudeKML(lFileContents)
        ElseIf lFileContents.IndexOf("google.com/latitude/") > 0 Then
            Return LoadLatitudeJSON(lFileContents)
        ElseIf lFileContents.IndexOf("InstaMapper API") = 0 Then
            Return LoadInstaMapperCSV(lFileContents)
        ElseIf lFileContents.IndexOf("device_label") > 0 Then
            Return LoadInstaMapperJSON(lFileContents)
        Else
            Return LoadDMTP(lFileContents)
        End If
    End Function

    ''' <summary>
    ''' Load details from a Google Latitude KML request
    ''' </summary>
    ''' <param name="aFileContents">Contents of downloaded point</param>
    ''' <returns>True if able to parse buddy location</returns>
    ''' <remarks></remarks>
    Public Function LoadLatitudeKML(ByVal aFileContents As String) As Boolean
        Dim lcoords() As String = GetXmlTagContents(aFileContents, "coordinates").Split(",")
        If lcoords.Length > 0 Then
            Dim lLongitude As Double, lLatitude As Double
            Try
                lLongitude = Double.Parse(lcoords(0))
                lLatitude = Double.Parse(lcoords(1))
                Waypoint = New clsGPXwaypoint("wpt", lLatitude, lLongitude)
                With Waypoint
                    .name = Name
                    .desc = GetXmlTagContents(aFileContents, "description")
                    .sym = Name
                    'TODO: parse time/accuracy from description
                    Return True
                End With
            Catch ex As Exception
                Debug.WriteLine(ex.Message)
            End Try
        End If
        Return False
    End Function

    ''' <summary>
    ''' Load details from a Google Latitude JSON request
    ''' </summary>
    ''' <param name="aFileContents">Contents of downloaded point</param>
    ''' <returns>True if able to parse buddy location</returns>
    ''' <remarks>If buddy does not yet have an icon, also try downloading the buddy icon</remarks>
    Public Function LoadLatitudeJSON(ByVal aFileContents As String) As Boolean
        Dim lcoords() As String = GetJSONTagContents(aFileContents, "coordinates").TrimStart("[").TrimEnd("]", "}").Split(",")
        If lcoords.Length = 2 Then
            Dim lLongitude As Double, lLatitude As Double
            Try
                lLongitude = Double.Parse(lcoords(0))
                lLatitude = Double.Parse(lcoords(1))
                Waypoint = New clsGPXwaypoint("wpt", lLatitude, lLongitude)
                With Waypoint
                    .desc = GetJSONTagContents(aFileContents, "reverseGeocode")
                    If Not IO.File.Exists(IconFilename) Then
                        IconURL = GetJSONTagContents(aFileContents, "photoUrl")
                        If IconURL.Length > 0 Then
                            IconFilename = IO.Path.Combine(IO.Path.GetTempPath, Name & ".png")
                        End If
                    End If
                    .sym = IconFilename
                    .name = Name
                    'TODO: decode timeStamp to GMT
                    Return True
                End With
            Catch ex As Exception
                Debug.WriteLine(ex.Message)
            End Try
        End If
        Return False
    End Function

    ''' <summary>
    ''' Load the default (comma-separated) point type returned by InstaMapper API
    ''' </summary>
    ''' <param name="aFileContents">Contents of downloaded point</param>
    ''' <returns>True if able to parse buddy location</returns>
    Public Function LoadInstaMapperCSV(ByVal aFileContents As String) As Boolean
        'Example URL: http://www.instamapper.com/api?action=getPositions&key=584014439054448247&num=1
        'Field names:
        'Device key,Device label,Timestamp in UTC (seconds since 1970-1-1),Latitude,Longitude,Altitude in meters,Speed in meters / second,Heading in degrees
        'Example Result:
        'InstaMapper API v1.00
        '0071543339905,Demo car,1209252229,47.50417,-122.19697,25.0,20.5,344
        Try
            Dim lPosition As String = aFileContents.Substring(aFileContents.IndexOf(vbLf) + 1)
            Dim lFields() As String = lPosition.Split(","c)
            If lFields.Length > 4 Then
                Dim lLat As Double = lFields(3)
                Dim lLon As Double = lFields(4)
                Waypoint = New clsGPXwaypoint("wpt", lLat, lLon)
                With Waypoint
                    Dim lBaseDate As New Date(1970, 1, 1)
                    .name = lFields(1)
                    .time = lBaseDate.AddSeconds(lFields(2))
                    If lFields.Length > 5 AndAlso lFields(5).Length > 0 Then .ele = lFields(5)
                    If lFields.Length > 6 AndAlso lFields(6).Length > 0 Then .speed = lFields(6)
                    If lFields.Length > 7 AndAlso lFields(7).Length > 0 Then .course = lFields(7)
                    Return True
                End With
            End If
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
        End Try
        Return False
    End Function

    ''' <summary>
    ''' Load JSON formatted result from InstaMapper API
    ''' </summary>
    ''' <param name="aFileContents">Contents of downloaded point</param>
    ''' <returns>True if able to parse buddy location</returns>
    Public Function LoadInstaMapperJSON(ByVal aFileContents As String) As Boolean
        'Example URL: http://www.instamapper.com/api?action=getPositions&key=584014439054448247&num=1&format=json
        'Example Result:
        ' { "version" : "1.0",
        '   "positions" : [
        '     { "device_key" : "0071543339995",
        '       "device_label" : "Demo car",
        '       "timestamp" : 1209252709,
        '       "latitude" : 47.60969,
        '       "longitude" : -122.18833,
        '       "altitude" : 0.0,
        '       "speed" : 25.0,
        '       "heading" : 349 }
        '   ]
        ' }
        Try
            Dim lLat As Double = GetJSONTagContents(aFileContents, "latitude")
            Dim lLon As Double = GetJSONTagContents(aFileContents, "longitude")
            Waypoint = New clsGPXwaypoint("wpt", lLat, lLon)

            With Waypoint
                Dim lBaseDate As New Date(1970, 1, 1)
                .time = lBaseDate.AddSeconds(GetJSONTagContents(aFileContents, "timestamp"))
                .name = GetJSONTagContents(aFileContents, "device_label")

                Dim lValue As String
                lValue = GetJSONTagContents(aFileContents, "altitude") : If IsNumeric(lValue) Then .ele = lValue
                lValue = GetJSONTagContents(aFileContents, "speed") : If IsNumeric(lValue) Then .speed = lValue
                lValue = GetJSONTagContents(aFileContents, "heading") : If IsNumeric(lValue) Then .course = lValue
            End With
            Return True
        Catch ex As Exception
            Debug.WriteLine(ex.Message)
        End Try
        Return False
    End Function

    ''' <summary>
    ''' Load details from OpenDMTP-formatted log entry
    ''' </summary>
    ''' <param name="aFileContents">Contents of downloaded point</param>
    ''' <returns>True if able to parse buddy location</returns>
    Public Function LoadDMTP(ByVal aFileContents As String) As Boolean
        Dim lFields() As String = aFileContents.Split(","c)
        If lFields.Length > 4 AndAlso lFields(0).Length > 0 Then
            Dim lLat As Double = lFields(3)
            Dim lLon As Double = lFields(4)

            Waypoint = New clsGPXwaypoint("wpt", lLat, lLon)
            With Waypoint
                If lFields.Length > 5 AndAlso lFields(5).Length > 0 Then .speed = lFields(5)
                If lFields.Length > 6 AndAlso lFields(6).Length > 0 Then .course = lFields(6)
                If lFields.Length > 7 AndAlso lFields(7).Length > 0 Then .ele = lFields(7)
                If lFields.Length > 8 AndAlso lFields(8).Length > 0 Then
                    .time = Date.Parse(lFields(8)) 'we have UTC from GPS in the record
                Else                               'use local time from logged point
                    .time = Date.Parse(lFields(0) & " " & lFields(1)).ToUniversalTime()
                End If
                .name = Name
                .sym = Name
            End With
            Return True
        End If
        Return False
    End Function

    Private Function GetXmlTagContents(ByVal aXML As String, ByVal aTag As String) As String
        Dim lStartPos As Integer = aXML.IndexOf("<" & aTag & ">", StringComparison.CurrentCultureIgnoreCase)
        If lStartPos < 0 Then Return ""
        lStartPos += aTag.Length + 2
        Dim lEndPos As Integer = aXML.IndexOf("</" & aTag & ">", lStartPos, StringComparison.CurrentCultureIgnoreCase)
        If lEndPos < 0 Then
            Return aXML.Substring(lStartPos)
        Else
            Return aXML.Substring(lStartPos, lEndPos - lStartPos)
        End If
    End Function

    Private Function GetJSONTagContents(ByVal aSource As String, ByVal aTag As String) As String
        Dim lStartPos As Integer = aSource.IndexOf("""" & aTag & """", StringComparison.CurrentCultureIgnoreCase)
        If lStartPos < 0 Then Return ""
        lStartPos += aTag.Length + 2
        Dim lEndPos As Integer = aSource.IndexOf(vbLf, lStartPos, StringComparison.CurrentCultureIgnoreCase)
        If lEndPos < 0 Then
            aSource = aSource.Substring(lStartPos)
        Else
            aSource = aSource.Substring(lStartPos, lEndPos - lStartPos)
        End If
        Return aSource.TrimStart(" ", ":", """").TrimEnd(" ", """", ",")
    End Function
End Class

Public Class clsLayerGPX
    Inherits clsLayer

    Public GPX As clsGPX

    Public PenTrack As Pen
    Public PenGeocache As Pen
    Public PenWaypoint As Pen

    Public LabelField As String
    Public BrushLabel As SolidBrush
    Public FontLabel As Font

    Public SymbolSize As Integer
    Public SymbolPen As Drawing.Pen

    Public ArrowSize As Integer
    Private pDistSinceArrow As Integer

    Private Sub SetDefaults()
        PenTrack = New Pen(Color.FromArgb(-2147432248), 4) '128, 0, 200, 200))
        PenGeocache = New Pen(Color.Green)
        PenWaypoint = New Pen(Color.Navy)

        LabelField = ""
        BrushLabel = New SolidBrush(Color.Black)
        FontLabel = New Font("Arial", 10, FontStyle.Regular)

        SymbolSize = 10
        SymbolPen = PenTrack

        ArrowSize = 0
        pDistSinceArrow = 0
    End Sub

    Public Sub New(ByVal aFilename As String, ByVal aMapForm As frmMap)
        MyBase.New(aFilename, aMapForm)
        SetDefaults()
        GPX = New clsGPX
        If IO.File.Exists(aFilename) Then
            GPX.LoadFile(aFilename)
        End If
    End Sub

    Public Sub New(ByVal aWaypoints As Generic.List(Of clsGPXwaypoint), ByVal aMapForm As frmMap)
        MyBase.New("", aMapForm)
        SetDefaults()
        GPX = New clsGPX
        GPX.wpt = aWaypoints
    End Sub

    Public Overrides Property Bounds() As clsGPXbounds
        Get
            Return GPX.bounds
        End Get
        Set(ByVal value As clsGPXbounds)
            GPX.bounds = value
        End Set
    End Property

    Public Overrides Sub Clear()
        MyBase.Clear()
        GPX.Clear()
    End Sub

    Public Overrides Property LegendColor() As System.Drawing.Color
        Get
            Return PenTrack.Color
        End Get
        Set(ByVal value As System.Drawing.Color)
            PenTrack.Color = value
        End Set
    End Property

    Public Overrides Sub Render(ByVal g As Graphics, ByVal aTopLeftTile As Point, ByVal aOffsetToCenter As Point)
        If Me.Visible Then
            Dim lDrawThisOne As Boolean = True
            If GPX.bounds IsNot Nothing Then
                With GPX.bounds 'Skip drawing this one if it is not in view
                    If .minlat > MapForm.CenterLat + MapForm.LatHeight / 2 OrElse _
                        .maxlat < MapForm.CenterLat - MapForm.LatHeight / 2 OrElse _
                        .minlon > MapForm.CenterLon + MapForm.LonWidth / 2 OrElse _
                        .maxlon < MapForm.CenterLon - MapForm.LonWidth / 2 Then
                        lDrawThisOne = False
                    End If
                End With
            End If
            If lDrawThisOne Then
                pDistSinceArrow = 0
                Dim lStartTime As Date = Date.Now
                For Each lWaypoint As clsGPXwaypoint In GPX.wpt
                    DrawWaypoint(g, lWaypoint, aTopLeftTile, aOffsetToCenter)
                    If Date.Now.Subtract(lStartTime).TotalSeconds > 2 Then Exit For
                Next

                If GPX.trk.Count = 1 AndAlso (GPX.trk(0).trkseg.Count = 1) AndAlso (GPX.trk(0).trkseg(0).trkpt.Count = 1) Then
                    DrawWaypoint(g, GPX.trk(0).trkseg(0).trkpt(0), aTopLeftTile, aOffsetToCenter)
                Else

#If Not Smartphone Then
                    Dim lSymbolPath As New Drawing2D.GraphicsPath()
                    Dim lTrackPath As New Drawing2D.GraphicsPath()
#End If

                    lStartTime = Date.Now
                    For Each lTrack As clsGPXtrack In GPX.trk
                        For Each lTrackSegment As clsGPXtracksegment In lTrack.trkseg
                            Dim lLastX As Integer = -1
                            Dim lLastY As Integer = -1
                            For Each lTrackPoint As clsGPXwaypoint In lTrackSegment.trkpt

#If Not Smartphone Then
                                If Not DrawTrackpoint(lTrackPoint, aTopLeftTile, aOffsetToCenter, lSymbolPath, lTrackPath, lLastX, lLastY) Then
                                    If lTrackPath.PointCount > 0 Then
                                        g.DrawPath(PenTrack, lTrackPath)
                                        lTrackPath.Reset()
                                    End If
#Else
                                If Not DrawTrackpoint(g, lTrackPoint, aTopLeftTile, aOffsetToCenter, lLastX, lLastY) Then
#End If
                                    lLastX = -1
                                    lLastY = -1
                                End If
                            Next
#If Not Smartphone Then
                            If lSymbolPath.PointCount > 0 AndAlso SymbolPen IsNot Nothing Then
                                g.DrawPath(SymbolPen, lSymbolPath)
                                lSymbolPath.Reset()
                            End If
                            If lTrackPath.PointCount > 0 AndAlso PenTrack IsNot Nothing Then
                                g.DrawPath(PenTrack, lTrackPath)
                                lTrackPath.Reset()
                            End If
#End If
                            If Date.Now.Subtract(lStartTime).TotalSeconds > 2 Then Exit For
                        Next
                    Next
                End If
            End If
        End If
    End Sub

    ''' <summary>
    ''' Draw a GPX waypoint, return True if it was drawn
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="aWaypoint"></param>
    ''' <param name="aTopLeftTile"></param>
    ''' <param name="aOffsetToCenter"></param>
    ''' <returns>True if waypoint was drawn, False if it was outside view</returns>
    ''' <remarks></remarks>
    Private Function DrawWaypoint(ByVal g As Graphics, _
                                  ByVal aWaypoint As clsGPXwaypoint, _
                                  ByVal aTopLeftTile As Point, _
                                  ByVal aOffsetToCenter As Point) As Boolean
        With aWaypoint
            If True Then 'MapForm.LatLonInView(.lat, .lon) Then
                Dim lTileXY As Point 'Which tile this point belongs in
                Dim lTileOffset As Point 'Offset within lTileXY in pixels
                lTileXY = CalcTileXY(.lat, .lon, MapForm.Zoom, lTileOffset)
                Dim lX As Integer = (lTileXY.X - aTopLeftTile.X) * g_TileSize + aOffsetToCenter.X + lTileOffset.X
                Dim lY As Integer = (lTileXY.Y - aTopLeftTile.Y) * g_TileSize + aOffsetToCenter.Y + lTileOffset.Y
                Dim lBitmap As Drawing.Bitmap = Nothing

                Select Case .sym
                    Case Nothing, "" 'Small X for no symbol
                        g.DrawLine(PenTrack, lX - 3, lY - 3, lX + 3, lY + 3)
                        g.DrawLine(PenTrack, lX - 3, lY + 3, lX + 3, lY - 3)
                    Case "Geocache"
                        If MapForm.Zoom > 10 AndAlso .type IsNot Nothing AndAlso g_WaypointIcons.ContainsKey(.type.ToLower) Then
                            lBitmap = g_WaypointIcons.Item(.type.ToLower)
                        Else
                            g.DrawLine(PenGeocache, lX, lY - 8, lX, lY + 8)
                            g.DrawLine(PenGeocache, lX - 8, lY, lX + 8, lY)
                        End If
                    Case "Waypoint"
                        g.DrawLine(PenWaypoint, lX - 8, lY - 8, lX + 8, lY + 8)
                        g.DrawLine(PenWaypoint, lX - 8, lY + 8, lX + 8, lY - 8)
                    Case Else
                        If g_WaypointIcons.ContainsKey(.sym.ToLower) Then
                            lBitmap = g_WaypointIcons.Item(.sym.ToLower)
                        ElseIf IO.File.Exists(.sym) Then
                            lBitmap = New Drawing.Bitmap(.sym)
                            g_WaypointIcons.Add(.sym.ToLower, lBitmap)
                        Else
                            g.DrawEllipse(PenTrack, lX - 5, lY - 5, 10, 10)
                        End If
                End Select
                If lBitmap IsNot Nothing Then
                    Dim lRectDest As New Rectangle(lX - lBitmap.Width / 2, lY - lBitmap.Height / 2, lBitmap.Width, lBitmap.Height)
                    Dim lRectSource As New Rectangle(0, 0, lBitmap.Width, lBitmap.Height)
                    Dim lImageAttributes As New Drawing.Imaging.ImageAttributes
                    Dim color As Color = lBitmap.GetPixel(0, 0)
                    lImageAttributes.SetColorKey(color, color)
                    g.DrawImage(lBitmap, lRectDest, 0, 0, lBitmap.Width, lBitmap.Height, GraphicsUnit.Pixel, lImageAttributes)
                    If MapForm.Zoom = g_ZoomMax Then  'Show more exact point at max zoom
                        g.DrawLine(PenGeocache, lX, lY - 8, lX, lY + 8)
                        g.DrawLine(PenGeocache, lX - 8, lY, lX + 8, lY)
                    End If
                End If

                'Debug.WriteLine(.lat & ", " & .lon & " -> " & lX & ", " & lY)
                If MapForm.Zoom > 12 Then 'AndAlso .sym IsNot Nothing Then
                    Try
                        Dim lLabelText As String = Nothing
                        Select Case LabelField
                            Case "name" : lLabelText = .name
                            Case "urlname" : lLabelText = .urlname
                            Case "desc" : lLabelText = .desc
                            Case "container" : lLabelText = .cache.container
                            Case "difficulty" : lLabelText = .cache.difficulty
                            Case "terrain" : lLabelText = .cache.difficulty
                            Case "encoded_hints" : lLabelText = .cache.encoded_hints
                            Case "hints" : lLabelText = ROT13(.cache.encoded_hints)
                            Case "age"
                                If aWaypoint.timeSpecified Then
                                    'Dim lTimeSince As TimeSpan = Now.Subtract(Date.Parse(lFields(0) & " " & lFields(1)))
                                    Dim lTimeSince As TimeSpan = Now.ToUniversalTime.Subtract(aWaypoint.time)
                                    Dim lTimeSinceString As String = " "
                                    If lTimeSince.TotalDays >= 1 Then lTimeSinceString &= lTimeSince.Days & "d "
                                    If lTimeSince.TotalHours >= 1 Then lTimeSinceString &= lTimeSince.Hours & "h "
                                    If lTimeSince.TotalMinutes > 1 Then lTimeSinceString &= lTimeSince.Minutes & "m "
                                    lLabelText = lTimeSinceString
                                End If

                        End Select
                        If lLabelText IsNot Nothing AndAlso lLabelText.Length > 0 Then
                            g.DrawString(lLabelText, FontLabel, BrushLabel, lX, lY)
                        End If
                    Catch
                    End Try
                End If
                Return True
            Else
                Return False
            End If
        End With
    End Function

    ''' <summary>
    ''' Draw a GPX track point or the GPS cursor, return True if it was drawn
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="aWaypoint"></param>
    ''' <param name="aTopLeftTile"></param>
    ''' <param name="aOffsetToCenter"></param>
    ''' <param name="aFromX">X coordinate of point to draw line from, -1 to skip drawing from</param>
    ''' <param name="aFromY">Y coordinate of point to draw line from, -1 to skip drawing from</param>
    ''' <returns>True if waypoint was drawn, False if it was outside view</returns>
    ''' <remarks></remarks>
    Public Function DrawTrackpoint(ByVal g As Graphics, _
                                    ByVal aWaypoint As clsGPXwaypoint, _
                                    ByVal aTopLeftTile As Point, _
                                    ByVal aOffsetToCenter As Point, _
                                    ByRef aFromX As Integer, _
                                    ByRef aFromY As Integer) As Boolean
        With aWaypoint
            If MapForm.LatLonInView(.lat, .lon) Then
                Dim lTileXY As Point 'Which tile this point belongs in
                Dim lTileOffset As Point 'Offset within lTileXY in pixels
                lTileXY = CalcTileXY(.lat, .lon, MapForm.Zoom, lTileOffset)
                Dim lX As Integer = (lTileXY.X - aTopLeftTile.X) * g_TileSize + aOffsetToCenter.X + lTileOffset.X
                Dim lY As Integer = (lTileXY.Y - aTopLeftTile.Y) * g_TileSize + aOffsetToCenter.Y + lTileOffset.Y

                If aFromX <> -1 OrElse aFromY <> -1 Then
                    g.DrawLine(PenTrack, aFromX, aFromY, lX, lY)
                    If ArrowSize > 0 Then pDistSinceArrow += Math.Sqrt((lX - aFromX) ^ 2 + (lY - aFromY) ^ 2)
                End If

                Select Case .sym
                    Case Nothing
                        If ArrowSize > 0 AndAlso pDistSinceArrow >= ArrowSize AndAlso .courseSpecified Then 'Draw arrow
                            DrawArrow(g, PenTrack, lX, lY, DegToRad(.course), ArrowSize)
                            pDistSinceArrow = 0
                        End If
                    Case "cursor"
                        If SymbolSize > 0 AndAlso SymbolPen IsNot Nothing Then
                            If .courseSpecified Then 'Draw arrow
                                DrawArrow(g, SymbolPen, lX, lY, DegToRad(.course), SymbolSize)
                            Else 'Draw an X
                                g.DrawLine(SymbolPen, lX - SymbolSize, lY - SymbolSize, lX + SymbolSize, lY + SymbolSize)
                                g.DrawLine(SymbolPen, lX - SymbolSize, lY + SymbolSize, lX + SymbolSize, lY - SymbolSize)
                            End If
                        End If
                    Case "circle"
                        If SymbolSize > 0 Then
                            g.DrawEllipse(SymbolPen, lX - SymbolSize, lY - SymbolSize, SymbolSize * 2, SymbolSize * 2)
                        End If
                End Select
                aFromX = lX
                aFromY = lY
                Return True
            Else
                Return False
            End If
        End With
    End Function

#If Not Smartphone Then

    ''' <summary>
    ''' Optimized version using GraphicsPath not available on Smartphone
    ''' </summary>
    ''' <param name="aWaypoint"></param>
    ''' <param name="aTopLeftTile"></param>
    ''' <param name="aOffsetToCenter"></param>
    ''' <param name="aSymbolPath"></param>
    ''' <param name="aTrackPath"></param>
    ''' <param name="aFromX">X coordinate of point to draw line from, -1 to skip drawing from</param>
    ''' <param name="aFromY">Y coordinate of point to draw line from, -1 to skip drawing from</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function DrawTrackpoint(ByVal aWaypoint As clsGPXwaypoint, _
                                ByVal aTopLeftTile As Point, _
                                ByVal aOffsetToCenter As Point, _
                                ByVal aSymbolPath As Drawing2D.GraphicsPath, _
                                ByVal aTrackPath As Drawing2D.GraphicsPath, _
                                ByRef aFromX As Integer, _
                                ByRef aFromY As Integer) As Boolean
        With aWaypoint
            If MapForm.LatLonInView(.lat, .lon) Then
                Dim lTileXY As Point 'Which tile this point belongs in
                Dim lTileOffset As Point 'Offset within lTileXY in pixels
                lTileXY = CalcTileXY(.lat, .lon, MapForm.Zoom, lTileOffset)
                Dim lX As Integer = (lTileXY.X - aTopLeftTile.X) * g_TileSize + aOffsetToCenter.X + lTileOffset.X
                Dim lY As Integer = (lTileXY.Y - aTopLeftTile.Y) * g_TileSize + aOffsetToCenter.Y + lTileOffset.Y

                If aFromX <> -1 OrElse aFromY <> -1 Then
                    aTrackPath.AddLine(aFromX, aFromY, lX, lY)
                    If ArrowSize > 0 Then pDistSinceArrow += Math.Sqrt((lX - aFromX) ^ 2 + (lY - aFromY) ^ 2)
                End If

                Select Case .sym
                    'Case Nothing
                    '    If ArrowSize > 0 AndAlso pDistSinceArrow >= ArrowSize AndAlso .courseSpecified Then 'Draw arrow
                    '        DrawArrow(g, PenTrack, lX, lY, DegToRad(.course), ArrowSize)
                    '        pDistSinceArrow = 0
                    '    End If
                    Case "cursor"
                        If SymbolSize > 0 Then
                            'If .courseSpecified Then 'Draw arrow
                            '    DrawArrow(g, SymbolPen, lX, lY, DegToRad(.course), SymbolSize)
                            'Else 'Draw an X
                            aSymbolPath.AddLine(lX - SymbolSize, lY - SymbolSize, lX + SymbolSize, lY + SymbolSize)
                            aSymbolPath.AddLine(lX - SymbolSize, lY + SymbolSize, lX + SymbolSize, lY - SymbolSize)
                            'End If
                        End If
                    Case "circle"
                        If SymbolSize > 0 Then
                            aSymbolPath.AddEllipse(lX - SymbolSize, lY - SymbolSize, SymbolSize * 2, SymbolSize * 2)
                        End If
                End Select
                aFromX = lX
                aFromY = lY
                Return True
            Else
                Return False
            End If
        End With
    End Function
#End If

End Class
