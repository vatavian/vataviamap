<CLSCompliant(False)> _
Public Class clsCellLocationProvider

    Protected pProviderCode As String = "Local"
    Protected pCacheLayer As clsCellLayer

    'Do not create objects of type clsCellLocationProvider directly, create a subclass
    Protected Sub New()
    End Sub

    Public Overridable Function GetCachedLocation(ByVal aCellCacheFolder As String, ByVal aCell As clsCell) As Boolean
        If HaveCacheLayer Then
            Return pCacheLayer.GetCellLocation(aCell)
        End If
        Return False
    End Function


    Public Overridable Function SaveCachedLocation(ByVal aCellCacheFolder As String, ByVal aCell As clsCell) As Boolean
        Try
            If HaveCacheLayer(aCellCacheFolder) OrElse CreateCacheLayer(aCellCacheFolder) Then
                pCacheLayer.AddCell(aCell, True)
                Return True
            End If
        Catch e As Exception
        End Try
        Return False
    End Function

    Public Overridable Function GetCellLocation(ByVal aCell As clsCell) As Boolean
        Return False
    End Function 'GetLocation

    Protected Overridable Function GetCacheFilename(ByVal aCellCacheFolder As String) As String
        Return IO.Path.Combine(aCellCacheFolder, pProviderCode)
    End Function

    Protected Overridable Function HaveCacheLayer(ByVal aCellCacheFolder As String) As Boolean
        If pCacheLayer Is Nothing Then
            Dim lCacheFilename As String = GetCacheFilename(aCellCacheFolder)
            If IO.File.Exists(lCacheFilename) Then
                pCacheLayer = New clsCellLayer(lCacheFilename, Nothing)
            End If
        End If
        Return pCacheLayer IsNot Nothing
    End Function

    Protected Overridable Function CreateCacheLayer(ByVal aCellCacheFolder As String) As Boolean
        Dim lCacheFilename As String = GetCacheFilename(aCellCacheFolder)
        If IO.Directory.Exists(IO.Path.GetDirectoryName(lCacheFilename)) OrElse IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(lCacheFilename)).Exists Then
            pCacheLayer = New clsCellLayer(Nothing)
            pCacheLayer.Filename = lCacheFilename
        End If
        Return pCacheLayer IsNot Nothing
    End Function

End Class
