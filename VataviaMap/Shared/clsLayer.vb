Public Class clsLayer
    Public Filename As String
    Public Visible As Boolean = True
    Public Map As ctlMap 'frmMap 'used for clipping to current view

    Public LabelField As String
    Public LabelMinZoom As Integer = 13
    Public BrushLabel As SolidBrush
    Public FontLabel As Font

    Protected pGroupField As String

    Protected pBounds As clsGPXbounds
    Protected pLegendColor As Color = Drawing.Color.HotPink

    Public Sub New(ByVal aFilename As String, ByVal aMap As ctlMap)
        Filename = aFilename
        Map = aMap
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

    Public Overridable Property GroupField() As String
        Get
            Return pGroupField
        End Get
        Set(ByVal value As String)
            pGroupField = value
        End Set
    End Property

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

    Public Overridable Function Fields() As String()
        Dim lFields() As String = {}
        Return lFields
    End Function
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
        If lFileContents.IndexOf("<UserInfo>") > -1 Then
            Return LoadNavizonXML(lFileContents)
        ElseIf lFileContents.IndexOf("navizon.com") > -1 Then
            Return LoadNavizonGears(lFileContents) 'try to find location even though user chose wrong link
        ElseIf lFileContents.IndexOf("<?xml") = 0 Then
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

    Public Function LoadNavizonXML(ByVal aFileContents As String) As Boolean
        Dim lLongitude As Double, lLatitude As Double
        Try
            lLongitude = Double.Parse(GetXmlTagContents(aFileContents, "Longitude"))
            lLatitude = Double.Parse(GetXmlTagContents(aFileContents, "Latitude"))
            Waypoint = New clsGPXwaypoint("wpt", lLatitude, lLongitude)
            With Waypoint
                .name = GetXmlTagContents(aFileContents, "UserName")
                .desc = GetXmlTagContents(aFileContents, "Timestamp")
                .sym = Name            
                Return True
            End With
        Catch ex As Exception
            Dbg(ex.Message)
        End Try
        Return False
    End Function

    Public Function LoadNavizonGears(ByVal aFileContents As String) As Boolean
        Dim lLongitude As Double, lLatitude As Double
        Try
            Dim lBeforeLat As Integer = aFileContents.IndexOf(").offsetHeight},")
            If lBeforeLat > 0 Then
                lBeforeLat += 16
                Dim lLatLonComma As Integer = aFileContents.IndexOf(",", lBeforeLat)
                If lLatLonComma > 0 Then
                    Dim lAfterLonComma As Integer = aFileContents.IndexOf(",", lLatLonComma + 1)
                    lLatitude = Double.Parse(aFileContents.Substring(lBeforeLat, lLatLonComma - lBeforeLat))
                    lLongitude = Double.Parse(aFileContents.Substring(lLatLonComma + 1, lAfterLonComma - lLatLonComma - 1))
                    Waypoint = New clsGPXwaypoint("wpt", lLatitude, lLongitude)
                    Return True                    
                End If
            End If

        Catch ex As Exception
            Dbg(ex.Message)
        End Try
        Return False
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
                Dbg(ex.Message)
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
                Dbg(ex.Message)
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
            Dbg(ex.Message)
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
            Dbg(ex.Message)
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

    Public SymbolSize As Integer
    Public SymbolPen As Pen

    Public ArrowSize As Integer
    Private pDistSinceArrow As Integer

    Private pLastLabelX As Integer = -100
    Private pLastLabelY As Integer = -100
    Private pLastLabelWidth As Integer = 0

    Private pFields() As String = {"name", _
                                   "urlname", _
                                   "desc", _
                                   "container", _
                                   "difficulty", _
                                   "terrain", _
                                   "encoded_hints", _
                                   "hints", _
                                   "time", _
                                   "age"}

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

    Public Sub New(ByVal aFilename As String, ByVal aMap As ctlMap)
        MyBase.New(aFilename, aMap)
        SetDefaults()
        GPX = New clsGPX
        If IO.File.Exists(aFilename) Then
            GPX.LoadFile(aFilename)
        End If
    End Sub

    Public Sub New(ByVal aWaypoints As Generic.List(Of clsGPXwaypoint), ByVal aMap As ctlMap)
        MyBase.New("", aMap)
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

    Public Overrides Function Fields() As String()
        Return pFields
    End Function

    Public Overrides Sub Render(ByVal g As Graphics, ByVal aTopLeftTile As Point, ByVal aOffsetToCenter As Point)
        If Me.Visible Then
            Dim lDrawThisOne As Boolean = True
            If GPX.bounds IsNot Nothing Then
                With GPX.bounds 'Skip drawing this one if it is not in view
                    If .minlat > Map.LatMax OrElse _
                        .maxlat < Map.LatMin OrElse _
                        .minlon > Map.LonMax OrElse _
                        .maxlon < Map.LonMin Then
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
                                If Not DrawTrackpoint(g, lTrackPoint, aTopLeftTile, aOffsetToCenter, lSymbolPath, lTrackPath, lLastX, lLastY) Then
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
                lTileXY = CalcTileXY(.lat, .lon, Map.Zoom, lTileOffset)
                Dim lX As Integer = (lTileXY.X - aTopLeftTile.X) * g_TileSize + aOffsetToCenter.X + lTileOffset.X
                Dim lY As Integer = (lTileXY.Y - aTopLeftTile.Y) * g_TileSize + aOffsetToCenter.Y + lTileOffset.Y
                Dim lBitmap As Drawing.Bitmap = Nothing

                Select Case .sym
                    Case Nothing, "" 'Small X for no symbol
                        g.DrawLine(PenWaypoint, lX - 3, lY - 3, lX + 3, lY + 3)
                        g.DrawLine(PenWaypoint, lX - 3, lY + 3, lX + 3, lY - 3)
                    Case "Geocache"
                        If Map.Zoom > 10 AndAlso .type IsNot Nothing AndAlso g_WaypointIcons.ContainsKey(.type.ToLower) Then
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
                            Try
                                lBitmap = New Drawing.Bitmap(.sym)
                                If lBitmap.Width > g_IconMaxSize OrElse lBitmap.Height > g_IconMaxSize Then
