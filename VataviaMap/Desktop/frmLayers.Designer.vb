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
        Me.components = New System.ComponentModel.Container
        Me.txtWidth = New System.Windows.Forms.TextBox
        Me.lblWidth = New System.Windows.Forms.Label
        Me.lblOpacity = New System.Windows.Forms.Label
        Me.txtOpacity = New System.Windows.Forms.TextBox
        Me.lblArrowSize = New System.Windows.Forms.Label
        Me.txtArrowSize = New System.Windows.Forms.TextBox
        Me.lstLayers = New System.Windows.Forms.ListView
        Me.Filename = New System.Windows.Forms.ColumnHeader
        Me.chkAllVisible = New System.Windows.Forms.CheckBox
        Me.grpColor = New System.Windows.Forms.GroupBox
        Me.btnColorSame = New System.Windows.Forms.Button
        Me.btnColorRamp = New System.Windows.Forms.Button
        Me.btnColorRandom = New System.Windows.Forms.Button
        Me.btnColor = New System.Windows.Forms.Button
        Me.Duration = New System.Windows.Forms.ColumnHeader
        Me.LayerGridRightContextMenuStrip = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ZoomToToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.DetailsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem
        Me.grpColor.SuspendLayout()
        Me.LayerGridRightContextMenuStrip.SuspendLayout()
        Me.SuspendLayout()
        '
        'txtWidth
        '
        Me.txtWidth.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.txtWidth.Location = New System.Drawing.Point(80, 604)
        Me.txtWidth.Name = "txtWidth"
        Me.txtWidth.Size = New System.Drawing.Size(28, 20)
        Me.txtWidth.TabIndex = 2
        '
        'lblWidth
        '
        Me.lblWidth.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblWidth.AutoSize = True
        Me.lblWidth.Location = New System.Drawing.Point(10, 607)
        Me.lblWidth.Name = "lblWidth"
        Me.lblWidth.Size = New System.Drawing.Size(35, 13)
        Me.lblWidth.TabIndex = 3
        Me.lblWidth.Text = "Width"
        '
        'lblOpacity
        '
        Me.lblOpacity.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblOpacity.AutoSize = True
        Me.lblOpacity.Location = New System.Drawing.Point(15, 111)
        Me.lblOpacity.Name = "lblOpacity"
        Me.lblOpacity.Size = New System.Drawing.Size(43, 13)
        Me.lblOpacity.TabIndex = 5
        Me.lblOpacity.Text = "Opacity"
        '
        'txtOpacity
        '
        Me.txtOpacity.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.txtOpacity.Location = New System.Drawing.Point(77, 108)
        Me.txtOpacity.Name = "txtOpacity"
        Me.txtOpacity.Size = New System.Drawing.Size(41, 20)
        Me.txtOpacity.TabIndex = 4
        '
        'lblArrowSize
        '
        Me.lblArrowSize.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblArrowSize.AutoSize = True
        Me.lblArrowSize.Location = New System.Drawing.Point(10, 633)
        Me.lblArrowSize.Name = "lblArrowSize"
        Me.lblArrowSize.Size = New System.Drawing.Size(57, 13)
        Me.lblArrowSize.TabIndex = 7
        Me.lblArrowSize.Text = "Arrow Size"
        '
        'txtArrowSize
        '
        Me.txtArrowSize.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.txtArrowSize.Location = New System.Drawing.Point(80, 630)
        Me.txtArrowSize.Name = "txtArrowSize"
        Me.txtArrowSize.Size = New System.Drawing.Size(28, 20)
        Me.txtArrowSize.TabIndex = 6
        '
        'lstLayers
        '
        Me.lstLayers.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lstLayers.CheckBoxes = True
        Me.lstLayers.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.Filename, Me.Duration})
        Me.lstLayers.Location = New System.Drawing.Point(0, 0)
        Me.lstLayers.Name = "lstLayers"
        Me.lstLayers.Size = New System.Drawing.Size(631, 516)
        Me.lstLayers.TabIndex = 8
        Me.lstLayers.UseCompatibleStateImageBehavior = False
        Me.lstLayers.View = System.Windows.Forms.View.Details
        '
        'Filename
        '
        Me.Filename.Text = "File Name"
        Me.Filename.Width = 600
        '
        'chkAllVisible
        '
        Me.chkAllVisible.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.chkAllVisible.AutoSize = True
        Me.chkAllVisible.Checked = True
        Me.chkAllVisible.CheckState = System.Windows.Forms.CheckState.Indeterminate
        Me.chkAllVisible.Location = New System.Drawing.Point(9, 522)
        Me.chkAllVisible.Name = "chkAllVisible"
        Me.chkAllVisible.Size = New System.Drawing.Size(70, 17)
        Me.chkAllVisible.TabIndex = 12
        Me.chkAllVisible.Text = "All Visible"
        Me.chkAllVisible.UseVisualStyleBackColor = True
        '
        'grpColor
        '
        Me.grpColor.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.grpColor.Controls.Add(Me.btnColorSame)
        Me.grpColor.Controls.Add(Me.btnColorRamp)
        Me.grpColor.Controls.Add(Me.btnColorRandom)
        Me.grpColor.Controls.Add(Me.btnColor)
        Me.grpColor.Controls.Add(Me.txtOpacity)
        Me.grpColor.Controls.Add(Me.lblOpacity)
        Me.grpColor.Location = New System.Drawing.Point(125, 522)
        Me.grpColor.Name = "grpColor"
        Me.grpColor.Size = New System.Drawing.Size(127, 135)
        Me.grpColor.TabIndex = 13
        Me.grpColor.TabStop = False
        Me.grpColor.Text = "Color"
        '
        'btnColorSame
        '
        Me.btnColorSame.Location = New System.Drawing.Point(6, 77)
        Me.btnColorSame.Name = "btnColorSame"
        Me.btnColorSame.Size = New System.Drawing.Size(65, 23)
        Me.btnColorSame.TabIndex = 16
        Me.btnColorSame.Text = "Same"
        Me.btnColorSame.UseVisualStyleBackColor = True
        '
        'btnColorRamp
        '
        Me.btnColorRamp.Location = New System.Drawing.Point(6, 48)
        Me.btnColorRamp.Name = "btnColorRamp"
        Me.btnColorRamp.Size = New System.Drawing.Size(65, 23)
        Me.btnColorRamp.TabIndex = 15
        Me.btnColorRamp.Text = "Ramp"
        Me.btnColorRamp.UseVisualStyleBackColor = True
        '
        'btnColorRandom
        '
        Me.btnColorRandom.Location = New System.Drawing.Point(6, 19)
        Me.btnColorRandom.Name = "btnColorRandom"
        Me.btnColorRandom.Size = New System.Drawing.Size(65, 23)
        Me.btnColorRandom.TabIndex = 14
        Me.btnColorRandom.Text = "Random"
        Me.btnColorRandom.UseVisualStyleBackColor = True
        '
        'btnColor
        '
        Me.btnColor.BackColor = System.Drawing.Color.Cyan
        Me.btnColor.Location = New System.Drawing.Point(77, 19)
        Me.btnColor.Name = "btnColor"
        Me.btnColor.Size = New System.Drawing.Size(41, 81)
        Me.btnColor.TabIndex = 13
        Me.btnColor.Text = "Set"
        Me.btnColor.UseVisualStyleBackColor = False
        '
        'LayerGridRightContextMenuStrip
        'Duration
        Me.LayerGridRightContextMenuStrip.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ZoomToToolStripMenuItem, Me.DetailsToolStripMenuItem})
        Me.LayerGridRightContextMenuStrip.Name = "LayerGridRightContextMenuStrip"
        Me.LayerGridRightContextMenuStrip.Size = New System.Drawing.Size(153, 70)
        '
        'ZoomToToolStripMenuItem
        '
        Me.ZoomToToolStripMenuItem.Name = "ZoomToToolStripMenuItem"
        Me.ZoomToToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.ZoomToToolStripMenuItem.Text = "ZoomTo"
        '
        'DetailsToolStripMenuItem
        '
        Me.DetailsToolStripMenuItem.Name = "DetailsToolStripMenuItem"
        Me.DetailsToolStripMenuItem.Size = New System.Drawing.Size(152, 22)
        Me.DetailsToolStripMenuItem.Text = "Details"
        '
        Me.Duration.Text = "Duration"
        'frmLayers
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(631, 669)
        Me.Controls.Add(Me.grpColor)
        Me.Controls.Add(Me.chkAllVisible)
        Me.Controls.Add(Me.lblArrowSize)
        Me.Controls.Add(Me.txtArrowSize)
        Me.Controls.Add(Me.lblWidth)
        Me.Controls.Add(Me.txtWidth)
        Me.Controls.Add(Me.lstLayers)
        Me.Name = "frmLayers"
        Me.Text = "Layers"
        Me.grpColor.ResumeLayout(False)
        Me.grpColor.PerformLayout()
        Me.LayerGridRightContextMenuStrip.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents txtWidth As System.Windows.Forms.TextBox
    Friend WithEvents lblWidth As System.Windows.Forms.Label
    Friend WithEvents lblOpacity As System.Windows.Forms.Label
    Friend WithEvents txtOpacity As System.Windows.Forms.TextBox
    Friend WithEvents lblArrowSize As System.Windows.Forms.Label
    Friend WithEvents txtArrowSize As System.Windows.Forms.TextBox
    Friend WithEvents lstLayers As System.Windows.Forms.ListView
    Friend WithEvents Filename As System.Windows.Forms.ColumnHeader
    Friend WithEvents chkAllVisible As System.Windows.Forms.CheckBox
    Friend WithEvents grpColor As System.Windows.Forms.GroupBox
    Friend WithEvents btnColor As System.Windows.Forms.Button
    Friend WithEvents btnColorSame As System.Windows.Forms.Button
    Friend WithEvents btnColorRamp As System.Windows.Forms.Button
    Friend WithEvents btnColorRandom As System.Windows.Forms.Button
    Friend WithEvents Duration As System.Windows.Forms.ColumnHeader
    Friend WithEvents LayerGridRightContextMenuStrip As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents ZoomToToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents DetailsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
End Class
