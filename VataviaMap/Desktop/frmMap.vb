Public Class frmMap

    WithEvents pLayersForm As frmLayers
    WithEvents pBuddyAlarmForm As frmBuddyAlarm
    WithEvents pBuddyListForm As frmBuddyList
    WithEvents pWaypointsListForm As frmWaypoints
    WithEvents pCoordinatesForm As frmCoordinates
    WithEvents pTileServerForm As frmServer
    WithEvents pTimeSpanForm As frmTimeSpan
    WithEvents pOpenCellIDForm As frmOpenCellID
    WithEvents pTimeZoneForm As frmTimeZone

    Private pLastKeyDown As Integer = 0
    Private Const pCopyPrefix As String = "Copy "

    Delegate Sub CheckZoomCallback()
    Private pCheckZoomCallback As New CheckZoomCallback(AddressOf CheckZoom)

    Public Sub New()
        Application.CurrentCulture = New Globalization.CultureInfo("")
        InitializeComponent()

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

        pMap.SharedNew(lTileCacheFolder)

        BuildWebsiteMenu()
        BuildTileServerMenu()
        BuildBookmarksMenu()

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
            .DefaultExt = pMap.TileServer.FileExtension
            .InitialDirectory = My.Computer.FileSystem.SpecialDirectories.MyDocuments
            .FileName = "OpenStreetMap-" & Format(Now, "yyyy-MM-dd") & "at" & Format(Now, "HH-mm-ss") & pMap.TileServer.FileExtension
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
        pMap.TileServerName = lItemClicked.Text
    End Sub

    Private Sub TransparentServer_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim lItemClicked As ToolStripMenuItem = sender
        lItemClicked.Checked = Not lItemClicked.Checked
        pMap.EnableTransparentTileServer(lItemClicked.Text, lItemClicked.Checked)
    End Sub

    Private Sub NewTileServerForm()
        If pTileServerForm IsNot Nothing Then
            Try
                pTileServerForm.Close()
            Catch
            End Try
        End If
        pTileServerForm = New frmServer
        pTileServerForm.Icon = Me.Icon
    End Sub

    Private Sub AddTileServerMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AddTileServerMenuItem.Click
        NewTileServerForm()
        pTileServerForm.AskUser("Add New Tile Server", New clsServer, False, g_TileServerExampleLabel, g_TileServerExampleFile)
    End Sub

    Private Sub EditTileServerMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        NewTileServerForm()
        pTileServerForm.AskUser("Edit Tile Server", pMap.Servers.Item(CType(sender, ToolStripMenuItem).Text), True, g_TileServerExampleLabel, g_TileServerExampleFile)
    End Sub

    Private Sub pTileServerForm_Add(ByVal aServer As clsServer) Handles pTileServerForm.Add
        'If aURL.Length > 0 AndAlso Not aURL.EndsWith("/") Then aURL &= "/"
        pMap.Servers.Add(aServer.Name, aServer)
        BuildWebsiteMenu()
        BuildTileServerMenu()
        pMap.NeedRedraw()
    End Sub

    Private Sub pTileServerForm_Change(ByVal aOriginalName As String, ByVal aServer As clsServer) Handles pTileServerForm.Change
        pMap.Servers.Remove(aOriginalName)
        pTileServerForm_Add(aServer)
    End Sub

    Private Sub pTileServerForm_Remove(ByVal aOriginalName As String) Handles pTileServerForm.Remove
        pMap.Servers.Remove(aOriginalName)
        If pMap.TileServer.Name = aOriginalName AndAlso pMap.Servers.Count > 0 Then
            'If we just removed the current tile server, set to the first one left
            For Each lName As String In pMap.Servers.Keys
                pMap.TileServerName = lName
                Exit For
            Next
            pMap.NeedRedraw()
        End If
        BuildWebsiteMenu()
        BuildTileServerMenu()
    End Sub

    Private Sub BuildTileServerMenu()
        TileServerToolStripMenuItem.DropDownItems.Clear()
        OverlayServerToolStripMenuItem.DropDownItems.Clear()
        EditTileServerMenuItem.DropDownItems.Clear()
        For Each lServer As clsServer In pMap.Servers.Values
            If Not String.IsNullOrEmpty(lServer.TilePattern) Then
                Dim lNewItem As New ToolStripMenuItem(lServer.Name)
                If lServer.Transparent Then
                    OverlayServerToolStripMenuItem.DropDownItems.Add(lNewItem)
                    AddHandler lNewItem.Click, AddressOf TransparentServer_Click
                Else
                    TileServerToolStripMenuItem.DropDownItems.Add(lNewItem)
                    AddHandler lNewItem.Click, AddressOf TileServer_Click
                    If lServer.Name = pMap.TileServer.Name Then lNewItem.Checked = True
                End If

                Dim lNewEditItem As New ToolStripMenuItem(lServer.Name)
                AddHandler lNewEditItem.Click, AddressOf EditTileServerMenuItem_Click
                EditTileServerMenuItem.DropDownItems.Add(lNewEditItem)
            End If
        Next
        TileServerToolStripMenuItem.DropDownItems.Add(AddTileServerMenuItem)
        TileServerToolStripMenuItem.DropDownItems.Add(EditTileServerMenuItem)
        TileServerToolStripMenuItem.DropDownItems.Add(DefaultsTileServerMenuItem)
    End Sub

    Private Sub BuildWebsiteMenu()
        Dim lWebsiteEventHandler As New EventHandler(AddressOf Website_Click)
        WebsiteToolStripMenuItem.DropDownItems.Clear()
        For Each lServer As clsServer In pMap.Servers.Values
            If Not String.IsNullOrEmpty(lServer.WebmapPattern) Then
                Dim lItem As New ToolStripMenuItem(lServer.Name, Nothing, lWebsiteEventHandler)
                Me.WebsiteToolStripMenuItem.DropDownItems.Add(lItem)
                lItem.DropDownItems.Add(New ToolStripMenuItem(pCopyPrefix & lServer.Name, Nothing, lWebsiteEventHandler))
            End If
        Next
    End Sub

    Private Sub DefaultsTileServerMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles DefaultsTileServerMenuItem.Click
        pMap.SetDefaultTileServers()
        BuildWebsiteMenu()
        BuildTileServerMenu()
    End Sub

    'Private Sub OverlayMaplintToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OverlayMaplintToolStripMenuItem.Click
    '    'TODO:
    '    'pMap.g_TileServerTransparentURL = "http://tah.openstreetmap.org/Tiles/maplint/"
    '    'OverlayMaplintToolStripMenuItem.Checked = Not OverlayMaplintToolStripMenuItem.Checked
    '    'If OverlayMaplintToolStripMenuItem.Checked Then
    '    '    OverlayYahooLabelsToolStripMenuItem.Checked = False
    '    '    pShowTransparentTiles = True
    '    'Else
    '    '    pShowTransparentTiles = OverlayYahooLabelsToolStripMenuItem.Checked
    '    'End If
    '    'Redraw()
    'End Sub

    'Private Sub OverlayYahooLabelsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OverlayYahooLabelsToolStripMenuItem.Click
    '    ''TODO: manage transparent tile server like main tile server, add Transparent field to server type
    '    ''pMap.g_TileServerTransparentURL = "..."
    '    'OverlayYahooLabelsToolStripMenuItem.Checked = Not OverlayYahooLabelsToolStripMenuItem.Checked
    '    'If OverlayYahooLabelsToolStripMenuItem.Checked Then
    '    '    OverlayMaplintToolStripMenuItem.Checked = False
    '    '    pShowTransparentTiles = True
    '    'Else
    '    '    pShowTransparentTiles = OverlayMaplintToolStripMenuItem.Checked
    '    'End If
    '    'Redraw()
    'End Sub

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

    Private Sub Magnify_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Magnify1.Click, Magnify2.Click, Magnify4.Click, Magnify8.Click
        Static MagnifyItems() As ToolStripMenuItem = {Magnify1, Magnify2, Magnify4, Magnify8}
        Dim lItem As ToolStripMenuItem = sender
        For Each lOtherItem As ToolStripMenuItem In MagnifyItems
            lOtherItem.Checked = False
        Next
        lItem.Checked = True
        MagnifyToolStripMenuItem.Text = lItem.Text
        pMap.Magnify = lItem.Name.Substring(7)
    End Sub

    Private Sub UseMarkedTilesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles UseMarkedTilesToolStripMenuItem.Click
        'TODO:
        'pMap.UseMarkedTiles = Not pMap.UseMarkedTiles
        'UseMarkedTilesToolStripMenuItem.Checked = pMap.UseMarkedTiles
        'pMap.Redraw()
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

        Dim lActualFilename As String = lJosmFilename
        If Not IO.File.Exists(lJosmFilename) AndAlso MsgBox("Download latest version of JOSM now?", MsgBoxStyle.YesNo, "Download JOSM") = MsgBoxResult.Yes Then
            Me.Cursor = Cursors.WaitCursor
            Dim lSaveTitle As String = Me.Text
            Me.Text = "Downloading http://josm.openstreetmap.de/josm-tested.jar ..."
            Application.DoEvents()
            If Not pMap.Downloader.DownloadFile(Nothing, "http://josm.openstreetmap.de/josm-tested.jar", lJosmFilename, lActualFilename) Then
                MsgBox("Please manually download JOSM from" & vbCrLf & "http://josm.openstreetmap.de/" & vbCrLf & "and save as " & vbCrLf & lJosmFilename, MsgBoxStyle.OkOnly, "Unable to download JOSM")
            End If
            Me.Text = lSaveTitle
            Me.Cursor = Cursors.Default
        End If

        If IO.File.Exists(lActualFilename) Then
            Dim lArguments As String = ""
            For Each lLayer As clsLayer In pMap.Layers
                lArguments &= """" & lLayer.Filename & """ "
            Next
            Process.Start(lActualFilename, lArguments)
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
        pCoordinatesForm.Show(pMap)
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

    Private Sub pBuddyListForm_Center(ByVal aLatitude As Double, ByVal aLongitude As Double, ByVal aZoom As Integer) Handles pBuddyListForm.Center, pWaypointsListForm.Center
        pMap.CenterLat = aLatitude
        pMap.CenterLon = aLongitude
        pMap.SanitizeCenterLatLon()
        If aZoom <> pMap.Zoom AndAlso aZoom >= pMap.TileServer.ZoomMin AndAlso aZoom <= pMap.TileServer.ZoomMax Then
            pMap.Zoom = aZoom
        Else
            pMap.NeedRedraw()
        End If
    End Sub

    Private Sub pWaypointsListForm_SelectionChanged() Handles pWaypointsListForm.SelectionChanged
        pMap.NeedRedraw()
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
        OpenFileOrURL("http://code.google.com/p/vataviamap/", False)
    End Sub

    Private Sub GetOSMBugsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles GetOSMBugsToolStripMenuItem.Click
        Dim lBugsGPXurl As String = "http://openstreetbugs.schokokeks.org/api/0.1/getGPX?" _
            & "b=" & pMap.LatMin.ToString("#.#######") _
            & "&t=" & pMap.LatMax.ToString("#.#######") _
            & "&l=" & pMap.LonMin.ToString("#.#######") _
            & "&r=" & pMap.LonMax.ToString("#.#######") _
            & "&open=yes"
        Dim lBugsGPXfilename As String = IO.Path.Combine(My.Computer.FileSystem.SpecialDirectories.MyDocuments, "bugs.gpx")
        Dim lActualFilename As String = Nothing
        If pMap.Downloader.DownloadFile(Nothing, lBugsGPXurl, lBugsGPXfilename, lActualFilename) Then
            pMap.OpenFile(lActualFilename)
            'TODO: set .LabelField = "desc"
        End If
    End Sub

    Private Sub FollowWebMapURLToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles FollowWebMapURLToolStripMenuItem.Click
        pMap.FollowWebsiteURL(Clipboard.GetText(Windows.Forms.TextDataFormat.Text))
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

    Private Function FindAllWaypoints() As Generic.List(Of clsGPXwaypoint)
        Dim lAllWaypoints As New Generic.List(Of clsGPXwaypoint)
        For Each lLayer As clsLayer In pMap.Layers
            Dim lGpxLayer As clsLayerGPX = TryCast(lLayer, clsLayerGPX)
            If lGpxLayer IsNot Nothing AndAlso lGpxLayer.GPX.wpt IsNot Nothing Then
                lAllWaypoints.AddRange(lGpxLayer.GPX.wpt)
            End If
        Next
        Return lAllWaypoints
    End Function

    Private Sub WaypointsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles WaypointsToolStripMenuItem.Click
        If pWaypointsListForm IsNot Nothing Then
            Try
                pWaypointsListForm.Close()
            Catch
            End Try
        End If
        pWaypointsListForm = New frmWaypoints
        pWaypointsListForm.Icon = Me.Icon

        pWaypointsListForm.AskUser(FindAllWaypoints)
    End Sub

    Private Sub pWaypointsListForm_Ok() Handles pWaypointsListForm.Ok
        pWaypointsListForm = Nothing
        pMap.NeedRedraw()
    End Sub

    Private Sub pLayersForm_ZoomTo(ByVal aBounds As clsGPXbounds) Handles pLayersForm.ZoomTo
        pMap.ZoomTo(aBounds)
    End Sub

    Private Sub pMap_ClosedLayer() Handles pMap.ClosedLayer
        If pLayersForm IsNot Nothing Then
            pLayersForm.PopulateList(pMap.Layers)
        End If
        If pWaypointsListForm IsNot Nothing Then
            pWaypointsListForm.AskUser(FindAllWaypoints)
        End If
    End Sub

    Private Sub pMap_OpenedLayer(ByVal aLayer As clsLayer) Handles pMap.OpenedLayer
        If pLayersForm IsNot Nothing Then
            pLayersForm.PopulateList(pMap.Layers)
        End If
        If pWaypointsListForm IsNot Nothing Then
            Dim lGpxLayer As clsLayerGPX = TryCast(aLayer, clsLayerGPX)
            If lGpxLayer IsNot Nothing AndAlso lGpxLayer.GPX IsNot Nothing AndAlso lGpxLayer.GPX.wpt IsNot Nothing AndAlso lGpxLayer.GPX.wpt.Count > 0 Then
                pWaypointsListForm.AskUser(FindAllWaypoints)
            End If
        End If
    End Sub

    Private Sub pMap_Panned() Handles pMap.Panned
        If pCoordinatesForm IsNot Nothing Then pCoordinatesForm.Show(pMap)
    End Sub

    Private Sub pMap_StatusChanged(ByVal aStatusMessage As String) Handles pMap.StatusChanged
        If String.IsNullOrEmpty(aStatusMessage) Then
            Me.Text = pMap.Text
        Else
            Me.Text = aStatusMessage
        End If
    End Sub

    Private Sub pMap_TileServerChanged() Handles pMap.TileServerChanged
        For Each lItem As ToolStripMenuItem In TileServerToolStripMenuItem.DropDownItems
            If lItem.Text = pMap.TileServer.Name Then
                lItem.Checked = True
            ElseIf lItem.Equals(AddTileServerMenuItem) Then
                Exit For
            Else
                lItem.Checked = False
            End If
        Next

        For Each lItem As ToolStripMenuItem In OverlayServerToolStripMenuItem.DropDownItems
            lItem.Checked = pMap.TransparentTileServers.Contains(pMap.Servers.Item(lItem.Text))
        Next

        'Populate Zoom menu for the new tile server
        ZoomToolStripMenuItem.DropDownItems.Clear()
        ZoomToolStripMenuItem.DropDownItems.Add(ZoomAllLayersToolStripMenuItem)
        For lZoomLevel As Integer = pMap.TileServer.ZoomMin To pMap.TileServer.ZoomMax
            Me.ZoomToolStripMenuItem.DropDownItems.Add(New ToolStripMenuItem(CStr(lZoomLevel), Nothing, New EventHandler(AddressOf ZoomToolStripMenuItem_Click)))
        Next
        CheckZoom()

        pMap.NeedRedraw()
        pMap_StatusChanged(Nothing)
    End Sub

    Private Sub pMap_Zoomed() Handles pMap.Zoomed
        'If running in another thread, can't call directly
        If Me.InvokeRequired Then
            Me.Invoke(pCheckZoomCallback)
        Else
            CheckZoom()
        End If
    End Sub

    Private Sub CheckZoom()
        ZoomToolStripMenuItem.Text = "Zoom " & pMap.Zoom
        For Each lItem As ToolStripMenuItem In ZoomToolStripMenuItem.DropDownItems
            If IsNumeric(lItem.Text) Then
                lItem.Checked = (lItem.Text = pMap.Zoom)
            End If
        Next
        If pCoordinatesForm IsNot Nothing Then pCoordinatesForm.Show(pMap)
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

    Private Sub TransparencyNone_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TransparencyNone.Click
        ClearTransparencyCheck()
        Dim lItem As ToolStripMenuItem = sender
        lItem.Checked = True
        Me.Opacity = 1
    End Sub

    Private Sub Transparency_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Transparency10.Click, Transparency20.Click, Transparency30.Click, Transparency40.Click, Transparency50.Click, Transparency60.Click, Transparency70.Click, Transparency80.Click, Transparency90.Click
        ClearTransparencyCheck()
        Dim lItem As ToolStripMenuItem = sender
        lItem.Checked = True
        Dim lTransparency As Double = lItem.Name.Substring(12)
        Me.Opacity = (100 - lTransparency) / 100
    End Sub

    Private Sub ClearTransparencyCheck()
        For Each lItem As ToolStripMenuItem In TransparencyToolStripMenuItem.DropDownItems
            lItem.Checked = False
        Next
    End Sub

    Private Sub Website_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Try
            Dim lItem As ToolStripMenuItem = sender
            Dim lServerName As String = lItem.Text
            Dim lOpen As Boolean = True
            If lServerName.StartsWith(pCopyPrefix) Then
                lServerName = lServerName.Substring(pCopyPrefix.Length)
                lOpen = False
            End If
            Dim lServer As clsServer = pMap.Servers(lServerName)
            Dim lURL As String = lServer.BuildWebmapURL(pMap.CenterLat, pMap.CenterLon, pMap.Zoom, pMap.LatMax, pMap.LonMin, pMap.LatMin, pMap.LonMax)
            If lURL IsNot Nothing AndAlso lURL.Length > 0 Then
                pMap.ShowURL = lURL
                If lOpen Then
                    OpenFileOrURL(lURL, False)
                Else
                    Clipboard.SetText(lURL)
                End If
            End If
        Catch ex As System.ComponentModel.Win32Exception
            'This happens when Firefox takes a moment to start due to updates, ignore it
        End Try
    End Sub

