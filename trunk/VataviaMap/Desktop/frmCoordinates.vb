Public Class frmCoordinates

    Public Event Center(ByVal aLatitude As Double, ByVal aLongitude As Double)

    Public Overloads Sub Show(ByVal aMap As frmMap)
        txtWestNorth.Text = aMap.LonMin & ", " & aMap.LatMax
        txtCenter.Text = aMap.CenterLon & ", " & aMap.CenterLat
        txtEastSouth.Text = aMap.LonMax & ", " & aMap.LatMin
        Me.Show()
    End Sub

End Class