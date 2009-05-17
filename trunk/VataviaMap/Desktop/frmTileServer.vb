Public Class frmTileServer

    ''' <summary>
    ''' Event is raised when user presses Ok or Remove
    ''' </summary>
    ''' <param name="aName">Name of tile server to show user</param>
    ''' <param name="aURL">Base URL of tile server. If blank, remove tile server from list</param>
    ''' <remarks></remarks>
    Public Event Ok(ByVal aName As String, ByVal aURL As String)

    ''' <summary>
    ''' Ask the user for a tile server name and URL
    ''' </summary>
    ''' <param name="aTitle">Dialog box title</param>
    ''' <param name="aDefaultName">Starting value for Name text box</param>
    ''' <param name="aDefaultURL">Starting value for URL text box</param>
    ''' <remarks></remarks>
    Public Sub AskUser(ByVal aTitle As String, ByVal aDefaultName As String, ByVal aDefaultURL As String)
        Me.Text = aTitle
        txtName.Text = aDefaultName
        txtURL.Text = aDefaultURL
        Me.Show()
    End Sub

    ''' <summary>
    ''' Show web page with tile server examples
    ''' </summary>
    Private Sub btnExamples_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnExamples.Click
        OpenFile("http://wiki.openstreetmap.org/wiki/Slippy_map_tilenames#Tiles")
    End Sub

    ''' <summary>
    ''' Add or edit server information
    ''' </summary>
    Private Sub btnOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOk.Click
        Me.Hide()
        If txtURL.Text.Length > 0 AndAlso Not txtURL.Text.EndsWith("/") Then txtURL.Text &= "/"
        RaiseEvent Ok(txtName.Text, txtURL.Text)
        Me.Close()
    End Sub

    ''' <summary>
    ''' Remove server by sending back Ok event with blank URL
    ''' </summary>
    Private Sub btnRemove_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRemove.Click
        Me.Hide()
        RaiseEvent Ok(txtName.Text, "")
        Me.Close()
    End Sub
End Class