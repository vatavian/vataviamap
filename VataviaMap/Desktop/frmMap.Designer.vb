<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMap
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            'TODO: SaveSettings()
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMap))
        Me.MenuStripMain = New System.Windows.Forms.MenuStrip
        Me.FileToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.SaveAsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.CopyToClipboardToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.AddLayerToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.LayersToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.RemoveAllLayersToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.OpenJOSMToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.FollowWebMapURLToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.GetOSMBugsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.ExitToolStripSeparator = New System.Windows.Forms.ToolStripSeparator
        Me.ExitToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.ViewToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.TileOutlinesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.TileNamesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.TileImagesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.PanToGPXToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.ZoomToGPXToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.UseMarkedTilesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.CoordinatesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.WaypointsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.TimestampToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.TransparencyToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.TransparencyNone = New System.Windows.Forms.ToolStripMenuItem
        Me.Transparency10 = New System.Windows.Forms.ToolStripMenuItem
        Me.Transparency20 = New System.Windows.Forms.ToolStripMenuItem
        Me.Transparency30 = New System.Windows.Forms.ToolStripMenuItem
        Me.Transparency40 = New System.Windows.Forms.ToolStripMenuItem
        Me.Transparency50 = New System.Windows.Forms.ToolStripMenuItem
        Me.Transparency60 = New System.Windows.Forms.ToolStripMenuItem
        Me.Transparency70 = New System.Windows.Forms.ToolStripMenuItem
        Me.Transparency80 = New System.Windows.Forms.ToolStripMenuItem
        Me.Transparency90 = New System.Windows.Forms.ToolStripMenuItem
        Me.ZoomToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.ZoomAllLayersToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.PlacesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.PlacesAddToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.TileServerToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.AddTileServerMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.EditTileServerMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.DefaultsTileServerMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.OverlayServerToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.OverlayMaplintToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.OverlayYahooLabelsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.BuddiesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.FindBuddyToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.SetBuddyAlarmToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.ListBuddiesToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.WheelToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.WheelZoomToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.WheelTileServerToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.WheelLayerToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.CellTowerToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.OpenCellIDToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.WebsiteToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.HelpToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.VataviaMapProjectPageToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.RefreshFromServerToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.pMap = New VataviaMap.ctlMap
        Me.TimeZoneToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.PrintToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.LetterPortraitToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.LetterLandscapeToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.MenuStripMain.SuspendLayout()
        Me.SuspendLayout()
        '
        'MenuStripMain
        '
        Me.MenuStripMain.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FileToolStripMenuItem, Me.ViewToolStripMenuItem, Me.ZoomToolStripMenuItem, Me.PlacesToolStripMenuItem, Me.TileServerToolStripMenuItem, Me.OverlayServerToolStripMenuItem, Me.BuddiesToolStripMenuItem, Me.WheelToolStripMenuItem, Me.CellTowerToolStripMenuItem, Me.WebsiteToolStripMenuItem, Me.HelpToolStripMenuItem})
        Me.MenuStripMain.Location = New System.Drawing.Point(0, 0)
        Me.MenuStripMain.Name = "MenuStripMain"
        Me.MenuStripMain.Size = New System.Drawing.Size(910, 27)
        Me.MenuStripMain.TabIndex = 0
        Me.MenuStripMain.Text = "MenuStrip1"
        '
        'FileToolStripMenuItem
        '
        Me.FileToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.SaveAsToolStripMenuItem, Me.CopyToClipboardToolStripMenuItem, Me.AddLayerToolStripMenuItem, Me.LayersToolStripMenuItem, Me.RemoveAllLayersToolStripMenuItem, Me.OpenJOSMToolStripMenuItem, Me.FollowWebMapURLToolStripMenuItem, Me.GetOSMBugsToolStripMenuItem, Me.PrintToolStripMenuItem, Me.ExitToolStripSeparator, Me.ExitToolStripMenuItem, Me.TimeZoneToolStripMenuItem})
        Me.FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        Me.FileToolStripMenuItem.Size = New System.Drawing.Size(45, 23)
        Me.FileToolStripMenuItem.Text = "File"
        '
        'SaveAsToolStripMenuItem
        '
        Me.SaveAsToolStripMenuItem.Name = "SaveAsToolStripMenuItem"
        Me.SaveAsToolStripMenuItem.Size = New System.Drawing.Size(337, 24)
        Me.SaveAsToolStripMenuItem.Text = "Save As..."
        '
        'CopyToClipboardToolStripMenuItem
        '
        Me.CopyToClipboardToolStripMenuItem.Name = "CopyToClipboardToolStripMenuItem"
        Me.CopyToClipboardToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.C), System.Windows.Forms.Keys)
        Me.CopyToClipboardToolStripMenuItem.Size = New System.Drawing.Size(337, 24)
        Me.CopyToClipboardToolStripMenuItem.Text = "Copy to Clipboard"
        '
        'AddLayerToolStripMenuItem
        '
        Me.AddLayerToolStripMenuItem.Name = "AddLayerToolStripMenuItem"
        Me.AddLayerToolStripMenuItem.Size = New System.Drawing.Size(337, 24)
        Me.AddLayerToolStripMenuItem.Text = "Add Layer..."
        '
        'LayersToolStripMenuItem
        '
        Me.LayersToolStripMenuItem.Name = "LayersToolStripMenuItem"
        Me.LayersToolStripMenuItem.Size = New System.Drawing.Size(337, 24)
        Me.LayersToolStripMenuItem.Text = "Layers..."
        '
        'RemoveAllLayersToolStripMenuItem
        '
        Me.RemoveAllLayersToolStripMenuItem.Name = "RemoveAllLayersToolStripMenuItem"
        Me.RemoveAllLayersToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete
        Me.RemoveAllLayersToolStripMenuItem.Size = New System.Drawing.Size(337, 24)
        Me.RemoveAllLayersToolStripMenuItem.Text = "Remove All Layers"
        '
        'OpenJOSMToolStripMenuItem
        '
        Me.OpenJOSMToolStripMenuItem.Name = "OpenJOSMToolStripMenuItem"
        Me.OpenJOSMToolStripMenuItem.Size = New System.Drawing.Size(337, 24)
        Me.OpenJOSMToolStripMenuItem.Text = "Open JOSM"
        '
        'FollowWebMapURLToolStripMenuItem
        '
        Me.FollowWebMapURLToolStripMenuItem.Name = "FollowWebMapURLToolStripMenuItem"
        Me.FollowWebMapURLToolStripMenuItem.Size = New System.Drawing.Size(337, 24)
        Me.FollowWebMapURLToolStripMenuItem.Text = "Follow Web Map URL on clipboard"
        '
        'GetOSMBugsToolStripMenuItem
        '
        Me.GetOSMBugsToolStripMenuItem.Name = "GetOSMBugsToolStripMenuItem"
        Me.GetOSMBugsToolStripMenuItem.Size = New System.Drawing.Size(337, 24)
        Me.GetOSMBugsToolStripMenuItem.Text = "Get OSM Bugs"
        '
        'ExitToolStripSeparator
        '
        Me.ExitToolStripSeparator.Name = "ExitToolStripSeparator"
        Me.ExitToolStripSeparator.Size = New System.Drawing.Size(334, 6)
        '
        'ExitToolStripMenuItem
        '
        Me.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem"
        Me.ExitToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.F4), System.Windows.Forms.Keys)
        Me.ExitToolStripMenuItem.Size = New System.Drawing.Size(337, 24)
        Me.ExitToolStripMenuItem.Text = "Exit"
        '
        'ViewToolStripMenuItem
        '
        Me.ViewToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.TileOutlinesToolStripMenuItem, Me.TileNamesToolStripMenuItem, Me.TileImagesToolStripMenuItem, Me.PanToGPXToolStripMenuItem, Me.ZoomToGPXToolStripMenuItem, Me.UseMarkedTilesToolStripMenuItem, Me.CoordinatesToolStripMenuItem, Me.WaypointsToolStripMenuItem, Me.TimestampToolStripMenuItem, Me.TransparencyToolStripMenuItem})
        Me.ViewToolStripMenuItem.Name = "ViewToolStripMenuItem"
        Me.ViewToolStripMenuItem.Size = New System.Drawing.Size(55, 23)
        Me.ViewToolStripMenuItem.Text = "View"
        '
        'TileOutlinesToolStripMenuItem
        '
        Me.TileOutlinesToolStripMenuItem.Name = "TileOutlinesToolStripMenuItem"
        Me.TileOutlinesToolStripMenuItem.Size = New System.Drawing.Size(214, 24)
        Me.TileOutlinesToolStripMenuItem.Text = "Tile Outlines"
        '
        'TileNamesToolStripMenuItem
        '
        Me.TileNamesToolStripMenuItem.Name = "TileNamesToolStripMenuItem"
        Me.TileNamesToolStripMenuItem.Size = New System.Drawing.Size(214, 24)
        Me.TileNamesToolStripMenuItem.Text = "Tile Names"
        '
        'TileImagesToolStripMenuItem
        '
        Me.TileImagesToolStripMenuItem.Checked = True
        Me.TileImagesToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked
        Me.TileImagesToolStripMenuItem.Name = "TileImagesToolStripMenuItem"
        Me.TileImagesToolStripMenuItem.Size = New System.Drawing.Size(214, 24)
        Me.TileImagesToolStripMenuItem.Text = "Tile Images"
        '
        'PanToGPXToolStripMenuItem
        '
        Me.PanToGPXToolStripMenuItem.Name = "PanToGPXToolStripMenuItem"
        Me.PanToGPXToolStripMenuItem.Size = New System.Drawing.Size(214, 24)
        Me.PanToGPXToolStripMenuItem.Text = "Pan to GPX"
        '
        'ZoomToGPXToolStripMenuItem
        '
        Me.ZoomToGPXToolStripMenuItem.Name = "ZoomToGPXToolStripMenuItem"
        Me.ZoomToGPXToolStripMenuItem.Size = New System.Drawing.Size(214, 24)
        Me.ZoomToGPXToolStripMenuItem.Text = "Zoom to GPX"
        '
        'UseMarkedTilesToolStripMenuItem
        '
        Me.UseMarkedTilesToolStripMenuItem.Name = "UseMarkedTilesToolStripMenuItem"
        Me.UseMarkedTilesToolStripMenuItem.Size = New System.Drawing.Size(214, 24)
        Me.UseMarkedTilesToolStripMenuItem.Text = "Use Marked Tiles"
        '
        'CoordinatesToolStripMenuItem
        '
        Me.CoordinatesToolStripMenuItem.Name = "CoordinatesToolStripMenuItem"
        Me.CoordinatesToolStripMenuItem.Size = New System.Drawing.Size(214, 24)
        Me.CoordinatesToolStripMenuItem.Text = "Coordinates"
        '
        'WaypointsToolStripMenuItem
        '
        Me.WaypointsToolStripMenuItem.Name = "WaypointsToolStripMenuItem"
        Me.WaypointsToolStripMenuItem.Size = New System.Drawing.Size(214, 24)
        Me.WaypointsToolStripMenuItem.Text = "Waypoints..."
        '
        'TimestampToolStripMenuItem
        '
        Me.TimestampToolStripMenuItem.Name = "TimestampToolStripMenuItem"
        Me.TimestampToolStripMenuItem.Size = New System.Drawing.Size(214, 24)
        Me.TimestampToolStripMenuItem.Text = "Timestamp"
        '
        'TransparencyToolStripMenuItem
        '
        Me.TransparencyToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.TransparencyNone, Me.Transparency10, Me.Transparency20, Me.Transparency30, Me.Transparency40, Me.Transparency50, Me.Transparency60, Me.Transparency70, Me.Transparency80, Me.Transparency90})
        Me.TransparencyToolStripMenuItem.Name = "TransparencyToolStripMenuItem"
        Me.TransparencyToolStripMenuItem.Size = New System.Drawing.Size(214, 24)
        Me.TransparencyToolStripMenuItem.Text = "Transparency"
        '
        'TransparencyNone
        '
        Me.TransparencyNone.Name = "TransparencyNone"
        Me.TransparencyNone.Size = New System.Drawing.Size(131, 24)
        Me.TransparencyNone.Text = "None"
        '
        'Transparency10
        '
        Me.Transparency10.Name = "Transparency10"
        Me.Transparency10.Size = New System.Drawing.Size(131, 24)
        Me.Transparency10.Text = "10%"
        '
        'Transparency20
        '
        Me.Transparency20.Name = "Transparency20"
        Me.Transparency20.Size = New System.Drawing.Size(131, 24)
        Me.Transparency20.Text = "20%"
        '
        'Transparency30
        '
        Me.Transparency30.Name = "Transparency30"
        Me.Transparency30.Size = New System.Drawing.Size(131, 24)
        Me.Transparency30.Text = "30%"
        '
        'Transparency40
        '
        Me.Transparency40.Name = "Transparency40"
        Me.Transparency40.Size = New System.Drawing.Size(131, 24)
        Me.Transparency40.Text = "40%"
        '
        'Transparency50
        '
        Me.Transparency50.Name = "Transparency50"
        Me.Transparency50.Size = New System.Drawing.Size(131, 24)
        Me.Transparency50.Text = "50%"
        '
        'Transparency60
        '
        Me.Transparency60.Name = "Transparency60"
        Me.Transparency60.Size = New System.Drawing.Size(131, 24)
        Me.Transparency60.Text = "60%"
        '
        'Transparency70
        '
        Me.Transparency70.Name = "Transparency70"
        Me.Transparency70.Size = New System.Drawing.Size(131, 24)
        Me.Transparency70.Text = "70%"
        '
        'Transparency80
        '
        Me.Transparency80.Name = "Transparency80"
        Me.Transparency80.Size = New System.Drawing.Size(131, 24)
        Me.Transparency80.Text = "80%"
        '
        'Transparency90
        '
        Me.Transparency90.Name = "Transparency90"
        Me.Transparency90.Size = New System.Drawing.Size(131, 24)
        Me.Transparency90.Text = "90%"
        '
        'ZoomToolStripMenuItem
        '
        Me.ZoomToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ZoomAllLayersToolStripMenuItem})
        Me.ZoomToolStripMenuItem.Name = "ZoomToolStripMenuItem"
        Me.ZoomToolStripMenuItem.Size = New System.Drawing.Size(62, 23)
        Me.ZoomToolStripMenuItem.Text = "Zoom"
        '
        'ZoomAllLayersToolStripMenuItem
        '
        Me.ZoomAllLayersToolStripMenuItem.Name = "ZoomAllLayersToolStripMenuItem"
        Me.ZoomAllLayersToolStripMenuItem.Size = New System.Drawing.Size(163, 24)
        Me.ZoomAllLayersToolStripMenuItem.Text = "All Layers"
        '
        'PlacesToolStripMenuItem
        '
        Me.PlacesToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.PlacesAddToolStripMenuItem})
        Me.PlacesToolStripMenuItem.Name = "PlacesToolStripMenuItem"
        Me.PlacesToolStripMenuItem.Size = New System.Drawing.Size(64, 23)
        Me.PlacesToolStripMenuItem.Text = "Places"
        '
        'PlacesAddToolStripMenuItem
        '
        Me.PlacesAddToolStripMenuItem.Name = "PlacesAddToolStripMenuItem"
        Me.PlacesAddToolStripMenuItem.Size = New System.Drawing.Size(152, 24)
        Me.PlacesAddToolStripMenuItem.Text = "Add..."
        '
        'TileServerToolStripMenuItem
        '
        Me.TileServerToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.AddTileServerMenuItem, Me.EditTileServerMenuItem, Me.DefaultsTileServerMenuItem})
        Me.TileServerToolStripMenuItem.Name = "TileServerToolStripMenuItem"
        Me.TileServerToolStripMenuItem.Size = New System.Drawing.Size(97, 23)
        Me.TileServerToolStripMenuItem.Text = "Tile Server"
        '
        'AddTileServerMenuItem
        '
        Me.AddTileServerMenuItem.Name = "AddTileServerMenuItem"
        Me.AddTileServerMenuItem.Size = New System.Drawing.Size(213, 24)
        Me.AddTileServerMenuItem.Text = "Add..."
        '
        'EditTileServerMenuItem
        '
        Me.EditTileServerMenuItem.Name = "EditTileServerMenuItem"
        Me.EditTileServerMenuItem.Size = New System.Drawing.Size(213, 24)
        Me.EditTileServerMenuItem.Text = "Edit"
        '
        'DefaultsTileServerMenuItem
        '
        Me.DefaultsTileServerMenuItem.Name = "DefaultsTileServerMenuItem"
        Me.DefaultsTileServerMenuItem.Size = New System.Drawing.Size(213, 24)
        Me.DefaultsTileServerMenuItem.Text = "Reset to Defaults"
        '
        'OverlayServerToolStripMenuItem
        '
        Me.OverlayServerToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.OverlayMaplintToolStripMenuItem, Me.OverlayYahooLabelsToolStripMenuItem})
        Me.OverlayServerToolStripMenuItem.Name = "OverlayServerToolStripMenuItem"
        Me.OverlayServerToolStripMenuItem.Size = New System.Drawing.Size(125, 23)
        Me.OverlayServerToolStripMenuItem.Text = "Overlay Server"
        '
        'OverlayMaplintToolStripMenuItem
        '
        Me.OverlayMaplintToolStripMenuItem.Name = "OverlayMaplintToolStripMenuItem"
        Me.OverlayMaplintToolStripMenuItem.Size = New System.Drawing.Size(183, 24)
        Me.OverlayMaplintToolStripMenuItem.Text = "Maplint"
        '
        'OverlayYahooLabelsToolStripMenuItem
        '
        Me.OverlayYahooLabelsToolStripMenuItem.Name = "OverlayYahooLabelsToolStripMenuItem"
        Me.OverlayYahooLabelsToolStripMenuItem.Size = New System.Drawing.Size(183, 24)
        Me.OverlayYahooLabelsToolStripMenuItem.Text = "YahooLabels"
        '
        'BuddiesToolStripMenuItem
        '
        Me.BuddiesToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FindBuddyToolStripMenuItem, Me.SetBuddyAlarmToolStripMenuItem, Me.ListBuddiesToolStripMenuItem})
        Me.BuddiesToolStripMenuItem.Name = "BuddiesToolStripMenuItem"
        Me.BuddiesToolStripMenuItem.Size = New System.Drawing.Size(76, 23)
        Me.BuddiesToolStripMenuItem.Text = "Buddies"
        '
        'FindBuddyToolStripMenuItem
        '
        Me.FindBuddyToolStripMenuItem.Name = "FindBuddyToolStripMenuItem"
        Me.FindBuddyToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.F), System.Windows.Forms.Keys)
        Me.FindBuddyToolStripMenuItem.Size = New System.Drawing.Size(238, 24)
        Me.FindBuddyToolStripMenuItem.Text = "Find Buddies"
        '
        'SetBuddyAlarmToolStripMenuItem
        '
        Me.SetBuddyAlarmToolStripMenuItem.Name = "SetBuddyAlarmToolStripMenuItem"
        Me.SetBuddyAlarmToolStripMenuItem.Size = New System.Drawing.Size(238, 24)
        Me.SetBuddyAlarmToolStripMenuItem.Text = "Set Buddy Alarm"
        '
        'ListBuddiesToolStripMenuItem
        '
        Me.ListBuddiesToolStripMenuItem.Name = "ListBuddiesToolStripMenuItem"
        Me.ListBuddiesToolStripMenuItem.Size = New System.Drawing.Size(238, 24)
        Me.ListBuddiesToolStripMenuItem.Text = "List"
        '
        'WheelToolStripMenuItem
        '
        Me.WheelToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.WheelZoomToolStripMenuItem, Me.WheelTileServerToolStripMenuItem, Me.WheelLayerToolStripMenuItem})
        Me.WheelToolStripMenuItem.Name = "WheelToolStripMenuItem"
        Me.WheelToolStripMenuItem.Size = New System.Drawing.Size(64, 23)
        Me.WheelToolStripMenuItem.Text = "Wheel"
        '
        'WheelZoomToolStripMenuItem
        '
        Me.WheelZoomToolStripMenuItem.Checked = True
        Me.WheelZoomToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked
        Me.WheelZoomToolStripMenuItem.Name = "WheelZoomToolStripMenuItem"
        Me.WheelZoomToolStripMenuItem.Size = New System.Drawing.Size(170, 24)
        Me.WheelZoomToolStripMenuItem.Text = "Zoom"
        '
        'WheelTileServerToolStripMenuItem
        '
        Me.WheelTileServerToolStripMenuItem.Name = "WheelTileServerToolStripMenuItem"
        Me.WheelTileServerToolStripMenuItem.Size = New System.Drawing.Size(170, 24)
        Me.WheelTileServerToolStripMenuItem.Text = "Tile Server"
        '
        'WheelLayerToolStripMenuItem
        '
        Me.WheelLayerToolStripMenuItem.Name = "WheelLayerToolStripMenuItem"
        Me.WheelLayerToolStripMenuItem.Size = New System.Drawing.Size(170, 24)
        Me.WheelLayerToolStripMenuItem.Text = "Layer"
        '
        'CellTowerToolStripMenuItem
        '
        Me.CellTowerToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.OpenCellIDToolStripMenuItem})
        Me.CellTowerToolStripMenuItem.Name = "CellTowerToolStripMenuItem"
        Me.CellTowerToolStripMenuItem.Size = New System.Drawing.Size(97, 23)
        Me.CellTowerToolStripMenuItem.Text = "Cell Tower"
        '
        'OpenCellIDToolStripMenuItem
        '
        Me.OpenCellIDToolStripMenuItem.Name = "OpenCellIDToolStripMenuItem"
        Me.OpenCellIDToolStripMenuItem.Size = New System.Drawing.Size(175, 24)
        Me.OpenCellIDToolStripMenuItem.Text = "OpenCellID"
        '
        'WebsiteToolStripMenuItem
        '
        Me.WebsiteToolStripMenuItem.Name = "WebsiteToolStripMenuItem"
        Me.WebsiteToolStripMenuItem.Size = New System.Drawing.Size(76, 23)
        Me.WebsiteToolStripMenuItem.Text = "Website"
        '
        'HelpToolStripMenuItem
        '
        Me.HelpToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.VataviaMapProjectPageToolStripMenuItem})
        Me.HelpToolStripMenuItem.Name = "HelpToolStripMenuItem"
        Me.HelpToolStripMenuItem.Size = New System.Drawing.Size(53, 23)
        Me.HelpToolStripMenuItem.Text = "Help"
        '
        'VataviaMapProjectPageToolStripMenuItem
        '
        Me.VataviaMapProjectPageToolStripMenuItem.Name = "VataviaMapProjectPageToolStripMenuItem"
        Me.VataviaMapProjectPageToolStripMenuItem.Size = New System.Drawing.Size(267, 24)
        Me.VataviaMapProjectPageToolStripMenuItem.Text = "VataviaMap Project Page"
        '
        'RefreshFromServerToolStripMenuItem
        '
        Me.RefreshFromServerToolStripMenuItem.Name = "RefreshFromServerToolStripMenuItem"
        Me.RefreshFromServerToolStripMenuItem.Size = New System.Drawing.Size(32, 19)
        '
        'pMap
        '
        Me.pMap.Dock = System.Windows.Forms.DockStyle.Fill
        Me.pMap.Location = New System.Drawing.Point(0, 27)
        Me.pMap.Name = "pMap"
        Me.pMap.Size = New System.Drawing.Size(910, 451)
        Me.pMap.TabIndex = 1
        Me.pMap.TileCacheFolder = ""
        Me.pMap.Zoom = 10
        '
        'TimeZoneToolStripMenuItem
        '
        Me.TimeZoneToolStripMenuItem.Name = "TimeZoneToolStripMenuItem"
        Me.TimeZoneToolStripMenuItem.Size = New System.Drawing.Size(223, 22)
        Me.TimeZoneToolStripMenuItem.Text = "Time Zone"
        '
        'PrintToolStripMenuItem
        '
        Me.PrintToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.LetterPortraitToolStripMenuItem, Me.LetterLandscapeToolStripMenuItem})
        Me.PrintToolStripMenuItem.Name = "PrintToolStripMenuItem"
        Me.PrintToolStripMenuItem.Size = New System.Drawing.Size(337, 24)
        Me.PrintToolStripMenuItem.Text = "Print"
        '
        'LetterPortraitToolStripMenuItem
        '
        Me.LetterPortraitToolStripMenuItem.Name = "LetterPortraitToolStripMenuItem"
        Me.LetterPortraitToolStripMenuItem.Size = New System.Drawing.Size(212, 24)
        Me.LetterPortraitToolStripMenuItem.Text = "Letter Portrait"
        '
        'LetterLandscapeToolStripMenuItem
        '
        Me.LetterLandscapeToolStripMenuItem.Name = "LetterLandscapeToolStripMenuItem"
        Me.LetterLandscapeToolStripMenuItem.Size = New System.Drawing.Size(212, 24)
        Me.LetterLandscapeToolStripMenuItem.Text = "Letter Landscape"
        '
        'frmMap
        '
        Me.AllowDrop = True
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(910, 478)
        Me.Controls.Add(Me.pMap)
        Me.Controls.Add(Me.MenuStripMain)
        Me.DoubleBuffered = True
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MainMenuStrip = Me.MenuStripMain
        Me.Name = "frmMap"
        Me.Text = "VataviaMap"
        Me.MenuStripMain.ResumeLayout(False)
        Me.MenuStripMain.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents MenuStripMain As System.Windows.Forms.MenuStrip
    Friend WithEvents FileToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SaveAsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ViewToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents TileOutlinesToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents TileNamesToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents RefreshFromServerToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents TileImagesToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents AddLayerToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents RemoveAllLayersToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents TileServerToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents PanToGPXToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ZoomToGPXToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ZoomToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents LayersToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents UseMarkedTilesToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ExitToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ExitToolStripSeparator As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents ZoomAllLayersToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents BuddiesToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents FindBuddyToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SetBuddyAlarmToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents AddTileServerMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents EditTileServerMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DefaultsTileServerMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents OpenJOSMToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ListBuddiesToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents WheelToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents WheelZoomToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents WheelTileServerToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents OverlayServerToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents OverlayMaplintToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents OverlayYahooLabelsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents WheelLayerToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents HelpToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents VataviaMapProjectPageToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents GetOSMBugsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents FollowWebMapURLToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents CellTowerToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents OpenCellIDToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents CoordinatesToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents CopyToClipboardToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents WaypointsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents TimestampToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents pMap As VataviaMap.ctlMap
    Friend WithEvents TimeZoneToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents TransparencyToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents TransparencyNone As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Transparency10 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Transparency20 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Transparency30 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Transparency40 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Transparency50 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Transparency60 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Transparency70 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Transparency80 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents Transparency90 As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents WebsiteToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents PlacesToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents PlacesAddToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents PrintToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents LetterPortraitToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents LetterLandscapeToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
End Class
