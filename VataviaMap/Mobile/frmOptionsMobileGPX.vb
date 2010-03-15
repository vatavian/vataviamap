Public Class frmOptionsMobileGPX
    Public LayersLoaded As Generic.List(Of clsLayer)

    Private Sub mnuCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuCancel.Click
        Me.DialogResult = Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub mnuOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuOk.Click
        Me.DialogResult = Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub txtGPXFolder_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtGPXFolder.KeyDown
        If e.KeyCode = Keys.Up Then 'rotate between common locations
            Select Case txtGPXFolder.Text
                Case g_PathChar & "My Documents" & g_PathChar : txtGPXFolder.Text = g_PathChar & "My Documents" & g_PathChar & "gps" & g_PathChar
                Case g_PathChar & "My Documents" & g_PathChar & "gps" & g_PathChar : txtGPXFolder.Text = g_PathChar & "Storage Card" & g_PathChar
                Case Else : txtGPXFolder.Text = g_PathChar & "My Documents" & g_PathChar
            End Select
        End If
    End Sub

    Private Sub txtGPXFolder_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtGPXFolder.TextChanged
        PopulateList()
    End Sub

    Public Sub PopulateList()
        lstGPX.Items.Clear()
        Dim lItem As ListViewItem
        Dim lHaveGPX As Boolean = LayersLoaded IsNot Nothing AndAlso LayersLoaded.Count > 0
        If lHaveGPX Then
            For Each lGPX As clsLayer In LayersLoaded
                lItem = New ListViewItem(lGPX.Filename)
                lItem.Checked = True
                lstGPX.Items.Add(lItem)
            Next
        End If
        If IO.Directory.Exists(txtGPXFolder.Text) Then
            For Each lFilename As String In IO.Directory.GetFiles(txtGPXFolder.Text)
                Select Case IO.Path.GetExtension(lFilename).ToLower
                    Case ".gpx", ".loc", ".cell"
                        If lHaveGPX Then
                            For Each lGPX As clsLayer In LayersLoaded
                                If lGPX.Filename = lFilename Then GoTo NextFile
                            Next
                        End If
                        lItem = New ListViewItem(lFilename)
                        lstGPX.Items.Add(lItem)
                End Select
NextFile:
            Next
        End If
    End Sub

    Private Sub txtGPXSymbolSize_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtGPXSymbolSize.KeyDown
        On Error Resume Next
        Select Case e.KeyCode
            Case Keys.Left, Keys.Down
                Dim lVal As Integer = 0
                lval = CInt(txtGPXSymbolSize.Text) - 1
                If lVal < 0 Then lVal = 0
                txtGPXSymbolSize.Text = CStr(lVal)
            Case Keys.Right, Keys.Up
                Dim lVal As Integer = 0
                lVal = CInt(txtGPXSymbolSize.Text) + 1
                txtGPXSymbolSize.Text = CStr(lVal)
        End Select
    End Sub

    Private Sub txtGPXSymbolColor_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtGPXSymbolColor.KeyDown
        Dim lRandom As New Random
        txtGPXSymbolColor.BackColor = Color.FromArgb(lRandom.Next(0, 255), lRandom.Next(0, 255), lRandom.Next(0, 255))
    End Sub
End Class