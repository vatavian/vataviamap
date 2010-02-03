Public Class frmTimeZone

    Public Event Changed(ByVal aUTCoffset As TimeSpan)

    Private Sub anyValueChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles numHours.ValueChanged, numMinutes.ValueChanged
        RaiseEvent Changed(New TimeSpan(numHours.Value, numMinutes.Value, 0))
    End Sub

End Class