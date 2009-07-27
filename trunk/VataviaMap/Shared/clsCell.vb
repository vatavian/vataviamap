<CLSCompliant(False)> _
Public Class clsCell
    Implements IComparable(Of clsCell)

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

    Private Shared Function GetFirstInt(ByRef aSource As String) As String
        Dim lInt As String = ""
        For Each lChar As Char In aSource.ToCharArray
            Select Case lChar
                Case "0"c To "9"c : lInt &= lChar
                Case Else : Exit For
            End Select
        Next
        aSource = aSource.Substring(lInt.Length)
        Return lInt
    End Function

    ''' <summary>
    ''' Parse a string representation of the cell information
    ''' </summary>
    ''' <param name="aCellIdentity"></param>
    ''' <param name="aFormat"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Shared Function Parse(ByVal aCellIdentity As String, Optional ByVal aFormat As String = "C.N.L.I") As clsCell
        Dim lCell As clsCell = Nothing
        If aCellIdentity IsNot Nothing Then
            Try
                lCell = New clsCell
                Dim lFormatIndex As Integer = 0
                Dim lIdentityIndex As Integer = 0
                For Each lChar As Char In aFormat.ToCharArray
                    Select Case lChar
                        Case "C"c : lCell.MCC = GetFirstInt(aCellIdentity)
                        Case "N"c : lCell.MNC = GetFirstInt(aCellIdentity)
                        Case "L"c : lCell.LAC = GetFirstInt(aCellIdentity)
                        Case "I"c : lCell.ID = GetFirstInt(aCellIdentity)
                        Case Else
                            If aCellIdentity.StartsWith(lChar) Then
                                aCellIdentity = aCellIdentity.Substring(1)
                            Else
                                Return Nothing
                            End If
                    End Select
                Next
            Catch
                lCell = Nothing
            End Try
        End If
        Return lCell
    End Function

    Function CompareTo(ByVal other As clsCell) As Integer Implements IComparable(Of clsCell).CompareTo
        With other
            Dim lCompare As Integer
            lCompare = MCC.CompareTo(.MCC) : If lCompare <> 0 Then Return lCompare
            lCompare = MNC.CompareTo(.MNC) : If lCompare <> 0 Then Return lCompare
            lCompare = LAC.CompareTo(.LAC) : If lCompare <> 0 Then Return lCompare
            Return ID.CompareTo(.ID)
        End With
    End Function

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
