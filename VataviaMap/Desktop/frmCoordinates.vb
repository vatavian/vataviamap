Public Class frmCoordinates

    Public Event Center(ByVal aLatitude As Double, ByVal aLongitude As Double)

    Public Overloads Sub Show(ByVal aMap As ctlMap)
        txtWestNorth.Text = aMap.LonMin.ToString("#.#######") & ", " & aMap.LatMax.ToString("#.#######")
        txtCenter.Text = aMap.CenterLon.ToString("#.#######") & ", " & aMap.CenterLat.ToString("#.#######")
        txtEastSouth.Text = aMap.LonMax.ToString("#.#######") & ", " & aMap.LatMin.ToString("#.#######")
        Me.Show()
    End Sub

End Class