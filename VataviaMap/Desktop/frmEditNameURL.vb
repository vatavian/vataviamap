Public Class frmEditNameURL

    Public Event Add(ByVal aName As String, ByVal aURL As String)
    Public Event Change(ByVal aOriginalName As String, ByVal aName As String, ByVal aURL As String)
    Public Event Remove(ByVal aOriginalName As String)

    Private pOrigName As String
    Private pEdit As Boolean = False

    ''' <summary>
    ''' Ask the user for a Name and URL
    ''' </summary>
    ''' <param name="aTitle">Dialog box title</param>
    ''' <param name="aName">Starting value for Name text box</param>
    ''' <param name="aURL">Starting value for URL text box</param>
    ''' <remarks></remarks>
    Public Sub AskUser(ByVal aTitle As String, ByVal aName As String, ByVal aURL As String, ByVal aEdit As Boolean, _
                       ByVal aLabelExamples As String, ByVal aButtonExamples As String)
        pOrigName = aName
        Me.Text = aTitle
        txtName.Text = aName
        txtURL.Text = aURL
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
        If pEdit Then
            RaiseEvent Change(pOrigName, txtName.Text, txtURL.Text)
        Else
            RaiseEvent Add(txtName.Text, txtURL.Text)
        End If
        Me.Close()
    End Sub

    ''' <summary>
    ''' User pressed Remove
    ''' </summary>
    Private Sub btnRemove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemove.Click
        Me.Hide()
        RaiseEvent Remove(pOrigName)
        Me.Close()
    End Sub
End Class