#If Smartphone Then
                                    lBitmap.Dispose()
                                    lBitmap = Nothing
#Else
                                    Dim lNewWidth As Integer = g_IconMaxSize
                                    Dim lNewHeight As Integer = g_IconMaxSize
                                    If lBitmap.Width > lBitmap.Height Then
                                        lNewHeight = lBitmap.Height * lNewWidth / lBitmap.Width
                                    Else
                                        lNewWidth = lBitmap.Width * lNewHeight / lBitmap.Height
                                    End If
                                    Dim lThumbnail as New Drawing.Bitmap(lBitmap, lNewWidth, lNewHeight)
                                    lBitmap.Dispose()
                                    lBitmap = lThumbnail
#End If
                                End If
                            Catch
                            End Try
                            If lBitmap Is Nothing Then 'Draw empty outline of icon if we could not get one the right size
                                Dim lSize As Integer = g_IconMaxSize / 2
                                g.DrawRectangle(PenWaypoint, lX - lSize >> 1, lY - lSize >> 1, lSize, lSize)
                            Else
                                g_WaypointIcons.Add(.sym.ToLower, lBitmap)
                            End If
                        Else
                            g.DrawEllipse(PenWaypoint, lX - 5, lY - 5, 10, 10)
                        End If
                End Select
                If lBitmap IsNot Nothing Then
                    Dim lRectDest As New Rectangle(lX - lBitmap.Width / 2, lY - lBitmap.Height / 2, lBitmap.Width, lBitmap.Height)
                    Dim lRectSource As New Rectangle(0, 0, lBitmap.Width, lBitmap.Height)
                    Dim lImageAttributes As New Drawing.Imaging.ImageAttributes
                    Dim color As Color = lBitmap.GetPixel(0, 0)
                    lImageAttributes.SetColorKey(color, color)
                    g.DrawImage(lBitmap, lRectDest, 0, 0, lBitmap.Width, lBitmap.Height, GraphicsUnit.Pixel, lImageAttributes)
                    If Map.Zoom = g_ZoomMax Then  'Show more exact point at max zoom
                        g.DrawLine(PenGeocache, lX, lY - 8, lX, lY + 8)
                        g.DrawLine(PenGeocache, lX - 8, lY, lX + 8, lY)
                    End If
                End If
                DrawLabel(g, aWaypoint, lX, lY)
                Return True
            Else
                Return False
            End If
        End With
    End Function

    Private Sub DrawLabel(ByVal g As Graphics, _
                          ByVal aWaypoint As clsGPXwaypoint, _
                          ByVal aX As Integer, ByVal aY As Integer)
        If Map.Zoom >= LabelMinZoom AndAlso LabelField IsNot Nothing AndAlso LabelField.Length > 0 Then
            Try
                Dim lCharHeight As Integer
