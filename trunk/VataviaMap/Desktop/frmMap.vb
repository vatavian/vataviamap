Public Class frmMap

    WithEvents pLayersForm As frmLayers
    WithEvents pBuddyAlarmForm As frmBuddyAlarm
    WithEvents pBuddyListForm As frmBuddyList
    WithEvents pTileServerForm As frmEditNameURL

    Public Sub New()
        InitializeComponent()

        'Create zoom level menu items
        For lZoomLevel As Integer = g_ZoomMin To g_ZoomMax
            Me.ZoomToolStripMenuItem.DropDownItems.Add(New ToolStripMenuItem(CStr(lZoomLevel), Nothing, New EventHandler(AddressOf ZoomToolStripMenuItem_Click)))
        Next

        pTileCacheFolder = GetAppSetting("TileCacheFolder", IO.Path.GetTempPath() & "tiles" & g_PathChar)

        If Not IO.Directory.Exists(pTileCacheFolder) Then
            Dim lDialog As New FolderBrowserDialog
            With lDialog
                .Description = "Select folder to save cached tiles in"
                .ShowNewFolderButton = True
                If .ShowDialog = Windows.Forms.DialogResult.OK Then
                    pTileCacheFolder = .SelectedPath
                Else 'There is no hope if we cannot cache tiles
                    pTileCacheFolder = ""
                End If
            End With
        End If
        If pTileCacheFolder.Length > 0 AndAlso Not pTileCacheFolder.EndsWith(g_PathChar) Then
            pTileCacheFolder &= g_PathChar
        End If

        SharedNew()

        BuildTileServerMenu()

        PanToGPXToolStripMenuItem.Checked = pGPXPanTo
        ZoomToGPXToolStripMenuItem.Checked = pGPXZoomTo
        TileImagesToolStripMenuItem.Checked = pShowTileImages
        TileOutlinesToolStripMenuItem.Checked = pShowTileOutlines
        TileNamesToolStripMenuItem.Checked = pShowTileNames

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

        For Each lArg As String In My.Application.CommandLineArgs
            If IO.File.Exists(lArg) AndAlso lArg.ToLower.EndsWith(".gpx") Then
                OpenGPX(lArg)
            End If
        Next
    End Sub

    Private Sub Redraw()
        If pFormVisible Then
            Dim lGraphics As Graphics = GetBitmapGraphics()
            If lGraphics IsNot Nothing Then
                DrawTiles(lGraphics)

                If pShowTransparentTiles Then
                    Dim lSaveURL As String = g_TileServerURL
                    Dim lSaveCacheDir As String = g_TileCacheFolder
                    g_TileServerURL = g_TileServerTransparentURL
                    SetCacheFolderFromTileServer()
                    DrawTiles(lGraphics)
                    g_TileServerURL = lSaveURL
                    g_TileCacheFolder = lSaveCacheDir
                End If

                ReleaseBitmapGraphics()
                Refresh()
            End If
            Application.DoEvents()
        End If
    End Sub

    ''' <summary>
    ''' Zoom level has been set, update Zoom menu to show current Zoom
    ''' </summary>
    Private Sub Zoomed()
        ZoomToolStripMenuItem.Text = "Zoom " & pZoom
        For Each lItem As ToolStripMenuItem In ZoomToolStripMenuItem.DropDownItems
            If IsNumeric(lItem.Text) Then
                lItem.Checked = (lItem.Text = pZoom)
            End If
        Next
        Redraw()
    End Sub

    Private Sub DrewTiles(ByVal g As Graphics, ByVal aTopLeft As Point, ByVal aOffsetToCenter As Point)
        If pBuddyAlarmEnabled OrElse pBuddyAlarmForm IsNot Nothing Then
            Dim lWaypoint As New clsGPXwaypoint("wpt", pBuddyAlarmLat, pBuddyAlarmLon)
            lWaypoint.sym = "circle"
            Dim pBuddyAlarmLayer As clsLayerGPX = New clsLayerGPX("cursor", Me)
            pBuddyAlarmLayer.SymbolPen = New Pen(Color.Red)
            pBuddyAlarmLayer.SymbolSize = pBuddyAlarmMeters / MetersPerPixel(pZoom)
            pBuddyAlarmLayer.DrawTrackpoint(g, lWaypoint, aTopLeft, aOffsetToCenter)
        End If
    End Sub

    Private Sub ManuallyNavigated()
        pGPSFollow = 0 'Manual navigation overrides automatic following of GPS
        SanitizeCenterLatLon()
        Redraw()
    End Sub

    Private Sub frmMap_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles Me.DragDrop
        Me.Activate()
        If e.Data.GetDataPresent(Windows.Forms.DataFormats.FileDrop) Then
            OpenGPXs(e.Data.GetData(Windows.Forms.DataFormats.FileDrop))
        End If
    End Sub

    Private Sub frmMap_DragEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles Me.DragEnter
        If e.Data.GetDataPresent(Windows.Forms.DataFormats.FileDrop) Then
            e.Effect = Windows.Forms.DragDropEffects.All
        End If
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
            Case Keys.A, Keys.OemMinus : Zoom -= 1 : lNeedRedraw = False
            Case Keys.Q, Keys.Oemplus : Zoom += 1 : lNeedRedraw = False
            Case Else
                e.Handled = False
                Exit Sub
        End Select
        If lNeedRedraw Then
            SanitizeCenterLatLon()
            Redraw()
        End If
    End Sub

    Private Sub frmMap_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseDown
        If pBuddyAlarmForm IsNot Nothing Then
            pBuddyAlarmLat = CenterLat - (e.Y - ClientRectangle.Height / 2) * LatHeight / ClientRectangle.Height
            pBuddyAlarmLon = CenterLon + (e.X - ClientRectangle.Width / 2) * LonWidth / ClientRectangle.Width
            pBuddyAlarmForm.AskUser(pBuddyAlarmLat, pBuddyAlarmLon)
            MouseDownLeft(e)
        Else
            Select Case e.Button
                Case Windows.Forms.MouseButtons.Left
                    MouseDownLeft(e)
                Case Windows.Forms.MouseButtons.Right
                    pClickedTileFilename = FindTileFilename(pBitmap.GetBounds(Drawing.GraphicsUnit.Pixel), e.X, e.Y)
                    If pClickedTileFilename.Length > 0 Then
                        Me.RightClickMenu.Show(Me, e.Location)
                    End If
            End Select
        End If
    End Sub

    Private Sub frmMap_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseMove
        If pMouseDragging Then
            If pBuddyAlarmForm IsNot Nothing Then
                pBuddyAlarmMeters = MetersBetweenLatLon(pBuddyAlarmLat, pBuddyAlarmLon, _
                                                        pBuddyAlarmLat + (e.Y - pMouseDragStartLocation.Y) * LatHeight / Me.ClientRectangle.Height, _
                                                        pBuddyAlarmLon - (e.X - pMouseDragStartLocation.X) * LonWidth / Me.ClientRectangle.Width)
                pBuddyAlarmForm.SetDistance(pBuddyAlarmMeters)
            Else
                pGPSFollow = 0
                CenterLat = pMouseDownLat + (e.Y - pMouseDragStartLocation.Y) * LatHeight / Me.ClientRectangle.Height
                CenterLon = pMouseDownLon - (e.X - pMouseDragStartLocation.X) * LonWidth / Me.ClientRectangle.Width
                SanitizeCenterLatLon()
            End If
            Redraw()
        End If
    End Sub

    Private Sub frmMap_MouseWheel(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseWheel
        If pMouseWheelZoom Then
            If e.Delta > 0 Then
                Zoom += 1
            ElseIf e.Delta < 0 Then
                Zoom -= 1
            End If
        ElseIf pMouseWheelTileServer Then
            For lItemIndex As Integer = 0 To TileServerToolStripMenuItem.DropDownItems.Count - 2
                Dim lItem As ToolStripMenuItem = TileServerToolStripMenuItem.DropDownItems(lItemIndex)
                If lItem.Checked Then
                    If e.Delta > 0 Then
                        lItemIndex += 1
                    ElseIf e.Delta < 0 Then
                        lItemIndex -= 1
                    End If
                    If lItemIndex < 0 Then
                        lItemIndex = pTileServers.Keys.Count - 1
                    ElseIf lItemIndex >= pTileServers.Keys.Count Then
                        lItemIndex = 0
                    End If
                    TileServer_Click(TileServerToolStripMenuItem.DropDownItems(lItemIndex), e)
                    Exit Sub
                End If
            Next
        End If
    End Sub

    Private Sub frmMap_Shown(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Shown
        Redraw()
    End Sub

    Private Sub TileOutlinesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TileOutlinesToolStripMenuItem.Click
        TileOutlinesToolStripMenuItem.Checked = Not TileOutlinesToolStripMenuItem.Checked
        pShowTileOutlines = TileOutlinesToolStripMenuItem.Checked
        Redraw()
    End Sub

    Private Sub TileNamesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TileNamesToolStripMenuItem.Click
        TileNamesToolStripMenuItem.Checked = Not TileNamesToolStripMenuItem.Checked
        pShowTileNames = TileNamesToolStripMenuItem.Checked
        Redraw()
    End Sub

    Private Sub TileImagesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TileImagesToolStripMenuItem.Click
        TileImagesToolStripMenuItem.Checked = Not TileImagesToolStripMenuItem.Checked
        pShowTileImages = TileImagesToolStripMenuItem.Checked
        Redraw()
    End Sub

    Private Sub RefreshFromServerToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RefreshFromServerToolStripMenuItem.Click
        If pClickedTileFilename.Length > 0 Then
            Try
                Debug.WriteLine("Refreshing  '" & pClickedTileFilename & "'")
                pDownloader.DeleteTile(pClickedTileFilename)
                Redraw()
            Catch ex As Exception
                MsgBox("Could not refresh '" & pClickedTileFilename & "'" & vbCrLf & ex.Message, MsgBoxStyle.Critical, "Error Refreshing Tile")
            End Try
            pClickedTileFilename = ""
        End If
    End Sub

    Private Sub GetAllDescendantsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles GetAllDescendantsToolStripMenuItem.Click
        If pClickedTileFilename.Length > 0 Then
            Me.UseWaitCursor = True
            Try
                Debug.WriteLine("Getting descendants of  '" & pClickedTileFilename & "'")
                Dim lParentY As Integer = IO.Path.GetFileNameWithoutExtension(pClickedTileFilename)
                Dim lParentX As Integer = IO.Path.GetFileName(IO.Path.GetDirectoryName(pClickedTileFilename))
                pDownloader.DownloadDescendants(New Point(lParentX, lParentY), pZoom, pOldestCache)
            Catch ex As Exception
                MsgBox("'" & pClickedTileFilename & "'" & vbCrLf & ex.Message, MsgBoxStyle.Critical, "Error Getting Descendant Tiles")
            End Try
            pClickedTileFilename = ""
            Me.UseWaitCursor = False
        End If
    End Sub

    Private Sub SaveAsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SaveAsToolStripMenuItem.Click
        Dim lSaveDialog As New SaveFileDialog
        With lSaveDialog
            .DefaultExt = g_TileExtension
            .InitialDirectory = My.Computer.FileSystem.SpecialDirectories.MyDocuments
            .FileName = "OpenStreetMap" & g_TileExtension
            If .ShowDialog = Windows.Forms.DialogResult.OK Then
                pBitmapMutex.WaitOne()
                pBitmap.Save(.FileName)
                SaveTiles(IO.Path.GetDirectoryName(.FileName) & "\" & IO.Path.GetFileNameWithoutExtension(.FileName) & "\")
                pBitmapMutex.ReleaseMutex()
                'CreateGeoReferenceFile(LatitudeToMeters(pCenterLat - pLatHeight * 1.66), _
                CreateGeoReferenceFile(LatitudeToMeters(CenterLat + LatHeight / 2), _
                                       LongitudeToMeters(CenterLon - LonWidth / 2), _
                                       pZoom, ImageWorldFilename(.FileName))
                IO.File.WriteAllText(IO.Path.ChangeExtension(.FileName, "prj"), "PROJCS[""unnamed"", GEOGCS[""unnamed ellipse"", DATUM[""unknown"", SPHEROID[""unnamed"",6378137,0]], PRIMEM[""Greenwich"",0], UNIT[""degree"",0.0174532925199433]], PROJECTION[""Mercator_2SP""], PARAMETER[""standard_parallel_1"",0], PARAMETER[""central_meridian"",0], PARAMETER[""false_easting"",0], PARAMETER[""false_northing"",0], UNIT[""Meter"",1], EXTENSION[""PROJ4"",""+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +k=1.0 +units=m +nadgrids=@null +wktext +no_defs""]]")
                'IO.File.WriteAllText(IO.Path.ChangeExtension(.FileName, "prj"), "PROJCS[""Mercator"",GEOGCS[""unnamed ellipse"",DATUM[""D_unknown"",SPHEROID[""Unknown"",6371000,0]],PRIMEM[""Greenwich"",0],UNIT[""Degree"",0.017453292519943295]],PROJECTION[""Mercator""],PARAMETER[""standard_parallel_1"",0],PARAMETER[""central_meridian"",0],PARAMETER[""scale_factor"",1],PARAMETER[""false_easting"",0],PARAMETER[""false_northing"",0],UNIT[""Meter"",1]]")
                'IO.File.WriteAllText(IO.Path.ChangeExtension(.FileName, "prj2"), "PROJCS[""Mercator Spheric"", GEOGCS[""WGS84basedSpheric_GCS"", DATUM[""WGS84basedSpheric_Datum"", SPHEROID[""WGS84based_Sphere"", 6378137, 0], TOWGS84[0, 0, 0, 0, 0, 0, 0]], PRIMEM[""Greenwich"", 0, AUTHORITY[""EPSG"", ""8901""]], UNIT[""degree"", 0.0174532925199433, AUTHORITY[""EPSG"", ""9102""]], AXIS[""E"", EAST], AXIS[""N"", NORTH]], PROJECTION[""Mercator""], PARAMETER[""False_Easting"", 0], PARAMETER[""False_Northing"", 0], PARAMETER[""Central_Meridian"", 0], PARAMETER[""Latitude_of_origin"", 0], UNIT[""metre"", 1, AUTHORITY[""EPSG"", ""9001""]], AXIS[""East"", EAST], AXIS[""North"", NORTH]]")
                'IO.File.WriteAllText(IO.Path.ChangeExtension(.FileName, "prj3"), "PROJCS[""WGS84 / Simple Mercator"",GEOGCS[""WGS 84"",DATUM[""WGS_1984"",SPHEROID[""WGS_1984"", 6378137.0, 298.257223563]],PRIMEM[""Greenwich"", 0.0],UNIT[""degree"", 0.017453292519943295],AXIS[""Longitude"", EAST],AXIS[""Latitude"", NORTH]],PROJECTION[""Mercator_1SP_Google""],PARAMETER[""latitude_of_origin"", 0.0],PARAMETER[""central_meridian"", 0.0],PARAMETER[""scale_factor"", 1.0],PARAMETER[""false_easting"", 0.0],PARAMETER[""false_northing"", 0.0],UNIT[""m"", 1.0],AXIS[""x"", EAST],AXIS[""y"", NORTH],AUTHORITY[""EPSG"",""900913""]]")
                'IO.File.WriteAllText(IO.Path.ChangeExtension(.FileName, "prj4"), "PROJCS[""Google Mercator"",GEOGCS[""WGS 84"",DATUM[""WGS_1984"",SPHEROID[""WGS84"",6378137,298.2572235630016,AUTHORITY[""EPSG"",""7030""]],AUTHORITY[""EPSG"",""6326""]],PRIMEM[""Greenwich"",0],UNIT[""degree"",0.0174532925199433],AUTHORITY[""EPSG"",""4326""]],PROJECTION[""Mercator_1SP""],PARAMETER[""central_meridian"",0],PARAMETER[""scale_factor"",1],PARAMETER[""false_easting"",0],PARAMETER[""false_northing"",0],UNIT[""metre"",1,AUTHORITY[""EPSG"",""9001""]]]")

                '+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +k=1.0 +units=m +nadgrids=@null +no_defs
                '+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +k=1.0 +units=m +nadgrids=@null +no_defs +over

            End If
        End With
    End Sub

    Private Function ImageWorldFilename(ByVal aImageFilename As String) As String
        Dim lImageExt As String = IO.Path.GetExtension(aImageFilename)
        If lImageExt.Length > 1 Then
            lImageExt = lImageExt.Substring(1, 1) & lImageExt.Substring(lImageExt.Length - 1, 1) & "w"
        End If
        Return IO.Path.ChangeExtension(aImageFilename, lImageExt)
    End Function

    ''' <summary>
    ''' Save all the visible tiles
    ''' </summary>
    Private Sub SaveTiles(ByVal aSaveIn As String)
        If IO.Directory.Exists(pTileCacheFolder) Then
            Dim lOffsetToCenter As Point
            Dim lTopLeft As Point
            Dim lBotRight As Point

            FindTileBounds(pBitmap.GetBounds(Drawing.GraphicsUnit.Pixel), lOffsetToCenter, lTopLeft, lBotRight)

            Dim lTilePoint As Point

            IO.Directory.CreateDirectory(aSaveIn)
            'Loop through each visible tile
            For x As Integer = lTopLeft.X To lBotRight.X
                For y As Integer = lTopLeft.Y To lBotRight.Y
                    lTilePoint = New Point(x, y)

                    Dim lTileFileName As String = TileFilename(lTilePoint, pZoom, pUseMarkedTiles)

                    Dim lOriginalTileFileName As String
                    If pUseMarkedTiles Then
                        lOriginalTileFileName = TileFilename(lTilePoint, pZoom, False)
                        ' Go ahead and use unmarked tile if we have it but not a marked version
                        If Not IO.File.Exists(lTileFileName) Then lTileFileName = lOriginalTileFileName
                    Else
                        lOriginalTileFileName = lTileFileName
                    End If

                    SaveTile(lTilePoint, pZoom, lTileFileName, aSaveIn)
                Next
            Next
        End If
    End Sub

    ''' <summary>
    ''' Save a tile with projection information
    ''' </summary>
    ''' <param name="aTileFilename">cache file name of tile to draw</param>
    ''' <param name="aSaveIn">Folder to save in</param>
    Private Sub SaveTile(ByVal aTilePoint As Point, _
                         ByVal aZoom As Integer, _
                         ByVal aTileFilename As String, _
                         ByVal aSaveIn As String)
        Dim lSaveAs As String = IO.Path.Combine(aSaveIn, aTileFilename.Substring(g_TileCacheFolder.Length).Replace("\", "_"))
        Dim lTileImage As Bitmap = TileImage(aTileFilename)
        If lTileImage IsNot Nothing Then
            lTileImage.Save(lSaveAs)
            Dim lNorth As Double, lWest As Double, lSouth As Double, lEast As Double
            CalcLatLonFromTileXY(aTilePoint, aZoom, lNorth, lWest, lSouth, lEast)
            CreateGeoReferenceFile(LatitudeToMeters(lNorth), _
                                   LongitudeToMeters(lWest), _
                                   aZoom, IO.Path.ChangeExtension(lSaveAs, "pgw"))
            IO.File.WriteAllText(IO.Path.ChangeExtension(lSaveAs, "prj"), "PROJCS[""WGS84 / Simple Mercator"",GEOGCS[""WGS 84"",DATUM[""WGS_1984"",SPHEROID[""WGS_1984"", 6378137.0, 298.257223563]],PRIMEM[""Greenwich"", 0.0],UNIT[""degree"", 0.017453292519943295],AXIS[""Longitude"", EAST],AXIS[""Latitude"", NORTH]],PROJECTION[""Mercator_1SP_Google""],PARAMETER[""latitude_of_origin"", 0.0],PARAMETER[""central_meridian"", 0.0],PARAMETER[""scale_factor"", 1.0],PARAMETER[""false_easting"", 0.0],PARAMETER[""false_northing"", 0.0],UNIT[""m"", 1.0],AXIS[""x"", EAST],AXIS[""y"", NORTH],AUTHORITY[""EPSG"",""900913""]]")
        End If
    End Sub

    Private Sub OpenGPXToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AddLayerToolStripMenuItem.Click
        Dim lDialog As New OpenFileDialog
        With lDialog
            .DefaultExt = ".gpx"
            .Filter = "GPX or LOC|*.gpx;*.loc|All Files|*.*"
            .FilterIndex = 0
            .Multiselect = True
            .Title = "Select GPX file(s) to open"
            If .ShowDialog(Me) = Windows.Forms.DialogResult.OK Then
                OpenGPXs(.FileNames)
            End If
        End With
    End Sub

    Private Sub CloseGPXToolStripMenuItem_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles RemoveAllLayersToolStripMenuItem.Click
        CloseAllLayers()
        Redraw()
    End Sub

    Private Sub TileServer_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim lItemClicked As ToolStripMenuItem = sender
        For Each lItem As ToolStripMenuItem In TileServerToolStripMenuItem.DropDownItems
            If lItem.Equals(lItemClicked) Then
                lItem.Checked = True
                TileServerName = lItem.Text
            ElseIf lItem.Equals(AddTileServerMenuItem) Then
                Exit For
            Else
                lItem.Checked = False
            End If
        Next
        Redraw()
    End Sub

    Private Sub NewTileServerForm()
        If pTileServerForm IsNot Nothing Then
            Try
                pTileServerForm.Close()
            Catch
            End Try
        End If
        pTileServerForm = New frmEditNameURL
    End Sub

    Private Sub AddTileServerMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AddTileServerMenuItem.Click
        NewTileServerForm()
        pTileServerForm.Icon = Me.Icon
        pTileServerForm.AskUser("Add New Tile Server", g_TileServerName, g_TileServerURL, g_TileServerExampleLabel, g_TileServerExampleFile)
        'http://opentiles.appspot.com/tile/get/ma/' 12/1242/1512.png
    End Sub

    Private Sub EditTileServerMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim lItemClicked As ToolStripMenuItem = sender
        NewTileServerForm()
        pTileServerForm.Icon = Me.Icon
        pTileServerForm.AskUser("Edit Tile Server", lItemClicked.Text, pTileServers.Item(lItemClicked.Text), g_TileServerExampleLabel, g_TileServerExampleFile)
    End Sub

    Private Sub pTileServerForm_Ok(ByVal aName As String, ByVal aURL As String) Handles pTileServerForm.Ok
        pTileServers.Remove(aName)
        If aURL.Length > 0 Then
            If aURL.Length > 0 AndAlso Not aURL.EndsWith("/") Then aURL &= "/"
            pTileServers.Add(aName, aURL)
            TileServerName = aName
        ElseIf g_TileServerName = aName AndAlso pTileServers.Count > 0 Then
            'If we just removed the current tile server, set to the first one left
            For Each lName As String In pTileServers.Keys
                TileServerName = lName
                Exit For
            Next
        End If
        BuildTileServerMenu()
        Redraw()
    End Sub

    Private Sub BuildTileServerMenu()
        TileServerToolStripMenuItem.DropDownItems.Clear()
        EditTileServerMenuItem.DropDownItems.Clear()
        For Each lName As String In pTileServers.Keys
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
        SetDefaultTileServers()
        BuildTileServerMenu()
    End Sub

    Private Sub OverlayMaplintToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OverlayMaplintToolStripMenuItem.Click
        g_TileServerTransparentURL = "http://tah.openstreetmap.org/Tiles/maplint/"
        OverlayMaplintToolStripMenuItem.Checked = Not OverlayMaplintToolStripMenuItem.Checked
        If OverlayMaplintToolStripMenuItem.Checked Then
            OverlayYahooLabelsToolStripMenuItem.Checked = False
            pShowTransparentTiles = True
        Else
            pShowTransparentTiles = OverlayYahooLabelsToolStripMenuItem.Checked
        End If
        Redraw()
    End Sub

    Private Sub OverlayYahooLabelsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OverlayYahooLabelsToolStripMenuItem.Click
        'TODO: manage transparent tile server like main tile server, add Transparent field to server type
        'g_TileServerTransparentURL = "..."
        OverlayYahooLabelsToolStripMenuItem.Checked = Not OverlayYahooLabelsToolStripMenuItem.Checked
        If OverlayYahooLabelsToolStripMenuItem.Checked Then
            OverlayMaplintToolStripMenuItem.Checked = False
            pShowTransparentTiles = True
        Else
            pShowTransparentTiles = OverlayMaplintToolStripMenuItem.Checked
        End If
        Redraw()
    End Sub

    Private Sub PanToGPXToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PanToGPXToolStripMenuItem.Click
        PanToGPXToolStripMenuItem.Checked = Not PanToGPXToolStripMenuItem.Checked
        pGPXPanTo = PanToGPXToolStripMenuItem.Checked
    End Sub

    Private Sub ZoomToGPXToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ZoomToGPXToolStripMenuItem.Click
        ZoomToGPXToolStripMenuItem.Checked = Not ZoomToGPXToolStripMenuItem.Checked
        pGPXZoomTo = ZoomToGPXToolStripMenuItem.Checked
        If pGPXZoomTo AndAlso Not pGPXPanTo Then 'If they want to zoom, they must want to pan too
            pGPXPanTo = True
            PanToGPXToolStripMenuItem.Checked = True
        End If
    End Sub

    Private Sub ZoomAllLayersToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ZoomAllLayersToolStripMenuItem.Click
        ZoomToAll()
    End Sub

    Private Sub ZoomToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Zoom = CInt(sender.ToString)
    End Sub

    Private Sub UseMarkedTilesToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles UseMarkedTilesToolStripMenuItem.Click
        UseMarkedTilesToolStripMenuItem.Checked = Not UseMarkedTilesToolStripMenuItem.Checked
        pUseMarkedTiles = UseMarkedTilesToolStripMenuItem.Checked
        Redraw()
    End Sub

    Private Sub OpenOSMWebsiteToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OpenOSMWebsiteToolStripMenuItem.Click
        Process.Start("http://www.openstreetmap.org/index.html?mlat=" & CenterLat & "&mlon=" & CenterLon & "&zoom=" & pZoom)
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
            If Not pDownloader.DownloadFile("http://josm.openstreetmap.de/josm-tested.jar", lJosmFilename) Then
                MsgBox("Please manually download JOSM from" & vbCrLf & "http://josm.openstreetmap.de/" & vbCrLf & "and save as " & vbCrLf & lJosmFilename, MsgBoxStyle.OkOnly, "Unable to download JOSM")
            End If
            Me.Text = lSaveTitle
            Me.Cursor = Cursors.Default
        End If

        If IO.File.Exists(lJosmFilename) Then
            Dim lArguments As String = ""
            For Each lLayer As clsLayer In pLayers
                lArguments &= """" & lLayer.Filename & """ "
            Next
            Process.Start(lJosmFilename, lArguments)
        End If
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
        pLayersForm.PopulateList(pLayers)
        pLayersForm.Show()
    End Sub

    Private Sub pLayersForm_Apply() Handles pLayersForm.Apply
        Redraw()
    End Sub

    Private Sub pLayersForm_CheckedItemsChanged(ByVal aSelectedLayers As ArrayList) Handles pLayersForm.CheckedItemsChanged
        'Change set of loaded/visible layers to match ones now checked
        Dim lFilename As String

        For Each lLayer As clsLayer In pLayers
            lLayer.Visible = False
        Next

        For Each lFilename In aSelectedLayers
            Dim lAlreadyOpen As Boolean = False
            For Each lLayer As clsLayer In pLayers
                If lLayer.Filename.ToLower = lFilename.ToLower Then
                    lLayer.Visible = True
                    lAlreadyOpen = True
                    Exit For
                End If
            Next

            If Not lAlreadyOpen Then
                Me.Text = "Opening " & IO.Path.GetFileNameWithoutExtension(lFilename)
                Application.DoEvents()
                OpenGPX(lFilename)
            End If
        Next
        Redraw()
    End Sub

    Private Sub ExitToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitToolStripMenuItem.Click
        Me.Close()
    End Sub

    Private Sub FindBuddyToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles FindBuddyToolStripMenuItem.Click
        FindBuddyToolStripMenuItem.Checked = Not FindBuddyToolStripMenuItem.Checked
        If FindBuddyToolStripMenuItem.Checked Then
            pBuddyTimer = New System.Threading.Timer(New Threading.TimerCallback(AddressOf RequestBuddyPoint), Nothing, 0, 60000)
        ElseIf pBuddyTimer IsNot Nothing Then
            pBuddyTimer.Dispose()
            pBuddyTimer = Nothing
        End If
    End Sub

    Private Sub SetBuddyAlarmToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SetBuddyAlarmToolStripMenuItem.Click
        If pBuddyAlarmForm IsNot Nothing Then
            Try
                pBuddyAlarmForm.Close()
            Catch
            End Try
        End If
        pBuddyAlarmForm = New frmBuddyAlarm
        pBuddyAlarmForm.Icon = Me.Icon
        If pBuddyAlarmLat = 0 AndAlso pBuddyAlarmLon = 0 Then
            pBuddyAlarmLat = CenterLat
            pBuddyAlarmLon = CenterLon
        End If
        pBuddyAlarmForm.AskUser(pBuddyAlarmLat, pBuddyAlarmLon, pBuddyAlarmMeters, True)
    End Sub

    Private Sub pBuddyAlarmForm_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles pBuddyAlarmForm.FormClosed
        pBuddyAlarmForm = Nothing
    End Sub

    Private Sub pBuddyAlarmForm_SetAlarm(ByVal aLatitude As Double, ByVal aLongitude As Double, ByVal aDistanceMeters As Double, ByVal aEnable As Boolean) Handles pBuddyAlarmForm.SetAlarm
        pBuddyAlarmLat = aLatitude
        pBuddyAlarmLon = aLongitude
        pBuddyAlarmMeters = aDistanceMeters
        pBuddyAlarmEnabled = aEnable
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
        pBuddyListForm.AskUser(pBuddies)
    End Sub

    Private Sub pBuddyListForm_Ok(ByVal aBuddies As System.Collections.Generic.Dictionary(Of String, clsBuddy)) Handles pBuddyListForm.Ok
        pBuddies.Clear()
        pBuddies = aBuddies
        Redraw()
    End Sub

    Private Sub WheelTileServerToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles WheelTileServerToolStripMenuItem.Click
        WheelTileServerToolStripMenuItem.Checked = True
        WheelZoomToolStripMenuItem.Checked = False
        pMouseWheelTileServer = True
        pMouseWheelZoom = False
    End Sub

    Private Sub WheelZoomToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles WheelZoomToolStripMenuItem.Click
        WheelTileServerToolStripMenuItem.Checked = False
        WheelZoomToolStripMenuItem.Checked = True
        pMouseWheelTileServer = False
        pMouseWheelZoom = True
    End Sub

End Class