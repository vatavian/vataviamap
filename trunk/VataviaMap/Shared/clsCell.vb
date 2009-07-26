<CLSCompliant(False)> _
Public Class clsCell
    Public MCC As UInt16 'MobileCountryCode
    Public MNC As UInt16 'MobileNetworkCode
    Public LAC As UInt16 'LocationAreaCode
    Public ID As UInt32  'CellID
    Public Latitude As Double
    Public Longitude As Double

    'Number of bytes this occupies in a binary file
    Public Const NumBytes As Integer = 26 'MCC=2 MNC=2 LAC=2 ID=4 Lat=8 Lon=8

    Public Sub New()
        Clear()
    End Sub

#If Smartphone Then

    Public Sub New(ByVal aTowerInfo As GPS_API.RIL.RILCELLTOWERINFO)
        With aTowerInfo
            MCC = .dwMobileCountryCode
            MNC = .dwMobileNetworkCode
            LAC = .dwLocationAreaCode
            ID = .dwCellID
        End With
    End Sub

#End If

    Public Sub Clear()
        MCC = 0
        MNC = 0
        LAC = 0
        ID = 0
        Latitude = -999
        Longitude = -999
    End Sub

    Public Function Label() As String
        Return MCC & "." & MNC & "." & LAC & "." & ID
    End Function

    Public Sub New(ByVal aReader As IO.BinaryReader)
        Me.Read(aReader)
    End Sub

    Public Sub Read(ByVal aReader As IO.BinaryReader)
        With aReader
            MCC = .ReadUInt16
            MNC = .ReadUInt16
            LAC = .ReadUInt16
            ID = .ReadUInt32
            Latitude = .ReadDouble
            Longitude = .ReadDouble
        End With
    End Sub

    Public Sub Write(ByVal aWriter As IO.BinaryWriter)
        With aWriter
            .Write(MCC)
            .Write(MNC)
            .Write(LAC)
            .Write(ID)
            .Write(Latitude)
            .Write(Longitude)
        End With
    End Sub
End Class
