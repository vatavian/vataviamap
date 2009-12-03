Public Class frmWaypoints

    Private pWaypoints As Generic.List(Of clsGPXwaypoint)
    Private WithEvents pEditBuddyForm As frmEditNameURL

    Public Event Ok(ByVal aWaypoints As Generic.List(Of clsGPXwaypoint))

    Public Event Center(ByVal aLatitude As Double, ByVal aLongitude As Double)

    Public Sub AskUser(ByVal aWaypoints As Generic.List(Of clsGPXwaypoint))
        pWaypoints = New Generic.List(Of clsGPXwaypoint)
        pWaypoints.AddRange(aWaypoints)
        PopulateList()
        Me.Show()
    End Sub

    Private Sub PopulateList()
        lst.Items.Clear()
        For Each lWaypoint As clsGPXwaypoint In pWaypoints
            If lWaypoint.name Is Nothing Then
                lst.Items.Add("")
            Else
                lst.Items.Add(lWaypoint.name)
            End If
            Dim lSelected As Boolean = True
            If lWaypoint.GetExtension("hide").ToLower = "true" Then lSelected = False
            lst.SetItemChecked(lst.Items.Count - 1, lSelected)
        Next
    End Sub

    'Private Sub NewEditBuddyForm()
    '    If pEditBuddyForm IsNot Nothing Then
    '        Try
    '            pEditBuddyForm.Close()
    '        Catch
    '        End Try
    '    End If
    '    pEditBuddyForm = New frmEditNameURL
    '    pEditBuddyForm.Icon = Me.Icon
    'End Sub

    Private Function SelectedWaypoint() As clsGPXwaypoint
        Dim lIndex As Integer = 0
        For Each lSearchWaypoint As clsGPXwaypoint In pWaypoints
            If lIndex = lst.SelectedIndex Then
                Return lSearchWaypoint
            End If
            lIndex += 1
        Next
        Return Nothing
    End Function

    Private Function CheckedWaypoints() As Generic.List(Of clsGPXwaypoint)
        Dim lIndex As Integer = 0
        Dim lSelected As New Generic.List(Of clsGPXwaypoint)
        For Each lSearchWaypoint As clsGPXwaypoint In pWaypoints
            If lst.CheckedIndices.Contains(lIndex) Then
                lSelected.Add(lSearchWaypoint)
            End If
            lIndex += 1
        Next
        Return lSelected
    End Function

    Private Sub btnOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOk.Click
        Dim lIndex As Integer = 0
        For Each lWaypoint As clsGPXwaypoint In pWaypoints
            lWaypoint.SetExtension("hide", Not lst.GetItemChecked(lIndex))
            lIndex += 1
        Next
        RaiseEvent Ok(pWaypoints)
        Me.Close()
    End Sub

    'Private Sub pEditBuddyForm_Ok(ByVal aOriginalName As String, ByVal aName As String, ByVal aURL As String) Handles pEditBuddyForm.Ok
    '    Dim lBuddy As clsBuddy
    '    If pWaypoints.ContainsKey(aOriginalName) Then
    '        lBuddy = pWaypoints(aOriginalName)
    '        If aOriginalName <> aName Then
    '            pWaypoints.Remove(aOriginalName)
    '            If pWaypoints.ContainsKey(aName) Then pWaypoints.Remove(aName)
    '            pWaypoints.Add(aName, lBuddy)
    '        End If
    '    Else
    '        lBuddy = New clsBuddy
    '        lBuddy.Name = aName
    '        pWaypoints.Add(aName, lBuddy)
    '    End If
    '    lBuddy.LocationURL = aURL
    '    lBuddy.Name = aName
    '    PopulateList()
    'End Sub

    'Private Sub pEditBuddyForm_Remove(ByVal aName As String) Handles pEditBuddyForm.Remove
    '    pWaypoints.Remove(aName)
    '    PopulateList()
    'End Sub

    'Private Sub btnEdit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEdit.Click
    '    Dim lWaypoint As clsGPXwaypoint = SelectedBuddy()
    '    If lWaypoint Is Nothing Then
    '        btnAdd_Click(sender, e)
    '    Else
    '        NewEditBuddyForm()
    '        pEditBuddyForm.AskUser("Edit Buddy", lBuddy.Name, lBuddy.LocationURL, "Google Latitude kml URL", "http://www.google.com/latitude/apps/badge")
    '    End If
    'End Sub

    Private Sub btnRemove_Click(ByVal sender As Object, ByVal e As System.EventArgs)
        If lst.SelectedIndex >= 0 Then
            Dim lWaypoint As clsGPXwaypoint = SelectedWaypoint()
            pWaypoints.Remove(lWaypoint)
            PopulateList()
        End If
    End Sub

    Private Sub lst_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lst.SelectedIndexChanged
        Dim lWaypoint As clsGPXwaypoint = SelectedWaypoint()
        If lWaypoint IsNot Nothing Then
            RaiseEvent Center(lWaypoint.lat, lWaypoint.lon)
        End If
    End Sub

    Private Sub btnSaveGPX_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSaveGPX.Click
        Dim lChecked As Generic.List(Of clsGPXwaypoint) = CheckedWaypoints()
        If lChecked.Count < 1 Then
            MsgBox("Check at least one waypoint before saving", MsgBoxStyle.OkOnly, "No Waypoints Checked")
        Else
            Dim lSaveDialog As New SaveFileDialog
            With lSaveDialog
                If lChecked.Count = 1 Then
                    .Title = "Save Waypoint As..."
                Else
                    .Title = "Save " & lChecked.Count & " Waypoints As..."
                End If
                .Filter = "*.gpx|*.gpx"
                .DefaultExt = ".gpx"
                If .ShowDialog = Windows.Forms.DialogResult.OK Then
                    Dim lSaveGpx As New clsGPX
                    lSaveGpx.creator = g_AppName
                    lSaveGpx.wpt = lChecked
                    IO.File.WriteAllText(.FileName, lSaveGpx.ToString())
                End If
            End With
        End If
    End Sub
End Class