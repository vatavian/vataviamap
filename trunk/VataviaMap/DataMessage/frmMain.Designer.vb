<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Public Class frmMain
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
        Me.mainMenu1 = New System.Windows.Forms.MainMenu
        Me.mnuOk = New System.Windows.Forms.MenuItem
        Me.mnuSend = New System.Windows.Forms.MenuItem
        Me.txtReceive = New System.Windows.Forms.TextBox
        Me.txtSend = New System.Windows.Forms.TextBox
        Me.SuspendLayout()
        '
        'mainMenu1
        '
        Me.mainMenu1.MenuItems.Add(Me.mnuOk)
        Me.mainMenu1.MenuItems.Add(Me.mnuSend)
        '
        'mnuOk
        '
        Me.mnuOk.Text = "Close"
        '
        'mnuSend
        '
        Me.mnuSend.Text = "Send"
        '
        'txtReceive
        '
        Me.txtReceive.AcceptsReturn = True
        Me.txtReceive.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtReceive.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular)
        Me.txtReceive.HideSelection = False
        Me.txtReceive.Location = New System.Drawing.Point(3, 3)
        Me.txtReceive.Multiline = True
        Me.txtReceive.Name = "txtReceive"
        Me.txtReceive.Size = New System.Drawing.Size(170, 117)
        Me.txtReceive.TabIndex = 2
        '
        'txtSend
        '
        Me.txtSend.AcceptsReturn = True
        Me.txtSend.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtSend.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular)
        Me.txtSend.HideSelection = False
        Me.txtSend.Location = New System.Drawing.Point(3, 126)
        Me.txtSend.Multiline = True
        Me.txtSend.Name = "txtSend"
        Me.txtSend.Size = New System.Drawing.Size(169, 51)
        Me.txtSend.TabIndex = 1
        '
        'frmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.AutoScroll = True
        Me.BackColor = System.Drawing.Color.Red
        Me.ClientSize = New System.Drawing.Size(176, 180)
        Me.Controls.Add(Me.txtSend)
        Me.Controls.Add(Me.txtReceive)
        Me.Menu = Me.mainMenu1
        Me.Name = "frmMain"
        Me.Text = "Message"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents mnuOk As System.Windows.Forms.MenuItem
    Friend WithEvents txtReceive As System.Windows.Forms.TextBox
    Friend WithEvents txtSend As System.Windows.Forms.TextBox
    Friend WithEvents mnuSend As System.Windows.Forms.MenuItem

End Class