#Region "Bookmarks"
    Private Sub BuildBookmarksMenu()
        For lBookmarkIndex As Integer = 0 To 50
            Dim lBookmarkKey As String = "Bookmark" & PlacesToolStripMenuItem.DropDownItems.Count
            Dim lBookmarkSetting As String = GetAppSetting(lBookmarkKey, "")
            If lBookmarkSetting.Length > 0 Then
                Dim lCoordinates() As String = lBookmarkSetting.Split("|")
                If lCoordinates.Length > 3 AndAlso IsNumeric(lCoordinates(0)) AndAlso IsNumeric(lCoordinates(1)) AndAlso IsNumeric(lCoordinates(2)) Then
                    Dim lNewBookmark As New ToolStripMenuItem(lCoordinates(3), Nothing, New EventHandler(AddressOf BookmarkItem_Click))
                    lNewBookmark.Tag = lCoordinates(0) & "|" & lCoordinates(1) & "|" & lCoordinates(2)
                    lNewBookmark.ToolTipText = lCoordinates(0) & ", " & lCoordinates(1) & " Zoom " & lCoordinates(2)
                    PlacesToolStripMenuItem.DropDownItems.Add(lNewBookmark)
                End If
            End If
        Next
    End Sub

    Private Sub BookmarksAddToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PlacesAddToolStripMenuItem.Click
        Dim lBookmarkKey As String = "Bookmark" & PlacesToolStripMenuItem.DropDownItems.Count
        Dim lBookmarkText As String = InputBox("Name of bookmark", "New Bookmark", lBookmarkKey)
        Dim lNewBookmark As New ToolStripMenuItem(lBookmarkText, Nothing, New EventHandler(AddressOf BookmarkItem_Click))
        lNewBookmark.Tag = pMap.CenterLon.ToString("#.#######") & "|" & pMap.CenterLat.ToString("#.#######") & "|" & pMap.Zoom
        lNewBookmark.ToolTipText = pMap.CenterLon.ToString("#.#######") & ", " & pMap.CenterLat.ToString("#.#######") & " Zoom " & pMap.Zoom
        PlacesToolStripMenuItem.DropDownItems.Add(lNewBookmark)
        SaveAppSetting(lBookmarkKey, lNewBookmark.Tag & "|" & lNewBookmark.Text)
    End Sub

    Private Sub BookmarksSaveToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PlacesSaveToolStripMenuItem.Click
        Dim lBookmarksGPX As New clsGPX
        Dim lLat As Double
        Dim lLon As Double
        For Each lBookmark As ToolStripMenuItem In PlacesToolStripMenuItem.DropDownItems
            If lBookmark IsNot Nothing AndAlso Not String.IsNullOrEmpty(lBookmark.Tag) Then
                Dim lCoordinates() As String = lBookmark.Tag.Split("|")
                If lCoordinates.Length > 1 AndAlso _
                   Double.TryParse(lCoordinates(0), lLon) AndAlso _
                   Double.TryParse(lCoordinates(1), lLat) Then
                    Dim lWaypoint As New clsGPXwaypoint("wpt", lLat, lLon)
                    lWaypoint.name = lBookmark.Text
                    If lCoordinates.Length > 2 Then lWaypoint.SetExtension("zoom", CInt(lCoordinates(2)))
                    lBookmarksGPX.wpt.Add(lWaypoint)
                End If
            End If
        Next
        If lBookmarksGPX.wpt.Count > 0 Then
            Dim lSaveDialog As New Windows.Forms.SaveFileDialog
            With lSaveDialog
                .Title = "Save " & lBookmarksGPX.wpt.Count & " Places As..."
                .Filter = "GPX|*.gpx"
                .FilterIndex = 0
                .DefaultExt = ".gpx"
                If .ShowDialog = Windows.Forms.DialogResult.OK Then
                    WriteTextFile(.FileName, lBookmarksGPX.ToString)
                End If
            End With
        End If
    End Sub

    Private Sub BookmarkItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim lBookmark As ToolStripMenuItem = sender
        Dim lCoordinates() As String = lBookmark.Tag.Split("|")
        If lCoordinates.Length > 1 AndAlso _
           Double.TryParse(lCoordinates(0), pMap.CenterLon) AndAlso _
           Double.TryParse(lCoordinates(1), pMap.CenterLat) Then
            pMap.SanitizeCenterLatLon()
            If lCoordinates.Length > 2 AndAlso lCoordinates(2) <> pMap.Zoom Then
                pMap.Zoom = CInt(lCoordinates(2))
            Else
                pMap.NeedRedraw()
            End If
        End If
    End Sub
