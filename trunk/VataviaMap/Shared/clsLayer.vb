Public Class clsLayer
    Public Filename As String
    Public Visible As Boolean = True
    Public MapForm As frmMap
    Private pBounds As clsGPXbounds

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

    Public Overridable Sub Render(ByVal g As Graphics, ByVal aTopLeftTile As Point, ByVal aOffsetToCenter As Point)
    End Sub
End Class

'Public Class clsLayerBuddy
'    Inherits clsLayer

'    Dim lWaypoint As clsGPXwaypoint

'    Public Sub New(ByVal aFilename As String, ByVal aMapForm As frmMap)
'        MyBase.New(aFilename, aMapForm)
'        If IO.File.Exists(aFilename) Then
'            lWaypoint = New clsGPXwaypoint(

'        End If
'    End Sub

'    Public Overrides Sub Render(ByVal g As Graphics, ByVal aTopLeftTile As Point, ByVal aOffsetToCenter As Point)
'        If Me.Visible Then
'            For Each lWaypoint As clsGPXwaypoint In GPX.wpt
'                DrawWaypoint(g, lWaypoint, aTopLeftTile, aOffsetToCenter)
'                If Date.Now.Subtract(lStartTime).TotalSeconds > 2 Then Exit For
'            Next

'            If GPX.trk.Count = 1 AndAlso (GPX.trk(0).trkseg.Count = 1) AndAlso (GPX.trk(0).trkseg(0).trkpt.Count = 1) Then
'                DrawWaypoint(g, GPX.trk(0).trkseg(0).trkpt(0), aTopLeftTile, aOffsetToCenter)
'            Else
'                lStartTime = Date.Now
'                For Each lTrack As clsGPXtrack In GPX.trk
'                    For Each lTrackSegment As clsGPXtracksegment In lTrack.trkseg
'                        Dim lLastX As Integer = -1
'                        Dim lLastY As Integer = -1
'                        For Each lTrackPoint As clsGPXwaypoint In lTrackSegment.trkpt
'                            If Not DrawTrackpoint(g, lTrackPoint, aTopLeftTile, aOffsetToCenter, lLastX, lLastY) Then
'                                'unset last point if it was not drawn to avoid incorrectly drawing line directly between non-adjacent points
'                                'todo: figure out how to draw segment from last point if last point was outside view
'                                lLastX = -1
'                                lLastY = -1
'                            End If
'                        Next
'                    Next
'                    If Date.Now.Subtract(lStartTime).TotalSeconds > 2 Then Exit For
'                Next
'            End If
'        End If
'        End If
'    End Sub

'End Class

Public Class clsLayerGPX
    Inherits clsLayer

    Public GPX As clsGPX

    Public PenTrack As New Pen(Color.FromArgb(-2147432248), 4) '128, 0, 200, 200))
    Public PenGeocache As New Pen(Color.Green)
    Public PenWaypoint As New Pen(Color.Navy)

    Public LabelField As String
    Public BrushLabel As New SolidBrush(Color.Black)
    Public FontLabel As New Font("Arial", 10, FontStyle.Regular)

    Public SymbolSize As Integer
    Public SymbolPen As Drawing.Pen = PenTrack

    Public Sub New(ByVal aFilename As String, ByVal aMapForm As frmMap)
        MyBase.New(aFilename, aMapForm)
        GPX = New clsGPX
        If IO.File.Exists(aFilename) Then
            GPX.LoadFile(aFilename)
        End If
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
                Dim lStartTime As Date = Date.Now
                For Each lWaypoint As clsGPXwaypoint In GPX.wpt
                    DrawWaypoint(g, lWaypoint, aTopLeftTile, aOffsetToCenter)
                    If Date.Now.Subtract(lStartTime).TotalSeconds > 2 Then Exit For
                Next

                If GPX.trk.Count = 1 AndAlso (GPX.trk(0).trkseg.Count = 1) AndAlso (GPX.trk(0).trkseg(0).trkpt.Count = 1) Then
                    DrawWaypoint(g, GPX.trk(0).trkseg(0).trkpt(0), aTopLeftTile, aOffsetToCenter)
                Else
                    lStartTime = Date.Now
                    For Each lTrack As clsGPXtrack In GPX.trk
                        For Each lTrackSegment As clsGPXtracksegment In lTrack.trkseg
                            Dim lLastX As Integer = -1
                            Dim lLastY As Integer = -1
                            For Each lTrackPoint As clsGPXwaypoint In lTrackSegment.trkpt
                                If Not DrawTrackpoint(g, lTrackPoint, aTopLeftTile, aOffsetToCenter, lLastX, lLastY) Then
                                    'unset last point if it was not drawn to avoid incorrectly drawing line directly between non-adjacent points
                                    'todo: figure out how to draw segment from last point if last point was outside view
                                    lLastX = -1
                                    lLastY = -1
                                End If
                            Next
                        Next
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
    Private Function DrawWaypoint(ByVal g As Graphics, _
                                  ByVal aWaypoint As clsGPXwaypoint, _
                                  ByVal aTopLeftTile As Point, _
                                  ByVal aOffsetToCenter As Point) As Boolean
        With aWaypoint
            If MapForm.LatLonInView(.lat, .lon) Then
                Dim lTileXY As Point 'Which tile this point belongs in
                Dim lTileOffset As Point 'Offset within lTileXY in pixels
                lTileXY = CalcTileXY(.lat, .lon, MapForm.Zoom, lTileOffset)
                Dim lX As Integer = (lTileXY.X - aTopLeftTile.X) * g_TileSize + aOffsetToCenter.X + lTileOffset.X
                Dim lY As Integer = (lTileXY.Y - aTopLeftTile.Y) * g_TileSize + aOffsetToCenter.Y + lTileOffset.Y

                Select Case .sym
                    Case Nothing, "" 'Small X for no symbol
                        g.DrawLine(PenTrack, lX - 3, lY - 3, lX + 3, lY + 3)
                        g.DrawLine(PenTrack, lX - 3, lY + 3, lX + 3, lY - 3)
                    Case "Geocache"
                        If MapForm.Zoom > 10 AndAlso .type IsNot Nothing AndAlso g_WaypointIcons.ContainsKey(.type) Then
                            Dim lBitmap As Drawing.Bitmap = g_WaypointIcons.Item(.type)
                            Dim lRectDest As New Rectangle(lX - lBitmap.Width / 2, lY - lBitmap.Height / 2, lBitmap.Width, lBitmap.Height)
                            Dim lRectSource As New Rectangle(0, 0, lBitmap.Width, lBitmap.Height)
                            Dim lImageAttributes As New Drawing.Imaging.ImageAttributes
                            Dim color As Color = lBitmap.GetPixel(0, 0)
                            lImageAttributes.SetColorKey(color, color)
                            g.DrawImage(lBitmap, lRectDest, 0, 0, lBitmap.Width, lBitmap.Height, GraphicsUnit.Pixel, lImageAttributes)
                            If MapForm.Zoom = g_ZoomMax Then GoTo DrawCrosshairs 'Show more exact point at max zoom
                        Else
