<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmLayers
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
        Me.lstLayers = New System.Windows.Forms.CheckedListBox
        Me.chkDifferentColors = New System.Windows.Forms.CheckBox
        Me.txtWidth = New System.Windows.Forms.TextBox
        Me.lblWidth = New System.Windows.Forms.Label
        Me.lblOpacity = New System.Windows.Forms.Label
        Me.txtOpacity = New System.Windows.Forms.TextBox
        Me.lblArrowSize = New System.Windows.Forms.Label
        Me.txtArrowSize = New System.Windows.Forms.TextBox
        Me.SuspendLayout()
        '
        'lstLayers
        '
        Me.lstLayers.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lstLayers.CheckOnClick = True
        Me.lstLayers.FormattingEnabled = True
        Me.lstLayers.IntegralHeight = False
        Me.lstLayers.Location = New System.Drawing.Point(0, 0)
        Me.lstLayers.Name = "lstLayers"
        Me.lstLayers.Size = New System.Drawing.Size(270, 278)
        Me.lstLayers.TabIndex = 0
        '
        'chkDifferentColors
        '
        Me.chkDifferentColors.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.chkDifferentColors.AutoSize = True
        Me.chkDifferentColors.Location = New System.Drawing.Point(12, 284)
        Me.chkDifferentColors.Name = "chkDifferentColors"
        Me.chkDifferentColors.Size = New System.Drawing.Size(98, 17)
        Me.chkDifferentColors.TabIndex = 1
        Me.chkDifferentColors.Text = "Different Colors"
        Me.chkDifferentColors.UseVisualStyleBackColor = True
        '
        'txtWidth
        '
        Me.txtWidth.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.txtWidth.Location = New System.Drawing.Point(82, 307)
        Me.txtWidth.Name = "txtWidth"
        Me.txtWidth.Size = New System.Drawing.Size(28, 20)
        Me.txtWidth.TabIndex = 2
        '
        'lblWidth
        '
        Me.lblWidth.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblWidth.AutoSize = True
        Me.lblWidth.Location = New System.Drawing.Point(12, 310)
        Me.lblWidth.Name = "lblWidth"
        Me.lblWidth.Size = New System.Drawing.Size(35, 13)
        Me.lblWidth.TabIndex = 3
        Me.lblWidth.Text = "Width"
        '
        'lblOpacity
        '
        Me.lblOpacity.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblOpacity.AutoSize = True
        Me.lblOpacity.Location = New System.Drawing.Point(12, 336)
        Me.lblOpacity.Name = "lblOpacity"
        Me.lblOpacity.Size = New System.Drawing.Size(43, 13)
        Me.lblOpacity.TabIndex = 5
        Me.lblOpacity.Text = "Opacity"
        '
        'txtOpacity
        '
        Me.txtOpacity.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.txtOpacity.Location = New System.Drawing.Point(82, 333)
        Me.txtOpacity.Name = "txtOpacity"
        Me.txtOpacity.Size = New System.Drawing.Size(28, 20)
        Me.txtOpacity.TabIndex = 4
        '
        'lblArrowSize
        '
        Me.lblArrowSize.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblArrowSize.AutoSize = True
        Me.lblArrowSize.Location = New System.Drawing.Point(158, 310)
        Me.lblArrowSize.Name = "lblArrowSize"
        Me.lblArrowSize.Size = New System.Drawing.Size(57, 13)
        Me.lblArrowSize.TabIndex = 7
        Me.lblArrowSize.Text = "Arrow Size"
        '
        'txtArrowSize
        '
        Me.txtArrowSize.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.txtArrowSize.Location = New System.Drawing.Point(228, 307)
        Me.txtArrowSize.Name = "txtArrowSize"
        Me.txtArrowSize.Size = New System.Drawing.Size(28, 20)
        Me.txtArrowSize.TabIndex = 6
        '
        'frmLayers
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(270, 365)
        Me.Controls.Add(Me.lblArrowSize)
        Me.Controls.Add(Me.txtArrowSize)
        Me.Controls.Add(Me.lblOpacity)
        Me.Controls.Add(Me.txtOpacity)
        Me.Controls.Add(Me.lblWidth)
        Me.Controls.Add(Me.txtWidth)
        Me.Controls.Add(Me.chkDifferentColors)
        Me.Controls.Add(Me.lstLayers)
        Me.Name = "frmLayers"
        Me.Text = "Layers"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lstLayers As System.Windows.Forms.CheckedListBox
    Friend WithEvents chkDifferentColors As System.Windows.Forms.CheckBox
    Friend WithEvents txtWidth As System.Windows.Forms.TextBox
    Friend WithEvents lblWidth As System.Windows.Forms.Label
    Friend WithEvents lblOpacity As System.Windows.Forms.Label
    Friend WithEvents txtOpacity As System.Windows.Forms.TextBox
    Friend WithEvents lblArrowSize As System.Windows.Forms.Label
    Friend WithEvents txtArrowSize As System.Windows.Forms.TextBox
End Class
