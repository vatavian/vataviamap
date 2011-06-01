Public Class frmWaypoints

    Private pWaypoints As Generic.List(Of clsGPXwaypoint)
    Private WithEvents pEditBuddyForm As frmEditNameURL

    Public Event Ok()

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

    Private Function SelectedWaypoint() As clsGPXwaypoint
        If pWaypoints IsNot Nothing Then
            Dim lIndex As Integer = 0
            For Each lSearchWaypoint As clsGPXwaypoint In pWaypoints
                If lIndex = lst.SelectedIndex Then
                    Return lSearchWaypoint
                End If
                lIndex += 1
            Next
        End If
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
        RaiseEvent Ok()
        Me.Close()
    End Sub

    Private Sub CenterMapAtSelectedWaypoint()
        If chkCenter.Checked Then
            Dim lWaypoint As clsGPXwaypoint = SelectedWaypoint()
            If lWaypoint IsNot Nothing Then
                RaiseEvent Center(lWaypoint.lat, lWaypoint.lon)
            End If
        End If
    End Sub

    Private Sub chkCenter_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkCenter.CheckedChanged
        CenterMapAtSelectedWaypoint()
    End Sub

    Private Sub lst_ItemCheck(ByVal sender As Object, ByVal e As System.Windows.Forms.ItemCheckEventArgs) Handles lst.ItemCheck
        Dim lIndex As Integer = 0
        If lst Is Nothing OrElse pWaypoints Is Nothing OrElse lst.Items.Count <> pWaypoints.Count Then Exit Sub
        Dim lChecked As Boolean = lst.GetItemChecked(lIndex)
        For Each lWaypoint As clsGPXwaypoint In pWaypoints
            If lIndex = e.Index Then
                lChecked = (e.NewValue = CheckState.Checked)
            Else
                lChecked = lst.GetItemChecked(lIndex)
            End If

            ' Add or remove "hide" extension if it does not match checkbox
            If lWaypoint.GetExtension("hide").ToLower = "true" Then
                If lChecked Then
                    lWaypoint.RemoveExtension("hide")
                End If
            Else
                If Not lChecked Then
                    lWaypoint.SetExtension("hide", "true")
                End If
            End If
            lIndex += 1
        Next
        RaiseEvent Ok()
    End Sub

    Private Sub lst_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lst.SelectedIndexChanged
        CenterMapAtSelectedWaypoint()
    End Sub

    Private Sub btnSaveGPX_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSaveGPX.Click
        Dim lChecked As Generic.List(Of clsGPXwaypoint) = CheckedWaypoints()
        If lChecked.Count < 1 Then
            MsgBox("Check at least one waypoint to save", MsgBoxStyle.OkOnly, "No Waypoints Checked")
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