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
            lst.Items.Add(lWaypoint.name)
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

    Private Sub btnAdd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAdd.Click
        MsgBox("todo: add")
        'NewEditBuddyForm()
        'pEditBuddyForm.AskUser("Add Buddy", "Buddy Name", "http://www.google.com/latitude/apps/badge/api?user=BuddyBadgeNumber&type=json", "Buddy Location URL", "http://code.google.com/p/vataviamap/wiki/FindBuddies")
    End Sub

    Private Function SelectedBuddy() As clsGPXwaypoint
        Dim lIndex As Integer = 0
        Dim lFound As clsGPXwaypoint = Nothing
        For Each lSearchWaypoint As clsGPXwaypoint In pWaypoints
            If lIndex = lst.SelectedIndex OrElse lFound Is Nothing Then
                lFound = lSearchWaypoint
            End If
            lIndex += 1
        Next
        Return lFound
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

    Private Sub btnRemove_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemove.Click
        If lst.SelectedIndex >= 0 Then
            Dim lWaypoint As clsGPXwaypoint = SelectedBuddy()
            pWaypoints.Remove(lWaypoint)
            PopulateList()
        End If
    End Sub

    Private Sub btnCenter_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCenter.Click
        Dim lWaypoint As clsGPXwaypoint = SelectedBuddy()
        If lWaypoint IsNot Nothing Then
            RaiseEvent Center(lWaypoint.lat, lWaypoint.lon)
        End If
    End Sub

End Class