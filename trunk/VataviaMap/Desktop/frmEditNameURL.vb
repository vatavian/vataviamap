Public Class frmEditNameURL

    ''' <summary>
    ''' Event is raised when user presses Ok
    ''' </summary>
    ''' <param name="aName">Name currently displayed</param>
    ''' <param name="aURL">URL to get buddy location</param>
    ''' <remarks></remarks>
    Public Event Ok(ByVal aOriginalName As String, ByVal aName As String, ByVal aURL As String)
    Public Event Remove(ByVal aOriginalName As String)

    Private pOrigName As String

    ''' <summary>
    ''' Ask the user for a Name and URL
    ''' </summary>
    ''' <param name="aTitle">Dialog box title</param>
    ''' <param name="aName">Starting value for Name text box</param>
    ''' <param name="aURL">Starting value for URL text box</param>
    ''' <remarks></remarks>
    Public Sub AskUser(ByVal aTitle As String, ByVal aName As String, ByVal aURL As String, _
                       ByVal aLabelExamples As String, ByVal aButtonExamples As String)
        pOrigName = aName
        Me.Text = aTitle
        txtName.Text = aName
        txtURL.Text = aURL
        lblExamples.Text = aLabelExamples
        btnExamples.Tag = aButtonExamples
        Me.Show()
    End Sub

    ''' <summary>
    ''' Show file or web page with examples
    ''' </summary>
    Private Sub btnExamples_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnExamples.Click
        OpenFile(btnExamples.Tag)
    End Sub

    ''' <summary>
    ''' User pressed Ok
    ''' </summary>
    Private Sub btnOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOk.Click
        Me.Hide()
        RaiseEvent Ok(pOrigName, txtName.Text, txtURL.Text)
        Me.Close()
    End Sub

    ''' <summary>
    ''' User pressed Remove, send Ok event with blank URL
    ''' </summary>
    Private Sub btnRemove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemove.Click
        Me.Hide()
        RaiseEvent Remove(pOrigName)
        Me.Close()
    End Sub
End Class