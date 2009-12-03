<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmWaypoints
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
        Me.lst = New System.Windows.Forms.CheckedListBox
        Me.btnCenter = New System.Windows.Forms.Button
        Me.btnSaveGPX = New System.Windows.Forms.Button
        Me.btnOk = New System.Windows.Forms.Button
        Me.SuspendLayout()
        '
        'lst
        '
        Me.lst.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lst.CheckOnClick = True
        Me.lst.FormattingEnabled = True
        Me.lst.IntegralHeight = False
        Me.lst.Location = New System.Drawing.Point(0, 1)
        Me.lst.Name = "lst"
        Me.lst.Size = New System.Drawing.Size(504, 233)
        Me.lst.TabIndex = 2
        '
        'btnCenter
        '
        Me.btnCenter.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnCenter.Location = New System.Drawing.Point(12, 240)
        Me.btnCenter.Name = "btnCenter"
        Me.btnCenter.Size = New System.Drawing.Size(75, 23)
        Me.btnCenter.TabIndex = 11
        Me.btnCenter.Text = "Center"
        Me.btnCenter.UseVisualStyleBackColor = True
        '
        'btnSaveGPX
        '
        Me.btnSaveGPX.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnSaveGPX.Location = New System.Drawing.Point(93, 240)
        Me.btnSaveGPX.Name = "btnSaveGPX"
        Me.btnSaveGPX.Size = New System.Drawing.Size(94, 23)
        Me.btnSaveGPX.TabIndex = 10
        Me.btnSaveGPX.Text = "Save to GPX"
        Me.btnSaveGPX.UseVisualStyleBackColor = True
        '
        'btnOk
        '
        Me.btnOk.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnOk.Location = New System.Drawing.Point(416, 240)
        Me.btnOk.Name = "btnOk"
        Me.btnOk.Size = New System.Drawing.Size(75, 23)
        Me.btnOk.TabIndex = 8
        Me.btnOk.Text = "Ok"
        Me.btnOk.UseVisualStyleBackColor = True
        '
        'frmWaypoints
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(503, 266)
        Me.Controls.Add(Me.btnCenter)
        Me.Controls.Add(Me.btnSaveGPX)
        Me.Controls.Add(Me.btnOk)
        Me.Controls.Add(Me.lst)
        Me.Name = "frmWaypoints"
        Me.Text = "frmWaypoints"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents lst As System.Windows.Forms.CheckedListBox
    Friend WithEvents btnCenter As System.Windows.Forms.Button
    Friend WithEvents btnSaveGPX As System.Windows.Forms.Button
    Friend WithEvents btnOk As System.Windows.Forms.Button
End Class
