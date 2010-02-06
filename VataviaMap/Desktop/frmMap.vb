Public Class frmMap

    WithEvents pLayersForm As frmLayers
    WithEvents pBuddyAlarmForm As frmBuddyAlarm
    WithEvents pBuddyListForm As frmBuddyList
    WithEvents pWaypointsListForm As frmWaypoints
    WithEvents pCoordinatesForm As frmCoordinates
    WithEvents pTileServerForm As frmEditNameURL
    WithEvents pOpenCellIDForm As frmOpenCellID
    WithEvents pTimeZoneForm As frmTimeZone

    Private pLastKeyDown As Integer = 0

    Public Sub New()
        Application.CurrentCulture = New Globalization.CultureInfo("")
        InitializeComponent()

        'Create zoom level menu items
        For lZoomLevel As Integer = g_ZoomMin To g_ZoomMax
            Me.ZoomToolStripMenuItem.DropDownItems.Add(New ToolStripMenuItem(CStr(lZoomLevel), Nothing, New EventHandler(AddressOf ZoomToolStripMenuItem_Click)))
        Next

        Dim lTileCacheFolder As String = GetAppSetting("TileCacheFolder", IO.Path.GetTempPath() & "tiles" & g_PathChar)

        If Not IO.Directory.Exists(lTileCacheFolder) Then
            Dim lDialog As New FolderBrowserDialog
            With lDialog
                .Description = "Select folder to save cached tiles in"
                .ShowNewFolderButton = True
                If .ShowDialog = Windows.Forms.DialogResult.OK Then
                    lTileCacheFolder = .SelectedPath
                Else 'There is no hope if we cannot cache tiles
                    lTileCacheFolder = ""
                End If
            End With
        End If
        If lTileCacheFolder.Length > 0 AndAlso Not lTileCacheFolder.EndsWith(g_PathChar) Then
            lTileCacheFolder &= g_PathChar
        End If

        pMap.TileCacheFolder = lTileCacheFolder
        pMap.SharedNew()

        BuildTileServerMenu()

        PanToGPXToolStripMenuItem.Checked = pMap.GPXPanTo
        ZoomToGPXToolStripMenuItem.Checked = pMap.GPXZoomTo
        TileImagesToolStripMenuItem.Checked = pMap.ShowTileImages
        TileOutlinesToolStripMenuItem.Checked = pMap.ShowTileOutlines
        TileNamesToolStripMenuItem.Checked = pMap.ShowTileNames

        Dim lTryInt As Integer

        If Integer.TryParse(GetAppSetting("WindowWidth", Me.Width), lTryInt) Then
            Me.Width = lTryInt
        End If
        If Integer.TryParse(GetAppSetting("WindowHeight", Me.Height), lTryInt) Then
            Me.Height = lTryInt
        End If

        If Disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If

        Dim lFilesOnCmdLine As New Generic.List(Of String)
        For Each lArg As String In My.Application.CommandLineArgs
            If IO.File.Exists(lArg) Then
                lFilesOnCmdLine.Add(lArg)
            End If
        Next
        If lFilesOnCmdLine.Count > 0 Then pMap.OpenFiles(lFilesOnCmdLine.ToArray)
    End Sub

    Private Sub frmMap_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles Me.DragDrop
        Me.Activate()
        If e.Data.GetDataPresent(Windows.Forms.DataFormats.FileDrop) Then
            pMap.OpenFiles(e.Data.GetData(Windows.Forms.DataFormats.FileDrop))
            pMap.NeedRedraw()
        End If
    End Sub

    Private Sub frmMap_DragEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles Me.DragEnter
        If e.Data.GetDataPresent(Windows.Forms.DataFormats.FileDrop) Then
            e.Effect = Windows.Forms.DragDropEffects.All
        End If
    End Sub

    Private Sub frmMap_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If Me.WindowState = FormWindowState.Normal Then
            SaveAppSetting("WindowWidth", Me.Width)
            SaveAppSetting("WindowHeight", Me.Height)
        End If
        pMap.SaveSettings()
    End Sub

    Private Sub frmMap_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        pMap.NeedRedraw()
        Dim lOffsetHours As Integer = pMap.ImportOffsetFromUTC.Hours
        Dim lOffsetSign As String = ""
        If lOffsetHours >= 0 Then lOffsetSign = "+"
        Dim lOffsetSuffix As String = ""
        If pMap.ImportOffsetFromUTC.Minutes > 0 Then
            lOffsetSuffix = ":" & Format(pMap.ImportOffsetFromUTC.Minutes, "00")
        End If
        TimeZoneToolStripMenuItem.Text = "Time Zone (UTC " & lOffsetSign & lOffsetHours & lOffsetSuffix & ")"
    End Sub

    ' Prevent flickering when default implementation redraws background
    Protected Overrides Sub OnPaintBackground(ByVal e As PaintEventArgs)
    End Sub

    Private Sub TileOutlinesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TileOutlinesToolStripMenuItem.Click
        pMap.ShowTileOutlines = Not pMap.ShowTileOutlines
        TileOutlinesToolStripMenuItem.Checked = pMap.ShowTileOutlines
    End Sub

    Private Sub TileNamesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TileNamesToolStripMenuItem.Click
        pMap.ShowTileNames = Not pMap.ShowTileNames
        TileNamesToolStripMenuItem.Checked = pMap.ShowTileNames
    End Sub

    Private Sub TileImagesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TileImagesToolStripMenuItem.Click
        pMap.ShowTileImages = Not pMap.ShowTileImages
        TileImagesToolStripMenuItem.Checked = pMap.ShowTileImages
    End Sub

    Private Sub TimestampToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TimestampToolStripMenuItem.Click
        pMap.ShowDate = Not pMap.ShowDate
        TimestampToolStripMenuItem.Checked = pMap.ShowDate
    End Sub

    Private Sub RefreshFromServerToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RefreshFromServerToolStripMenuItem.Click
        If pMap.ClickedTileFilename.Length > 0 Then
            Try
                Debug.WriteLine("Refreshing  '" & pMap.ClickedTileFilename & "'")
                pMap.Downloader.DeleteTile(pMap.ClickedTileFilename)
                pMap.NeedRedraw()
            Catch ex As Exception
                MsgBox("Could not refresh '" & pMap.ClickedTileFilename & "'" & vbCrLf & ex.Message, MsgBoxStyle.Critical, "Error Refreshing Tile")
            End Try
            pMap.ClickedTileFilename = ""
        End If
    End Sub

    Private Sub SaveAsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SaveAsToolStripMenuItem.Click
        Dim lSaveDialog As New SaveFileDialog
        With lSaveDialog
            .DefaultExt = g_TileExtension
            .InitialDirectory = My.Computer.FileSystem.SpecialDirectories.MyDocuments
            .FileName = "OpenStreetMap-" & Format(Now, "yyyy-MM-dd") & "at" & Format(Now, "HH-mm-ss") & g_TileExtension
            If .ShowDialog = Windows.Forms.DialogResult.OK Then
                pMap.SaveImageAs(.FileName)
            End If
        End With
    End Sub

    Private Sub CopyToClipboardToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CopyToClipboardToolStripMenuItem.Click
        pMap.CopyToClipboard()
    End Sub

    '''' <summary>
    '''' Save all the visible tiles
    '''' </summary>
    'Private Sub SaveTiles(ByVal aSaveIn As String)
    '    If IO.Directory.Exists(pTileCacheFolder) Then
    '        Dim lOffsetToCenter As Point
    '        Dim lTopLeft As Point
    '        Dim lBotRight As Point

    '        FindTileBounds(pBitmap.GetBounds(Drawing.GraphicsUnit.Pixel), lOffsetToCenter, lTopLeft, lBotRight)

    '        Dim lTilePoint As Point

    '        IO.Directory.CreateDirectory(aSaveIn)
    '        'Loop through each visible tile
    '        For x As Integer = lTopLeft.X To lBotRight.X
    '            For y As Integer = lTopLeft.Y To lBotRight.Y
    '                lTilePoint = New Point(x, y)
    '                SaveTile(lTilePoint, pZoom, aSaveIn)
    '            Next
    '        Next
    '    End If
    'End Sub

    '''' <summary>
    '''' Save a tile with projection information
    '''' </summary>
    '''' <param name="aSaveIn">Folder to save in</param>
    'Private Sub SaveTile(ByVal aTilePoint As Point, _
    '                     ByVal aZoom As Integer, _
    '                     ByVal aSaveIn As String)
    '    'TODO:
    '    Dim lTileImage As Bitmap = TileBitmap(aTilePoint, aZoom, -1)
    '    If lTileImage IsNot Nothing Then
    '        Dim lTileFileName As String = TileFilename(aTilePoint, pZoom, pUseMarkedTiles)
    '        Dim lSaveAs As String = IO.Path.Combine(aSaveIn, lTileFileName.Substring(g_TileCacheFolder.Length).Replace(g_PathChar, "_"))
    '        lTileImage.Save(lSaveAs)
    '        Dim lNorth As Double, lWest As Double, lSouth As Double, lEast As Double
    '        CalcLatLonFromTileXY(aTilePoint, aZoom, lNorth, lWest, lSouth, lEast)
    '        CreateGeoReferenceFile(LatitudeToMeters(lNorth), _
    '                               LongitudeToMeters(lWest), _
    '                               aZoom, IO.Path.ChangeExtension(lSaveAs, "pgw"))
    '        IO.File.WriteAllText(IO.Path.ChangeExtension(lSaveAs, "prj"), "PROJCS[""WGS84 / Simple Mercator"",GEOGCS[""WGS 84"",DATUM[""WGS_1984"",SPHEROID[""WGS_1984"", 6378137.0, 298.257223563]],PRIMEM[""Greenwich"", 0.0],UNIT[""degree"", 0.017453292519943295],AXIS[""Longitude"", EAST],AXIS[""Latitude"", NORTH]],PROJECTION[""Mercator_1SP_Google""],PARAMETER[""latitude_of_origin"", 0.0],PARAMETER[""central_meridian"", 0.0],PARAMETER[""scale_factor"", 1.0],PARAMETER[""false_easting"", 0.0],PARAMETER[""false_northing"", 0.0],UNIT[""m"", 1.0],AXIS[""x"", EAST],AXIS[""y"", NORTH],AUTHORITY[""EPSG"",""900913""]]")
    '    End If
    'End Sub

    Private Sub OpenGPXToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AddLayerToolStripMenuItem.Click
        Dim lDialog As New OpenFileDialog
        With lDialog
            .DefaultExt = ".gpx"
            .Filter = "GPX or LOC|*.gpx;*.loc|All Files|*.*"
            .FilterIndex = 0
            .Multiselect = True
            .Title = "Select GPX file(s) to open"
            If .ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
                pMap.OpenFiles(.FileNames)
            End If
        End With
    End Sub

    Private Sub CloseGPXToolStripMenuItem_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles RemoveAllLayersToolStripMenuItem.Click
        pMap.CloseAllLayers()
        pMap.NeedRedraw()
    End Sub

    Private Sub TileServer_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim lItemClicked As ToolStripMenuItem = sender
        For Each lItem As ToolStripMenuItem In TileServerToolStripMenuItem.DropDownItems
            If lItem.Equals(lItemClicked) Then
                lItem.Checked = True
                pMap.TileServerName = lItem.Text
            ElseIf lItem.Equals(AddTileServerMenuItem) Then
                Exit For
            Else
                lItem.Checked = False
            End If
        Next
        pMap.NeedRedraw()
    End Sub

    Private Sub NewTileServerForm()
        If pTileServerForm IsNot Nothing Then
            Try
                pTileServerForm.Close()
            Catch
            End Try
        End If
        pTileServerForm = New frmEditNameURL
        pTileServerForm.Icon = Me.Icon
    End Sub

    Private Sub AddTileServerMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AddTileServerMenuItem.Click
        NewTileServerForm()
        pTileServerForm.AskUser("Add New Tile Server", g_TileServerName, g_TileServerURL, g_TileServerExampleLabel, g_TileServerExampleFile)
        'http://opentiles.appspot.com/tile/get/ma/' 12/1242/1512.png
    End Sub

    Private Sub EditTileServerMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim lItemClicked As ToolStripMenuItem = sender
        NewTileServerForm()
        pTileServerForm.AskUser("Edit Tile Server", lItemClicked.Text, pMap.TileServers.Item(lItemClicked.Text), g_TileServerExampleLabel, g_TileServerExampleFile)
    End Sub

    Private Sub pTileServerForm_Ok(ByVal aOriginalName As String, ByVal aName As String, ByVal aURL As String) Handles pTileServerForm.Ok
        pMap.TileServers.Remove(aOriginalName)

        If aURL.Length > 0 AndAlso Not aURL.EndsWith("/") Then aURL &= "/"
        pMap.TileServers.Add(aName, aURL)
        pMap.TileServerName = aName
        If g_TileServerName = aOriginalName Then
            pMap.TileServerName = aName
        End If
        BuildTileServerMenu()
        pMap.NeedRedraw()
    End Sub

    Private Sub pTileServerForm_Remove(ByVal aOriginalName As String) Handles pTileServerForm.Remove
        pMap.TileServers.Remove(aOriginalName)
        If g_TileServerName = aOriginalName AndAlso pMap.TileServers.Count > 0 Then
            'If we just removed the current tile server, set to the first one left
            For Each lName As String In pMap.TileServers.Keys
                pMap.TileServerName = lName
                Exit For
            Next
            pMap.NeedRedraw()
        End If
        BuildTileServerMenu()
    End Sub

    Private Sub BuildTileServerMenu()
        TileServerToolStripMenuItem.DropDownItems.Clear()
        EditTileServerMenuItem.DropDownItems.Clear()
        For Each lName As String In pMap.TileServers.Keys
            Dim lNewItem As New ToolStripMenuItem(lName)
            AddHandler lNewItem.Click, AddressOf TileServer_Click
            If lName = g_TileServerName Then lNewItem.Checked = True
            TileServerToolStripMenuItem.DropDownItems.Add(lNewItem)

            Dim lNewEditItem As New ToolStripMenuItem(lName)
            AddHandler lNewEditItem.Click, AddressOf EditTileServerMenuItem_Click
            EditTileServerMenuItem.DropDownItems.Add(lNewEditItem)
        Next
        TileServerToolStripMenuItem.DropDownItems.Add(AddTileServerMenuItem)
        TileServerToolStripMenuItem.DropDownItems.Add(EditTileServerMenuItem)
        TileServerToolStripMenuItem.DropDownItems.Add(DefaultsTileServerMenuItem)
    End Sub

    Private Sub DefaultsTileServerMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DefaultsTileServerMenuItem.Click
        pMap.SetDefaultTileServers()
        BuildTileServerMenu()
    End Sub

    Private Sub OverlayMaplintToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OverlayMaplintToolStripMenuItem.Click
        'TODO:
        'g_TileServerTransparentURL = "http://tah.openstreetmap.org/Tiles/maplint/"
        'OverlayMaplintToolStripMenuItem.Checked = Not OverlayMaplintToolStripMenuItem.Checked
        'If OverlayMaplintToolStripMenuItem.Checked Then
        '    OverlayYahooLabelsToolStripMenuItem.Checked = False
        '    pShowTransparentTiles = True
        'Else
        '    pShowTransparentTiles = OverlayYahooLabelsToolStripMenuItem.Checked
        'End If
        'Redraw()
    End Sub

    Private Sub OverlayYahooLabelsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OverlayYahooLabelsToolStripMenuItem.Click
        ''TODO: manage transparent tile server like main tile server, add Transparent field to server type
        ''g_TileServerTransparentURL = "..."
        'OverlayYahooLabelsToolStripMenuItem.Checked = Not OverlayYahooLabelsToolStripMenuItem.Checked
        'If OverlayYahooLabelsToolStripMenuItem.Checked Then
        '    OverlayMaplintToolStripMenuItem.Checked = False
        '    pShowTransparentTiles = True
        'Else
        '    pShowTransparentTiles = OverlayMaplintToolStripMenuItem.Checked
        'End If
        'Redraw()
    End Sub

    Private Sub PanToGPXToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PanToGPXToolStripMenuItem.Click
        pMap.GPXPanTo = Not pMap.GPXPanTo
        PanToGPXToolStripMenuItem.Checked = pMap.GPXPanTo
    End Sub

    Private Sub ZoomToGPXToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ZoomToGPXToolStripMenuItem.Click
        pMap.GPXZoomTo = Not pMap.GPXZoomTo
        ZoomToGPXToolStripMenuItem.Checked = pMap.GPXZoomTo
        If pMap.GPXZoomTo AndAlso Not pMap.GPXPanTo Then 'If they want to zoom, they must want to pan too
            pMap.GPXPanTo = True
            PanToGPXToolStripMenuItem.Checked = True
        End If
    End Sub

    Private Sub ZoomAllLayersToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ZoomAllLayersToolStripMenuItem.Click
        pMap.ZoomToAll()
    End Sub

    Private Sub ZoomToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        pMap.Zoom = CInt(sender.ToString)
    End Sub

    Private Sub UseMarkedTilesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles UseMarkedTilesToolStripMenuItem.Click
        'TODO:
        'pMap.UseMarkedTiles = Not pMap.UseMarkedTiles
        'UseMarkedTilesToolStripMenuItem.Checked = pMap.UseMarkedTiles
        'pMap.Redraw()
    End Sub

    Private Sub OpenOSMWebsiteToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OpenOSMWebsiteToolStripMenuItem.Click
        Try
            Process.Start("http://www.openstreetmap.org/?lat=" & pMap.CenterLat.ToString("#.#######") & "&lon=" & pMap.CenterLon.ToString("#.#######") & "&zoom=" & pMap.Zoom)
        Catch ex As System.ComponentModel.Win32Exception
            'This happens when Firefox takes a moment to start due to updates, ignore it
        End Try
    End Sub

    Private Sub OpenJOSMToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OpenJOSMToolStripMenuItem.Click
        Dim lJosmFilename As String = GetAppSetting("JOSM", IO.Path.Combine(IO.Path.GetTempPath, "josm-tested.jar"))

        If Not IO.File.Exists(lJosmFilename) Then
            If MsgBox("Is JOSM already installed on this machine?", MsgBoxStyle.YesNo, "Opening JOSM") = MsgBoxResult.Yes Then
                'If Not IO.File.Exists(lJosmFilename) AndAlso Not pDownloader.DownloadFile("http://josm.openstreetmap.de/download/josm.jnlp", lJosmFilename) Then
                Dim lOpenDialog As New Windows.Forms.OpenFileDialog
                With lOpenDialog
                    .Title = "Please locate josm-tested.jar"
                    .FileName = "josm-tested.jar"
                    If .ShowDialog = Windows.Forms.DialogResult.OK AndAlso IO.File.Exists(.FileName) Then
                        lJosmFilename = .FileName
                        SaveAppSetting("JOSM", lJosmFilename)
                    Else : Exit Sub
                    End If
                End With
            End If
        End If

        If Not IO.File.Exists(lJosmFilename) AndAlso MsgBox("Download latest version of JOSM now?", MsgBoxStyle.YesNo, "Download JOSM") = MsgBoxResult.Yes Then
            Me.Cursor = Cursors.WaitCursor
            Dim lSaveTitle As String = Me.Text
            Me.Text = "Downloading http://josm.openstreetmap.de/josm-tested.jar ..."
            Application.DoEvents()
            If Not pMap.Downloader.DownloadFile("http://josm.openstreetmap.de/josm-tested.jar", lJosmFilename, False) Then
                MsgBox("Please manually download JOSM from" & vbCrLf & "http://josm.openstreetmap.de/" & vbCrLf & "and save as " & vbCrLf & lJosmFilename, MsgBoxStyle.OkOnly, "Unable to download JOSM")
            End If
            Me.Text = lSaveTitle
            Me.Cursor = Cursors.Default
        End If

        If IO.File.Exists(lJosmFilename) Then
            Dim lArguments As String = ""
            For Each lLayer As clsLayer In pMap.Layers
                lArguments &= """" & lLayer.Filename & """ "
            Next
            Process.Start(lJosmFilename, lArguments)
        End If
    End Sub

    Private Sub CoordinatesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles CoordinatesToolStripMenuItem.Click
        If pCoordinatesForm IsNot Nothing Then
            Try
                pCoordinatesForm.Close()
            Catch
            End Try
        End If
        pCoordinatesForm = New frmCoordinates
        pCoordinatesForm.Icon = Me.Icon
        pCoordinatesForm.Show(Me)
    End Sub

    Private Sub LayersToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LayersToolStripMenuItem.Click
        If pLayersForm IsNot Nothing Then
            Try
                pLayersForm.Close()
            Catch
            End Try
        End If
        pLayersForm = New frmLayers
        pLayersForm.Icon = Me.Icon
        pLayersForm.PopulateList(pMap.Layers)
        pLayersForm.Show()
    End Sub

    Private Sub pLayersForm_Apply() Handles pLayersForm.Apply
        pMap.NeedRedraw()
    End Sub

    Private Sub pLayersForm_CheckedItemsChanged(ByVal aSelectedLayers As Generic.List(Of String)) Handles pLayersForm.CheckedItemsChanged
        'Change set of loaded/visible layers to match ones now checked
        Dim lFilename As String

        For Each lLayer As clsLayer In pMap.Layers
            lLayer.Visible = False
        Next

        For Each lFilename In aSelectedLayers
            Dim lAlreadyOpen As Boolean = False
            For Each lLayer As clsLayer In pMap.Layers
                If lLayer.Filename.ToLower = lFilename.ToLower Then
                    lLayer.Visible = True
                    lAlreadyOpen = True
                    Exit For
                End If
            Next

            If Not lAlreadyOpen Then
                Me.Text = "Opening " & IO.Path.GetFileNameWithoutExtension(lFilename)
                Application.DoEvents()
                pMap.OpenFile(lFilename)
            End If
        Next
        pMap.NeedRedraw()
    End Sub

    Private Sub ExitToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitToolStripMenuItem.Click
        Me.Close()
    End Sub

    Private Sub FindBuddyToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles FindBuddyToolStripMenuItem.Click
        FindBuddyToolStripMenuItem.Checked = Not FindBuddyToolStripMenuItem.Checked
        pMap.FindBuddy = FindBuddyToolStripMenuItem.Checked
    End Sub

    Private Sub SetBuddyAlarmToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SetBuddyAlarmToolStripMenuItem.Click
        'TODO:
        'If pBuddyAlarmForm IsNot Nothing Then
        '    Try
        '        pBuddyAlarmForm.Close()
        '    Catch
        '    End Try
        'End If
        'pBuddyAlarmForm = New frmBuddyAlarm
        'pBuddyAlarmForm.Icon = Me.Icon
        'If pBuddyAlarmLat = 0 AndAlso pBuddyAlarmLon = 0 Then
        '    pBuddyAlarmLat = CenterLat
        '    pBuddyAlarmLon = CenterLon
        'End If
        'pBuddyAlarmForm.AskUser(pBuddyAlarmLat, pBuddyAlarmLon, pBuddyAlarmMeters, True)
    End Sub

    Private Sub pBuddyAlarmForm_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles pBuddyAlarmForm.FormClosed
        pBuddyAlarmForm = Nothing
    End Sub

    Private Sub pBuddyAlarmForm_SetAlarm(ByVal aLatitude As Double, ByVal aLongitude As Double, ByVal aDistanceMeters As Double, ByVal aEnable As Boolean) Handles pBuddyAlarmForm.SetAlarm
        'TODO:
        'pBuddyAlarmLat = aLatitude
        'pBuddyAlarmLon = aLongitude
        'pBuddyAlarmMeters = aDistanceMeters
        'pBuddyAlarmEnabled = aEnable
    End Sub

    Private Sub ListBuddiesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ListBuddiesToolStripMenuItem.Click
        If pBuddyListForm IsNot Nothing Then
            Try
                pBuddyListForm.Close()
            Catch
            End Try
        End If
        pBuddyListForm = New frmBuddyList
        pBuddyListForm.Icon = Me.Icon
        pBuddyListForm.AskUser(pMap.Buddies)
    End Sub

    Private Sub pBuddyListForm_Ok(ByVal aBuddies As System.Collections.Generic.Dictionary(Of String, clsBuddy)) Handles pBuddyListForm.Ok
        pMap.Buddies.Clear()
        pMap.Buddies = aBuddies
        pMap.NeedRedraw()
    End Sub

    Private Sub WheelTileServerToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles WheelTileServerToolStripMenuItem.Click
        WheelTileServerToolStripMenuItem.Checked = True
        WheelZoomToolStripMenuItem.Checked = False
        WheelLayerToolStripMenuItem.Checked = False
        pMap.MouseWheelAction = ctlMap.EnumWheelAction.TileServer
    End Sub

    Private Sub WheelZoomToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles WheelZoomToolStripMenuItem.Click
        WheelTileServerToolStripMenuItem.Checked = False
        WheelZoomToolStripMenuItem.Checked = True
        WheelLayerToolStripMenuItem.Checked = False
        pMap.MouseWheelAction = ctlMap.EnumWheelAction.Zoom
    End Sub

    Private Sub WheelLayerToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles WheelLayerToolStripMenuItem.Click
        WheelTileServerToolStripMenuItem.Checked = False
        WheelZoomToolStripMenuItem.Checked = False
        WheelLayerToolStripMenuItem.Checked = True
        pMap.MouseWheelAction = ctlMap.EnumWheelAction.Layer
    End Sub

    Private Sub VataviaMapProjectPageToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles VataviaMapProjectPageToolStripMenuItem.Click
        OpenFile("http://code.google.com/p/vataviamap/")
    End Sub

    Private Sub GetOSMBugsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles GetOSMBugsToolStripMenuItem.Click
        Dim lBugsGPXurl As String = "http://openstreetbugs.schokokeks.org/api/0.1/getGPX?" _
            & "b=" & pMap.LatMin.ToString("#.#######") _
            & "&t=" & pMap.LatMax.ToString("#.#######") _
            & "&l=" & pMap.LonMin.ToString("#.#######") _
            & "&r=" & pMap.LonMax.ToString("#.#######") _
            & "&open=yes"
        Dim lBugsGPXfilename As String = IO.Path.Combine(My.Computer.FileSystem.SpecialDirectories.MyDocuments, "bugs.gpx")
        If pMap.Downloader.DownloadFile(lBugsGPXurl, lBugsGPXfilename, False) Then pMap.OpenFile(lBugsGPXfilename) 'TODO: set .LabelField = "desc"
    End Sub

    Private Sub FollowOSMURLToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles FollowOSMURLToolStripMenuItem.Click
        'http://www.openstreetmap.org/?lat=33.794266&lon=-84.29019&zoom=18&layers=B000FTF
        If Clipboard.ContainsText Then
            Dim lLat As Double = 0
            Dim lLon As Double = 0
            Dim lZoom As Integer = 14
            For Each lArg As String In Clipboard.GetText.Split("&"c, "?"c)
                Dim lArgPart() As String = lArg.Split("=")
                If lArgPart.Length = 2 Then
                    Select Case lArgPart(0)
                        Case "lat"
                            If IsNumeric(lArgPart(1)) Then
                                lLat = Double.Parse(lArgPart(1))
                            Else
                                MsgBox("Non-numeric Latitude: " & lArgPart(1), MsgBoxStyle.OkOnly, "OpenStreetMap URL")
                            End If
                        Case "lon"
                            If IsNumeric(lArgPart(1)) Then
                                lLon = Double.Parse(lArgPart(1))
                            Else
                                MsgBox("Non-numeric Longitude: " & lArgPart(1), MsgBoxStyle.OkOnly, "OpenStreetMap URL")
                            End If
                        Case "zoom"
                            If IsNumeric(lArgPart(1)) Then
                                lZoom = Integer.Parse(lArgPart(1))
                            Else
                                MsgBox("Non-numeric Zoom: " & lArgPart(1), MsgBoxStyle.OkOnly, "OpenStreetMap URL")
                            End If
                    End Select
                End If
            Next
            If lLat <> 0 AndAlso lLon <> 0 AndAlso lZoom >= g_ZoomMin AndAlso lZoom <= g_ZoomMax Then
                pMap.CenterLat = lLat
                pMap.CenterLon = lLon
                pMap.SanitizeCenterLatLon()
                pMap.Zoom = lZoom
            End If
        Else
            MsgBox("Copy an OpenStreetMap Permalink to the clipboard before using this menu", MsgBoxStyle.OkOnly, "OpenStreetMap URL")
        End If
    End Sub

    Private Sub OpenCellIDToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OpenCellIDToolStripMenuItem.Click
        If pOpenCellIDForm IsNot Nothing Then
            Try
                pOpenCellIDForm.Close()
            Catch ex As Exception
            End Try
        End If
        pOpenCellIDForm = New frmOpenCellID
        pOpenCellIDForm.Icon = Me.Icon
        pOpenCellIDForm.AskUser(pMap, pMap.Downloader)
    End Sub

    Private Sub pCoordinatesForm_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles pCoordinatesForm.FormClosing
        pCoordinatesForm = Nothing
    End Sub

    Private Sub WaypointsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles WaypointsToolStripMenuItem.Click
        If pWaypointsListForm IsNot Nothing Then
            Try
                pWaypointsListForm.Close()
            Catch
            End Try
        End If
        pWaypointsListForm = New frmWaypoints
        pWaypointsListForm.Icon = Me.Icon

        Dim lAllWaypoints As New Generic.List(Of clsGPXwaypoint)
        For Each lLayer As clsLayer In pMap.Layers
            Try
                Dim lGpx As clsLayerGPX = lLayer
                lAllWaypoints.AddRange(lGpx.GPX.wpt)
            Catch ex As Exception

            End Try
        Next
        pWaypointsListForm.AskUser(lAllWaypoints)
    End Sub

    Private Sub pLayersForm_ZoomTo(ByVal aBounds As clsGPXbounds) Handles pLayersForm.ZoomTo
        pMap.PanTo(aBounds)
        pMap.NeedRedraw()
    End Sub

    Private Sub pMap_Zoomed() Handles pMap.Zoomed
        ZoomToolStripMenuItem.Text = "Zoom " & pMap.Zoom
        For Each lItem As ToolStripMenuItem In ZoomToolStripMenuItem.DropDownItems
            If IsNumeric(lItem.Text) Then
                lItem.Checked = (lItem.Text = pMap.Zoom)
            End If
        Next
    End Sub

    Private Sub TimeZoneToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TimeZoneToolStripMenuItem.Click
        If pTimeZoneForm IsNot Nothing Then
            Try
                pTimeZoneForm.Close()
            Catch
            End Try
        End If
        pTimeZoneForm = New frmTimeZone
        pTimeZoneForm.Icon = Me.Icon
        pTimeZoneForm.numHours.Value = pMap.ImportOffsetFromUTC.Hours
        pTimeZoneForm.numMinutes.Value = pMap.ImportOffsetFromUTC.Minutes
        pTimeZoneForm.Show()
    End Sub

    Private Sub pTimeZoneForm_Changed(ByVal aUTCoffset As System.TimeSpan) Handles pTimeZoneForm.Changed
        pMap.ImportOffsetFromUTC = aUTCoffset
    End Sub
End Class