#If Smartphone Then
                lCharHeight = 10
#Else
                lCharHeight = FontLabel.Height
#End If
                If Math.Abs(pLastLabelY - aY) > lCharHeight OrElse Math.Abs(pLastLabelX - aX) > pLastLabelWidth Then
                    Dim lLabelText As String = LabelText(aWaypoint)
                    If lLabelText IsNot Nothing AndAlso lLabelText.Length > 0 Then
                        pLastLabelWidth = lLabelText.Length * lCharHeight 'cheaper than calculating width
                        g.DrawString(lLabelText, FontLabel, BrushLabel, aX, aY)
                        pLastLabelX = aX
                        pLastLabelY = aY
                    End If
                End If
            Catch
            End Try
        End If
    End Sub

    Private Function LabelText(ByVal aWaypoint As clsGPXwaypoint) As String
        Dim lLabelText As String = Nothing
        With aWaypoint
            Select Case LabelField
                Case "name" : lLabelText = .name
                Case "urlname" : lLabelText = .urlname
                Case "desc" : lLabelText = .desc
                Case "container" : lLabelText = .cache.container
                Case "difficulty" : lLabelText = .cache.difficulty
                Case "terrain" : lLabelText = .cache.difficulty
                Case "encoded_hints" : lLabelText = .cache.encoded_hints
                Case "hints" : lLabelText = ROT13(.cache.encoded_hints)
                Case "time" : If .timeSpecified Then lLabelText = .time.ToString
                Case "age"
                    If .timeSpecified Then
                        'Dim lTimeSince As TimeSpan = Now.Subtract(Date.Parse(lFields(0) & " " & lFields(1)))
                        Dim lTimeSince As TimeSpan = Now.ToUniversalTime.Subtract(.time)
                        Dim lTimeSinceString As String = ""
                        If lTimeSince.TotalDays >= 365 Then
                            lTimeSinceString = Format(lTimeSince.TotalDays / 365, "0.#") & "yr"
                        ElseIf lTimeSince.TotalDays >= 1 Then
                            lTimeSinceString = lTimeSince.Days & "dy"
                        ElseIf lTimeSince.TotalHours >= 1 Then
                            lTimeSinceString = lTimeSince.Hours & "hr"
                        ElseIf lTimeSince.TotalMinutes >= 1 Then
                            lTimeSinceString = lTimeSince.Minutes & "min"
                        End If
                        lLabelText = lTimeSinceString
                    End If
            End Select
        End With
        Return lLabelText
    End Function

    ''' <summary>
    ''' Draw a GPX track point or the GPS cursor, return True if it was drawn
    ''' </summary>
    ''' <param name="g">Graphics to draw on</param>
    ''' <param name="aWaypoint">Waypoint to draw</param>
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
            If Map.LatLonInView(.lat, .lon) Then
                Dim lTileXY As Point 'Which tile this point belongs in
                Dim lTileOffset As Point 'Offset within lTileXY in pixels
                lTileXY = CalcTileXY(.lat, .lon, Map.Zoom, lTileOffset)
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
                DrawLabel(g, aWaypoint, lX, lY)
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
    ''' <param name="g">Graphics to draw on (needed for DrawString which cannot go into GraphicsPath)</param>
    ''' <param name="aWaypoint">Waypoint to draw</param>
    ''' <param name="aTopLeftTile"></param>
    ''' <param name="aOffsetToCenter"></param>
    ''' <param name="aSymbolPath">GraphicsPath for drawing symbols</param>
    ''' <param name="aTrackPath">GraphicsPath for drawing track</param>
    ''' <param name="aFromX">X coordinate of point to draw line from, -1 to skip drawing from</param>
    ''' <param name="aFromY">Y coordinate of point to draw line from, -1 to skip drawing from</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function DrawTrackpoint(ByVal g As Graphics, _
                                    ByVal aWaypoint As clsGPXwaypoint, _
                                    ByVal aTopLeftTile As Point, _
                                    ByVal aOffsetToCenter As Point, _
                                    ByVal aSymbolPath As Drawing2D.GraphicsPath, _
                                    ByVal aTrackPath As Drawing2D.GraphicsPath, _
                                    ByRef aFromX As Integer, _
                                    ByRef aFromY As Integer) As Boolean
        With aWaypoint
            If Map.LatLonInView(.lat, .lon) Then
                Dim lTileXY As Point 'Which tile this point belongs in
                Dim lTileOffset As Point 'Offset within lTileXY in pixels
                lTileXY = CalcTileXY(.lat, .lon, Map.Zoom, lTileOffset)
                Dim lX As Integer = (lTileXY.X - aTopLeftTile.X) * g_TileSize + aOffsetToCenter.X + lTileOffset.X
                Dim lY As Integer = (lTileXY.Y - aTopLeftTile.Y) * g_TileSize + aOffsetToCenter.Y + lTileOffset.Y

                If aFromX <> -1 OrElse aFromY <> -1 Then
                    aTrackPath.AddLine(aFromX, aFromY, lX, lY)
                    If ArrowSize > 0 Then pDistSinceArrow += Math.Sqrt((lX - aFromX) ^ 2 + (lY - aFromY) ^ 2)
                End If

                Select Case .sym
                    Case Nothing
                        If ArrowSize > 0 AndAlso pDistSinceArrow >= ArrowSize AndAlso .courseSpecified Then 'Draw arrow
                            AddArrow(aSymbolPath, lX, lY, DegToRad(.course), ArrowSize)
                            pDistSinceArrow = 0
                        End If
                    Case "cursor"
                        If SymbolSize > 0 Then
                            If .courseSpecified Then 'Draw arrow
                                AddArrow(aSymbolPath, lX, lY, DegToRad(.course), SymbolSize)
                            Else 'Draw an X
                                aSymbolPath.AddLine(lX - SymbolSize, lY - SymbolSize, lX + SymbolSize, lY + SymbolSize)
                                aSymbolPath.AddLine(lX - SymbolSize, lY + SymbolSize, lX + SymbolSize, lY - SymbolSize)
                            End If
                        End If
                    Case "circle"
                        If SymbolSize > 0 Then
                            aSymbolPath.AddEllipse(lX - SymbolSize, lY - SymbolSize, SymbolSize * 2, SymbolSize * 2)
                        End If
                End Select
                DrawLabel(g, aWaypoint, lX, lY)
                aFromX = lX
                aFromY = lY
                Return True
            Else
                Return False
            End If
        End With
    End Function

    Private Sub AddArrow(ByVal aSymbolPath As Drawing2D.GraphicsPath, ByVal aXcenter As Integer, ByVal aYcenter As Integer, ByVal aRadians As Double, ByVal aRadius As Integer)
        Dim ldx As Integer = Math.Sin(aRadians) * aRadius
        Dim ldy As Integer = Math.Cos(aRadians) * aRadius
        AddArrow(aSymbolPath, aXcenter - ldx, aYcenter + ldy, aXcenter, aYcenter, aRadius)
    End Sub

    Private Sub AddArrow(ByVal aSymbolPath As Drawing2D.GraphicsPath, _
                         ByVal aTailX As Integer, ByVal aTailY As Integer, _
                         ByVal aHeadX As Integer, ByVal aHeadY As Integer, ByVal aHeadLength As Double)
        'main line of arrow is easy
        'g.DrawLine(aPen, aTailX, aTailY, aHeadX, aHeadY)

        Dim aLeaf1X, aLeaf1Y, aLeaf2X, aLeaf2Y As Integer
        ArrowLeaves(aTailX, aTailY, aHeadX, aHeadY, aHeadLength, aLeaf1X, aLeaf1Y, aLeaf2X, aLeaf2Y)
        aSymbolPath.AddLine(aLeaf1X, aLeaf1Y, aHeadX, aHeadY)
        aSymbolPath.AddLine(aLeaf2X, aLeaf2Y, aHeadX, aHeadY)
    End Sub

#End If

End Class
