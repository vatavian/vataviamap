Public Class frmWaypoints

    Private pWaypoints As Generic.List(Of clsGPXwaypoint)
    Private WithEvents pEditBuddyForm As frmEditNameURL

    Private pLastSelectedWaypoint As clsGPXwaypoint = Nothing
    Private pPopulating As Boolean = False

    ''' <summary>
    ''' Fires when user selects a waypoint
    ''' </summary>
    ''' <param name="aLatitude">Latitude of waypoint</param>
    ''' <param name="aLongitude">Longitude of waypoint</param>
    ''' <param name="aZoom">Zoom of waypoint, or -1 if no zoom specified</param>
    ''' <remarks>It is expected that the map will be centered (and zoomed if specified) in response to this event</remarks>
    Public Event Center(ByVal aLatitude As Double, ByVal aLongitude As Double, ByVal aZoom As Integer)

    ''' <summary>
    ''' Fires when the use clicks Ok and the form is being closed
    ''' </summary>
    ''' <remarks></remarks>
    Public Event Ok()

    Public Event SelectionChanged()

    Public Sub AskUser(ByVal aWaypoints As Generic.List(Of clsGPXwaypoint))
        pWaypoints = New Generic.List(Of clsGPXwaypoint)
        pWaypoints.AddRange(aWaypoints)
        PopulateList()
        Me.Show()
    End Sub

    Private Sub PopulateList()
        pPopulating = True
        lst.Items.Clear()
        For Each lWaypoint As clsGPXwaypoint In pWaypoints
            With lWaypoint
                Dim lSelected As Boolean = True
                If .GetExtension("hide").ToLower = "true" Then lSelected = False
                Dim lLabel As String = .name
                If lLabel Is Nothing Then lLabel = "<unnamed>"

                Dim lItem As ListViewItem = lst.Items.Add(lLabel)
                lItem.Checked = lSelected
                If .timeSpecified Then
                    lItem.SubItems.Add(.time.ToString)
                End If
            End With
        Next
        pPopulating = False
    End Sub

    Private Function SelectedWaypoint() As clsGPXwaypoint
        Try
            If pWaypoints IsNot Nothing AndAlso lst IsNot Nothing AndAlso pWaypoints.Count = lst.Items.Count AndAlso lst.SelectedIndices.Count > 0 Then
                Return pWaypoints.Item(lst.SelectedIndices(0))
            End If
        Catch
        End Try
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
            If lWaypoint IsNot Nothing AndAlso Not lWaypoint.Equals(pLastSelectedWaypoint) Then
                pLastSelectedWaypoint = lWaypoint
                Dim lZoom As Integer = -1
                Dim lZoomStr As String = lWaypoint.GetExtension("zoom")
                If IsNumeric(lZoomStr) Then lZoom = CInt(lZoomStr)
                RaiseEvent Center(lWaypoint.lat, lWaypoint.lon, lZoom)
            End If
        End If
    End Sub

    Private Sub chkCenter_CheckedChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles chkCenter.CheckedChanged
        CenterMapAtSelectedWaypoint()
    End Sub

    Private Sub lst_ItemCheck(ByVal sender As Object, ByVal e As System.Windows.Forms.ItemCheckEventArgs) Handles lst.ItemCheck
        If pPopulating OrElse lst Is Nothing OrElse pWaypoints Is Nothing OrElse lst.Items.Count <> pWaypoints.Count Then Exit Sub

        Dim lSelectionChanged As Boolean = False
        Dim lChecked As Boolean = e.NewValue.Equals(CheckState.Checked)
        Dim lWaypoint As clsGPXwaypoint = pWaypoints.Item(e.Index)
        ' Add or remove "hide" extension if it does not match checkbox
        If lWaypoint.GetExtension("hide").ToLower = "true" Then
            If lChecked Then
                lWaypoint.RemoveExtension("hide")
                lSelectionChanged = True
            End If
        Else
            If Not lChecked Then
                lWaypoint.SetExtension("hide", "true")
                lSelectionChanged = True
            End If
        End If
        If lSelectionChanged Then RaiseEvent SelectionChanged()
    End Sub

    Private Sub lst_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles lst.SelectedIndexChanged
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

    Private Sub btnAll_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAll.Click
        SelectAll(True)
    End Sub

    Private Sub btnNone_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnNone.Click
        SelectAll(False)
    End Sub

    Private Sub SelectAll(ByVal aSelect As Boolean)
        For Each lWaypoint As clsGPXwaypoint In pWaypoints
            If aSelect Then
                lWaypoint.RemoveExtension("hide")
            Else
                lWaypoint.SetExtension("hide", "true")
            End If
        Next
        RaiseEvent SelectionChanged()
        pPopulating = True
        For Each lItem As ListViewItem In lst.Items
            lItem.Checked = aSelect
        Next
        pPopulating = False
    End Sub
End Class