#End Region 'Bookmarks

    Private Sub LetterPortraitToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LetterPortraitToolStripMenuItem.Click
        Dim lCurAspectRatio As Single = pMap.Width / pMap.Height
        Dim lDesiredMapWidth As Integer = (7.5 / 10) * pMap.Height
        Me.Width += lDesiredMapWidth - pMap.Width
    End Sub

    Private Sub LetterLandscapeToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LetterLandscapeToolStripMenuItem.Click
        Dim lCurAspectRatio As Single = pMap.Width / pMap.Height
        Dim lDesiredMapWidth As Integer = (10 / 7.5) * pMap.Height
        Me.Width += lDesiredMapWidth - pMap.Width
    End Sub

    Private Sub TimeSpanToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TimeSpanToolStripMenuItem.Click
        If pTimeSpanForm IsNot Nothing Then
            Try
                pTimeSpanForm.Close()
            Catch
            End Try
        End If
        pTimeSpanForm = New frmTimeSpan
        pTimeSpanForm.Icon = Me.Icon
        pTimeSpanForm.PopulateList(pMap.Layers)
        pTimeSpanForm.Show()
    End Sub

    Private Sub pTimeSpanForm_Changed(ByVal aUTCoffset As System.TimeSpan) Handles pTimeSpanForm.Changed

    End Sub

    Private Sub RefreshAllToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RefreshAllToolStripMenuItem.Click
        pMap.ReDownloadAllVisibleTiles()
    End Sub

    Private Sub OpenTileCacheFolderToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OpenTileCacheFolderToolStripMenuItem.Click
        OpenFileOrURL(pMap.TileCacheFolder, False)
    End Sub
End Class