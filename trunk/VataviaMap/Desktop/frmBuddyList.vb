Public Class frmBuddyList

    Private pBuddies As Generic.Dictionary(Of String, clsBuddy)
    Private WithEvents pEditBuddyForm As frmEditNameURL

    Public Event Ok(ByVal aBuddies As Generic.Dictionary(Of String, clsBuddy))

    Public Sub AskUser(ByVal aBuddies As Generic.Dictionary(Of String, clsBuddy))
        pBuddies = New Generic.Dictionary(Of String, clsBuddy)
        For Each lBuddy As clsBuddy In aBuddies.Values
            pBuddies.Add(lBuddy.Label, lBuddy)
        Next
        PopulateList()
        Me.Show()
    End Sub

    Private Sub PopulateList()
        lstBuddies.Items.Clear()
        For Each lBuddy As clsBuddy In pBuddies.Values
            lstBuddies.Items.Add(lBuddy.Label)
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
        pEditBuddyForm.AskUser("Add Buddy", "Buddy Name", "http://www.google.com/latitude/apps/badge/api?user=BuddyBadgeNumber&type=kml", "Google Latitude kml URL", "http://www.google.com/latitude/apps/badge")
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

    Private Sub pEditBuddyForm_Ok(ByVal aName As String, ByVal aURL As String) Handles pEditBuddyForm.Ok
        If aURL.Length > 0 Then
            Dim lBuddy As clsBuddy
            If pBuddies.ContainsKey(aName) Then
                lBuddy = pBuddies(aName)
            Else
                lBuddy = New clsBuddy
                lBuddy.Label = aName
                pBuddies.Add(aName, lBuddy)
            End If
            lBuddy.URL = aURL
        Else
            pBuddies.Remove(aName)
        End If
        PopulateList()
    End Sub

    Private Sub btnEdit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnEdit.Click
        Dim lBuddy As clsBuddy = SelectedBuddy()
        If lBuddy Is Nothing Then
            btnAdd_Click(sender, e)
        Else
            NewEditBuddyForm()
            pEditBuddyForm.AskUser("Edit Buddy", lBuddy.Label, lBuddy.URL, "Google Latitude kml URL", "http://www.google.com/latitude/apps/badge")
        End If
    End Sub

    Private Sub btnRemove_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnRemove.Click
        If lstBuddies.SelectedIndex >= 0 Then
            Dim lBuddy As clsBuddy = SelectedBuddy()
            pBuddies.Remove(lBuddy.Label)
            PopulateList()
        End If
    End Sub
End Class