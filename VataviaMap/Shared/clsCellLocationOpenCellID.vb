Imports System.io
Imports System.Net
Imports GPS_API.RIL

Class clsCellLocationOpenCellID
    Inherits clsCellLocationProvider

    Public Sub New()
        Me.pProviderCode = "OpenCellID"
    End Sub

    Public Overrides Function GetCellLocation(ByVal aCell As clsCell) As Boolean
        'TODO implement getting cell location from OpenCellID.org
        Return False
    End Function
End Class