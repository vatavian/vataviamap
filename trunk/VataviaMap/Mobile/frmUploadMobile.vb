Public Class frmUploadMobile

    Friend Latitude As Double
    Friend Longitude As Double
    Friend DegreeFormat As EnumDegreeFormat

    Private Sub mnuCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuCancel.Click
        Me.DialogResult = Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub mnuOk_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuOk.Click
        Me.DialogResult = Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub comboDegreeFormat_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles comboDegreeFormat.SelectedIndexChanged
        ParseLatLon()
        DegreeFormat = comboDegreeFormat.SelectedIndex
        FormatLatLon()
    End Sub

    Public Sub FormatLatLon()
        Select Case DegreeFormat
            Case EnumDegreeFormat.DecimalDegrees
                txtLat.Text = FormattedDegrees(Latitude, DegreeFormat)
                txtLon.Text = FormattedDegrees(Longitude, DegreeFormat)

                txtLat.Visible = True
                txtLatDeg.Visible = False
                txtLatDecimalMin.Visible = False
                txtLatMin.Visible = False
                txtLatSec.Visible = False

                txtLon.Visible = True
                txtLonDeg.Visible = False
                txtLonDecimalMin.Visible = False
                txtLonMin.Visible = False
                txtLonSec.Visible = False
            Case EnumDegreeFormat.DegreesDecimalMinutes
                DegreesMinutes(Latitude, txtLatDeg.Text, txtLatDecimalMin.Text)
                DegreesMinutes(Longitude, txtLonDeg.Text, txtLonDecimalMin.Text)

                txtLat.Visible = False
                txtLatDeg.Visible = True
                txtLatDecimalMin.Visible = True
                txtLatMin.Visible = False
                txtLatSec.Visible = False

                txtLon.Visible = False
                txtLonDeg.Visible = True
                txtLonDecimalMin.Visible = True
                txtLonMin.Visible = False
                txtLonSec.Visible = False
            Case EnumDegreeFormat.DegreesMinutesSeconds
                DegreesMinutesSeconds(Latitude, txtLatDeg.Text, txtLatMin.Text, txtLatSec.Text)
                DegreesMinutesSeconds(Longitude, txtLonDeg.Text, txtLonMin.Text, txtLonSec.Text)

                txtLat.Visible = False
                txtLatDeg.Visible = True
                txtLatDecimalMin.Visible = False
                txtLatMin.Visible = True
                txtLatSec.Visible = True

                txtLon.Visible = False
                txtLonDeg.Visible = True
                txtLonDecimalMin.Visible = False
                txtLonMin.Visible = True
                txtLonSec.Visible = True
        End Select
    End Sub

    Private Sub ParseLatLon()
        Try
            Select Case DegreeFormat
                Case EnumDegreeFormat.DecimalDegrees
                    Latitude = CDbl(txtLat.Text)
                    Longitude = CDbl(txtLon.Text)

                Case EnumDegreeFormat.DegreesDecimalMinutes
                    Latitude = CDbl(txtLatDeg.Text)
                    If Latitude < 0 Then
                        Latitude -= CDbl(txtLatDecimalMin.Text) / 60
                    Else
                        Latitude += CDbl(txtLatDecimalMin.Text) / 60
                    End If
                    Longitude = CDbl(txtLonDeg.Text)
                    If Longitude < 0 Then
                        Longitude -= CDbl(txtLonDecimalMin.Text) / 60
                    Else
                        Longitude += CDbl(txtLonDecimalMin.Text) / 60
                    End If

                Case EnumDegreeFormat.DegreesMinutesSeconds
                    Latitude = CDbl(txtLatDeg.Text)
                    If Latitude < 0 Then
                        Latitude -= CDbl(txtLatMin.Text) / 60 + CDbl(txtLatSec.Text) / 3600
                    Else
                        Latitude += CDbl(txtLatMin.Text) / 60 + CDbl(txtLatSec.Text) / 3600
                    End If

                    Longitude = CDbl(txtLonDeg.Text)
                    If Longitude < 0 Then
                        Longitude -= CDbl(txtLonMin.Text) / 60 + CDbl(txtLonSec.Text) / 3600
                    Else
                        Longitude += CDbl(txtLonMin.Text) / 60 + CDbl(txtLonSec.Text) / 3600
                    End If
            End Select
        Catch
        End Try
    End Sub

    Private Sub frmUploadMobile_Activated(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Activated
        FormatLatLon()
        Select Case DegreeFormat
            Case EnumDegreeFormat.DecimalDegrees
                comboDegreeFormat.SelectedIndex = 0
            Case EnumDegreeFormat.DegreesDecimalMinutes
                comboDegreeFormat.SelectedIndex = 1
            Case EnumDegreeFormat.DegreesMinutesSeconds
                comboDegreeFormat.SelectedIndex = 2
        End Select
        Dim lRightWidth As Integer = Screen.PrimaryScreen.Bounds.Width - txtUploadURL.Left - 2
        txtUploadURL.Width = lRightWidth
        comboDegreeFormat.Width = lRightWidth
        txtLat.Width = lRightWidth
        txtLon.Width = lRightWidth
        txtLatDecimalMin.Width = Screen.PrimaryScreen.Bounds.Width - txtLatDecimalMin.Left - 2
        txtLonDecimalMin.Width = txtLatDecimalMin.Width
        txtLatSec.Width = Screen.PrimaryScreen.Bounds.Width - txtLatSec.Left - 2
        txtLonSec.Width = txtLatSec.Width
    End Sub

    Private Sub txtUploadInterval_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtUploadInterval.KeyDown
        Dim lVal As Integer = 0
        On Error Resume Next
        Select Case e.KeyCode
            Case Keys.Left, Keys.Down
                lVal = CInt(txtUploadInterval.Text) - 1
                If lVal < 0 Then lVal = 0
                txtUploadInterval.Text = CStr(lVal)
            Case Keys.Right, Keys.Up
                lVal = CInt(txtUploadInterval.Text) + 1
                txtUploadInterval.Text = CStr(lVal)
        End Select
    End Sub

End Class
