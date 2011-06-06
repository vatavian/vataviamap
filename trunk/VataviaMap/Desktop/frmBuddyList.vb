Public Class frmBuddyList

    Private pBuddies As Generic.Dictionary(Of String, clsBuddy)
    Private WithEvents pEditBuddyForm As frmEditNameURL

    Public Event Ok(ByVal aBuddies As Generic.Dictionary(Of String, clsBuddy))

    Public Event Center(ByVal aLatitude As Double, ByVal aLongitude As Double, ByVal aZoom As Integer)

    Public Sub AskUser(ByVal aBuddies As Generic.Dictionary(Of String, clsBuddy))
        pBuddies = New Generic.Dictionary(Of String, clsBuddy)
        For Each lBuddy As clsBuddy In aBuddies.Values
            pBuddies.Add(lBuddy.Name, lBuddy)
        Next
        PopulateList()
        Me.Show()
    End Sub

    Private Sub PopulateList()
        lstBuddies.Items.Clear()
        For Each lBuddy As clsBuddy In pBuddies.Values
            lstBuddies.Items.Add(lBuddy.Name)
            lstBuddies.SetItemChecked(lstBuddies.Items.Count - 1, lBuddy.Selected)
        Next
    End Sub

    Private Sub NewEditBuddyForm()
        If pEditBuddyForm IsNot Nothing Then
            Try
                pEditBuddyForm.Close()
            Catch
            End Try
        End If
        pEditBuddyForm = New frmEditNameURL
        pEditBuddyForm.Icon = Me.Icon
    End Sub

    Private Sub btnAdd_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAdd.Click
        NewEditBuddyForm()
        pEditBuddyForm.AskUser("Add Buddy", "Buddy Name", "http://www.google.com/latitude/apps/badge/api?user=BuddyBadgeNumber&type=json", False, "Buddy Location URL", "http://code.google.com/p/vataviamap/wiki/FindBuddies")
    End Sub

    Private Function SelectedBuddy() As clsBuddy
        Dim lIndex As Integer = 0
        Dim lBuddy As clsBuddy = Nothing
        For Each lSearchBuddy As clsBuddy In pBuddies.Values
            If lIndex = lstBuddies.SelectedIndex OrElse lBuddy Is Nothing Then
                lBuddy = lSearchBuddy
            End If
            lIndex += 1
        Next
        Return lBuddy
    End Function

    Private Sub btnOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOk.Click
        Dim lIndex As Integer = 0
        For Each lBuddy As clsBuddy In pBuddies.Values
            lBuddy.Selected = lstBuddies.GetItemChecked(lIndex)
            lIndex += 1
        Next
        RaiseEvent Ok(pBuddies)
        Me.Close()
    End Sub

    Private Sub pEditBuddyForm_Add(ByVal aName As String, ByVal aURL As String) Handles pEditBuddyForm.Add
        BuddyAddOrEdit(Nothing, aName, aURL)
    End Sub

    Private Sub pEditBuddyForm_Change(ByVal aOriginalName As String, ByVal aName As String, ByVal aURL As String) Handles pEditBuddyForm.Change
        BuddyAddOrEdit(aOriginalName, aName, aURL)
    End Sub

    Private Sub BuddyAddOrEdit(ByVal aOriginalName As String, ByVal aName As String, ByVal aURL As String)
        Dim lBuddy As clsBuddy
        If aOriginalName IsNot Nothing AndAlso pBuddies.ContainsKey(aOriginalName) Then
            lBuddy = pBuddies(aOriginalName)
            If aOriginalName <> aName Then
                pBuddies.Remove(aOriginalName)
                If pBuddies.ContainsKey(aName) Then pBuddies.Remove(aName)
                pBuddies.Add(aName, lBuddy)
            End If
        Else
            lBuddy = New clsBuddy
            lBuddy.Name = aName
            pBuddies.Add(aName, lBuddy)
        End If
        lBuddy.LocationURL = aURL
        lBuddy.Name = aName
        PopulateList()
    End Sub

    Private Sub pEditBuddyForm_Remove(ByVal aName As String) Handles pEditBuddyForm.Remove
        pBuddies.Remove(aName)
        PopulateList()
    End Sub

    Private Sub btnEdit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEdit.Click
        Dim lBuddy As clsBuddy = SelectedBuddy()
        If lBuddy Is Nothing Then
            btnAdd_Click(sender, e)
        Else
            NewEditBuddyForm()
            pEditBuddyForm.AskUser("Edit Buddy", lBuddy.Name, lBuddy.LocationURL, True, "Google Latitude kml URL", "http://www.google.com/latitude/apps/badge")
        End If
    End Sub

    Private Sub btnRemove_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemove.Click
        If lstBuddies.SelectedIndex >= 0 Then
            Dim lBuddy As clsBuddy = SelectedBuddy()
            pBuddies.Remove(lBuddy.Name)
            PopulateList()
        End If
    End Sub

    Private Sub btnCenter_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCenter.Click
        Dim lBuddy As clsBuddy = SelectedBuddy()
        If lBuddy IsNot Nothing AndAlso lBuddy.Waypoint IsNot Nothing Then
            With lBuddy.Waypoint
                RaiseEvent Center(.lat, .lon, -1)
            End With
        End If
    End Sub
End Class