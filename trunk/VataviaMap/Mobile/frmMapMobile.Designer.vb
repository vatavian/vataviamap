<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Public Class frmMap
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer
    Private mnuMain As System.Windows.Forms.MainMenu

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.mnuMain = New System.Windows.Forms.MainMenu
        Me.mnuStartStopGPS = New System.Windows.Forms.MenuItem
        Me.mnuOptions = New System.Windows.Forms.MenuItem
        Me.mnuOptionsDownload = New System.Windows.Forms.MenuItem
        Me.mnuOptionsUpload = New System.Windows.Forms.MenuItem
        Me.mnuLayers = New System.Windows.Forms.MenuItem
        Me.mnuGeocache = New System.Windows.Forms.MenuItem
        Me.mnuView = New System.Windows.Forms.MenuItem
        Me.mnuViewMapTiles = New System.Windows.Forms.MenuItem
        Me.mnuViewGPSdetails = New System.Windows.Forms.MenuItem
        Me.mnuViewDark = New System.Windows.Forms.MenuItem
        Me.mnuViewTileOutlines = New System.Windows.Forms.MenuItem
        Me.mnuViewTileNames = New System.Windows.Forms.MenuItem
        Me.mnuViewTrack = New System.Windows.Forms.MenuItem
        Me.mnuViewControls = New System.Windows.Forms.MenuItem
        Me.mnuGPS = New System.Windows.Forms.MenuItem
        Me.mnuCenter = New System.Windows.Forms.MenuItem
        Me.mnuFollow = New System.Windows.Forms.MenuItem
        Me.mnuAutoStart = New System.Windows.Forms.MenuItem
        Me.mnuRecordTrack = New System.Windows.Forms.MenuItem
        Me.mnuRefreshOnClick = New System.Windows.Forms.MenuItem
        Me.mnuFindBuddy = New System.Windows.Forms.MenuItem
        Me.mnuTakePicture = New System.Windows.Forms.MenuItem
        Me.mnuSetTime = New System.Windows.Forms.MenuItem
        Me.SuspendLayout()
        '
        'mnuMain
        '
        Me.mnuMain.MenuItems.Add(Me.mnuStartStopGPS)
        Me.mnuMain.MenuItems.Add(Me.mnuOptions)
        '
        'mnuStartStopGPS
        '
        Me.mnuStartStopGPS.Text = "Start GPS"
        '
        'mnuOptions
        '
        Me.mnuOptions.MenuItems.Add(Me.mnuOptionsDownload)
        Me.mnuOptions.MenuItems.Add(Me.mnuOptionsUpload)
        Me.mnuOptions.MenuItems.Add(Me.mnuLayers)
        Me.mnuOptions.MenuItems.Add(Me.mnuGeocache)
        Me.mnuOptions.MenuItems.Add(Me.mnuView)
        Me.mnuOptions.MenuItems.Add(Me.mnuGPS)
        Me.mnuOptions.MenuItems.Add(Me.mnuRefreshOnClick)
        Me.mnuOptions.MenuItems.Add(Me.mnuFindBuddy)
        Me.mnuOptions.MenuItems.Add(Me.mnuTakePicture)
        Me.mnuOptions.Text = "Options"
        '
        'mnuOptionsDownload
        '
        Me.mnuOptionsDownload.Text = "Tile Download..."
        '
        'mnuOptionsUpload
        '
        Me.mnuOptionsUpload.Text = "Position Upload..."
        '
        'mnuLayers
        '
        Me.mnuLayers.Text = "Layers..."
        '
        'mnuGeocache
        '
        Me.mnuGeocache.Text = "Geocache"
        '
        'mnuView
        '
        Me.mnuView.MenuItems.Add(Me.mnuViewMapTiles)
        Me.mnuView.MenuItems.Add(Me.mnuViewGPSdetails)
        Me.mnuView.MenuItems.Add(Me.mnuViewDark)
        Me.mnuView.MenuItems.Add(Me.mnuViewTileOutlines)
        Me.mnuView.MenuItems.Add(Me.mnuViewTileNames)
        Me.mnuView.MenuItems.Add(Me.mnuViewTrack)
        Me.mnuView.MenuItems.Add(Me.mnuViewControls)
        Me.mnuView.Text = "View"
        '
        'mnuViewMapTiles
        '
        Me.mnuViewMapTiles.Checked = True
        Me.mnuViewMapTiles.Text = "Map Tiles"
        '
        'mnuViewGPSdetails
        '
        Me.mnuViewGPSdetails.Text = "GPS Details"
        '
        'mnuViewDark
        '
        Me.mnuViewDark.Text = "Dark"
        '
        'mnuViewTileOutlines
        '
        Me.mnuViewTileOutlines.Text = "Tile Outlines"
        '
        'mnuViewTileNames
        '
        Me.mnuViewTileNames.Text = "Tile Names"
        '
        'mnuViewTrack
        '
        Me.mnuViewTrack.Text = "Current Track"
        '
        'mnuViewControls
        '
        Me.mnuViewControls.Checked = True
        Me.mnuViewControls.Text = "Controls"
        '
        'mnuGPS
        '
        Me.mnuGPS.MenuItems.Add(Me.mnuCenter)
        Me.mnuGPS.MenuItems.Add(Me.mnuFollow)
        Me.mnuGPS.MenuItems.Add(Me.mnuAutoStart)
        Me.mnuGPS.MenuItems.Add(Me.mnuRecordTrack)
        Me.mnuGPS.MenuItems.Add(Me.mnuSetTime)
        Me.mnuGPS.Text = "GPS"
        '
        'mnuCenter
        '
        Me.mnuCenter.Text = "Center"
        '
        'mnuFollow
        '
        Me.mnuFollow.Text = "Follow"
        '
        'mnuAutoStart
        '
        Me.mnuAutoStart.Text = "Auto Start"
        '
        'mnuRecordTrack
        '
        Me.mnuRecordTrack.Text = "Record Track"
        '
        'mnuRefreshOnClick
        '
        Me.mnuRefreshOnClick.Text = "Refresh On Click"
        '
        'mnuFindBuddy
        '
        Me.mnuFindBuddy.Text = "Find Buddy"
        '
        'mnuTakePicture
        '
        Me.mnuTakePicture.Text = "Take Picture"
        '
        'mnuSetTime
        '
        Me.mnuSetTime.Text = "Set Time from GPS"
        '
        'frmMap
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoScroll = True
        Me.ClientSize = New System.Drawing.Size(176, 180)
        Me.Menu = Me.mnuMain
        Me.Name = "frmMap"
        Me.Text = "VataviaMap"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents mnuStartStopGPS As System.Windows.Forms.MenuItem
    Friend WithEvents mnuOptions As System.Windows.Forms.MenuItem
    Friend WithEvents mnuOptionsDownload As System.Windows.Forms.MenuItem
    Friend WithEvents mnuLayers As System.Windows.Forms.MenuItem
    Friend WithEvents mnuGeocache As System.Windows.Forms.MenuItem
    Friend WithEvents mnuOptionsUpload As System.Windows.Forms.MenuItem
    Friend WithEvents mnuView As System.Windows.Forms.MenuItem
    Friend WithEvents mnuViewMapTiles As System.Windows.Forms.MenuItem
    Friend WithEvents mnuViewGPSdetails As System.Windows.Forms.MenuItem
    Friend WithEvents mnuViewDark As System.Windows.Forms.MenuItem
    Friend WithEvents mnuViewTileOutlines As System.Windows.Forms.MenuItem
    Friend WithEvents mnuViewTileNames As System.Windows.Forms.MenuItem
    Friend WithEvents mnuRefreshOnClick As System.Windows.Forms.MenuItem
    Friend WithEvents mnuGPS As System.Windows.Forms.MenuItem
    Friend WithEvents mnuCenter As System.Windows.Forms.MenuItem
    Friend WithEvents mnuFollow As System.Windows.Forms.MenuItem
    Friend WithEvents mnuAutoStart As System.Windows.Forms.MenuItem
    Friend WithEvents mnuViewTrack As System.Windows.Forms.MenuItem
    Friend WithEvents mnuRecordTrack As System.Windows.Forms.MenuItem
    Friend WithEvents mnuFindBuddy As System.Windows.Forms.MenuItem
    Friend WithEvents mnuViewControls As System.Windows.Forms.MenuItem
    Friend WithEvents mnuTakePicture As System.Windows.Forms.MenuItem
    Friend WithEvents mnuSetTime As System.Windows.Forms.MenuItem

End Class
