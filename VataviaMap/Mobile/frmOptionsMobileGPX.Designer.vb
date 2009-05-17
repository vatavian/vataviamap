<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Public Class frmOptionsMobileGPX
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
        Me.txtGPXFolder = New System.Windows.Forms.TextBox
        Me.lstGPX = New System.Windows.Forms.ListView
        Me.comboLabels = New System.Windows.Forms.ComboBox
        Me.lblLabels = New System.Windows.Forms.Label
        Me.txtGPXSymbolSize = New System.Windows.Forms.TextBox
        Me.lblGPXsymbolSize = New System.Windows.Forms.Label
        Me.txtGPXSymbolColor = New System.Windows.Forms.TextBox
        Me.lblGPXsymbolColor = New System.Windows.Forms.Label
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
        'txtGPXFolder
        '
        Me.txtGPXFolder.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtGPXFolder.Location = New System.Drawing.Point(0, 0)
        Me.txtGPXFolder.Name = "txtGPXFolder"
        Me.txtGPXFolder.Size = New System.Drawing.Size(176, 22)
        Me.txtGPXFolder.TabIndex = 28
        '
        'lstGPX
        '
        Me.lstGPX.Activation = System.Windows.Forms.ItemActivation.OneClick
        Me.lstGPX.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lstGPX.CheckBoxes = True
        Me.lstGPX.Location = New System.Drawing.Point(0, 24)
        Me.lstGPX.Name = "lstGPX"
        Me.lstGPX.Size = New System.Drawing.Size(176, 97)
        Me.lstGPX.TabIndex = 29
        Me.lstGPX.View = System.Windows.Forms.View.List
        '
        'comboLabels
        '
        Me.comboLabels.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.comboLabels.Items.Add("name")
        Me.comboLabels.Items.Add("urlname")
        Me.comboLabels.Items.Add("desc")
        Me.comboLabels.Items.Add("container")
        Me.comboLabels.Items.Add("difficulty")
        Me.comboLabels.Items.Add("terrain")
        Me.comboLabels.Items.Add("encoded_hints")
        Me.comboLabels.Items.Add("hints")
        Me.comboLabels.Location = New System.Drawing.Point(52, 127)
        Me.comboLabels.Name = "comboLabels"
        Me.comboLabels.Size = New System.Drawing.Size(121, 22)
        Me.comboLabels.TabIndex = 30
        '
        'lblLabels
        '
        Me.lblLabels.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblLabels.Location = New System.Drawing.Point(3, 130)
        Me.lblLabels.Name = "lblLabels"
        Me.lblLabels.Size = New System.Drawing.Size(43, 22)
        Me.lblLabels.Text = "Labels"
        '
        'txtGPXSymbolSize
        '
        Me.txtGPXSymbolSize.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.txtGPXSymbolSize.Location = New System.Drawing.Point(72, 155)
        Me.txtGPXSymbolSize.Name = "txtGPXSymbolSize"
        Me.txtGPXSymbolSize.Size = New System.Drawing.Size(22, 22)
        Me.txtGPXSymbolSize.TabIndex = 31
        '
        'lblGPXsymbolSize
        '
        Me.lblGPXsymbolSize.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblGPXsymbolSize.Location = New System.Drawing.Point(3, 157)
        Me.lblGPXsymbolSize.Name = "lblGPXsymbolSize"
        Me.lblGPXsymbolSize.Size = New System.Drawing.Size(91, 19)
        Me.lblGPXsymbolSize.Text = "Track Size"
        '
        'txtGPXSymbolColor
        '
        Me.txtGPXSymbolColor.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.txtGPXSymbolColor.Location = New System.Drawing.Point(146, 154)
        Me.txtGPXSymbolColor.Name = "txtGPXSymbolColor"
        Me.txtGPXSymbolColor.Size = New System.Drawing.Size(22, 22)
        Me.txtGPXSymbolColor.TabIndex = 33
        '
        'lblGPXsymbolColor
        '
        Me.lblGPXsymbolColor.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblGPXsymbolColor.Location = New System.Drawing.Point(100, 157)
        Me.lblGPXsymbolColor.Name = "lblGPXsymbolColor"
        Me.lblGPXsymbolColor.Size = New System.Drawing.Size(68, 19)
        Me.lblGPXsymbolColor.Text = "Color"
        '
        'frmOptionsMobileGPX
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoScroll = True
        Me.ClientSize = New System.Drawing.Size(176, 180)
        Me.Controls.Add(Me.txtGPXSymbolColor)
        Me.Controls.Add(Me.txtGPXSymbolSize)
        Me.Controls.Add(Me.lblLabels)
        Me.Controls.Add(Me.comboLabels)
        Me.Controls.Add(Me.lstGPX)
        Me.Controls.Add(Me.txtGPXFolder)
        Me.Controls.Add(Me.lblGPXsymbolColor)
        Me.Controls.Add(Me.lblGPXsymbolSize)
        Me.Menu = Me.mnuMain
        Me.Name = "frmOptionsMobileGPX"
        Me.Text = "GPX Options"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents txtGPXFolder As System.Windows.Forms.TextBox
    Friend WithEvents lstGPX As System.Windows.Forms.ListView
    Friend WithEvents mnuOk As System.Windows.Forms.MenuItem
    Friend WithEvents mnuCancel As System.Windows.Forms.MenuItem
    Friend WithEvents comboLabels As System.Windows.Forms.ComboBox
    Friend WithEvents lblLabels As System.Windows.Forms.Label
    Friend WithEvents txtGPXSymbolSize As System.Windows.Forms.TextBox
    Friend WithEvents lblGPXsymbolSize As System.Windows.Forms.Label
    Friend WithEvents txtGPXSymbolColor As System.Windows.Forms.TextBox
    Friend WithEvents lblGPXsymbolColor As System.Windows.Forms.Label
End Class
