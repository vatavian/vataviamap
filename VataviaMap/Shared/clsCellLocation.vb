Imports GPS_API.RIL

<CLSCompliant(False)> _
Public Class clsCellLocationProvider

    Protected pProviderCode As String = "Local"
    Protected Const BinaryMagic As UInt32 = &H43454C4C 'spells "CELL"

    'Do not create objects of type clsCellLocationProvider directly, create a subclass
    Protected Sub New()
    End Sub

    Public Overridable Function GetCachedLocation(ByVal aCellCacheFolder As String, ByVal aCell As clsCell) As Boolean
        Dim lCacheFilename As String = GetCacheFilename(aCellCacheFolder, aCell)
        If IO.File.Exists(lCacheFilename) Then
            Try
                Dim lCell As New clsCell
                Dim lFileStream As New IO.FileStream(lCacheFilename, IO.FileMode.Open, IO.FileAccess.Read)
                Dim lReader As New IO.BinaryReader(lFileStream)
                If lReader.ReadUInt32 <> BinaryMagic Then
                    Throw New ApplicationException("Unknown file type '" & lCacheFilename & "'")
                Else
                    Dim lFileLen As Long = lFileStream.Length
                    While lFileStream.Position < lFileLen
                        lCell.Read(lReader)
                        If lCell.MCC = aCell.MCC AndAlso lCell.MNC = aCell.MNC AndAlso lCell.LAC = aCell.LAC AndAlso lCell.ID = aCell.ID Then
                            aCell.Latitude = lCell.Latitude
                            aCell.Longitude = lCell.Longitude
                            lReader.Close()
                            Return True
                        End If
                    End While
                    lReader.Close()
                    Return True
                End If
            Catch
            End Try
        End If
        Return False
    End Function


    Public Overridable Function SaveCachedLocation(ByVal aCellCacheFolder As String, ByVal aCell As clsCell) As Boolean
        Try
            Dim lCacheFilename As String = GetCacheFilename(aCellCacheFolder, aCell)
            IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(lCacheFilename))
            Dim lWriter As New IO.BinaryWriter(New IO.FileStream(lCacheFilename, IO.FileMode.Append, IO.FileAccess.Write))
            aCell.Write(lWriter)
            lWriter.Close()
            Return True
        Catch e As Exception
        End Try
        Return False
    End Function


    Public Overridable Function GetCellLocation(ByVal aCell As clsCell) As Boolean
        Return False
    End Function 'GetLocation


    Protected Overridable Function GetCacheFilename(ByVal aCellCacheFolder As String, ByVal aCell As clsCell) As String
        With aCell
            Return IO.Path.Combine(aCellCacheFolder, pProviderCode)
        End With
    End Function

End Class
