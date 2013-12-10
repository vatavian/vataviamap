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
        Return GPX.Fields
    End Function

    Public Overrides Sub Render(ByVal aTileServer As clsServer, _
                                ByVal g As Graphics, _
                                ByVal aTopLeftTile As Point, _
                                ByVal aOffsetToCenter As Point)
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
                    DrawWaypoint(aTileServer, g, lWaypoint, aTopLeftTile, aOffsetToCenter)
                    If Date.Now.Subtract(lStartTime).TotalSeconds > 2 Then Exit For
                Next

                If GPX.trk.Count = 1 AndAlso (GPX.trk(0).trkseg.Count = 1) AndAlso (GPX.trk(0).trkseg(0).trkpt.Count = 1) Then
                    'Draw a waypoint when we just have a 1-point track
                    DrawWaypoint(aTileServer, g, GPX.trk(0).trkseg(0).trkpt(0), aTopLeftTile, aOffsetToCenter)
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
                                If Not lTrackPoint.timeSpecified OrElse _
                                   (lTrackPoint.time >= OmitBefore AndAlso lTrackPoint.time <= OmitAfter) Then
#If Not Smartphone Then
                                    If Not DrawTrackpoint(aTileServer, g, lTrackPoint, aTopLeftTile, aOffsetToCenter, lSymbolPath, lTrackPath, lLastX, lLastY) Then
                                        If lTrackPath.PointCount > 0 Then 'Draw the path we already had accumulated for this track
                                            g.DrawPath(PenTrack, lTrackPath)
                                            lTrackPath.Reset()
                                        End If
#Else
                                    If Not DrawTrackpoint(aTileServer, g, lTrackPoint, aTopLeftTile, aOffsetToCenter, lLastX, lLastY) Then
#End If
                                        lLastX = -1
                                        lLastY = -1
                                    End If
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

                    For Each lTrack As clsGPXroute In GPX.rte
                        Dim lLastX As Integer = -1
                        Dim lLastY As Integer = -1
                        For Each lTrackPoint As clsGPXwaypoint In lTrack.rtept
                            If Not lTrackPoint.timeSpecified OrElse _
                               (lTrackPoint.time >= OmitBefore AndAlso lTrackPoint.time <= OmitAfter) Then
#If Not Smartphone Then
                                If Not DrawTrackpoint(aTileServer, g, lTrackPoint, aTopLeftTile, aOffsetToCenter, lSymbolPath, lTrackPath, lLastX, lLastY) Then
                                    If lTrackPath.PointCount > 0 Then 'Draw the path we already had accumulated for this track
                                        g.DrawPath(PenTrack, lTrackPath)
                                        lTrackPath.Reset()
                                    End If
#Else
                                    If Not DrawTrackpoint(aTileServer, g, lTrackPoint, aTopLeftTile, aOffsetToCenter, lLastX, lLastY) Then
#End If
                                    lLastX = -1
                                    lLastY = -1
                                End If
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
    Private Function DrawWaypoint(ByVal aTileServer As clsServer, _
                                  ByVal g As Graphics, _
                                  ByVal aWaypoint As clsGPXwaypoint, _
                                  ByVal aTopLeftTile As Point, _
                                  ByVal aOffsetToCenter As Point) As Boolean
        With aWaypoint
            'If MapForm.LatLonInView(.lat, .lon) Then
            If aWaypoint.GetExtension("hide").ToLower <> "true" Then
                Dim lTileXY As Point 'Which tile this point belongs in
                Dim lTileOffset As Point 'Offset within lTileXY in pixels
                lTileXY = CalcTileXY(aTileServer, .lat, .lon, Map.Zoom, lTileOffset)
                Dim lX As Integer = (lTileXY.X - aTopLeftTile.X) * aTileServer.TileSize + aOffsetToCenter.X + lTileOffset.X
                Dim lY As Integer = (lTileXY.Y - aTopLeftTile.Y) * aTileServer.TileSize + aOffsetToCenter.Y + lTileOffset.Y
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
                                    Dim lThumbnail As New Drawing.Bitmap(lBitmap, lNewWidth, lNewHeight)
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
                    If Map.Zoom > 16 Then  'Show more exact point at high zoom
                        g.DrawLine(PenGeocache, lX, lY - 8, lX, lY + 8)
                        g.DrawLine(PenGeocache, lX - 8, lY, lX + 8, lY)
                    End If
                    'Put label below bitmap
                    lX -= lBitmap.Width / 2
                    lY += lBitmap.Height / 2
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
                Case Else
                    lLabelText = aWaypoint.GetExtension(LabelField)
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
    Public Function DrawTrackpoint(ByVal aTileServer As clsServer, _
                                   ByVal g As Graphics, _
                                   ByVal aWaypoint As clsGPXwaypoint, _
                                   ByVal aTopLeftTile As Point, _
                                   ByVal aOffsetToCenter As Point, _
                                   ByRef aFromX As Integer, _
                                   ByRef aFromY As Integer) As Boolean
        With aWaypoint
            If Map.LatLonInView(.lat, .lon) Then
                Dim lTileXY As Point 'Which tile this point belongs in
                Dim lTileOffset As Point 'Offset within lTileXY in pixels
                lTileXY = CalcTileXY(aTileServer, .lat, .lon, Map.Zoom, lTileOffset)
                Dim lX As Integer = (lTileXY.X - aTopLeftTile.X) * aTileServer.TileSize + aOffsetToCenter.X + lTileOffset.X
                Dim lY As Integer = (lTileXY.Y - aTopLeftTile.Y) * aTileServer.TileSize + aOffsetToCenter.Y + lTileOffset.Y

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
    Private Function DrawTrackpoint(ByVal aTileServer As clsServer, _
                                    ByVal g As Graphics, _
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
                lTileXY = CalcTileXY(aTileServer, .lat, .lon, Map.Zoom, lTileOffset)
                Dim lX As Integer = (lTileXY.X - aTopLeftTile.X) * aTileServer.TileSize + aOffsetToCenter.X + lTileOffset.X
                Dim lY As Integer = (lTileXY.Y - aTopLeftTile.Y) * aTileServer.TileSize + aOffsetToCenter.Y + lTileOffset.Y

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

