<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmBuddyAlarm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
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
        Me.txtLat = New System.Windows.Forms.TextBox
        Me.lblDirections = New System.Windows.Forms.Label
        Me.lblLat = New System.Windows.Forms.Label
        Me.lblLon = New System.Windows.Forms.Label
        Me.txtLon = New System.Windows.Forms.TextBox
        Me.lblDistance = New System.Windows.Forms.Label
        Me.txtDistance = New System.Windows.Forms.TextBox
        Me.Label1 = New System.Windows.Forms.Label
        Me.btnOk = New System.Windows.Forms.Button
        Me.comboDistanceUnits = New System.Windows.Forms.ComboBox
        Me.chkEnable = New System.Windows.Forms.CheckBox
        Me.SuspendLayout()
        '
        'txtLat
        '
        Me.txtLat.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtLat.Location = New System.Drawing.Point(162, 61)
        Me.txtLat.Name = "txtLat"
        Me.txtLat.Size = New System.Drawing.Size(118, 20)
        Me.txtLat.TabIndex = 0
        '
        'lblDirections
        '
        Me.lblDirections.AutoSize = True
        Me.lblDirections.Location = New System.Drawing.Point(12, 9)
        Me.lblDirections.Name = "lblDirections"
        Me.lblDirections.Size = New System.Drawing.Size(242, 13)
        Me.lblDirections.TabIndex = 1
        Me.lblDirections.Text = "Click map to set location and drag to set distance,"
        '
        'lblLat
        '
        Me.lblLat.AutoSize = True
        Me.lblLat.Location = New System.Drawing.Point(12, 64)
        Me.lblLat.Name = "lblLat"
        Me.lblLat.Size = New System.Drawing.Size(45, 13)
        Me.lblLat.TabIndex = 2
        Me.lblLat.Text = "Latitude"
        '
        'lblLon
        '
        Me.lblLon.AutoSize = True
        Me.lblLon.Location = New System.Drawing.Point(12, 90)
        Me.lblLon.Name = "lblLon"
        Me.lblLon.Size = New System.Drawing.Size(54, 13)
        Me.lblLon.TabIndex = 4
        Me.lblLon.Text = "Longitude"
        '
        'txtLon
        '
        Me.txtLon.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtLon.Location = New System.Drawing.Point(162, 87)
        Me.txtLon.Name = "txtLon"
        Me.txtLon.Size = New System.Drawing.Size(118, 20)
        Me.txtLon.TabIndex = 3
        '
        'lblDistance
        '
        Me.lblDistance.AutoSize = True
        Me.lblDistance.Location = New System.Drawing.Point(12, 116)
        Me.lblDistance.Name = "lblDistance"
        Me.lblDistance.Size = New System.Drawing.Size(49, 13)
        Me.lblDistance.TabIndex = 6
        Me.lblDistance.Text = "Distance"
        '
        'txtDistance
        '
        Me.txtDistance.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtDistance.Location = New System.Drawing.Point(162, 113)
        Me.txtDistance.Name = "txtDistance"
        Me.txtDistance.Size = New System.Drawing.Size(118, 20)
        Me.txtDistance.TabIndex = 5
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 34)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(111, 13)
        Me.Label1.TabIndex = 7
        Me.Label1.Text = "or enter values below:"
        '
        'btnOk
        '
        Me.btnOk.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.btnOk.Location = New System.Drawing.Point(205, 149)
        Me.btnOk.Name = "btnOk"
        Me.btnOk.Size = New System.Drawing.Size(75, 23)
        Me.btnOk.TabIndex = 8
        Me.btnOk.Text = "Ok"
        Me.btnOk.UseVisualStyleBackColor = True
        '
        'comboDistanceUnits
        '
        Me.comboDistanceUnits.FormattingEnabled = True
        Me.comboDistanceUnits.Items.AddRange(New Object() {"miles", "kilometers", "meters"})
        Me.comboDistanceUnits.Location = New System.Drawing.Point(87, 112)
        Me.comboDistanceUnits.Name = "comboDistanceUnits"
        Me.comboDistanceUnits.Size = New System.Drawing.Size(69, 21)
        Me.comboDistanceUnits.TabIndex = 9
        Me.comboDistanceUnits.Text = "meters"
        '
        'chkEnable
        '
        Me.chkEnable.AutoSize = True
        Me.chkEnable.Checked = True
        Me.chkEnable.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkEnable.Location = New System.Drawing.Point(15, 153)
        Me.chkEnable.Name = "chkEnable"
        Me.chkEnable.Size = New System.Drawing.Size(88, 17)
        Me.chkEnable.TabIndex = 10
        Me.chkEnable.Text = "Enable Alarm"
        Me.chkEnable.UseVisualStyleBackColor = True
        '
        'frmBuddyAlarm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(292, 184)
        Me.Controls.Add(Me.chkEnable)
        Me.Controls.Add(Me.comboDistanceUnits)
        Me.Controls.Add(Me.btnOk)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.lblDistance)
        Me.Controls.Add(Me.txtDistance)
        Me.Controls.Add(Me.lblLon)
        Me.Controls.Add(Me.txtLon)
        Me.Controls.Add(Me.lblLat)
        Me.Controls.Add(Me.lblDirections)
        Me.Controls.Add(Me.txtLat)
        Me.Name = "frmBuddyAlarm"
        Me.Text = "Buddy Alarm"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents txtLat As System.Windows.Forms.TextBox
    Friend WithEvents lblDirections As System.Windows.Forms.Label
    Friend WithEvents lblLat As System.Windows.Forms.Label
    Friend WithEvents lblLon As System.Windows.Forms.Label
    Friend WithEvents txtLon As System.Windows.Forms.TextBox
    Friend WithEvents lblDistance As System.Windows.Forms.Label
    Friend WithEvents txtDistance As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents btnOk As System.Windows.Forms.Button
    Friend WithEvents comboDistanceUnits As System.Windows.Forms.ComboBox
    Friend WithEvents chkEnable As System.Windows.Forms.CheckBox
End Class
