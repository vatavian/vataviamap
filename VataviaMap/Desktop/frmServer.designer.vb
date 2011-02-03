<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmServer
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
        Me.lblName = New System.Windows.Forms.Label
        Me.txtName = New System.Windows.Forms.TextBox
        Me.txtLink = New System.Windows.Forms.TextBox
        Me.lblLink = New System.Windows.Forms.Label
        Me.btnOk = New System.Windows.Forms.Button
        Me.lblExamples = New System.Windows.Forms.Label
        Me.btnExamples = New System.Windows.Forms.Button
        Me.btnRemove = New System.Windows.Forms.Button
        Me.txtTilePattern = New System.Windows.Forms.TextBox
        Me.lblTilePattern = New System.Windows.Forms.Label
        Me.txtWebmapPattern = New System.Windows.Forms.TextBox
        Me.lblWebmapPattern = New System.Windows.Forms.Label
        Me.txtCopyright = New System.Windows.Forms.TextBox
        Me.lblCopyright = New System.Windows.Forms.Label
        Me.txtZoomMin = New System.Windows.Forms.TextBox
        Me.lblZoomMin = New System.Windows.Forms.Label
        Me.txtZoomMax = New System.Windows.Forms.TextBox
        Me.lblZoomMax = New System.Windows.Forms.Label
        Me.txtLatMax = New System.Windows.Forms.TextBox
        Me.lblLatMax = New System.Windows.Forms.Label
        Me.txtLatMin = New System.Windows.Forms.TextBox
        Me.lblLatMin = New System.Windows.Forms.Label
        Me.txtLonMax = New System.Windows.Forms.TextBox
        Me.lblLonMax = New System.Windows.Forms.Label
        Me.txtLonMin = New System.Windows.Forms.TextBox
        Me.lblLonMin = New System.Windows.Forms.Label
        Me.chkTransparent = New System.Windows.Forms.CheckBox
        Me.SuspendLayout()
        '
        'lblName
        '
        Me.lblName.AutoSize = True
        Me.lblName.Location = New System.Drawing.Point(12, 15)
        Me.lblName.Name = "lblName"
        Me.lblName.Size = New System.Drawing.Size(35, 13)
        Me.lblName.TabIndex = 0
        Me.lblName.Text = "Name"
        '
        'txtName
        '
        Me.txtName.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtName.Location = New System.Drawing.Point(116, 12)
        Me.txtName.Name = "txtName"
        Me.txtName.Size = New System.Drawing.Size(230, 20)
        Me.txtName.TabIndex = 1
        '
        'txtLink
        '
        Me.txtLink.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtLink.Location = New System.Drawing.Point(116, 38)
        Me.txtLink.Name = "txtLink"
        Me.txtLink.Size = New System.Drawing.Size(230, 20)
        Me.txtLink.TabIndex = 3
        '
        'lblLink
        '
        Me.lblLink.AutoSize = True
        Me.lblLink.Location = New System.Drawing.Point(12, 41)
        Me.lblLink.Name = "lblLink"
        Me.lblLink.Size = New System.Drawing.Size(58, 13)
        Me.lblLink.TabIndex = 2
        Me.lblLink.Text = "Main Page"
        '
        'btnOk
        '
        Me.btnOk.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnOk.Location = New System.Drawing.Point(271, 270)
        Me.btnOk.Name = "btnOk"
        Me.btnOk.Size = New System.Drawing.Size(75, 23)
        Me.btnOk.TabIndex = 4
        Me.btnOk.Text = "Ok"
        Me.btnOk.UseVisualStyleBackColor = True
        '
        'lblExamples
        '
        Me.lblExamples.AutoSize = True
        Me.lblExamples.Location = New System.Drawing.Point(12, 69)
        Me.lblExamples.Name = "lblExamples"
        Me.lblExamples.Size = New System.Drawing.Size(0, 13)
        Me.lblExamples.TabIndex = 5
        '
        'btnExamples
        '
        Me.btnExamples.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnExamples.Location = New System.Drawing.Point(109, 270)
        Me.btnExamples.Name = "btnExamples"
        Me.btnExamples.Size = New System.Drawing.Size(75, 23)
        Me.btnExamples.TabIndex = 6
        Me.btnExamples.Text = "Examples..."
        Me.btnExamples.UseVisualStyleBackColor = True
        '
        'btnRemove
        '
        Me.btnRemove.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnRemove.Location = New System.Drawing.Point(190, 270)
        Me.btnRemove.Name = "btnRemove"
        Me.btnRemove.Size = New System.Drawing.Size(75, 23)
        Me.btnRemove.TabIndex = 7
        Me.btnRemove.Text = "Remove"
        Me.btnRemove.UseVisualStyleBackColor = True
        '
        'txtTilePattern
        '
        Me.txtTilePattern.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtTilePattern.Location = New System.Drawing.Point(116, 64)
        Me.txtTilePattern.Name = "txtTilePattern"
        Me.txtTilePattern.Size = New System.Drawing.Size(230, 20)
        Me.txtTilePattern.TabIndex = 9
        '
        'lblTilePattern
        '
        Me.lblTilePattern.AutoSize = True
        Me.lblTilePattern.Location = New System.Drawing.Point(12, 67)
        Me.lblTilePattern.Name = "lblTilePattern"
        Me.lblTilePattern.Size = New System.Drawing.Size(61, 13)
        Me.lblTilePattern.TabIndex = 8
        Me.lblTilePattern.Text = "Tile Pattern"
        '
        'txtWebmapPattern
        '
        Me.txtWebmapPattern.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtWebmapPattern.Location = New System.Drawing.Point(116, 90)
        Me.txtWebmapPattern.Name = "txtWebmapPattern"
        Me.txtWebmapPattern.Size = New System.Drawing.Size(230, 20)
        Me.txtWebmapPattern.TabIndex = 11
        '
        'lblWebmapPattern
        '
        Me.lblWebmapPattern.AutoSize = True
        Me.lblWebmapPattern.Location = New System.Drawing.Point(12, 93)
        Me.lblWebmapPattern.Name = "lblWebmapPattern"
        Me.lblWebmapPattern.Size = New System.Drawing.Size(91, 13)
        Me.lblWebmapPattern.TabIndex = 10
        Me.lblWebmapPattern.Text = "Web Map Pattern"
        '
        'txtCopyright
        '
        Me.txtCopyright.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtCopyright.Location = New System.Drawing.Point(116, 116)
        Me.txtCopyright.Name = "txtCopyright"
        Me.txtCopyright.Size = New System.Drawing.Size(230, 20)
        Me.txtCopyright.TabIndex = 13
        '
        'lblCopyright
        '
        Me.lblCopyright.AutoSize = True
        Me.lblCopyright.Location = New System.Drawing.Point(12, 119)
        Me.lblCopyright.Name = "lblCopyright"
        Me.lblCopyright.Size = New System.Drawing.Size(51, 13)
        Me.lblCopyright.TabIndex = 12
        Me.lblCopyright.Text = "Copyright"
        '
        'txtZoomMin
        '
        Me.txtZoomMin.Location = New System.Drawing.Point(116, 142)
        Me.txtZoomMin.Name = "txtZoomMin"
        Me.txtZoomMin.Size = New System.Drawing.Size(50, 20)
        Me.txtZoomMin.TabIndex = 15
        '
        'lblZoomMin
        '
        Me.lblZoomMin.AutoSize = True
        Me.lblZoomMin.Location = New System.Drawing.Point(12, 145)
        Me.lblZoomMin.Name = "lblZoomMin"
        Me.lblZoomMin.Size = New System.Drawing.Size(78, 13)
        Me.lblZoomMin.TabIndex = 14
        Me.lblZoomMin.Text = "Zoom Minimum"
        '
        'txtZoomMax
        '
        Me.txtZoomMax.Location = New System.Drawing.Point(296, 142)
        Me.txtZoomMax.Name = "txtZoomMax"
        Me.txtZoomMax.Size = New System.Drawing.Size(50, 20)
        Me.txtZoomMax.TabIndex = 17
        '
        'lblZoomMax
        '
        Me.lblZoomMax.AutoSize = True
        Me.lblZoomMax.Location = New System.Drawing.Point(189, 145)
        Me.lblZoomMax.Name = "lblZoomMax"
        Me.lblZoomMax.Size = New System.Drawing.Size(81, 13)
        Me.lblZoomMax.TabIndex = 16
        Me.lblZoomMax.Text = "Zoom Maximum"
        '
        'txtLatMax
        '
        Me.txtLatMax.Location = New System.Drawing.Point(296, 168)
        Me.txtLatMax.Name = "txtLatMax"
        Me.txtLatMax.Size = New System.Drawing.Size(50, 20)
        Me.txtLatMax.TabIndex = 21
        '
        'lblLatMax
        '
        Me.lblLatMax.AutoSize = True
        Me.lblLatMax.Location = New System.Drawing.Point(189, 171)
        Me.lblLatMax.Name = "lblLatMax"
        Me.lblLatMax.Size = New System.Drawing.Size(92, 13)
        Me.lblLatMax.TabIndex = 20
        Me.lblLatMax.Text = "Latitude Maximum"
        '
        'txtLatMin
        '
        Me.txtLatMin.Location = New System.Drawing.Point(116, 168)
        Me.txtLatMin.Name = "txtLatMin"
        Me.txtLatMin.Size = New System.Drawing.Size(50, 20)
        Me.txtLatMin.TabIndex = 19
        '
        'lblLatMin
        '
        Me.lblLatMin.AutoSize = True
        Me.lblLatMin.Location = New System.Drawing.Point(12, 171)
        Me.lblLatMin.Name = "lblLatMin"
        Me.lblLatMin.Size = New System.Drawing.Size(89, 13)
        Me.lblLatMin.TabIndex = 18
        Me.lblLatMin.Text = "Latitude Minimum"
        '
        'txtLonMax
        '
        Me.txtLonMax.Location = New System.Drawing.Point(296, 194)
        Me.txtLonMax.Name = "txtLonMax"
        Me.txtLonMax.Size = New System.Drawing.Size(50, 20)
        Me.txtLonMax.TabIndex = 25
        '
        'lblLonMax
        '
        Me.lblLonMax.AutoSize = True
        Me.lblLonMax.Location = New System.Drawing.Point(189, 197)
        Me.lblLonMax.Name = "lblLonMax"
        Me.lblLonMax.Size = New System.Drawing.Size(101, 13)
        Me.lblLonMax.TabIndex = 24
        Me.lblLonMax.Text = "Longitude Maximum"
        '
        'txtLonMin
        '
        Me.txtLonMin.Location = New System.Drawing.Point(116, 194)
        Me.txtLonMin.Name = "txtLonMin"
        Me.txtLonMin.Size = New System.Drawing.Size(50, 20)
        Me.txtLonMin.TabIndex = 23
        '
        'lblLonMin
        '
        Me.lblLonMin.AutoSize = True
        Me.lblLonMin.Location = New System.Drawing.Point(12, 197)
        Me.lblLonMin.Name = "lblLonMin"
        Me.lblLonMin.Size = New System.Drawing.Size(98, 13)
        Me.lblLonMin.TabIndex = 22
        Me.lblLonMin.Text = "Longitude Minimum"
        '
        'chkTransparent
        '
        Me.chkTransparent.AutoSize = True
        Me.chkTransparent.Location = New System.Drawing.Point(116, 229)
        Me.chkTransparent.Name = "chkTransparent"
        Me.chkTransparent.Size = New System.Drawing.Size(83, 17)
        Me.chkTransparent.TabIndex = 26
        Me.chkTransparent.Text = "Transparent"
        Me.chkTransparent.UseVisualStyleBackColor = True
        '
        'frmServer
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(358, 305)
        Me.Controls.Add(Me.chkTransparent)
        Me.Controls.Add(Me.txtLonMax)
        Me.Controls.Add(Me.lblLonMax)
        Me.Controls.Add(Me.txtLonMin)
        Me.Controls.Add(Me.lblLonMin)
        Me.Controls.Add(Me.txtLatMax)
        Me.Controls.Add(Me.lblLatMax)
        Me.Controls.Add(Me.txtLatMin)
        Me.Controls.Add(Me.lblLatMin)
        Me.Controls.Add(Me.txtZoomMax)
        Me.Controls.Add(Me.lblZoomMax)
        Me.Controls.Add(Me.txtZoomMin)
        Me.Controls.Add(Me.lblZoomMin)
        Me.Controls.Add(Me.txtCopyright)
        Me.Controls.Add(Me.lblCopyright)
        Me.Controls.Add(Me.txtWebmapPattern)
        Me.Controls.Add(Me.lblWebmapPattern)
        Me.Controls.Add(Me.txtTilePattern)
        Me.Controls.Add(Me.lblTilePattern)
        Me.Controls.Add(Me.btnRemove)
        Me.Controls.Add(Me.btnExamples)
        Me.Controls.Add(Me.lblExamples)
        Me.Controls.Add(Me.btnOk)
        Me.Controls.Add(Me.txtLink)
        Me.Controls.Add(Me.lblLink)
        Me.Controls.Add(Me.txtName)
        Me.Controls.Add(Me.lblName)
        Me.Name = "frmServer"
        Me.Text = "Tile Server"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lblName As System.Windows.Forms.Label
    Friend WithEvents txtName As System.Windows.Forms.TextBox
    Friend WithEvents txtLink As System.Windows.Forms.TextBox
    Friend WithEvents lblLink As System.Windows.Forms.Label
    Friend WithEvents btnOk As System.Windows.Forms.Button
    Friend WithEvents lblExamples As System.Windows.Forms.Label
    Friend WithEvents btnExamples As System.Windows.Forms.Button
    Friend WithEvents btnRemove As System.Windows.Forms.Button
    Friend WithEvents txtTilePattern As System.Windows.Forms.TextBox
    Friend WithEvents lblTilePattern As System.Windows.Forms.Label
    Friend WithEvents txtWebmapPattern As System.Windows.Forms.TextBox
    Friend WithEvents lblWebmapPattern As System.Windows.Forms.Label
    Friend WithEvents txtCopyright As System.Windows.Forms.TextBox
    Friend WithEvents lblCopyright As System.Windows.Forms.Label
    Friend WithEvents txtZoomMin As System.Windows.Forms.TextBox
    Friend WithEvents lblZoomMin As System.Windows.Forms.Label
    Friend WithEvents txtZoomMax As System.Windows.Forms.TextBox
    Friend WithEvents lblZoomMax As System.Windows.Forms.Label
    Friend WithEvents txtLatMax As System.Windows.Forms.TextBox
    Friend WithEvents lblLatMax As System.Windows.Forms.Label
    Friend WithEvents txtLatMin As System.Windows.Forms.TextBox
    Friend WithEvents lblLatMin As System.Windows.Forms.Label
    Friend WithEvents txtLonMax As System.Windows.Forms.TextBox
    Friend WithEvents lblLonMax As System.Windows.Forms.Label
    Friend WithEvents txtLonMin As System.Windows.Forms.TextBox
    Friend WithEvents lblLonMin As System.Windows.Forms.Label
    Friend WithEvents chkTransparent As System.Windows.Forms.CheckBox
End Class
