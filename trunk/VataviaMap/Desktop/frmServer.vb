Public Class frmServer

    Public Event Add(ByVal aServer As clsServer)
    Public Event Change(ByVal aOriginalName As String, ByVal aServer As clsServer)
    Public Event Remove(ByVal aOriginalName As String)

    Private pServer As clsServer
    Private pOriginalName As String
    Private pEdit As Boolean = False

    ''' <summary>
    ''' Ask the user for a Name and URL
    ''' </summary>
    ''' <param name="aTitle">Dialog box title</param>
    ''' <param name="aServer">Server to edit</param>
    ''' <remarks></remarks>
    Public Sub AskUser(ByVal aTitle As String, ByVal aServer As clsServer, ByVal aEdit As Boolean, _
                       ByVal aLabelExamples As String, ByVal aButtonExamples As String)
        Me.Text = aTitle
        pServer = aServer
        With pServer
            pOriginalName = .Name.Clone
            txtName.Text = .Name
            txtLink.Text = .Link
            txtTilePattern.Text = .TilePattern
            txtWebmapPattern.Text = .WebmapPattern
            txtCopyright.Text = .Copyright
            txtZoomMin.Text = .ZoomMin
            txtZoomMax.Text = .ZoomMax
            txtLatMin.Text = .LatMin
            txtLatMax.Text = .LatMax
            txtLonMin.Text = .LonMin
            txtLonMax.Text = .LonMax
            chkTransparent.Checked = .Transparent
        End With
        pEdit = aEdit
        lblExamples.Text = aLabelExamples
        btnExamples.Tag = aButtonExamples
        Me.Show()
    End Sub

    ''' <summary>
    ''' Show file or web page with examples
    ''' </summary>
    Private Sub btnExamples_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnExamples.Click
        OpenFileOrURL(btnExamples.Tag, False)
    End Sub

    ''' <summary>
    ''' User pressed Ok
    ''' </summary>
    Private Sub btnOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOk.Click
        Me.Hide()
        With pServer
            .Name = txtName.Text
            .Link = txtLink.Text
            .TilePattern = txtTilePattern.Text
            .WebmapPattern = txtWebmapPattern.Text
            .Copyright = txtCopyright.Text
            .ZoomMin = txtZoomMin.Text
            .ZoomMax = txtZoomMax.Text
            .LatMin = txtLatMin.Text
            .LatMax = txtLatMax.Text
            .LonMin = txtLonMin.Text
            .LonMax = txtLonMax.Text
            .Transparent = chkTransparent.Checked
        End With

        If pEdit Then
            RaiseEvent Change(pOriginalName, pServer)
        Else
            RaiseEvent Add(pServer)
        End If
        Me.Close()
    End Sub

    ''' <summary>
    ''' User pressed Remove
    ''' </summary>
    Private Sub btnRemove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemove.Click
        Me.Hide()
        RaiseEvent Remove(pOriginalName)
        Me.Close()
    End Sub
End Class
