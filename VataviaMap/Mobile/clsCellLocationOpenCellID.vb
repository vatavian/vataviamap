Imports System.io
Imports System.Net
Imports GPS_API.RIL

Class clsCellLocationOpenCellID
    Inherits clsCellLocationProvider

    Public Sub New()
        Me.pProviderCode = "o"
    End Sub

    Public Overrides Function GetCellLocation(ByVal aCell As RILCELLTOWERINFO, ByRef Latitude As Double, ByRef Longitude As Double) As Boolean
        'TODO implement getting cell location from OpenCellID.org
        Return False
    End Function
End Class