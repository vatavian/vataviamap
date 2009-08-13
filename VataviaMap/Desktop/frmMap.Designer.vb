<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMap
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            SaveSettings()
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
        Me.OpenOSMWebsiteToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.OpenJOSMToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.FollowOSMURLToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
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
        Me.ZoomToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.ZoomAllLayersToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
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
        Me.HelpToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.VataviaMapProjectPageToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.RightClickMenu = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.RefreshFromServerToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.GetAllDescendantsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.MenuStripMain.SuspendLayout()
        Me.RightClickMenu.SuspendLayout()
        Me.SuspendLayout()
        '
        'MenuStripMain
        '
        Me.MenuStripMain.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.FileToolStripMenuItem, Me.ViewToolStripMenuItem, Me.ZoomToolStripMenuItem, Me.TileServerToolStripMenuItem, Me.OverlayServerToolStripMenuItem, Me.BuddiesToolStripMenuItem, Me.WheelToolStripMenuItem, Me.CellTowerToolStripMenuItem, Me.HelpToolStripMenuItem})
        Me.MenuStripMain.Location = New System.Drawing.Point(0, 0)
        Me.MenuStripMain.Name = "MenuStripMain"
        Me.MenuStripMain.Size = New System.Drawing.Size(785, 27)
        Me.MenuStripMain.TabIndex = 0
        Me.MenuStripMain.Text = "MenuStrip1"
        '
        'FileToolStripMenuItem
        '
        Me.FileToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.SaveAsToolStripMenuItem, Me.CopyToClipboardToolStripMenuItem, Me.AddLayerToolStripMenuItem, Me.LayersToolStripMenuItem, Me.RemoveAllLayersToolStripMenuItem, Me.OpenOSMWebsiteToolStripMenuItem, Me.OpenJOSMToolStripMenuItem, Me.FollowOSMURLToolStripMenuItem, Me.GetOSMBugsToolStripMenuItem, Me.ExitToolStripSeparator, Me.ExitToolStripMenuItem})
        Me.FileToolStripMenuItem.Name = "FileToolStripMenuItem"
        Me.FileToolStripMenuItem.Size = New System.Drawing.Size(45, 23)
        Me.FileToolStripMenuItem.Text = "File"
        '
        'SaveAsToolStripMenuItem
        '
        Me.SaveAsToolStripMenuItem.Name = "SaveAsToolStripMenuItem"
        Me.SaveAsToolStripMenuItem.Size = New System.Drawing.Size(305, 24)
        Me.SaveAsToolStripMenuItem.Text = "Save As..."
        '
        'CopyToClipboardToolStripMenuItem
        '
        Me.CopyToClipboardToolStripMenuItem.Name = "CopyToClipboardToolStripMenuItem"
        Me.CopyToClipboardToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Control Or System.Windows.Forms.Keys.C), System.Windows.Forms.Keys)
        Me.CopyToClipboardToolStripMenuItem.Size = New System.Drawing.Size(305, 24)
        Me.CopyToClipboardToolStripMenuItem.Text = "Copy to Clipboard"
        '
        'AddLayerToolStripMenuItem
        '
        Me.AddLayerToolStripMenuItem.Name = "AddLayerToolStripMenuItem"
        Me.AddLayerToolStripMenuItem.Size = New System.Drawing.Size(305, 24)
        Me.AddLayerToolStripMenuItem.Text = "Add Layer..."
        '
        'LayersToolStripMenuItem
        '
        Me.LayersToolStripMenuItem.Name = "LayersToolStripMenuItem"
        Me.LayersToolStripMenuItem.Size = New System.Drawing.Size(305, 24)
        Me.LayersToolStripMenuItem.Text = "Layers..."
        '
        'RemoveAllLayersToolStripMenuItem
        '
        Me.RemoveAllLayersToolStripMenuItem.Name = "RemoveAllLayersToolStripMenuItem"
        Me.RemoveAllLayersToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete
        Me.RemoveAllLayersToolStripMenuItem.Size = New System.Drawing.Size(305, 24)
        Me.RemoveAllLayersToolStripMenuItem.Text = "Remove All Layers"
        '
        'OpenOSMWebsiteToolStripMenuItem
        '
        Me.OpenOSMWebsiteToolStripMenuItem.Name = "OpenOSMWebsiteToolStripMenuItem"
        Me.OpenOSMWebsiteToolStripMenuItem.Size = New System.Drawing.Size(305, 24)
        Me.OpenOSMWebsiteToolStripMenuItem.Text = "Open OSM Website"
        '
        'OpenJOSMToolStripMenuItem
        '
        Me.OpenJOSMToolStripMenuItem.Name = "OpenJOSMToolStripMenuItem"
        Me.OpenJOSMToolStripMenuItem.Size = New System.Drawing.Size(305, 24)
        Me.OpenJOSMToolStripMenuItem.Text = "Open JOSM"
        '
        'FollowOSMURLToolStripMenuItem
        '
        Me.FollowOSMURLToolStripMenuItem.Name = "FollowOSMURLToolStripMenuItem"
        Me.FollowOSMURLToolStripMenuItem.Size = New System.Drawing.Size(305, 24)
        Me.FollowOSMURLToolStripMenuItem.Text = "Follow OSM URL on clipboard"
        '
        'GetOSMBugsToolStripMenuItem
        '
        Me.GetOSMBugsToolStripMenuItem.Name = "GetOSMBugsToolStripMenuItem"
        Me.GetOSMBugsToolStripMenuItem.Size = New System.Drawing.Size(305, 24)
        Me.GetOSMBugsToolStripMenuItem.Text = "Get OSM Bugs"
        '
        'ExitToolStripSeparator
        '
        Me.ExitToolStripSeparator.Name = "ExitToolStripSeparator"
        Me.ExitToolStripSeparator.Size = New System.Drawing.Size(302, 6)
        '
        'ExitToolStripMenuItem
        '
        Me.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem"
        Me.ExitToolStripMenuItem.ShortcutKeys = CType((System.Windows.Forms.Keys.Alt Or System.Windows.Forms.Keys.F4), System.Windows.Forms.Keys)
        Me.ExitToolStripMenuItem.Size = New System.Drawing.Size(305, 24)
        Me.ExitToolStripMenuItem.Text = "Exit"
        '
        'ViewToolStripMenuItem
        '
        Me.ViewToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.TileOutlinesToolStripMenuItem, Me.TileNamesToolStripMenuItem, Me.TileImagesToolStripMenuItem, Me.PanToGPXToolStripMenuItem, Me.ZoomToGPXToolStripMenuItem, Me.UseMarkedTilesToolStripMenuItem, Me.CoordinatesToolStripMenuItem})
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
        'RightClickMenu
        '
        Me.RightClickMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.RefreshFromServerToolStripMenuItem, Me.GetAllDescendantsToolStripMenuItem})
        Me.RightClickMenu.Name = "ContextMenuStrip1"
        Me.RightClickMenu.Size = New System.Drawing.Size(240, 52)
        '
        'RefreshFromServerToolStripMenuItem
        '
        Me.RefreshFromServerToolStripMenuItem.Name = "RefreshFromServerToolStripMenuItem"
        Me.RefreshFromServerToolStripMenuItem.Size = New System.Drawing.Size(239, 24)
        Me.RefreshFromServerToolStripMenuItem.Text = "Refresh From Server"
        '
        'GetAllDescendantsToolStripMenuItem
        '
        Me.GetAllDescendantsToolStripMenuItem.Name = "GetAllDescendantsToolStripMenuItem"
        Me.GetAllDescendantsToolStripMenuItem.Size = New System.Drawing.Size(239, 24)
        Me.GetAllDescendantsToolStripMenuItem.Text = "Get All Descendants"
        '
        'frmMap
        '
        Me.AllowDrop = True
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(785, 398)
        Me.Controls.Add(Me.MenuStripMain)
        Me.DoubleBuffered = True
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MainMenuStrip = Me.MenuStripMain
        Me.Name = "frmMap"
        Me.Text = "Map"
        Me.MenuStripMain.ResumeLayout(False)
        Me.MenuStripMain.PerformLayout()
        Me.RightClickMenu.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents MenuStripMain As System.Windows.Forms.MenuStrip
    Friend WithEvents FileToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SaveAsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ViewToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents TileOutlinesToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents TileNamesToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents RightClickMenu As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents RefreshFromServerToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents TileImagesToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents GetAllDescendantsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents AddLayerToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents RemoveAllLayersToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents TileServerToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents PanToGPXToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ZoomToGPXToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ZoomToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents OpenOSMWebsiteToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
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
    Friend WithEvents FollowOSMURLToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents CellTowerToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents OpenCellIDToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents CoordinatesToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents CopyToClipboardToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
End Class
