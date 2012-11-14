<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmCoordinates
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
        Me.lblWestNorth = New System.Windows.Forms.Label
        Me.txtWestNorth = New System.Windows.Forms.TextBox
        Me.txtCenter = New System.Windows.Forms.TextBox
        Me.lblCenter = New System.Windows.Forms.Label
        Me.txtEastSouth = New System.Windows.Forms.TextBox
        Me.lblEastSouth = New System.Windows.Forms.Label
        Me.btnSet = New System.Windows.Forms.Button
        Me.SuspendLayout()
        '
        'lblWestNorth
        '
        Me.lblWestNorth.AutoSize = True
        Me.lblWestNorth.Location = New System.Drawing.Point(12, 15)
        Me.lblWestNorth.Name = "lblWestNorth"
        Me.lblWestNorth.Size = New System.Drawing.Size(64, 13)
        Me.lblWestNorth.TabIndex = 0
        Me.lblWestNorth.Text = "West, North"
        '
        'txtWestNorth
        '
        Me.txtWestNorth.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtWestNorth.BackColor = System.Drawing.SystemColors.InactiveCaptionText
        Me.txtWestNorth.Location = New System.Drawing.Point(94, 12)
        Me.txtWestNorth.Name = "txtWestNorth"
        Me.txtWestNorth.Size = New System.Drawing.Size(186, 20)
        Me.txtWestNorth.TabIndex = 1
        '
        'txtCenter
        '
        Me.txtCenter.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtCenter.Location = New System.Drawing.Point(94, 64)
        Me.txtCenter.Name = "txtCenter"
        Me.txtCenter.Size = New System.Drawing.Size(132, 20)
        Me.txtCenter.TabIndex = 5
        '
        'lblCenter
        '
        Me.lblCenter.AutoSize = True
        Me.lblCenter.Location = New System.Drawing.Point(12, 67)
        Me.lblCenter.Name = "lblCenter"
        Me.lblCenter.Size = New System.Drawing.Size(38, 13)
        Me.lblCenter.TabIndex = 4
        Me.lblCenter.Text = "Center"
        '
        'txtEastSouth
        '
        Me.txtEastSouth.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtEastSouth.BackColor = System.Drawing.SystemColors.InactiveCaptionText
        Me.txtEastSouth.Location = New System.Drawing.Point(94, 38)
        Me.txtEastSouth.Name = "txtEastSouth"
        Me.txtEastSouth.Size = New System.Drawing.Size(186, 20)
        Me.txtEastSouth.TabIndex = 3
        '
        'lblEastSouth
        '
        Me.lblEastSouth.AutoSize = True
        Me.lblEastSouth.Location = New System.Drawing.Point(12, 41)
        Me.lblEastSouth.Name = "lblEastSouth"
        Me.lblEastSouth.Size = New System.Drawing.Size(62, 13)
        Me.lblEastSouth.TabIndex = 2
        Me.lblEastSouth.Text = "East, South"
        '
        'btnSet
        '
        Me.btnSet.Location = New System.Drawing.Point(232, 64)
        Me.btnSet.Name = "btnSet"
        Me.btnSet.Size = New System.Drawing.Size(48, 20)
        Me.btnSet.TabIndex = 6
        Me.btnSet.Text = "Set"
        Me.btnSet.UseVisualStyleBackColor = True
        '
        'frmCoordinates
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(292, 101)
        Me.Controls.Add(Me.btnSet)
        Me.Controls.Add(Me.txtEastSouth)
        Me.Controls.Add(Me.lblEastSouth)
        Me.Controls.Add(Me.txtCenter)
        Me.Controls.Add(Me.lblCenter)
        Me.Controls.Add(Me.txtWestNorth)
        Me.Controls.Add(Me.lblWestNorth)
        Me.Name = "frmCoordinates"
        Me.Text = "Coordinates"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lblWestNorth As System.Windows.Forms.Label
    Friend WithEvents txtWestNorth As System.Windows.Forms.TextBox
    Friend WithEvents txtCenter As System.Windows.Forms.TextBox
    Friend WithEvents lblCenter As System.Windows.Forms.Label
    Friend WithEvents txtEastSouth As System.Windows.Forms.TextBox
    Friend WithEvents lblEastSouth As System.Windows.Forms.Label
    Friend WithEvents btnSet As System.Windows.Forms.Button
End Class
