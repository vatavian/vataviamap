Imports GPS_API.RIL

Public Class clsCellLocationProvider

    Protected pProviderCode As String = "x"

    'Do not create objects of type clsCellLocationProvider directly, create a subclass
    Protected Sub New()
    End Sub

    <CLSCompliant(False)> _
    Public Overridable Function GetCachedLocation(ByVal aCellCacheFolder As String, ByVal aCell As RILCELLTOWERINFO, _
                                             ByRef Latitude As Double, ByRef Longitude As Double) As Boolean
        Dim lCacheFilename As String = GetCacheFilename(aCellCacheFolder, aCell)
        If IO.File.Exists(lCacheFilename) Then
            Try
                Dim lReader As New IO.BinaryReader(New IO.FileStream(lCacheFilename, IO.FileMode.Open, IO.FileAccess.Read))
                Latitude = lReader.ReadDouble
                Longitude = lReader.ReadDouble
                lReader.Close()
                Return True
            Catch
            End Try
        End If
        Return False
    End Function

    <CLSCompliant(False)> _
    Public Overridable Function SaveCachedLocation(ByVal aCellCacheFolder As String, ByVal aCell As RILCELLTOWERINFO, _
                                       ByVal Latitude As Double, ByVal Longitude As Double) As Boolean
        Try
            Dim lCacheFilename As String = GetCacheFilename(aCellCacheFolder, aCell)
            IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(lCacheFilename))
            Dim lWriter As New IO.BinaryWriter(New IO.FileStream(lCacheFilename, IO.FileMode.Open, IO.FileAccess.Write))
            lWriter.Write(Latitude)
            lWriter.Write(Longitude)
            lWriter.Close()
            Return True
        Catch e As Exception
        End Try
        Return False
    End Function

    <CLSCompliant(False)> _
    Public Overridable Function GetCellLocation(ByVal aCell As RILCELLTOWERINFO, ByRef Latitude As Double, ByRef Longitude As Double) As Boolean
        Return False
    End Function 'GetLocation

    <CLSCompliant(False)> _
    Protected Overridable Function GetCacheFilename(ByVal aCellCacheFolder As String, ByVal aCell As RILCELLTOWERINFO) As String
        With aCell
            Return IO.Path.Combine(aCellCacheFolder, .dwMobileCountryCode & g_PathChar & .dwMobileNetworkCode & g_PathChar & .dwLocationAreaCode & g_PathChar & .dwCellID & g_PathChar & pProviderCode)
        End With
    End Function

End Class
