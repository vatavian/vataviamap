Public Class frmCoordinates

    Public Event Center(ByVal aLatitude As Double, ByVal aLongitude As Double)

    Public Overloads Sub Show(ByVal aMap As ctlMap)
        txtWestNorth.Text = aMap.LonMin.ToString("#.#######") & ", " & aMap.LatMax.ToString("#.#######")
        txtCenter.Text = aMap.CenterLon.ToString("#.#######") & ", " & aMap.CenterLat.ToString("#.#######")
        txtEastSouth.Text = aMap.LonMax.ToString("#.#######") & ", " & aMap.LatMin.ToString("#.#######")
        Me.Show()
    End Sub

    Private Sub btnSet_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSet.Click
        RaiseCenter()
    End Sub

    Private Sub txtCenter_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtCenter.KeyUp
        If e.KeyCode = Keys.Enter Then
            RaiseCenter()
        End If
    End Sub

    Private Sub RaiseCenter()
        Dim lCoordinates() As String = txtCenter.Text.Split(","c, " "c)
        Dim aLatitude As Double
        Dim aLongitude As Double
        Dim lIndex As Integer = 0
        While lIndex < lCoordinates.Length AndAlso Not DoubleTryParse(lCoordinates(lIndex), aLongitude)
            lIndex += 1
        End While
        lIndex += 1
        While lIndex < lCoordinates.Length AndAlso Not DoubleTryParse(lCoordinates(lIndex), aLatitude)
            lIndex += 1
        End While
        If lIndex < lCoordinates.Length Then
            RaiseEvent Center(aLatitude, aLongitude)
        End If
    End Sub
End Class