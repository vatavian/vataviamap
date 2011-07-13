<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Public Class frmDownloadMobile
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
        Me.mnuOk = New System.Windows.Forms.MenuItem
        Me.mnuCancel = New System.Windows.Forms.MenuItem
        Me.txtLat = New System.Windows.Forms.TextBox
        Me.lblLat = New System.Windows.Forms.Label
        Me.lblLon = New System.Windows.Forms.Label
        Me.txtLon = New System.Windows.Forms.TextBox
        Me.lblTileFolder = New System.Windows.Forms.Label
        Me.txtTileFolder = New System.Windows.Forms.TextBox
        Me.lblGPSsymbolSize = New System.Windows.Forms.Label
        Me.txtGPSSymbolSize = New System.Windows.Forms.TextBox
        Me.lblDegreeFormat = New System.Windows.Forms.Label
        Me.comboDegreeFormat = New System.Windows.Forms.ComboBox
        Me.txtLatDeg = New System.Windows.Forms.TextBox
        Me.txtLatMin = New System.Windows.Forms.TextBox
        Me.txtLatSec = New System.Windows.Forms.TextBox
        Me.txtLatDecimalMin = New System.Windows.Forms.TextBox
        Me.txtLonDecimalMin = New System.Windows.Forms.TextBox
        Me.txtLonDeg = New System.Windows.Forms.TextBox
        Me.txtLonSec = New System.Windows.Forms.TextBox
        Me.txtLonMin = New System.Windows.Forms.TextBox
        Me.lblUploadURL = New System.Windows.Forms.Label
        Me.comboTileServer = New System.Windows.Forms.ComboBox
        Me.comboOverlayServer = New System.Windows.Forms.ComboBox
        Me.SuspendLayout()
        '
        'mnuMain
        '
        Me.mnuMain.MenuItems.Add(Me.mnuOk)
        Me.mnuMain.MenuItems.Add(Me.mnuCancel)
        '
        'mnuOk
        '
        Me.mnuOk.Text = "Ok"
        '
        'mnuCancel
        '
        Me.mnuCancel.Text = "Cancel"
        '
        'txtLat
        '
        Me.txtLat.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtLat.Location = New System.Drawing.Point(73, 87)
        Me.txtLat.Name = "txtLat"
        Me.txtLat.Size = New System.Drawing.Size(154, 22)
        Me.txtLat.TabIndex = 1
        '
        'lblLat
        '
        Me.lblLat.Location = New System.Drawing.Point(3, 91)
        Me.lblLat.Name = "lblLat"
        Me.lblLat.Size = New System.Drawing.Size(64, 15)
        Me.lblLat.Text = "Center Lat"
        '
        'lblLon
        '
        Me.lblLon.Location = New System.Drawing.Point(3, 119)
        Me.lblLon.Name = "lblLon"
        Me.lblLon.Size = New System.Drawing.Size(64, 15)
        Me.lblLon.Text = "Center Lon"
        '
        'txtLon
        '
        Me.txtLon.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtLon.Location = New System.Drawing.Point(73, 115)
        Me.txtLon.Name = "txtLon"
        Me.txtLon.Size = New System.Drawing.Size(154, 22)
        Me.txtLon.TabIndex = 2
        '
        'lblTileFolder
        '
        Me.lblTileFolder.Location = New System.Drawing.Point(3, 32)
        Me.lblTileFolder.Name = "lblTileFolder"
        Me.lblTileFolder.Size = New System.Drawing.Size(64, 18)
        Me.lblTileFolder.Text = "Tile Folder"
        '
        'txtTileFolder
        '
        Me.txtTileFolder.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtTileFolder.Location = New System.Drawing.Point(73, 31)
        Me.txtTileFolder.Name = "txtTileFolder"
        Me.txtTileFolder.Size = New System.Drawing.Size(154, 22)
        Me.txtTileFolder.TabIndex = 0
        '
        'lblGPSsymbolSize
        '
        Me.lblGPSsymbolSize.Location = New System.Drawing.Point(3, 146)
        Me.lblGPSsymbolSize.Name = "lblGPSsymbolSize"
        Me.lblGPSsymbolSize.Size = New System.Drawing.Size(122, 19)
        Me.lblGPSsymbolSize.Text = "Current Position Size"
        '
        'txtGPSSymbolSize
        '
        Me.txtGPSSymbolSize.Location = New System.Drawing.Point(143, 143)
        Me.txtGPSSymbolSize.Name = "txtGPSSymbolSize"
        Me.txtGPSSymbolSize.Size = New System.Drawing.Size(22, 22)
        Me.txtGPSSymbolSize.TabIndex = 37
        '
        'lblDegreeFormat
        '
        Me.lblDegreeFormat.Location = New System.Drawing.Point(3, 63)
        Me.lblDegreeFormat.Name = "lblDegreeFormat"
        Me.lblDegreeFormat.Size = New System.Drawing.Size(64, 15)
        Me.lblDegreeFormat.Text = "Lat/Lon"
        '
        'comboDegreeFormat
        '
        Me.comboDegreeFormat.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.comboDegreeFormat.Items.Add("Degrees")
        Me.comboDegreeFormat.Items.Add("Minutes")
        Me.comboDegreeFormat.Items.Add("Seconds")
        Me.comboDegreeFormat.Location = New System.Drawing.Point(73, 59)
        Me.comboDegreeFormat.Name = "comboDegreeFormat"
        Me.comboDegreeFormat.Size = New System.Drawing.Size(154, 22)
        Me.comboDegreeFormat.TabIndex = 45
        '
        'txtLatDeg
        '
        Me.txtLatDeg.Location = New System.Drawing.Point(73, 87)
        Me.txtLatDeg.Name = "txtLatDeg"
        Me.txtLatDeg.Size = New System.Drawing.Size(29, 22)
        Me.txtLatDeg.TabIndex = 52
        Me.txtLatDeg.Visible = False
        '
        'txtLatMin
        '
        Me.txtLatMin.Location = New System.Drawing.Point(108, 87)
        Me.txtLatMin.Name = "txtLatMin"
        Me.txtLatMin.Size = New System.Drawing.Size(29, 22)
        Me.txtLatMin.TabIndex = 53
        Me.txtLatMin.Visible = False
        '
        'txtLatSec
        '
        Me.txtLatSec.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtLatSec.Location = New System.Drawing.Point(143, 87)
        Me.txtLatSec.Name = "txtLatSec"
        Me.txtLatSec.Size = New System.Drawing.Size(84, 22)
        Me.txtLatSec.TabIndex = 54
        Me.txtLatSec.Visible = False
        '
        'txtLatDecimalMin
        '
        Me.txtLatDecimalMin.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtLatDecimalMin.Location = New System.Drawing.Point(108, 87)
        Me.txtLatDecimalMin.Name = "txtLatDecimalMin"
        Me.txtLatDecimalMin.Size = New System.Drawing.Size(119, 22)
        Me.txtLatDecimalMin.TabIndex = 55
        Me.txtLatDecimalMin.Visible = False
        '
        'txtLonDecimalMin
        '
        Me.txtLonDecimalMin.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtLonDecimalMin.Location = New System.Drawing.Point(108, 115)
        Me.txtLonDecimalMin.Name = "txtLonDecimalMin"
        Me.txtLonDecimalMin.Size = New System.Drawing.Size(119, 22)
        Me.txtLonDecimalMin.TabIndex = 57
        Me.txtLonDecimalMin.Visible = False
        '
        'txtLonDeg
        '
        Me.txtLonDeg.Location = New System.Drawing.Point(73, 115)
        Me.txtLonDeg.Name = "txtLonDeg"
        Me.txtLonDeg.Size = New System.Drawing.Size(29, 22)
        Me.txtLonDeg.TabIndex = 56
        Me.txtLonDeg.Visible = False
        '
        'txtLonSec
        '
        Me.txtLonSec.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtLonSec.Location = New System.Drawing.Point(143, 115)
        Me.txtLonSec.Name = "txtLonSec"
        Me.txtLonSec.Size = New System.Drawing.Size(84, 22)
        Me.txtLonSec.TabIndex = 59
        Me.txtLonSec.Visible = False
        '
        'txtLonMin
        '
        Me.txtLonMin.Location = New System.Drawing.Point(108, 115)
        Me.txtLonMin.Name = "txtLonMin"
        Me.txtLonMin.Size = New System.Drawing.Size(29, 22)
        Me.txtLonMin.TabIndex = 58
        Me.txtLonMin.Visible = False
        '
        'lblUploadURL
        '
        Me.lblUploadURL.Location = New System.Drawing.Point(3, 7)
        Me.lblUploadURL.Name = "lblUploadURL"
        Me.lblUploadURL.Size = New System.Drawing.Size(64, 15)
        Me.lblUploadURL.Text = "Tile Server"
        '
        'comboTileServer
        '
        Me.comboTileServer.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.comboTileServer.Location = New System.Drawing.Point(73, 3)
        Me.comboTileServer.Name = "comboTileServer"
        Me.comboTileServer.Size = New System.Drawing.Size(64, 22)
        Me.comboTileServer.TabIndex = 74
        '
        'comboOverlayServer
        '
        Me.comboOverlayServer.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.comboOverlayServer.Location = New System.Drawing.Point(143, 3)
        Me.comboOverlayServer.Name = "comboOverlayServer"
        Me.comboOverlayServer.Size = New System.Drawing.Size(64, 22)
        Me.comboOverlayServer.TabIndex = 81
        '
        'frmDownloadMobile
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoScroll = True
        Me.AutoValidate = System.Windows.Forms.AutoValidate.Disable
        Me.ClientSize = New System.Drawing.Size(176, 180)
        Me.Controls.Add(Me.comboOverlayServer)
        Me.Controls.Add(Me.comboTileServer)
        Me.Controls.Add(Me.lblUploadURL)
        Me.Controls.Add(Me.comboDegreeFormat)
        Me.Controls.Add(Me.lblDegreeFormat)
        Me.Controls.Add(Me.lblGPSsymbolSize)
        Me.Controls.Add(Me.txtGPSSymbolSize)
        Me.Controls.Add(Me.txtTileFolder)
        Me.Controls.Add(Me.lblTileFolder)
        Me.Controls.Add(Me.lblLon)
        Me.Controls.Add(Me.lblLat)
        Me.Controls.Add(Me.txtLatDeg)
        Me.Controls.Add(Me.txtLonSec)
        Me.Controls.Add(Me.txtLonMin)
        Me.Controls.Add(Me.txtLatSec)
        Me.Controls.Add(Me.txtLatMin)
        Me.Controls.Add(Me.txtLonDecimalMin)
        Me.Controls.Add(Me.txtLonDeg)
        Me.Controls.Add(Me.txtLatDecimalMin)
        Me.Controls.Add(Me.txtLon)
        Me.Controls.Add(Me.txtLat)
        Me.Menu = Me.mnuMain
        Me.Name = "frmDownloadMobile"
        Me.Text = "Map Options"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents txtLat As System.Windows.Forms.TextBox
    Friend WithEvents lblLat As System.Windows.Forms.Label
    Friend WithEvents lblLon As System.Windows.Forms.Label
    Friend WithEvents txtLon As System.Windows.Forms.TextBox
    Friend WithEvents lblTileFolder As System.Windows.Forms.Label
    Friend WithEvents txtTileFolder As System.Windows.Forms.TextBox
    Friend WithEvents mnuOk As System.Windows.Forms.MenuItem
    Friend WithEvents mnuCancel As System.Windows.Forms.MenuItem
    Friend WithEvents lblGPSsymbolSize As System.Windows.Forms.Label
    Friend WithEvents txtGPSSymbolSize As System.Windows.Forms.TextBox
    Friend WithEvents lblDegreeFormat As System.Windows.Forms.Label
    Friend WithEvents comboDegreeFormat As System.Windows.Forms.ComboBox
    Friend WithEvents txtLatDeg As System.Windows.Forms.TextBox
    Friend WithEvents txtLatMin As System.Windows.Forms.TextBox
    Friend WithEvents txtLatSec As System.Windows.Forms.TextBox
    Friend WithEvents txtLatDecimalMin As System.Windows.Forms.TextBox
    Friend WithEvents txtLonDecimalMin As System.Windows.Forms.TextBox
    Friend WithEvents txtLonDeg As System.Windows.Forms.TextBox
    Friend WithEvents txtLonSec As System.Windows.Forms.TextBox
    Friend WithEvents txtLonMin As System.Windows.Forms.TextBox
    Friend WithEvents lblUploadURL As System.Windows.Forms.Label
    Friend WithEvents comboTileServer As System.Windows.Forms.ComboBox
    Friend WithEvents comboOverlayServer As System.Windows.Forms.ComboBox
End Class
