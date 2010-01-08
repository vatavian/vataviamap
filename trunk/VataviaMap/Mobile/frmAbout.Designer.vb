<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Public Class frmAbout
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
    Private mainMenu1 As System.Windows.Forms.MainMenu

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmAbout))
        Me.mainMenu1 = New System.Windows.Forms.MainMenu
        Me.mnuClose = New System.Windows.Forms.MenuItem
        Me.picDownloadBarcode = New System.Windows.Forms.PictureBox
        Me.lblProjectURL = New System.Windows.Forms.Label
        Me.comboPage = New System.Windows.Forms.ComboBox
        Me.lblDownloadURL = New System.Windows.Forms.Label
        Me.picProjectBarcode = New System.Windows.Forms.PictureBox
        Me.SuspendLayout()
        '
        'mainMenu1
        '
        Me.mainMenu1.MenuItems.Add(Me.mnuClose)
        '
        'mnuClose
        '
        Me.mnuClose.Text = "Close"
        '
        'picDownloadBarcode
        '
        Me.picDownloadBarcode.Image = CType(resources.GetObject("picDownloadBarcode.Image"), System.Drawing.Image)
        Me.picDownloadBarcode.Location = New System.Drawing.Point(3, 53)
        Me.picDownloadBarcode.Name = "picDownloadBarcode"
        Me.picDownloadBarcode.Size = New System.Drawing.Size(96, 94)
        Me.picDownloadBarcode.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        Me.picDownloadBarcode.Tag = ""
        '
        'lblProjectURL
        '
        Me.lblProjectURL.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblProjectURL.Location = New System.Drawing.Point(3, 28)
        Me.lblProjectURL.Name = "lblProjectURL"
        Me.lblProjectURL.Size = New System.Drawing.Size(170, 22)
        Me.lblProjectURL.Text = "http://code.google.com/p/vataviamap/"
        '
        'comboPage
        '
        Me.comboPage.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.comboPage.Items.Add("Mobile Download")
        Me.comboPage.Items.Add("Project Home")
        Me.comboPage.Location = New System.Drawing.Point(3, 3)
        Me.comboPage.Name = "comboPage"
        Me.comboPage.Size = New System.Drawing.Size(170, 22)
        Me.comboPage.TabIndex = 2
        '
        'lblDownloadURL
        '
        Me.lblDownloadURL.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblDownloadURL.Location = New System.Drawing.Point(3, 28)
        Me.lblDownloadURL.Name = "lblDownloadURL"
        Me.lblDownloadURL.Size = New System.Drawing.Size(170, 22)
        Me.lblDownloadURL.Text = "http://vatavia.net/mobileinstaller.html"
        '
        'picProjectBarcode
        '
        Me.picProjectBarcode.Image = CType(resources.GetObject("picProjectBarcode.Image"), System.Drawing.Image)
        Me.picProjectBarcode.Location = New System.Drawing.Point(3, 53)
        Me.picProjectBarcode.Name = "picProjectBarcode"
        Me.picProjectBarcode.Size = New System.Drawing.Size(108, 104)
        Me.picProjectBarcode.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage
        '
        'frmAbout
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoScroll = True
        Me.ClientSize = New System.Drawing.Size(176, 180)
        Me.Controls.Add(Me.comboPage)
        Me.Controls.Add(Me.picDownloadBarcode)
        Me.Controls.Add(Me.picProjectBarcode)
        Me.Controls.Add(Me.lblDownloadURL)
        Me.Controls.Add(Me.lblProjectURL)
        Me.Menu = Me.mainMenu1
        Me.Name = "frmAbout"
        Me.Text = "VataviaMap Mobile"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents picDownloadBarcode As System.Windows.Forms.PictureBox
    Friend WithEvents lblProjectURL As System.Windows.Forms.Label
    Friend WithEvents mnuClose As System.Windows.Forms.MenuItem
    Friend WithEvents comboPage As System.Windows.Forms.ComboBox
    Friend WithEvents lblDownloadURL As System.Windows.Forms.Label
    Friend WithEvents picProjectBarcode As System.Windows.Forms.PictureBox
End Class
