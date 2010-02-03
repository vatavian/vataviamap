<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmTimeZone
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

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.lblInstructions = New System.Windows.Forms.Label
        Me.numHours = New System.Windows.Forms.NumericUpDown
        Me.lblHours = New System.Windows.Forms.Label
        Me.lblMinutes = New System.Windows.Forms.Label
        Me.numMinutes = New System.Windows.Forms.NumericUpDown
        CType(Me.numHours, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.numMinutes, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lblInstructions
        '
        Me.lblInstructions.AutoSize = True
        Me.lblInstructions.Location = New System.Drawing.Point(12, 9)
        Me.lblInstructions.Name = "lblInstructions"
        Me.lblInstructions.Size = New System.Drawing.Size(312, 13)
        Me.lblInstructions.TabIndex = 0
        Me.lblInstructions.Text = "Specify the offset from UTC for matching photos with GPS traces"
        '
        'numHours
        '
        Me.numHours.Location = New System.Drawing.Point(53, 38)
        Me.numHours.Name = "numHours"
        Me.numHours.Size = New System.Drawing.Size(47, 20)
        Me.numHours.TabIndex = 1
        '
        'lblHours
        '
        Me.lblHours.AutoSize = True
        Me.lblHours.Location = New System.Drawing.Point(12, 40)
        Me.lblHours.Name = "lblHours"
        Me.lblHours.Size = New System.Drawing.Size(35, 13)
        Me.lblHours.TabIndex = 2
        Me.lblHours.Text = "Hours"
        '
        'lblMinutes
        '
        Me.lblMinutes.AutoSize = True
        Me.lblMinutes.Location = New System.Drawing.Point(106, 40)
        Me.lblMinutes.Name = "lblMinutes"
        Me.lblMinutes.Size = New System.Drawing.Size(44, 13)
        Me.lblMinutes.TabIndex = 4
        Me.lblMinutes.Text = "Minutes"
        '
        'numMinutes
        '
        Me.numMinutes.Location = New System.Drawing.Point(156, 38)
        Me.numMinutes.Name = "numMinutes"
        Me.numMinutes.Size = New System.Drawing.Size(47, 20)
        Me.numMinutes.TabIndex = 3
        '
        'frmTimeZone
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(341, 109)
        Me.Controls.Add(Me.lblMinutes)
        Me.Controls.Add(Me.numMinutes)
        Me.Controls.Add(Me.lblHours)
        Me.Controls.Add(Me.numHours)
        Me.Controls.Add(Me.lblInstructions)
        Me.Name = "frmTimeZone"
        Me.Text = "Time Zone"
        CType(Me.numHours, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.numMinutes, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lblInstructions As System.Windows.Forms.Label
    Friend WithEvents numHours As System.Windows.Forms.NumericUpDown
    Friend WithEvents lblHours As System.Windows.Forms.Label
    Friend WithEvents lblMinutes As System.Windows.Forms.Label
    Friend WithEvents numMinutes As System.Windows.Forms.NumericUpDown
End Class
