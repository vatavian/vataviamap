<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Public Class frmUploadMobile
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
        Me.txtUploadURL = New System.Windows.Forms.TextBox
        Me.lblUploadURL = New System.Windows.Forms.Label
        Me.chkUploadNow = New System.Windows.Forms.CheckBox
        Me.lblUploadWhen = New System.Windows.Forms.Label
        Me.chkUploadOnStart = New System.Windows.Forms.CheckBox
        Me.chkUploadOnStop = New System.Windows.Forms.CheckBox
        Me.chkUploadInterval = New System.Windows.Forms.CheckBox
        Me.txtUploadInterval = New System.Windows.Forms.TextBox
        Me.lblUploadIntervalUnits = New System.Windows.Forms.Label
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
        Me.txtLat.Location = New System.Drawing.Point(73, 59)
        Me.txtLat.Name = "txtLat"
        Me.txtLat.Size = New System.Drawing.Size(194, 22)
        Me.txtLat.TabIndex = 2
        '
        'lblLat
        '
        Me.lblLat.Location = New System.Drawing.Point(3, 63)
        Me.lblLat.Name = "lblLat"
        Me.lblLat.Size = New System.Drawing.Size(64, 15)
        Me.lblLat.Text = "Current Lat"
        '
        'lblLon
        '
        Me.lblLon.Location = New System.Drawing.Point(3, 91)
        Me.lblLon.Name = "lblLon"
        Me.lblLon.Size = New System.Drawing.Size(72, 15)
        Me.lblLon.Text = "Current Lon"
        '
        'txtLon
        '
        Me.txtLon.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtLon.Location = New System.Drawing.Point(73, 87)
        Me.txtLon.Name = "txtLon"
        Me.txtLon.Size = New System.Drawing.Size(194, 22)
        Me.txtLon.TabIndex = 7
        '
        'lblDegreeFormat
        '
        Me.lblDegreeFormat.Location = New System.Drawing.Point(3, 31)
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
        Me.comboDegreeFormat.Location = New System.Drawing.Point(73, 31)
        Me.comboDegreeFormat.Name = "comboDegreeFormat"
        Me.comboDegreeFormat.Size = New System.Drawing.Size(194, 22)
        Me.comboDegreeFormat.TabIndex = 1
        '
        'txtLatDeg
        '
        Me.txtLatDeg.Location = New System.Drawing.Point(73, 59)
        Me.txtLatDeg.Name = "txtLatDeg"
        Me.txtLatDeg.Size = New System.Drawing.Size(29, 22)
        Me.txtLatDeg.TabIndex = 3
        Me.txtLatDeg.Visible = False
        '
        'txtLatMin
        '
        Me.txtLatMin.Location = New System.Drawing.Point(108, 59)
        Me.txtLatMin.Name = "txtLatMin"
        Me.txtLatMin.Size = New System.Drawing.Size(29, 22)
        Me.txtLatMin.TabIndex = 4
        Me.txtLatMin.Visible = False
        '
        'txtLatSec
        '
        Me.txtLatSec.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtLatSec.Location = New System.Drawing.Point(143, 59)
        Me.txtLatSec.Name = "txtLatSec"
        Me.txtLatSec.Size = New System.Drawing.Size(124, 22)
        Me.txtLatSec.TabIndex = 5
        Me.txtLatSec.Visible = False
        '
        'txtLatDecimalMin
        '
        Me.txtLatDecimalMin.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtLatDecimalMin.Location = New System.Drawing.Point(108, 59)
        Me.txtLatDecimalMin.Name = "txtLatDecimalMin"
        Me.txtLatDecimalMin.Size = New System.Drawing.Size(159, 22)
        Me.txtLatDecimalMin.TabIndex = 6
        Me.txtLatDecimalMin.Visible = False
        '
        'txtLonDecimalMin
        '
        Me.txtLonDecimalMin.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtLonDecimalMin.Location = New System.Drawing.Point(108, 87)
        Me.txtLonDecimalMin.Name = "txtLonDecimalMin"
        Me.txtLonDecimalMin.Size = New System.Drawing.Size(159, 22)
        Me.txtLonDecimalMin.TabIndex = 11
        Me.txtLonDecimalMin.Visible = False
        '
        'txtLonDeg
        '
        Me.txtLonDeg.Location = New System.Drawing.Point(73, 87)
        Me.txtLonDeg.Name = "txtLonDeg"
        Me.txtLonDeg.Size = New System.Drawing.Size(29, 22)
        Me.txtLonDeg.TabIndex = 8
        Me.txtLonDeg.Visible = False
        '
        'txtLonSec
        '
        Me.txtLonSec.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtLonSec.Location = New System.Drawing.Point(143, 87)
        Me.txtLonSec.Name = "txtLonSec"
        Me.txtLonSec.Size = New System.Drawing.Size(124, 22)
        Me.txtLonSec.TabIndex = 10
        Me.txtLonSec.Visible = False
        '
        'txtLonMin
        '
        Me.txtLonMin.Location = New System.Drawing.Point(108, 87)
        Me.txtLonMin.Name = "txtLonMin"
        Me.txtLonMin.Size = New System.Drawing.Size(29, 22)
        Me.txtLonMin.TabIndex = 9
        Me.txtLonMin.Visible = False
        '
        'txtUploadURL
        '
        Me.txtUploadURL.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtUploadURL.Location = New System.Drawing.Point(73, 3)
        Me.txtUploadURL.Name = "txtUploadURL"
        Me.txtUploadURL.Size = New System.Drawing.Size(194, 22)
        Me.txtUploadURL.TabIndex = 0
        '
        'lblUploadURL
        '
        Me.lblUploadURL.Location = New System.Drawing.Point(3, 3)
        Me.lblUploadURL.Name = "lblUploadURL"
        Me.lblUploadURL.Size = New System.Drawing.Size(64, 15)
        Me.lblUploadURL.Text = "Upload URL"
        '
        'chkUploadNow
        '
        Me.chkUploadNow.Location = New System.Drawing.Point(108, 115)
        Me.chkUploadNow.Name = "chkUploadNow"
        Me.chkUploadNow.Size = New System.Drawing.Size(63, 22)
        Me.chkUploadNow.TabIndex = 12
        Me.chkUploadNow.Text = "Now"
        '
        'lblUploadWhen
        '
        Me.lblUploadWhen.Location = New System.Drawing.Point(3, 115)
        Me.lblUploadWhen.Name = "lblUploadWhen"
        Me.lblUploadWhen.Size = New System.Drawing.Size(99, 22)
        Me.lblUploadWhen.Text = "Upload position:"
        '
        'chkUploadOnStart
        '
        Me.chkUploadOnStart.Location = New System.Drawing.Point(4, 137)
        Me.chkUploadOnStart.Name = "chkUploadOnStart"
        Me.chkUploadOnStart.Size = New System.Drawing.Size(71, 22)
        Me.chkUploadOnStart.TabIndex = 13
        Me.chkUploadOnStart.Text = "GPS Start"
        '
        'chkUploadOnStop
        '
        Me.chkUploadOnStop.Location = New System.Drawing.Point(108, 137)
        Me.chkUploadOnStop.Name = "chkUploadOnStop"
        Me.chkUploadOnStop.Size = New System.Drawing.Size(71, 22)
        Me.chkUploadOnStop.TabIndex = 14
        Me.chkUploadOnStop.Text = "GPS Stop"
        '
        'chkUploadInterval
        '
        Me.chkUploadInterval.Location = New System.Drawing.Point(4, 158)
        Me.chkUploadInterval.Name = "chkUploadInterval"
        Me.chkUploadInterval.Size = New System.Drawing.Size(51, 22)
        Me.chkUploadInterval.TabIndex = 15
        Me.chkUploadInterval.Text = "Every"
        '
        'txtUploadInterval
        '
        Me.txtUploadInterval.Location = New System.Drawing.Point(73, 155)
        Me.txtUploadInterval.Name = "txtUploadInterval"
        Me.txtUploadInterval.Size = New System.Drawing.Size(29, 22)
        Me.txtUploadInterval.TabIndex = 16
        '
        'lblUploadIntervalUnits
        '
        Me.lblUploadIntervalUnits.Location = New System.Drawing.Point(108, 162)
        Me.lblUploadIntervalUnits.Name = "lblUploadIntervalUnits"
        Me.lblUploadIntervalUnits.Size = New System.Drawing.Size(46, 15)
        Me.lblUploadIntervalUnits.Text = "minutes"
        '
        'frmUploadMobile
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoScroll = True
        Me.AutoValidate = System.Windows.Forms.AutoValidate.Disable
        Me.ClientSize = New System.Drawing.Size(176, 180)
        Me.Controls.Add(Me.lblUploadIntervalUnits)
        Me.Controls.Add(Me.txtUploadInterval)
        Me.Controls.Add(Me.chkUploadInterval)
        Me.Controls.Add(Me.chkUploadOnStop)
        Me.Controls.Add(Me.chkUploadOnStart)
        Me.Controls.Add(Me.chkUploadNow)
        Me.Controls.Add(Me.txtUploadURL)
        Me.Controls.Add(Me.lblUploadURL)
        Me.Controls.Add(Me.comboDegreeFormat)
        Me.Controls.Add(Me.lblDegreeFormat)
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
        Me.Controls.Add(Me.lblUploadWhen)
        Me.Controls.Add(Me.lblLon)
        Me.Menu = Me.mnuMain
        Me.Name = "frmUploadMobile"
        Me.Text = "Upload Options"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents txtLat As System.Windows.Forms.TextBox
    Friend WithEvents lblLat As System.Windows.Forms.Label
    Friend WithEvents lblLon As System.Windows.Forms.Label
    Friend WithEvents txtLon As System.Windows.Forms.TextBox
    Friend WithEvents mnuOk As System.Windows.Forms.MenuItem
    Friend WithEvents mnuCancel As System.Windows.Forms.MenuItem
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
    Friend WithEvents txtUploadURL As System.Windows.Forms.TextBox
    Friend WithEvents lblUploadURL As System.Windows.Forms.Label
    Friend WithEvents chkUploadNow As System.Windows.Forms.CheckBox
    Friend WithEvents lblUploadWhen As System.Windows.Forms.Label
    Friend WithEvents chkUploadOnStart As System.Windows.Forms.CheckBox
    Friend WithEvents chkUploadOnStop As System.Windows.Forms.CheckBox
    Friend WithEvents chkUploadInterval As System.Windows.Forms.CheckBox
    Friend WithEvents txtUploadInterval As System.Windows.Forms.TextBox
    Friend WithEvents lblUploadIntervalUnits As System.Windows.Forms.Label
End Class
