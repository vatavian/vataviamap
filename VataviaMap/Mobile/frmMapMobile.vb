Public Class frmMap

    WithEvents pDownloadForm As frmDownloadMobile
    WithEvents pUploadForm As frmUploadMobile
    WithEvents pLayersForm As frmOptionsMobileGPX
    WithEvents pActiveState As New Microsoft.WindowsMobile.Status.SystemState(Microsoft.WindowsMobile.Status.SystemProperty.ActiveApplication)

    Private Sub pActiveState_Changed(ByVal sender As Object, ByVal args As Microsoft.WindowsMobile.Status.ChangeEventArgs) Handles pActiveState.Changed
        pMap.Active = CStr(args.NewValue).EndsWith(Chr(27) & Me.Text)
    End Sub

    Public Sub New()
        InitializeComponent()

        mnuRecordTrack.Checked = pMap.RecordTrack
        mnuViewTrack.Checked = pMap.DisplayTrack
        mnuViewMapTiles.Checked = pMap.ShowTileImages
        mnuViewGPSdetails.Checked = pMap.ShowGPSdetails
        mnuViewTileOutlines.Checked = pMap.ShowTileOutlines
        mnuViewTileNames.Checked = pMap.ShowTileNames
        mnuViewTrack.Checked = pMap.DisplayTrack
        mnuViewControls.Checked = pMap.ControlsShow
        mnuRefreshOnClick.Checked = pMap.ClickRefreshTile
        Select Case pMap.GPSCenterBehavior
            Case 1 : mnuFollow.Checked = True
            Case 2 : mnuCenter.Checked = True
        End Select

        pMap.Active = True

        If pMap.AutoStart Then
            mnuAutoStart.Checked = True
            mnuStartStopGPS.Text = "Starting GPS..." : Application.DoEvents()
            pMap.StartGPS()
        End If
    End Sub

    Private Sub frmMap_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles MyBase.Closing
        mnuStartStopGPS.Text = "Closing..." : Application.DoEvents()
        pMap.StopGPS()
    End Sub

    Private Sub mnuStartStopGPS_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuStartStopGPS.Click
        If (mnuStartStopGPS.Text.Equals("Start GPS")) Then
            mnuStartStopGPS.Text = "Starting GPS..." : Application.DoEvents()
            pMap.StartGPS()
        Else
            mnuStartStopGPS.Text = "Stopping..." : Application.DoEvents()
            pMap.StopGPS()
            mnuStartStopGPS.Text = "Start GPS" : Application.DoEvents()
        End If
    End Sub

    Private Sub mnuGeocache_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuGeocache.Click
        pMap.ShowGeocacheHTML(pMap.ClosestGeocache(pMap.CenterLat, pMap.CenterLon))
    End Sub

    Private Sub mnuRecordTrack_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuRecordTrack.Click
        mnuRecordTrack.Checked = Not mnuRecordTrack.Checked
        pMap.RecordTrack = mnuRecordTrack.Checked
    End Sub

    Private Sub mnuDisplayTrack_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuViewTrack.Click
        mnuViewTrack.Checked = Not mnuViewTrack.Checked
        pMap.DisplayTrack = mnuViewTrack.Checked
    End Sub

    Private Sub mnuOptionsDownload_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuOptionsDownload.Click
        'pFormVisible = False
        pDownloadForm = New frmDownloadMobile
        With pDownloadForm
            On Error Resume Next
            .comboTileServer.Items.Clear()
            For Each lTileServerName As String In pMap.Servers.Keys
                .comboTileServer.Items.Add(lTileServerName)
            Next
            .comboTileServer.Text = pMap.TileServerName
            .DegreeFormat = g_DegreeFormat
            .txtTileFolder.Text = pMap.TileCacheFolder
            .Latitude = pMap.CenterLat
            .Longitude = pMap.CenterLon
            .txtGPSSymbolSize.Text = pMap.GPSSymbolSize
            .Show()
        End With
    End Sub

    Private Sub pDownloadForm_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles pDownloadForm.Closing
        If pDownloadForm.DialogResult = Windows.Forms.DialogResult.OK Then
            With pDownloadForm
                .Visible = False

                On Error Resume Next 'Ignore dysfunctional (e.g. non-numeric) settings
                pMap.TileCacheFolder = .txtTileFolder.Text
                pMap.TileServerName = .comboTileServer.Text
                g_DegreeFormat = .DegreeFormat
                pMap.CenterLat = .Latitude
                pMap.CenterLon = .Longitude

                pMap.GPSSymbolSize = .txtGPSSymbolSize.Text
            End With
        End If
        pDownloadForm = Nothing
        'pFormVisible = True
        pMap.NeedRedraw()
    End Sub

    Private Sub mnuOptionsUpload_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuOptionsUpload.Click
        'pFormVisible = False
        pUploadForm = New frmUploadMobile
        With pUploadForm
            On Error Resume Next
            .DegreeFormat = g_DegreeFormat
            .Latitude = pMap.CenterLat
            .Longitude = pMap.CenterLon
            .txtUploadURL.Text = g_UploadPointURL
            .chkUploadOnStart.Checked = pMap.UploadOnStart
            .chkUploadOnStop.Checked = pMap.UploadOnStop
            .chkUploadInterval.Checked = pMap.UploadPeriodic
            .txtUploadInterval.Text = pMap.UploadMinInterval.TotalMinutes
            .Show()
        End With
    End Sub

    Private Sub pUploadForm_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles pUploadForm.Closing
        If pUploadForm.DialogResult = Windows.Forms.DialogResult.OK Then
            With pUploadForm
                .Visible = False

                g_DegreeFormat = .DegreeFormat
                pMap.CenterLat = .Latitude
                pMap.CenterLon = .Longitude

                g_UploadPointURL = .txtUploadURL.Text
                If g_UploadPointURL.Length > 0 AndAlso g_UploadPointURL.IndexOf(":/") = -1 Then
                    g_UploadPointURL = "http://" & g_UploadPointURL
                End If

                pMap.UploadMinInterval = Nothing
                If .chkUploadNow.Checked Then pMap.UploadGpsPosition()
                pMap.UploadOnStart = .chkUploadOnStart.Checked
                pMap.UploadOnStop = .chkUploadOnStop.Checked
                pMap.UploadPeriodic = .chkUploadInterval.Checked
                If IsNumeric(.txtUploadInterval.Text) Then
                    pMap.UploadMinInterval = New TimeSpan(0, 0, CDbl(.txtUploadInterval.Text) * 60)
                End If
            End With
        End If
        pUploadForm = Nothing
        'pFormVisible = True
        pMap.NeedRedraw()
    End Sub

    Private Sub mnuLayers_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuLayers.Click
        'pFormVisible = False
        pLayersForm = New frmOptionsMobileGPX
        With pLayersForm
            On Error Resume Next
            .LayersLoaded = pMap.Layers
            .txtGPXFolder.Text = pMap.GPXFolder
            .comboLabels.Text = pMap.GPXLabelField
            .txtGPXSymbolSize.Text = pMap.TrackSymbolSize
            'TODO: .txtGPXSymbolColor.BackColor = pMap.pPenTrack.Color
            .Show()
        End With
    End Sub

    Private Sub pLayersForm_Closing(ByVal sender As Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles pLayersForm.Closing
        If pLayersForm.DialogResult = Windows.Forms.DialogResult.OK Then
            With pLayersForm
                pMap.GPXFolder = .txtGPXFolder.Text
                pMap.GPXLabelField = .comboLabels.Text

                'Change set of loaded GPX files to match ones now checked
                Dim lOldGPX As Generic.List(Of clsLayer) = pMap.Layers
                pMap.Layers = New Generic.List(Of clsLayer)
                pMap.TrackSymbolSize = .txtGPXSymbolSize.Text
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
                                    lGPXlayer.LabelField = pMap.GPXLabelField
                                Catch
                                End Try
                                pMap.Layers.Add(lLayer)
                                lAlreadyOpen = True
                                Exit For
                            End If
                        Next
                        If Not lAlreadyOpen Then
                            Me.Text = "Opening " & IO.Path.GetFileNameWithoutExtension(lFilename) & "..."
                            Application.DoEvents()
                            pMap.OpenFile(lFilename)
                        End If
                    End If
                Next
                Me.Text = lSaveText
            End With
        End If
        pLayersForm = Nothing
        'pFormVisible = True
        pMap.NeedRedraw()
    End Sub

    Private Sub mnuGPSdetails_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuViewGPSdetails.Click
        mnuViewGPSdetails.Checked = Not pMap.ShowGPSdetails
        pMap.ShowGPSdetails = mnuViewGPSdetails.Checked
    End Sub

    Private Sub mnuViewTileOutlines_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuViewTileOutlines.Click
        mnuViewTileOutlines.Checked = Not pMap.ShowTileOutlines
        pMap.ShowTileOutlines = mnuViewTileOutlines.Checked
    End Sub

    Private Sub mnuViewTileNames_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuViewTileNames.Click
        mnuViewTileNames.Checked = Not pMap.ShowTileNames
        pMap.ShowTileNames = mnuViewTileNames.Checked
    End Sub

    Private Sub mnuRefreshOnClick_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuRefreshOnClick.Click
        mnuRefreshOnClick.Checked = Not pMap.ClickRefreshTile
        mnuViewTileOutlines.Checked = mnuRefreshOnClick.Checked
        pMap.ClickRefreshTile = mnuRefreshOnClick.Checked
        pMap.ShowTileOutlines = mnuRefreshOnClick.Checked
    End Sub

    Private Sub mnuViewDark_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuViewDark.Click
        mnuViewDark.Checked = Not mnuViewDark.Checked
        pMap.Dark = mnuViewDark.Checked
    End Sub

    Private Sub mnuFollow_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFollow.Click
        mnuFollow.Checked = (pMap.GPSCenterBehavior <> 1)
        If mnuFollow.Checked Then
            pMap.GPSCenterBehavior = 1
        Else
            pMap.GPSCenterBehavior = 0
        End If
    End Sub

    Private Sub mnuCenter_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuCenter.Click
        mnuCenter.Checked = (pMap.GPSCenterBehavior <> 2)
        If mnuCenter.Checked Then
            pMap.GPSCenterBehavior = 2
        Else
            pMap.GPSCenterBehavior = 0
        End If
    End Sub

    Private Sub mnuAutoStart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuAutoStart.Click
        mnuAutoStart.Checked = Not mnuAutoStart.Checked
        pMap.AutoStart = mnuAutoStart.Checked
    End Sub

    Private Sub mnuViewMapTiles_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuViewMapTiles.Click
        mnuViewMapTiles.Checked = Not mnuViewMapTiles.Checked
        pMap.ShowTileImages = mnuViewMapTiles.Checked
    End Sub

    Private Sub mnuFindBuddy_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuFindBuddy.Click
        mnuFindBuddy.Checked = Not mnuFindBuddy.Checked
        pMap.FindBuddy = mnuFindBuddy.Checked
    End Sub

    Private Sub mnuViewControls_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuViewControls.Click
        mnuViewControls.Checked = Not mnuViewControls.Checked
        pMap.ControlsShow = mnuViewControls.Checked
        pMap.ControlsUse = pMap.ControlsShow
        pMap.Refresh()
    End Sub

    Private Sub mnuTakePicture_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuTakePicture.Click
        Dim ccd As New Microsoft.WindowsMobile.Forms.CameraCaptureDialog
        With ccd
            .InitialDirectory = g_PathChar & "My Documents" & g_PathChar & "My Pictures"
            Dim lBaseName As String = g_PathChar & "MapImage_"
            Dim lFilename As String
            Dim lIndex As Integer = 1
            Do
                lFilename = .InitialDirectory & lBaseName & Format(lIndex, "000") & ".jpg"
                lIndex += 1
            Loop While IO.File.Exists(lFilename)
            .DefaultFileName = IO.Path.GetFileName(lFilename)
            .StillQuality = Microsoft.WindowsMobile.Forms.CameraCaptureStillQuality.High
            .Resolution = New Drawing.Size(2048, 1536)
            .Mode = Microsoft.WindowsMobile.Forms.CameraCaptureMode.Still
            .Owner = Me
            .Title = .DefaultFileName
            If ccd.ShowDialog() = Windows.Forms.DialogResult.OK Then
                'Todo: geotag inside .jpg, upload photo
                Dim lWaypoint As clsGPXwaypoint = pMap.LatestPositionWaypoint("wpt")
                If lWaypoint IsNot Nothing AndAlso Math.Abs(Now.ToUniversalTime.Subtract(lWaypoint.time).TotalSeconds) < 30 Then
                    lWaypoint.sym = "photo"
                    lWaypoint.name = IO.Path.GetFileNameWithoutExtension(ccd.FileName)
                    lWaypoint.link.Add(New clsGPXlink(lWaypoint.name, "image/jpeg", IO.Path.GetFileName(ccd.FileName)))
                    Dim lPhotoGPX As New clsGPX
                    lPhotoGPX.wpt.Add(lWaypoint)
                    WriteTextFile(ccd.FileName & ".gpx", lPhotoGPX.ToString)
                Else
                    If lWaypoint IsNot Nothing Then Debug.WriteLine(Math.Abs(Now.ToUniversalTime.Subtract(lWaypoint.time).TotalSeconds) & " > 30 seconds")
                End If
            End If
        End With
    End Sub

    Private Sub mnuSetTime_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuSetTime.Click
        pMap.SetTime()
    End Sub

    Private Sub mnuWaypoint_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuWaypoint.Click
        mnuWaypoint.Checked = Not mnuWaypoint.Checked
        pMap.ClickMakeWaypoint = mnuWaypoint.Checked
    End Sub

    Private Sub pMap_CenterBehaviorChanged() Handles pMap.CenterBehaviorChanged
        Select Case pMap.GPSCenterBehavior
            Case 1 : mnuCenter.Checked = False : mnuFollow.Checked = True
            Case 2 : mnuCenter.Checked = True : mnuFollow.Checked = False
            Case Else : mnuCenter.Checked = False : mnuFollow.Checked = False
        End Select
    End Sub

    Private Sub pMap_LocationChanged(ByVal aPosition As GPS_API.GpsPosition) Handles pMap.LocationChanged
        mnuStartStopGPS.Text = "Stop GPS " & aPosition.SatelliteCount & "/" & aPosition.SatellitesInViewCount
        Application.DoEvents()
    End Sub

    Private Sub mnuAbout_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuAbout.Click
        Dim lAbout As New frmAbout
        lAbout.Show()
    End Sub
End Class
