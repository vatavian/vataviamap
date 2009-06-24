Public Class frmMap

    Private GPS_Listen As Boolean = False
    Private WithEvents GPS As GPS_API.GPS
    Private GPS_DEVICE_STATE As GPS_API.GpsDeviceState
    Private GPS_POSITION As GPS_API.GpsPosition = Nothing

    Private Shared pTrackMutex As New Threading.Mutex()

    ' File name of track log currently being written
    Private pTrackLogFilename As String

    ' When we last uploaded a track point (UTC)
    Private pUploadLastTime As DateTime

    ' True if we want to upload when GPS starts and have not yet done so
    Private pPendingUploadStart As Boolean = False

    ' When we last logged a track point (UTC)
    Private pTrackLastTime As DateTime
    Private pTrackLastLatitude As Double = 0.0  ' Degrees latitude.  North is positive
    Private pTrackLastLongitude As Double = 0.0 ' Degrees longitude.  East is positive

    Private pClickDownload As Boolean = False ' Always false on startup, not saved to registry

    Private pCursorLayer As clsLayerGPX

    WithEvents pDownloadForm As frmDownloadMobile
    WithEvents pUploadForm As frmUploadMobile
    WithEvents pLayersForm As frmOptionsMobileGPX

    Private pKeepAwakeTimer As System.Threading.Timer

    Public Sub New()
        InitializeComponent()

        'Default to using these for mobile even though default to not in frmMapCommon
        pControlsUse = True
        pControlsShow = True

        pTrackLastTime = DateTime.UtcNow.Subtract(pTrackMinInterval)
        pUploadLastTime = New DateTime(1, 1, 1)

        updateDataHandler = New EventHandler(AddressOf UpdateData)

        pTileCacheFolder = GetAppSetting("TileCacheFolder", "\My Documents\tiles\")

        If pTileCacheFolder.Length > 0 AndAlso Not pTileCacheFolder.EndsWith(g_PathChar) Then
            pTileCacheFolder &= g_PathChar
        End If

        pGPXFolder = IO.Path.GetDirectoryName(pTileCacheFolder)
        Try
            IO.Directory.CreateDirectory(pTileCacheFolder)
        Catch e As Exception
            'TODO: open frmDownloadMobile or let user choose this folder somehow rather than just error message
            MsgBox("Could not create cache folder" & vbLf & pTileCacheFolder & vbLf & "Edit registry in CurrentUser\Software\" & g_AppName & "\TileCacheFolder to change", MsgBoxStyle.OkOnly, "TileCacheFolder Needed")
        End Try

        SharedNew()

        pCursorLayer = New clsLayerGPX("cursor", Me)
        pCursorLayer.SymbolPen = New Pen(Color.Red)
        pCursorLayer.SymbolSize = pGPSSymbolSize

        mnuRecordTrack.Checked = pRecordTrack
        mnuViewTrack.Checked = pDisplayTrack
        mnuViewMapTiles.Checked = pShowTileImages
        mnuViewGPSdetails.Checked = pShowGPSdetails
        mnuViewTileOutlines.Checked = pShowTileOutlines
        mnuViewTileNames.Checked = pShowTileNames
        mnuViewTrack.Checked = pDisplayTrack
        mnuViewControls.Checked = pControlsShow
        mnuRefreshOnClick.Checked = pClickDownload
        Select Case pGPSFollow
            Case 1 : mnuFollow.Checked = True
            Case 2 : mnuCenter.Checked = True
        End Select
        mnuAutoStart.Checked = pGPSAutoStart
        If pGPSAutoStart Then StartGPS()
    End Sub

    Private Sub Redraw()
        If pRedrawing Then
            pRedrawPending = True
        Else
            pRedrawing = True
            Dim lGraphics As Graphics = Nothing
            Dim lDetailsBrush As Brush = pBrushBlack

RestartRedraw:
            If pFormVisible Then
                lGraphics = GetBitmapGraphics()
                If lGraphics IsNot Nothing Then
                    If pFormDark Then
                        'pDetailsForm.Details = GPSdetailsString()
                        lGraphics.Clear(Color.Black)
                        lDetailsBrush = pBrushWhite
                    Else
                        DrawTiles(lGraphics)
                    End If
                    If pShowGPSdetails Then
                        lGraphics.DrawString(GPSdetailsString, pFontTileLabel, lDetailsBrush, 5, 5)
                    End If
                    ReleaseBitmapGraphics()
                    Refresh()
                End If
            End If
            Application.DoEvents()
            If pRedrawPending Then
                pRedrawPending = False
                GoTo RestartRedraw
            End If
            pRedrawing = False
        End If
    End Sub

    Private Function EnsureCurrentTrack() As clsLayerGPX
        If pLayers.Count = 0 OrElse pLayers(0).Filename <> "current" Then
            Dim lCurrentTrack As New clsLayerGPX("current", Me)
            lCurrentTrack.Filename = "current"
            With lCurrentTrack.GPX
                .trk = New Generic.List(Of clsGPXtrack)
                .trk.Add(New clsGPXtrack("current"))
                .trk(0).trkseg.Add(New clsGPXtracksegment())
            End With
            pLayers.Insert(0, lCurrentTrack)
        End If
        Return pLayers(0)
    End Function

    Private Function MapRectangle() As Rectangle
        Return ClientRectangle
    End Function

    Private Sub Zoomed()
        Redraw()
    End Sub

    Private Sub DrewTiles(ByVal g As Graphics, ByVal aTopLeft As Point, ByVal aOffsetToCenter As Point)
        If GPS IsNot Nothing AndAlso GPS.Opened AndAlso GPS_POSITION IsNot Nothing AndAlso GPS_POSITION.LatitudeValid AndAlso GPS_POSITION.LongitudeValid Then
            If pGPSSymbolSize > 0 Then
                pCursorLayer.SymbolSize = pGPSSymbolSize
                Dim lWaypoint As New clsGPXwaypoint("wpt", GPS_POSITION.Latitude, GPS_POSITION.Longitude)
                lWaypoint.sym = "cursor"
                lWaypoint.course = GPS_POSITION.Heading
                pCursorLayer.DrawTrackpoint(g, lWaypoint, aTopLeft, aOffsetToCenter, -1, -1)
            End If
        End If
    End Sub

    Private Sub ManuallyNavigated()
        pGPSFollow = 0 'Manual navigation overrides automatic following of GPS
        mnuFollow.Checked = False
        mnuCenter.Checked = False
        SanitizeCenterLatLon()
        Redraw()
    End Sub

    Private Function GPSdetailsString() As String
        Dim lDetails As String = ""
        If GPS Is Nothing Then
            lDetails = "GPS not initialized"
        ElseIf Not GPS.Opened Then
            lDetails = "GPS not opened"
        ElseIf GPS_POSITION Is Nothing Then
            lDetails = "No position"
        ElseIf Not GPS_POSITION.LatitudeValid OrElse Not GPS_POSITION.LongitudeValid Then
            lDetails = "Position not valid"
        Else
            lDetails = "(" & FormattedDegrees(GPS_POSITION.Latitude, g_DegreeFormat) & ", " _
                                         & FormattedDegrees(GPS_POSITION.Longitude, g_DegreeFormat) & ")"
            'If (GPS_POSITION.SeaLevelAltitudeValid) Then lDetails &= " " & GPS_POSITION.SeaLevelAltitude & "m"
            'If (GPS_POSITION.SpeedValid AndAlso GPS_POSITION.Speed > 0) Then lDetails &= " " & GPS_POSITION.Speed & "knots"

            Dim lGPStime As DateTime = GPS_POSITION.Time.ToLocalTime()
            If lGPStime.Hour > 12 Then
                lDetails &= " " & lGPStime.Hour - 12 & ":" & Format(lGPStime.Minute, "00") & "p"
            Else
                lDetails &= " " & lGPStime.Hour & ":" & Format(lGPStime.Minute, "00") & "a"
            End If
            Dim lAgeOfPosition As TimeSpan = DateTime.Now - lGPStime
            If (Math.Abs(lAgeOfPosition.TotalSeconds) > 5) Then
                lDetails &= vbLf + " (" + lAgeOfPosition.ToString().TrimEnd("0"c) + " ago)"
            End If

            If Not pRecordTrack Then lDetails &= vbLf & "Logging Off"
        End If
        If pRecordCellID Then lDetails &= vbLf & GPS_API.RIL.GetCellTowerString
        'If pUsedCacheCount + pAddedCacheCount > 0 Then
        '    lDetails &= " Cache " & Format(pUsedCacheCount / (pUsedCacheCount + pAddedCacheCount), "0.0 %") & " " & pDownloader.TileRAMcacheLimit
        'End If
        Return lDetails
    End Function

    Private Sub frmMap_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        StopGPS()
    End Sub

    Private Sub frmMap_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles MyBase.KeyUp
        e.Handled = True
        Dim lNeedRedraw As Boolean = True
        Select Case e.KeyCode
            Case Keys.Right : CenterLon += MetersPerPixel(pZoom) * Me.Width / g_CircumferenceOfEarth * 180
            Case Keys.Left : CenterLon -= MetersPerPixel(pZoom) * Me.Width / g_CircumferenceOfEarth * 180
            Case Keys.Up
                If pLastKeyDown = 131 Then 'using scroll wheel
                    Zoom += 1 : lNeedRedraw = False
                Else
                    CenterLat += MetersPerPixel(pZoom) * Me.Height / g_CircumferenceOfEarth * 90
                End If
            Case Keys.Down
                If pLastKeyDown = 131 Then 'using scroll wheel
                    Zoom -= 1 : lNeedRedraw = False
                Else
                    CenterLat -= MetersPerPixel(pZoom) * Me.Height / g_CircumferenceOfEarth * 90
                End If
            Case Keys.A : Zoom -= 1 : lNeedRedraw = False
            Case Keys.Q : Zoom += 1 : lNeedRedraw = False
            Case Else
                e.Handled = False
                lNeedRedraw = False
        End Select
        If lNeedRedraw Then
            ManuallyNavigated()
        End If
    End Sub

    Private Sub frmMap_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseDown
        Select Case e.Button
            Case Windows.Forms.MouseButtons.Left
                If pClickDownload Then
                    Dim lBounds As New RectangleF(0, 0, pBitmap.Width, pBitmap.Height)
                    pClickedTileFilename = FindTileFilename(lBounds, e.X, e.Y)
                    pDownloader.DeleteTile(pClickedTileFilename)
                    Redraw()
                Else
                    MouseDownLeft(e)
                End If
        End Select
    End Sub

    Private Sub frmMap_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseMove
        If pMouseDragging Then
            If Not pControlsUse OrElse _
               (e.X > pControlsMargin AndAlso e.X < (Me.Width - pControlsMargin)) OrElse _
               (e.Y > pControlsMargin AndAlso e.Y < (Me.Height - pControlsMargin)) OrElse _
               Math.Abs(pMouseDragStartLocation.X - e.X) > pControlsMargin / 2 OrElse _
               Math.Abs(pMouseDragStartLocation.Y - e.Y) > pControlsMargin / 2 Then

                CenterLat = pMouseDownLat + (e.Y - pMouseDragStartLocation.Y) * LatHeight / Me.ClientRectangle.Height
                CenterLon = pMouseDownLon - (e.X - pMouseDragStartLocation.X) * LonWidth / Me.ClientRectangle.Width
                ManuallyNavigated()
            End If
        End If
    End Sub

    Private Sub mnuStartStopGPS_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuStartStopGPS.Click
        If (mnuStartStopGPS.Text.StartsWith("Start")) Then
            StartGPS()
        Else
            StopGPS()
        End If
    End Sub

    Private Sub StartGPS()
        mnuStartStopGPS.Text = "Starting GPS..."
        Application.DoEvents()
        If pGPSFollow = 0 Then
            pGPSFollow = 2
            mnuFollow.Checked = False
            mnuCenter.Checked = True
        End If
        If GPS Is Nothing Then GPS = New GPS_API.GPS
        If Not GPS.Opened Then
            SetPowerRequirement(DeviceGPS, True)
            If Not pFormDark Then
                StartKeepAwake()
            End If

            Dim lLogIndex As Integer = 0
            Dim lBaseFilename As String = IO.Path.Combine(pGPXFolder, DateTime.Now.ToString("yyyy-MM-dd_HH-mm"))

            pTrackMutex.WaitOne()
            pTrackLogFilename = lBaseFilename & ".gpx"
            While System.IO.File.Exists(pTrackLogFilename)
                pTrackLogFilename = lBaseFilename & "_" & ++lLogIndex & ".gpx"
            End While
            pTrackMutex.ReleaseMutex()

            mnuStartStopGPS.Text = "Opening GPS..."
            Application.DoEvents()
            GPS.Open()
            GPS_Listen = True
            pPendingUploadStart = pUploadOnStart
        End If
    End Sub

    Private Sub StopGPS()
        GPS_Listen = False
        Try
            mnuStartStopGPS.Text = "Stopping"
            Application.DoEvents()
        Catch
        End Try
        Threading.Thread.Sleep(100)
        Me.SaveSettings() 'In case of crash, at least we can start again with the same settings

        pTrackMutex.WaitOne()
        If System.IO.File.Exists(pTrackLogFilename) Then
            Try
                mnuStartStopGPS.Text = "Finishing Log..."
                Application.DoEvents()
            Catch
            End Try

            Dim lFile As System.IO.StreamWriter = System.IO.File.AppendText(pTrackLogFilename)
            lFile.Write("</trkseg>" & vbLf & "</trk>" & vbLf & "</gpx>" & vbLf)
            lFile.Close()
            pTrackLogFilename = ""
        End If
        pTrackMutex.ReleaseMutex()

        If GPS IsNot Nothing Then
            If GPS.Opened Then
                Try
                    mnuStartStopGPS.Text = "Closing GPS..."
                    Application.DoEvents()
                Catch
                End Try
                If pUploadOnStop Then UploadGpsPosition()
                GPS.Close()
            End If
            GPS = Nothing
        End If
        SetPowerRequirement(DeviceGPS, False)
        StopKeepAwake()
        Try
            mnuStartStopGPS.Text = "Start GPS"
            Application.DoEvents()
        Catch
        End Try
    End Sub

    Private updateDataHandler As EventHandler

    Private Sub GPS_DEVICE_LocationChanged(ByVal sender As Object, ByVal args As GPS_API.LocationChangedEventArgs) Handles GPS.LocationChanged
        If GPS_Listen Then
            GPS_POSITION = args.Position
            Invoke(updateDataHandler)
        End If
    End Sub

    Private Sub UpdateData(ByVal sender As Object, ByVal args As System.EventArgs)
        Try
            If (GPS.Opened) Then
                Try
                    mnuStartStopGPS.Text = "Stop GPS " & GPS_POSITION.SatelliteCount & "/" & GPS_POSITION.SatellitesInViewCount
                    Application.DoEvents()
                Catch
                End Try
                If (GPS_POSITION IsNot Nothing AndAlso GPS_POSITION.LatitudeValid AndAlso GPS_POSITION.LongitudeValid) Then
                    Dim lNeedRedraw As Boolean = False
                    Select Case pGPSFollow
                        Case 1
                            If Not LatLonInView(GPS_POSITION.Latitude, GPS_POSITION.Longitude) Then
                                GoTo SetCenter
                            End If
                        Case 2
SetCenter:
                            If CenterLat <> GPS_POSITION.Latitude OrElse CenterLon <> GPS_POSITION.Longitude Then
                                CenterLat = GPS_POSITION.Latitude
                                CenterLon = GPS_POSITION.Longitude
                                lNeedRedraw = True
                            End If
                    End Select

                    If GPS_Listen AndAlso (pRecordTrack OrElse pDisplayTrack) AndAlso DateTime.UtcNow.Subtract(pTrackMinInterval) >= pTrackLastTime Then
                        TrackAddPoint()
                        pTrackLastTime = DateTime.UtcNow
                    End If

                    If pGPSSymbolSize > 0 OrElse lNeedRedraw Then
                        Redraw()
                    End If

                    If pPendingUploadStart OrElse (pUploadPeriodic AndAlso DateTime.UtcNow.Subtract(pUploadMinInterval) >= pUploadLastTime) Then
                        UploadGpsPosition()
                    End If

                    'If (GPS_POSITION.SeaLevelAltitudeValid) Then
                    ' str &= "Elev " & GPS_POSITION.SeaLevelAltitude & "m"
                End If
            End If
        Catch e As Exception
            'MsgBox(e.Message & vbLf & e.StackTrace)
        End Try
    End Sub

    Private Sub TrackAddPoint()
        If GPS_POSITION IsNot Nothing _
           AndAlso GPS_POSITION.TimeValid _
           AndAlso GPS_POSITION.LatitudeValid _
           AndAlso GPS_POSITION.LongitudeValid _
           AndAlso GPS_POSITION.SatellitesInSolutionValid _
           AndAlso GPS_POSITION.SatelliteCount > 2 Then
            ' If subsequent track points vary this much, we don't believe it, don't log till they are closer
            If Math.Abs((GPS_POSITION.Latitude - pTrackLastLatitude)) + Math.Abs((GPS_POSITION.Longitude - pTrackLastLongitude)) < 0.5 Then
                Dim lGPXheader As String = Nothing

                Dim lTrackPoint As New clsGPXwaypoint("trkpt", GPS_POSITION.Latitude, GPS_POSITION.Longitude)
                If GPS_POSITION.SeaLevelAltitudeValid Then
                    lTrackPoint.ele = GPS_POSITION.SeaLevelAltitude
                End If
                lTrackPoint.time = GPS_POSITION.Time
                lTrackPoint.sat = GPS_POSITION.SatelliteCount

                If GPS_POSITION.SpeedValid Then
                    lTrackPoint.speed = GPS_POSITION.Speed
                End If
                If GPS_POSITION.HeadingValid Then
                    lTrackPoint.course = GPS_POSITION.Heading
                End If

                If pRecordCellID Then lTrackPoint.SetExtension("celltower", GPS_API.RIL.GetCellTowerString)
                lTrackPoint.SetExtension("phonesignal", Microsoft.WindowsMobile.Status.SystemState.PhoneSignalStrength)

                If pRecordTrack Then
                    pTrackMutex.WaitOne()
                    Try
                        If System.IO.File.Exists(pTrackLogFilename) Then
                            'System.IO.StreamReader lReader = new System.IO.StreamReader(logFilename);
                            'str = lReader.ReadToEnd();
                            'lReader.Close();
                            'int lEndGPX = str.LastIndexOf("</trkseg>");
                            'if (lEndGPX < 0)
                            '    lEndGPX = str.LastIndexOf("</gpx>");
                            'if (lEndGPX > 0) str = str.Substring(0, lEndGPX - 1);
                            'if (!aNewTrackSeg) aNewTrackSeg = (str.IndexOf("<trkseg>") < 0);
                        Else
                            lGPXheader = "<?xml version=""1.0"" encoding=""UTF-8""?>" & vbLf & "<gpx xmlns=""http://www.topografix.com/GPX/1/1"" version=""1.1"" creator=""" & g_AppName & """ xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd http://www.topografix.com/GPX/gpx_overlay/0/3 http://www.topografix.com/GPX/gpx_overlay/0/3/gpx_overlay.xsd http://www.topografix.com/GPX/gpx_modified/0/1 http://www.topografix.com/GPX/gpx_modified/0/1/gpx_modified.xsd"">" & vbLf & "<trk>" & vbLf & "<name>" & g_AppName & " Log " & System.IO.Path.GetFileNameWithoutExtension(pTrackLogFilename) & " </name>" & vbLf & "<type>GPS Tracklog</type>" & vbLf & "<trkseg>" & vbLf
                            IO.Directory.CreateDirectory(pGPXFolder)
                        End If
                        Dim lFile As System.IO.StreamWriter = System.IO.File.AppendText(pTrackLogFilename)
                        If lGPXheader IsNot Nothing Then
                            lFile.Write(lGPXheader)
                        End If
                        lFile.Write(lTrackPoint.ToString)
                        lFile.Close()
                    Catch exRecordTrack As Exception
                        Windows.Forms.MessageBox.Show("Could not save " & vbLf & pTrackLogFilename & vbLf & exRecordTrack.Message)
                    End Try
                    pTrackMutex.ReleaseMutex()
                End If
                pTrackLastTime = DateTime.UtcNow

                If pDisplayTrack Then
                    EnsureCurrentTrack.GPX.trk(0).trkseg(0).trkpt.Add(lTrackPoint)
                End If
            End If
            pTrackLastLatitude = GPS_POSITION.Latitude
            pTrackLastLongitude = GPS_POSITION.Longitude
        End If
    End Sub 'TrackAddPoint

    Private Sub UploadGpsPosition()
        If pUploader.Enabled _
           AndAlso GPS_POSITION IsNot Nothing _
           AndAlso GPS_POSITION.TimeValid _
           AndAlso GPS_POSITION.LatitudeValid _
           AndAlso GPS_POSITION.LongitudeValid _
           AndAlso GPS_POSITION.SatellitesInSolutionValid _
           AndAlso GPS_POSITION.SatelliteCount > 2 Then
            Try
                Dim lURL As String = g_UploadURL
                If lURL IsNot Nothing AndAlso lURL.Length > 0 Then
                    BuildURL(lURL, "Time", GPS_POSITION.Time.ToString("yyyy-MM-ddTHH:mm:ss.fff") & "Z", "", GPS_POSITION.TimeValid)
                    BuildURL(lURL, "Lat", GPS_POSITION.Latitude.ToString("#.########"), "", GPS_POSITION.LatitudeValid)
                    BuildURL(lURL, "Lon", GPS_POSITION.Longitude.ToString("#.########"), "", GPS_POSITION.LongitudeValid)

                    BuildURL(lURL, "Alt", GPS_POSITION.SeaLevelAltitude, "", GPS_POSITION.SeaLevelAltitudeValid)
                    BuildURL(lURL, "Speed", GPS_POSITION.Speed, "", GPS_POSITION.SpeedValid)
                    BuildURL(lURL, "Heading", GPS_POSITION.Heading, "", GPS_POSITION.HeadingValid)
                    If pPendingUploadStart Then
                        BuildURL(lURL, "Label", g_AppName & "-Start", "", True)
                    Else
                        BuildURL(lURL, "Label", g_AppName, "", True)
                    End If
                    If lURL.IndexOf("CellID") > -1 Then
                        BuildURL(lURL, "CellID", GPS_API.RIL.GetCellTowerString, "", True)
                    End If

                    pUploader.ClearQueue(0)
                    pUploader.Enqueue(lURL, "", QueueItemType.PointItem, 0)
                    pUploadLastTime = DateTime.UtcNow
                    pPendingUploadStart = False
                End If
            Catch
            End Try
        End If
    End Sub 'UploadPoint

    Private Sub BuildURL(ByRef aURL As String, ByVal aTag As String, ByVal aReplaceTrue As String, ByVal aReplaceFalse As String, ByVal aTest As Boolean)
        Dim lReplacement As String
        If aTest Then
            lReplacement = aReplaceTrue
        Else
            lReplacement = aReplaceFalse
        End If
        aURL = aURL.Replace("#" & aTag & "#", lReplacement)
    End Sub

    Private Sub mnuGeocache_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuGeocache.Click
        pFormVisible = False

        If pLayers IsNot Nothing AndAlso pLayers.Count > 0 Then
            Dim lMiddlemostCache As clsGPXwaypoint = Nothing
            Dim lClosestDistance As Double = Double.MaxValue
            Dim lThisDistance As Double
            For Each lLayer As clsLayer In pLayers
                Try
                    Dim lGPXLayer As clsLayerGPX = lLayer
                    Dim lDrawThisOne As Boolean = True
                    If lGPXLayer.GPX.bounds IsNot Nothing Then
                        With lGPXLayer.GPX.bounds 'Skip drawing this one if it is not in view
                            If .minlat > CenterLat + LatHeight / 2 OrElse _
                                .maxlat < CenterLat - LatHeight / 2 OrElse _
                                .minlon > CenterLon + LonWidth / 2 OrElse _
                                .maxlon < CenterLon - LonWidth / 2 Then
                                lDrawThisOne = False
                            End If
                        End With
                    End If
                    If lDrawThisOne Then
                        For Each lWaypoint As clsGPXwaypoint In lGPXLayer.GPX.wpt
                            lThisDistance = (lWaypoint.lat - CenterLat) ^ 2 + (lWaypoint.lon - CenterLon) ^ 2
                            If lThisDistance < lClosestDistance Then
                                lMiddlemostCache = lWaypoint
                                lClosestDistance = lThisDistance
                            End If
                        Next
                    End If
                Catch
                End Try
            Next
            If lMiddlemostCache IsNot Nothing Then
                'Dim lGeocacheForm As New frmGeocacheMobile
                'With lMiddlemostCache
                '    lGeocacheForm.URL = .url
                '    Dim lText As String = ""

                '    lText &= .name & " " _
                '          & FormattedDegrees(.lat, g_DegreeFormat) & ", " _
                '          & FormattedDegrees(.lon, g_DegreeFormat) & vbLf

                '    If .desc IsNot Nothing AndAlso .desc.Length > 0 Then
                '        lText &= .desc & vbLf
                '    ElseIf .urlname IsNot Nothing AndAlso .urlname.Length > 0 Then
                '        lText &= .urlname
                '        If .cache IsNot Nothing Then
                '            lText &= " (" & Format(.cache.difficulty, "#.#") & "/" _
                '                          & Format(.cache.terrain, "#.#") & ")"
                '        End If
                '        lText &= vbLf
                '    End If

                '    If .cmt IsNot Nothing AndAlso .cmt.Length > 0 Then lText &= "comment: " & .cmt & vbLf

                '    If .cache IsNot Nothing AndAlso .cache.container IsNot Nothing AndAlso .cache.container.Length > 0 Then lText &= .cache.container & ", "
                '    If .type IsNot Nothing AndAlso .type.Length > 0 Then
                '        If .type.StartsWith("Geocache|") Then
                '            lText &= "type: " & .type.Substring(9) & vbLf
                '        Else
                '            lText &= "type: " & .type & vbLf
                '        End If
                '    ElseIf .cache IsNot Nothing AndAlso .cache.cachetype IsNot Nothing AndAlso .cache.cachetype.Length > 0 Then
                '        lText &= "type: " & .type & vbLf
                '    End If

                '    If .cache IsNot Nothing Then
                '        If .cache.archived Then lText &= "ARCHIVED" & vbLf

                '        If .cache.short_description IsNot Nothing AndAlso .cache.short_description.Length > 0 Then
                '            lText &= .cache.short_description.Replace("<br>", vbLf) & vbLf
                '        End If
                '        If .cache.long_description IsNot Nothing AndAlso .cache.long_description.Length > 0 Then
                '            lText &= .cache.long_description.Replace("<br>", vbLf) & vbLf
                '        End If

                '        If .cache.encoded_hints IsNot Nothing AndAlso .cache.encoded_hints.Length > 0 Then
                '            lText &= "Hint: " & .cache.encoded_hints & vbLf
                '        End If

                '        If .cache.logs IsNot Nothing AndAlso .cache.logs.Length > 0 Then lText &= "Logs: " & .cache.logs & vbLf
                '        If .cache.placed_by IsNot Nothing AndAlso .cache.placed_by.Length > 0 Then lText &= "Placed by: " & .cache.placed_by & vbLf
                '        If .cache.owner IsNot Nothing AndAlso .cache.owner.Length > 0 Then lText &= "Owner: " & .cache.owner & vbLf
                '        If .cache.travelbugs IsNot Nothing AndAlso .cache.travelbugs.Length > 0 Then lText &= "Travellers: " & .cache.travelbugs & vbLf

                '    End If
                '    If .extensions IsNot Nothing Then lText &= .extensions.OuterXml

                '    lGeocacheForm.txtMain.Text = lText
                'End With
                'lGeocacheForm.Show()

                With lMiddlemostCache
                    Windows.Forms.Clipboard.SetDataObject(.name)
                    Dim lText As String = "<html><head><title>" & .name & "</title></head><body>"

                    lText &= "<b><a href=""" & .url & """>" & .name & "</a></b> " _
                          & FormattedDegrees(.lat, g_DegreeFormat) & ", " _
                          & FormattedDegrees(.lon, g_DegreeFormat) & "<br>"

                    If .desc IsNot Nothing AndAlso .desc.Length > 0 Then
                        lText &= .desc
                    ElseIf .urlname IsNot Nothing AndAlso .urlname.Length > 0 Then
                        lText &= .urlname
                        If .cache IsNot Nothing Then
                            lText &= " (" & Format(.cache.difficulty, "#.#") & "/" _
                                          & Format(.cache.terrain, "#.#") & ")"
                        End If
                    End If

                    Dim lType As String = Nothing
                    If .type IsNot Nothing AndAlso .type.Length > 0 Then
                        lType = .type
                        If lType.StartsWith("Geocache|") Then lType = lType.Substring(9)
                    End If
                    If lType Is Nothing AndAlso .cache IsNot Nothing AndAlso .cache.cachetype IsNot Nothing AndAlso .cache.cachetype.Length > 0 Then
                        lType = .cache.cachetype
                    End If
                    If lType IsNot Nothing AndAlso lText.IndexOf(lType) = -1 Then
                        lText &= " <b>" & .type & "</b>"
                    End If

                    If .cache IsNot Nothing AndAlso .cache.container IsNot Nothing AndAlso .cache.container.Length > 0 Then lText &= " <b>" & .cache.container & "</b>"
                    lText &= "<br>"

                    If .cmt IsNot Nothing AndAlso .cmt.Length > 0 Then lText &= "<b>comment:</b> " & .cmt & "<br>"

                    If .cache IsNot Nothing Then
                        If .cache.archived Then lText &= "<h2>ARCHIVED</h2><br>"

                        If .cache.short_description IsNot Nothing AndAlso .cache.short_description.Length > 0 Then
                            lText &= .cache.short_description & "<br>"
                        End If
                        If .cache.long_description IsNot Nothing AndAlso .cache.long_description.Length > 0 Then
                            lText &= .cache.long_description & "<br>"
                        End If

                        If .cache.encoded_hints IsNot Nothing AndAlso .cache.encoded_hints.Length > 0 Then
                            lText &= "<b>Hint:</b> " & .cache.encoded_hints & "<br>"
                        End If

                        If .cache.logs IsNot Nothing AndAlso .cache.logs.Count > 0 Then
                            'T19:00:00 is the time for all logs? remove it and format the entries a bit
                            Dim lIconPath As String = IO.Path.Combine(pTileCacheFolder, "Icons" & g_PathChar & "Geocache" & g_PathChar)
                            Dim lIconFilename As String
                            lText &= "<b>Logs:</b><br>"
                            For Each lLog As clsGroundspeakLog In .cache.logs
                                Select Case lLog.logtype
                                    Case "Found it" : lIconFilename = lIconPath & "icon_smile.gif"
                                    Case "Didn't find it" : lIconFilename = lIconPath & "icon_sad.gif"
                                    Case "Write note" : lIconFilename = lIconPath & "icon_note.gif"
                                    Case "Enable Listing" : lIconFilename = lIconPath & "icon_enabled.gif"
                                    Case "Temporarily Disable Listing" : lIconFilename = lIconPath & "icon_disabled.gif"
                                    Case "Needs Maintenance" : lIconFilename = lIconPath & "icon_needsmaint.gif"
                                    Case "Owner Maintenance" : lIconFilename = lIconPath & "icon_maint.gif"
                                    Case "Publish Listing" : lIconFilename = lIconPath & "icon_greenlight.gif"
                                    Case "Update Coordinates" : lIconFilename = lIconPath & "coord_update.gif"
                                    Case "Will Attend" : lIconFilename = lIconPath & "icon_rsvp.gif"
                                    Case Else : lIconFilename = ""
                                End Select
                                If IO.File.Exists(lIconFilename) Then
                                    lText &= "<img src=""" & lIconFilename & """>"
                                Else
                                    lText &= lLog.logtype
                                End If
                                lText &= "<b>" & lLog.logdate.Replace("T19:00:00", "") & "</b> " & lLog.logfinder & ": " & lLog.logtext & "<br>"
                            Next
                            'Dim lLastTimePos As Integer = 0
                            'Dim lTimePos As Integer = .cache.logs.IndexOf()
                            'While lTimePos > 0
                            '    lText &= .cache.logs.Substring(lLastTimePos, lTimePos - lLastTimePos - 10) & "<br><b>" & .cache.logs.Substring(lTimePos - 10, 10) & "</b> "
                            '    lLastTimePos = lTimePos + 9
                            '    lTimePos = .cache.logs.IndexOf("T19:00:00", lTimePos + 10)
                            'End While
                            'lText &= .cache.logs.Substring(lLastTimePos + 9) & "<p>"
                        End If
                        If .cache.placed_by IsNot Nothing AndAlso .cache.placed_by.Length > 0 Then lText &= "<b>Placed by:</b> " & .cache.placed_by & "<br>"
                        If .cache.owner IsNot Nothing AndAlso .cache.owner.Length > 0 Then lText &= "<b>Owner:</b> " & .cache.owner & "<br>"
                        If .cache.travelbugs IsNot Nothing AndAlso .cache.travelbugs.Length > 0 Then lText &= "<b>Travellers:</b> " & .cache.travelbugs & "<br>"
                    End If
                    lText &= .extensionsString

                    Dim lStream As IO.StreamWriter = IO.File.CreateText("\My Documents\cache.html")
                    lStream.Write(lText)
                    lStream.Write("<br><a href=""http://wap.geocaching.com/"">Official WAP</a><br>")
                    lStream.Write("<br><a href=""http://rtr.ca/geo?W=" & .name & """>rtr.ca WAP</a></body></html>")
                    lStream.Close()
                    OpenFile("\My Documents\cache.html")
                End With

            End If
        End If
        pFormVisible = True
        Redraw()
    End Sub

    Private Sub mnuRecordTrack_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuRecordTrack.Click
        mnuRecordTrack.Checked = Not mnuRecordTrack.Checked
        pRecordTrack = mnuRecordTrack.Checked
    End Sub

    Private Sub mnuDisplayTrack_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuViewTrack.Click
        mnuViewTrack.Checked = Not mnuViewTrack.Checked
        pDisplayTrack = mnuViewTrack.Checked

        If pLayers.Count > 0 AndAlso pLayers(0).Filename = "current" Then
            pLayers.RemoveAt(0)
        End If
        If pDisplayTrack Then
            pTrackMutex.WaitOne()
            If System.IO.File.Exists(pTrackLogFilename) Then
                OpenGPX(pTrackLastTime, 0)
            End If
            pTrackMutex.ReleaseMutex()
        End If
    End Sub

    Private Sub mnuOptionsDownload_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuOptionsDownload.Click
        pFormVisible = False
        pDownloadForm = New frmDownloadMobile
        With pDownloadForm
            On Error Resume Next
            .comboTileServer.Items.Clear()
            For Each lTileServerName As String In pTileServers.Keys
                .comboTileServer.Items.Add(lTileServerName)
            Next
            .comboTileServer.Text = TileServerName
            .DegreeFormat = g_DegreeFormat
            .txtTileFolder.Text = pTileCacheFolder
            .Latitude = CenterLat
            .Longitude = CenterLon
            .txtGPSSymbolSize.Text = pGPSSymbolSize
            .Show()
        End With
    End Sub

    Private Sub pDownloadForm_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles pDownloadForm.Closing
        If pDownloadForm.DialogResult = Windows.Forms.DialogResult.OK Then
            With pDownloadForm
                .Visible = False

                pTileCacheFolder = .txtTileFolder.Text
                TileServerName = .comboTileServer.Text
                g_DegreeFormat = .DegreeFormat
                CenterLat = .Latitude
                CenterLon = .Longitude

                pGPSSymbolSize = .txtGPSSymbolSize.Text
            End With
        End If
        pDownloadForm = Nothing
        pFormVisible = True
        Redraw()
    End Sub

    Private Sub mnuOptionsUpload_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuOptionsUpload.Click
        pFormVisible = False
        pUploadForm = New frmUploadMobile
        With pUploadForm
            On Error Resume Next
            .DegreeFormat = g_DegreeFormat
            .Latitude = CenterLat
            .Longitude = CenterLon
            .txtUploadURL.Text = g_UploadURL
            .chkUploadOnStart.Checked = pUploadOnStart
            .chkUploadOnStop.Checked = pUploadOnStop
            .chkUploadInterval.Checked = pUploadPeriodic
            .txtUploadInterval.Text = pUploadMinInterval.TotalMinutes
            .Show()
        End With
    End Sub

    Private Sub pUploadForm_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles pUploadForm.Closing
        If pUploadForm.DialogResult = Windows.Forms.DialogResult.OK Then
            With pUploadForm
                .Visible = False

                g_DegreeFormat = .DegreeFormat
                CenterLat = .Latitude
                CenterLon = .Longitude

                g_UploadURL = .txtUploadURL.Text
                If g_UploadURL.Length > 0 AndAlso g_UploadURL.IndexOf(":/") = -1 Then
                    g_UploadURL = "http://" & g_UploadURL
                End If

                pUploadMinInterval = Nothing

                If .chkUploadNow.Checked Then UploadGpsPosition()
                pUploadOnStart = .chkUploadOnStart.Checked
                pUploadOnStop = .chkUploadOnStop.Checked
                pUploadPeriodic = .chkUploadInterval.Checked
                If IsNumeric(.txtUploadInterval.Text) Then
                    pUploadMinInterval = New TimeSpan(0, 0, CDbl(.txtUploadInterval.Text) * 60)
                End If
            End With
        End If
        pUploadForm = Nothing
        pFormVisible = True
        Redraw()
    End Sub

    Private Sub mnuLayers_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuLayers.Click
        pFormVisible = False
        pLayersForm = New frmOptionsMobileGPX
        With pLayersForm
            On Error Resume Next
            .LayersLoaded = pLayers
            .txtGPXFolder.Text = pGPXFolder
            .comboLabels.Text = pGPXLabelField
            .txtGPXSymbolSize.Text = pTrackSymbolSize
            'TODO: .txtGPXSymbolColor.BackColor = pPenTrack.Color
            .Show()
        End With
    End Sub

    Private Sub pLayersForm_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles pLayersForm.Closing
        If pLayersForm.DialogResult = Windows.Forms.DialogResult.OK Then
            With pLayersForm
                pGPXFolder = .txtGPXFolder.Text
                pGPXLabelField = .comboLabels.Text

                'Change set of loaded GPX files to match ones now checked
                Dim lOldGPX As Generic.List(Of clsLayer) = pLayers
                pLayers = New Generic.List(Of clsLayer)
                pTrackSymbolSize = .txtGPXSymbolSize.Text
                'TODO: pPenTrack.Color = .txtGPXSymbolColor.BackColor
                Dim lSaveText As String = Me.Text
                Dim lFilename As String
                For Each lItem As ListViewItem In .lstGPX.Items
                    If lItem.Checked Then
                        lFilename = lItem.Text
                        Dim lAlreadyOpen As Boolean = False
                        For Each lLayer As clsLayer In lOldGPX
                            If lLayer.Filename = lFilename Then
                                Try 'Assign GPX-specific attribute LabelField
                                    Dim lGPXlayer As clsLayerGPX = lLayer
                                    lGPXlayer.LabelField = pGPXLabelField
                                Catch
                                End Try
                                pLayers.Add(lLayer)
                                lAlreadyOpen = True
                                Exit For
                            End If
                        Next
                        If Not lAlreadyOpen Then
                            Me.Text = "Opening " & IO.Path.GetFileNameWithoutExtension(lFilename) & "..."
                            Application.DoEvents()
                            OpenGPX(lFilename)
                        End If
                    End If
                Next
                Me.Text = lSaveText
            End With
        End If
        pLayersForm = Nothing
        pFormVisible = True
        Redraw()
    End Sub

    Private Sub mnuGPSdetails_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuViewGPSdetails.Click
        mnuViewGPSdetails.Checked = Not mnuViewGPSdetails.Checked
        pShowGPSdetails = mnuViewGPSdetails.Checked
        Redraw()
    End Sub

    Private Sub mnuViewTileOutlines_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuViewTileOutlines.Click
        mnuViewTileOutlines.Checked = Not mnuViewTileOutlines.Checked
        pShowTileOutlines = mnuViewTileOutlines.Checked
        Redraw()
    End Sub

    Private Sub mnuViewTileNames_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuViewTileNames.Click
        mnuViewTileNames.Checked = Not mnuViewTileNames.Checked
        pShowTileNames = mnuViewTileNames.Checked
        Redraw()
    End Sub

    Private Sub mnuRefreshOnClick_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuRefreshOnClick.Click
        mnuRefreshOnClick.Checked = Not mnuRefreshOnClick.Checked
        pClickDownload = mnuRefreshOnClick.Checked
        If pClickDownload Then
            pShowTileOutlines = True
            mnuViewTileOutlines.Checked = True
            Refresh()
        End If
    End Sub

    Private Sub mnuViewDark_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuViewDark.Click
        mnuViewDark.Checked = Not mnuViewDark.Checked
        pFormDark = mnuViewDark.Checked
        Redraw()
        If pFormDark Then
            StopKeepAwake()
        Else
            StartKeepAwake()
        End If
    End Sub

    ''' <summary>
    ''' Start timer that keeps system idle from turning off device
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub StartKeepAwake()
        SystemIdleTimerReset()
        If pKeepAwakeTimer Is Nothing Then
            pKeepAwakeTimer = New System.Threading.Timer(New Threading.TimerCallback(AddressOf IdleTimeout), Nothing, 0, 50000)
        End If
    End Sub
    Private Sub StopKeepAwake()
        If pKeepAwakeTimer IsNot Nothing Then
            pKeepAwakeTimer.Dispose()
            pKeepAwakeTimer = Nothing
        End If
    End Sub

    Private Sub IdleTimeout(ByVal o As Object)
        SystemIdleTimerReset()
    End Sub

    Private Sub mnuFollow_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFollow.Click
        mnuFollow.Checked = Not mnuFollow.Checked
        If mnuFollow.Checked Then
            pGPSFollow = 1
        Else
            pGPSFollow = 0
        End If
    End Sub

    Private Sub mnuCenter_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuCenter.Click
        mnuCenter.Checked = Not mnuCenter.Checked
        If mnuCenter.Checked Then
            pGPSFollow = 2
        Else
            pGPSFollow = 0
        End If
    End Sub

    Private Sub mnuAutoStart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuAutoStart.Click
        mnuAutoStart.Checked = Not mnuAutoStart.Checked
        pGPSAutoStart = mnuAutoStart.Checked
    End Sub

    Private Sub mnuViewMapTiles_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuViewMapTiles.Click
        mnuViewMapTiles.Checked = Not mnuViewMapTiles.Checked
        pShowTileImages = mnuViewMapTiles.Checked
        Redraw()
    End Sub

    Private Sub mnuFindBuddy_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFindBuddy.Click
        mnuFindBuddy.Checked = Not mnuFindBuddy.Checked
        If mnuFindBuddy.Checked Then
            StartBuddyTimer()
        Else
            StopBuddyTimer()
        End If
    End Sub

    Private Sub mnuViewControls_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuViewControls.Click
        mnuViewControls.Checked = Not mnuViewControls.Checked
        pControlsShow = mnuViewControls.Checked
        pControlsUse = pControlsShow
        Redraw()
    End Sub
End Class
