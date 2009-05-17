Public Class frmBuddyAlarm

    Public Event SetAlarm(ByVal aLatitude As Double, ByVal aLongitude As Double, ByVal aDistanceMeters As Double, ByVal aEnable As Boolean)

    Private pDistanceMeters As Double
    Private pSettingDistanceText As Boolean = False 'Flag to tell whether we are currently setting the distance text programmatically

    Public Sub AskUser(ByVal aLatitude As Double, ByVal aLongitude As Double)
        txtLat.Text = aLatitude
        txtLon.Text = aLongitude
        Me.Show()
    End Sub

    Public Sub AskUser(ByVal aLatitude As Double, ByVal aLongitude As Double, ByVal aDistanceMeters As Double, ByVal aEnable As Boolean)
        txtLat.Text = aLatitude
        txtLon.Text = aLongitude
        SetDistance(aDistanceMeters)
        chkEnable.Checked = aEnable
        Me.Show()
    End Sub

    Private Sub btnOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOk.Click
        RaiseEvent SetAlarm(txtLat.Text, txtLon.Text, pDistanceMeters, chkEnable.Checked)
        Me.Close()
    End Sub

    Private Sub comboDistanceUnits_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles comboDistanceUnits.SelectedIndexChanged
        SetDistance(pDistanceMeters)
    End Sub

    Public Sub SetDistance(ByVal aMeters As Double)
        pSettingDistanceText = True
        pDistanceMeters = aMeters
        Select Case comboDistanceUnits.Text
            Case "meters" : txtDistance.Text = Format(pDistanceMeters, "#,###")
            Case "kilometers" : txtDistance.Text = Format(pDistanceMeters / 1000, "#,###.#")
            Case "miles" : txtDistance.Text = Format(pDistanceMeters / g_MetersPerMile, "#,###.#")
        End Select
        pSettingDistanceText = False
    End Sub

    Private Sub txtDistance_TextChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtDistance.TextChanged
        If Not pSettingDistanceText Then
            Dim lDistance As Double
            If Double.TryParse(txtDistance.Text, lDistance) Then
                Select Case comboDistanceUnits.Text
                    Case "meters" : pDistanceMeters = lDistance
                    Case "kilometers" : pDistanceMeters = lDistance * 1000
                    Case "miles" : pDistanceMeters = lDistance * g_MetersPerMile
                End Select
            End If
        End If
    End Sub
End Class