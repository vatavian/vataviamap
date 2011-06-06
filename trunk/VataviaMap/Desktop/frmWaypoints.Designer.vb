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
        Me.btnSaveGPX = New System.Windows.Forms.Button
        Me.btnOk = New System.Windows.Forms.Button
        Me.chkCenter = New System.Windows.Forms.CheckBox
        Me.btnAll = New System.Windows.Forms.Button
        Me.btnNone = New System.Windows.Forms.Button
        Me.lst = New System.Windows.Forms.ListView
        Me.ColumnName = New System.Windows.Forms.ColumnHeader
        Me.ColumnDate = New System.Windows.Forms.ColumnHeader
        Me.SuspendLayout()
        '
        'btnSaveGPX
        '
        Me.btnSaveGPX.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnSaveGPX.Location = New System.Drawing.Point(316, 530)
        Me.btnSaveGPX.Name = "btnSaveGPX"
        Me.btnSaveGPX.Size = New System.Drawing.Size(94, 23)
        Me.btnSaveGPX.TabIndex = 4
        Me.btnSaveGPX.Text = "Save to GPX"
        Me.btnSaveGPX.UseVisualStyleBackColor = True
        '
        'btnOk
        '
        Me.btnOk.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnOk.Location = New System.Drawing.Point(416, 530)
        Me.btnOk.Name = "btnOk"
        Me.btnOk.Size = New System.Drawing.Size(75, 23)
        Me.btnOk.TabIndex = 5
        Me.btnOk.Text = "Ok"
        Me.btnOk.UseVisualStyleBackColor = True
        '
        'chkCenter
        '
        Me.chkCenter.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.chkCenter.AutoSize = True
        Me.chkCenter.Checked = True
        Me.chkCenter.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkCenter.Location = New System.Drawing.Point(104, 534)
        Me.chkCenter.Name = "chkCenter"
        Me.chkCenter.Size = New System.Drawing.Size(142, 17)
        Me.chkCenter.TabIndex = 3
        Me.chkCenter.Text = "Center Current Waypoint"
        Me.chkCenter.UseVisualStyleBackColor = True
        '
        'btnAll
        '
        Me.btnAll.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnAll.Location = New System.Drawing.Point(12, 530)
        Me.btnAll.Name = "btnAll"
        Me.btnAll.Size = New System.Drawing.Size(34, 23)
        Me.btnAll.TabIndex = 1
        Me.btnAll.Text = "All"
        Me.btnAll.UseVisualStyleBackColor = True
        '
        'btnNone
        '
        Me.btnNone.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnNone.Location = New System.Drawing.Point(52, 530)
        Me.btnNone.Name = "btnNone"
        Me.btnNone.Size = New System.Drawing.Size(46, 23)
        Me.btnNone.TabIndex = 2
        Me.btnNone.Text = "None"
        Me.btnNone.UseVisualStyleBackColor = True
        '
        'lst
        '
        Me.lst.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lst.CheckBoxes = True
        Me.lst.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnName, Me.ColumnDate})
        Me.lst.FullRowSelect = True
        Me.lst.HideSelection = False
        Me.lst.Location = New System.Drawing.Point(0, 0)
        Me.lst.Name = "lst"
        Me.lst.Size = New System.Drawing.Size(505, 520)
        Me.lst.TabIndex = 9
        Me.lst.UseCompatibleStateImageBehavior = False
        Me.lst.View = System.Windows.Forms.View.Details
        '
        'ColumnName
        '
        Me.ColumnName.Text = "Name"
        Me.ColumnName.Width = 320
        '
        'ColumnDate
        '
        Me.ColumnDate.Text = "Date"
        Me.ColumnDate.Width = 150
        '
        'frmWaypoints
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(503, 556)
        Me.Controls.Add(Me.lst)
        Me.Controls.Add(Me.btnNone)
        Me.Controls.Add(Me.btnAll)
        Me.Controls.Add(Me.chkCenter)
        Me.Controls.Add(Me.btnSaveGPX)
        Me.Controls.Add(Me.btnOk)
        Me.Name = "frmWaypoints"
        Me.Text = "Waypoints"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents btnSaveGPX As System.Windows.Forms.Button
    Friend WithEvents btnOk As System.Windows.Forms.Button
    Friend WithEvents chkCenter As System.Windows.Forms.CheckBox
    Friend WithEvents btnAll As System.Windows.Forms.Button
    Friend WithEvents btnNone As System.Windows.Forms.Button
    Friend WithEvents lst As System.Windows.Forms.ListView
    Friend WithEvents ColumnName As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnDate As System.Windows.Forms.ColumnHeader
End Class
