<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmOpenCellID
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
        Me.btnWebSite = New System.Windows.Forms.Button
        Me.lblWebSite = New System.Windows.Forms.Label
        Me.lblRawData = New System.Windows.Forms.Label
        Me.btnDownloadRaw = New System.Windows.Forms.Button
        Me.btnImportRaw = New System.Windows.Forms.Button
        Me.lstMCC = New System.Windows.Forms.CheckedListBox
        Me.lblMCC = New System.Windows.Forms.Label
        Me.txtMCC = New System.Windows.Forms.TextBox
        Me.btnMapCells = New System.Windows.Forms.Button
        Me.btnImportGPX = New System.Windows.Forms.Button
        Me.SuspendLayout()
        '
        'btnWebSite
        '
        Me.btnWebSite.Location = New System.Drawing.Point(12, 12)
        Me.btnWebSite.Name = "btnWebSite"
        Me.btnWebSite.Size = New System.Drawing.Size(134, 23)
        Me.btnWebSite.TabIndex = 0
        Me.btnWebSite.Text = "Visit Web Site"
        Me.btnWebSite.UseVisualStyleBackColor = True
        '
        'lblWebSite
        '
        Me.lblWebSite.AutoSize = True
        Me.lblWebSite.Location = New System.Drawing.Point(152, 17)
        Me.lblWebSite.Name = "lblWebSite"
        Me.lblWebSite.Size = New System.Drawing.Size(0, 13)
        Me.lblWebSite.TabIndex = 1
        '
        'lblRawData
        '
        Me.lblRawData.AutoSize = True
        Me.lblRawData.Location = New System.Drawing.Point(152, 46)
        Me.lblRawData.Name = "lblRawData"
        Me.lblRawData.Size = New System.Drawing.Size(0, 13)
        Me.lblRawData.TabIndex = 3
        '
        'btnDownloadRaw
        '
        Me.btnDownloadRaw.Location = New System.Drawing.Point(12, 41)
        Me.btnDownloadRaw.Name = "btnDownloadRaw"
        Me.btnDownloadRaw.Size = New System.Drawing.Size(134, 23)
        Me.btnDownloadRaw.TabIndex = 2
        Me.btnDownloadRaw.Text = "Download Raw Data"
        Me.btnDownloadRaw.UseVisualStyleBackColor = True
        '
        'btnImportRaw
        '
        Me.btnImportRaw.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnImportRaw.Location = New System.Drawing.Point(12, 238)
        Me.btnImportRaw.Name = "btnImportRaw"
        Me.btnImportRaw.Size = New System.Drawing.Size(123, 23)
        Me.btnImportRaw.TabIndex = 7
        Me.btnImportRaw.Text = "Import Raw Data"
        Me.btnImportRaw.UseVisualStyleBackColor = True
        '
        'lstMCC
        '
        Me.lstMCC.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lstMCC.CheckOnClick = True
        Me.lstMCC.FormattingEnabled = True
        Me.lstMCC.IntegralHeight = False
        Me.lstMCC.Location = New System.Drawing.Point(12, 98)
        Me.lstMCC.Name = "lstMCC"
        Me.lstMCC.Size = New System.Drawing.Size(363, 108)
        Me.lstMCC.TabIndex = 5
        '
        'lblMCC
        '
        Me.lblMCC.AutoSize = True
        Me.lblMCC.Location = New System.Drawing.Point(12, 82)
        Me.lblMCC.Name = "lblMCC"
        Me.lblMCC.Size = New System.Drawing.Size(123, 13)
        Me.lblMCC.TabIndex = 4
        Me.lblMCC.Text = "Country Codes to Import:"
        '
        'txtMCC
        '
        Me.txtMCC.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtMCC.Location = New System.Drawing.Point(12, 212)
        Me.txtMCC.Name = "txtMCC"
        Me.txtMCC.Size = New System.Drawing.Size(363, 20)
        Me.txtMCC.TabIndex = 6
        '
        'btnMapCells
        '
        Me.btnMapCells.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnMapCells.Location = New System.Drawing.Point(265, 238)
        Me.btnMapCells.Name = "btnMapCells"
        Me.btnMapCells.Size = New System.Drawing.Size(110, 23)
        Me.btnMapCells.TabIndex = 8
        Me.btnMapCells.Text = "Add Cells To Map"
        Me.btnMapCells.UseVisualStyleBackColor = True
        '
        'btnImportGPX
        '
        Me.btnImportGPX.AllowDrop = True
        Me.btnImportGPX.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.btnImportGPX.Location = New System.Drawing.Point(141, 238)
        Me.btnImportGPX.Name = "btnImportGPX"
        Me.btnImportGPX.Size = New System.Drawing.Size(118, 23)
        Me.btnImportGPX.TabIndex = 9
        Me.btnImportGPX.Text = "Import From GPX"
        Me.btnImportGPX.UseVisualStyleBackColor = True
        '
        'frmOpenCellID
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(387, 273)
        Me.Controls.Add(Me.btnImportGPX)
        Me.Controls.Add(Me.btnMapCells)
        Me.Controls.Add(Me.txtMCC)
        Me.Controls.Add(Me.lblMCC)
        Me.Controls.Add(Me.lstMCC)
        Me.Controls.Add(Me.btnImportRaw)
        Me.Controls.Add(Me.lblRawData)
        Me.Controls.Add(Me.btnDownloadRaw)
        Me.Controls.Add(Me.lblWebSite)
        Me.Controls.Add(Me.btnWebSite)
        Me.Name = "frmOpenCellID"
        Me.Text = "OpenCellID"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents btnWebSite As System.Windows.Forms.Button
    Friend WithEvents lblWebSite As System.Windows.Forms.Label
    Friend WithEvents lblRawData As System.Windows.Forms.Label
    Friend WithEvents btnDownloadRaw As System.Windows.Forms.Button
    Friend WithEvents btnImportRaw As System.Windows.Forms.Button
    Friend WithEvents lstMCC As System.Windows.Forms.CheckedListBox
    Friend WithEvents lblMCC As System.Windows.Forms.Label
    Friend WithEvents txtMCC As System.Windows.Forms.TextBox
    Friend WithEvents btnMapCells As System.Windows.Forms.Button
    Friend WithEvents btnImportGPX As System.Windows.Forms.Button
End Class