DrawCrosshairs:
                            g.DrawLine(PenGeocache, lX, lY - 8, lX, lY + 8)
                            g.DrawLine(PenGeocache, lX - 8, lY, lX + 8, lY)
                        End If
                    Case "Waypoint"
                        g.DrawLine(PenWaypoint, lX - 8, lY - 8, lX + 8, lY + 8)
                        g.DrawLine(PenWaypoint, lX - 8, lY + 8, lX + 8, lY - 8)
                    Case Else
                        g.DrawEllipse(PenTrack, lX - 5, lY - 5, 10, 10)
                End Select
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
                        End Select
                        If Not lLabelText Is Nothing AndAlso lLabelText.Length > 0 Then
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
    ''' <param name="aFromX"></param>
    ''' <param name="aFromY"></param>
    ''' <returns>True if waypoint was drawn, False if it was outside view</returns>
    ''' <remarks></remarks>
    Public Function DrawTrackpoint(ByVal g As Graphics, _
                                    ByVal aWaypoint As clsGPXwaypoint, _
                                    ByVal aTopLeftTile As Point, _
                                    ByVal aOffsetToCenter As Point, _
                           Optional ByRef aFromX As Integer = -1, _
                           Optional ByRef aFromY As Integer = -1) As Boolean
        With aWaypoint
            If MapForm.LatLonInView(.lat, .lon) Then
                Dim lTileXY As Point 'Which tile this point belongs in
                Dim lTileOffset As Point 'Offset within lTileXY in pixels
                lTileXY = CalcTileXY(.lat, .lon, MapForm.Zoom, lTileOffset)
                Dim lX As Integer = (lTileXY.X - aTopLeftTile.X) * g_TileSize + aOffsetToCenter.X + lTileOffset.X
                Dim lY As Integer = (lTileXY.Y - aTopLeftTile.Y) * g_TileSize + aOffsetToCenter.Y + lTileOffset.Y

                If aFromX <> -1 OrElse aFromY <> -1 Then
                    g.DrawLine(PenTrack, aFromX, aFromY, lX, lY)
                End If
                Select Case .sym
                    Case "cursor"
                        If SymbolSize > 0 AndAlso SymbolPen IsNot Nothing Then
                            'If .course > 0 Then 'Draw arrow
                            '    DrawArrow(g, lX, lY, DegToRad(.course), lRadius)
                            'Else 'Draw an X
                            g.DrawLine(SymbolPen, lX - SymbolSize, lY - SymbolSize, lX + SymbolSize, lY + SymbolSize)
                            g.DrawLine(SymbolPen, lX - SymbolSize, lY + SymbolSize, lX + SymbolSize, lY - SymbolSize)
                            'End If
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

End Class
