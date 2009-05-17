Public Class frmMain

    Private Sub mnuOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuOk.Click
        Me.Visible = False
        Me.Close()
    End Sub

    Private Sub mnuSend_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuSend.Click
        Me.BackColor = Color.Black
        Me.Refresh()
        txtReceive.Text = SendURL("http://vatavia.net/cgi-bin/txtrachel.pl?realname=Mark&subject=&email=mark-rss@vatavia.net&comments=" & UrlEncode(txtSend.Text))
        Me.BackColor = SystemColors.Window
    End Sub
End Class
