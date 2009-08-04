Imports System.IO
Imports System.Net

Class clsCellLocationOpenCellID
    Inherits clsCellLocationProvider

    Public Shared WebsiteURL As String = "http://www.opencellid.org/"
    Public Shared RawDatabaseURL As String = "http://myapp.fr/cellsIdData/cells.txt.gz"

    Public Sub New()
        Me.pProviderCode = "OpenCellID"
    End Sub

    Public Overrides Function GetCellLocation(ByVal aCell As clsCell) As Boolean
        'TODO implement getting cell location from OpenCellID.org
        Return False
    End Function
End Class