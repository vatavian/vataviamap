Public Class frmMain
    Public Touched As Boolean = False

    Friend Sub Touch()
        Me.Touched = True
        Me.BackColor = SystemColors.Control
    End Sub

    Private Sub mnuOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuOk.Click
        Touch()
        Me.Visible = False
        Me.Close()
    End Sub

    Private Sub mnuSend_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuSend.Click
        Touch()
        If txtSend.Text.Length > 1 Then
            Me.BackColor = Color.Black
            Application.DoEvents()
            txtReceive.Text = SendURL("http://vatavia.net/cgi-bin/txtrachel.pl?realname=Mark&subject=&email=mark-rss@vatavia.net&comments=" & UrlEncode(txtSend.Text))
            Me.BackColor = SystemColors.Window
        End If
    End Sub

    Private Sub txtAny_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtReceive.KeyDown, txtSend.KeyDown
        Touch()
    End Sub
End Class
