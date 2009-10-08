'This code allow asynchronous downloading of tiles and other files while allowing the user to continue interacting with the user interface

''' <summary>
''' Manage a queue of download requests.
''' .Enabled = True must be set before downloading from the queue will start
''' </summary>
''' <remarks></remarks>
Public Class clsDownloader
    Inherits clsQueueManager

    Public TileCacheOldest As Date = Date.MinValue ' Keep cached tiles up to this age, download replacement only if cached tile is older

    Private pTileRAMcacheLimit As Integer = 10
    Private pTileRAMcache As New Generic.Dictionary(Of String, Bitmap)
    Private pTileRAMcacheRecent As New Generic.List(Of String)
    Private pPartialDownloadExtension As String = ".part"
    Private pLastCheckedExtension As String = ".Last-Checked"
    Private pETagExt As String = ".ETag."

    ''' <summary>
    ''' Add bitmap to RAM cache
    ''' </summary>
    ''' <param name="aTileFilename">simple file name to use as key in RAM cache</param>
    ''' <param name="aBitmap">data to add to cache</param>
    ''' <returns>True if successfully added</returns>
    ''' <remarks>This is where pTileRAMcacheLimit is enforced</remarks>
    Private Function TileRAMcacheAddTile(ByVal aTileFilename As String, ByVal aBitmap As Bitmap) As Boolean
        Try
            If aTileFilename IsNot Nothing Then
                TileRAMcacheForgetTile(aTileFilename)
                If aBitmap IsNot Nothing Then
                    pTileRAMcache.Add(aTileFilename, aBitmap)
                    pTileRAMcacheRecent.Insert(0, aTileFilename)
                    'Make space in tile cache for new tile
                    While pTileRAMcache.Count > pTileRAMcacheLimit
                        Dim lVictimFilename As String = pTileRAMcacheRecent.Item(pTileRAMcacheRecent.Count - 1)
                        pTileRAMcacheRecent.RemoveAt(pTileRAMcacheRecent.Count - 1)
                        Dim lVictimTile As Bitmap = pTileRAMcache.Item(lVictimFilename)
                        If lVictimTile IsNot Nothing Then
                            Try
                                lVictimTile.Dispose()
                            Catch
                            End Try
                        End If
                        pTileRAMcache.Remove(lVictimFilename)
                    End While
                    Return True
                End If
            End If
        Catch
        End Try
        Return False
    End Function

    ''' <summary>
    ''' Try to read bitmap from aActualFilename and add to RAM cache, queue download if missing or expired
    ''' </summary>
    ''' <param name="aTileFilename">simple file name for use as key in RAM cache</param>
    ''' <param name="aActualFilename">actual file name may contain additional parts such as ETag, Expiry date</param>
    ''' <param name="aTilePoint">which tile</param>
    ''' <param name="aZoom">which zoom level</param>
    ''' <param name="aPriority">download priority for getting tile if not present, -1 to skip download, 0 for highest priority</param>
    ''' <param name="aReplaceExisting">True to download even if we already have a recently cached tile</param>
    ''' <returns>True if bitmap was successfully added</returns>
    ''' <remarks>missing tiles downloaded with aPriority, downloads from expired or aReplaceExisting=true done at aPriority+1 which is lower</remarks>
    Private Function TileRAMcacheAddTile(ByVal aTileFilename As String, _
                                         ByVal aActualFilename As String, _
                                         ByVal aTilePoint As Point, _
                                         ByVal aZoom As Integer, _
                                         ByVal aPriority As Integer, _
                                         ByVal aReplaceExisting As Boolean) As Boolean

        Dim lBitmap As Bitmap = Nothing
        If IO.File.Exists(aActualFilename) Then
            Try 'TODO: check for PNG magic numbers? 89 50 4e 47 (note: now also loading .jpg sometimes)
                If New IO.FileInfo(aActualFilename).Length > 0 Then
                    lBitmap = New Bitmap(aActualFilename)
                    If lBitmap IsNot Nothing Then
                        'Checking (TileCacheOldest > Date.MinValue) saves file system calls if we are not expiring
                        If aPriority > -1 AndAlso Enabled AndAlso _
                          (aReplaceExisting OrElse _
                           (TileCacheOldest > Date.MinValue AndAlso TileLastCheckedDate(aActualFilename) < TileCacheOldest)) Then
                            Enqueue(aTilePoint, aZoom, aPriority + 1, True) 'Request stale tile replacement with lower priority
                        End If

                        Return TileRAMcacheAddTile(aTileFilename, lBitmap)
                    End If
                End If
            Catch
                'If IO.File.Exists(aActualFilename & ".error") Then IO.File.Delete(aActualFilename & ".error")
                'IO.File.Move(aActualFilename, aActualFilename & ".error")
            End Try

            'Try tile with error again?
            If aPriority > -1 AndAlso Enabled AndAlso TileLastCheckedDate(aActualFilename).AddDays(1) < Date.UtcNow Then
                Enqueue(aTilePoint, aZoom, aPriority, True) 'Request error tile replacement
            End If
            'Add dummy tile for now so we know we can't draw it, will be replaced soon if enqueued
            TileRAMcacheAddTile(aTileFilename, Nothing)
            Return False

        End If

        If aPriority > -1 AndAlso Enabled Then
            Enqueue(aTilePoint, aZoom, aPriority, True) 'Request missing tile replacement with given priority
        End If

        Return False
    End Function

    Private Sub TileRAMcacheForgetTile(ByVal aTileFilename As String)
        If pTileRAMcache.ContainsKey(aTileFilename) Then
            Try
                pTileRAMcache.Item(aTileFilename).Dispose()
            Catch
            End Try
            pTileRAMcache.Remove(aTileFilename)
            pTileRAMcacheRecent.Remove(aTileFilename)
        End If
    End Sub

    Public Sub DeleteTile(ByVal aTileFilename As String)
        TileRAMcacheForgetTile(aTileFilename)

        Try
            IO.File.Delete(aTileFilename)
        Catch ex As Exception
        End Try

        For Each lFilename As String In IO.Directory.GetFiles(IO.Path.GetDirectoryName(aTileFilename), IO.Path.GetFileName(aTileFilename) & ".*")
            Try
                IO.File.Delete(lFilename)
            Catch ex As Exception
            End Try
        Next

    End Sub

    ''' <summary>
    ''' Number of tiles kept in RAM cache
    ''' </summary>
    ''' <remarks>
    ''' Cached tiles are faster to access but use RAM, this setting allows some control over that balance.
    ''' This limit is currently only checked/enforced when adding a tile to the cache.
    ''' </remarks>
    Public Property TileRAMcacheLimit() As Integer
        Get
            Return pTileRAMcacheLimit
        End Get
        Set(ByVal value As Integer)
            pTileRAMcacheLimit = value
        End Set
    End Property

    ''' <summary>
    ''' Retrieve a tile bitmap from the RAM or disk cache, optionally enqueue download if not found
    ''' </summary>
    ''' <param name="aTileFilename"></param>
    ''' <param name="aTilePoint"></param>
    ''' <param name="aZoom"></param>
    ''' <param name="aPriority">download priority for getting tile if not present, -1 to skip download, 0 for highest priority</param>
    ''' <param name="aReplaceExisting"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetTile(ByVal aTileFilename As String, _
                            ByVal aTilePoint As Point, _
                            ByVal aZoom As Integer, _
                            ByVal aPriority As Integer, _
                            ByVal aReplaceExisting As Boolean) As Bitmap
        If aTileFilename Is Nothing Then Stop
        Dim lBitmap As Bitmap = Nothing
        Dim lTriedLoad As Boolean = False
CheckCache:
        'If IO.File.Exists(LatestTileFileName(aTileFilename)) AndAlso Not LatestTileFileName(aTileFilename).Contains("png") Then Stop
        If pTileRAMcache.ContainsKey(aTileFilename) Then
            lBitmap = pTileRAMcache.Item(aTileFilename)
            ' Move this name to most recently used position
            pTileRAMcacheRecent.Remove(aTileFilename)
            pTileRAMcacheRecent.Insert(0, aTileFilename)
        ElseIf Not lTriedLoad Then 'Try loading bitmap from cached file
            lTriedLoad = True
            Dim lActualFilename As String = LatestTileFileName(aTileFilename)
            If TileRAMcacheAddTile(aTileFilename, lActualFilename, _
                                   aTilePoint, _
                                   aZoom, _
                                   aPriority, _
                                   aReplaceExisting) Then GoTo CheckCache
        End If
        Return lBitmap
    End Function

    ''' <summary>
    ''' Return the date the tile file was last checked for updates in UTC
    ''' </summary>
    ''' <param name="aActualFilename">Actual file name, possibly with ETag extension, not simplified key version of file name</param>
    ''' <remarks>
    ''' Uses date stamp from inside external file if that file exists, or time stamp of file if not
    ''' Using external file to store date because:
    ''' 1. file dates are unreliable
    ''' 2. we are unable to change the file date while the file is open (can't update while tile is on map)
    ''' </remarks>
    Private Function TileLastCheckedDate(ByVal aActualFilename As String) As Date
        If IO.File.Exists(aActualFilename & pLastCheckedExtension) Then
            Return Date.Parse(ReadTextFile(aActualFilename & pLastCheckedExtension))
        Else
#If Smartphone Then
            Return IO.Directory.GetLastWriteTime(aActualFilename).ToUniversalTime
#Else
            Return IO.Directory.GetLastWriteTimeUtc(aActualFilename)
#End If
        End If
    End Function

    ''' <summary>
    ''' Find the most recent tile file fitting the given file name key
    ''' </summary>
    ''' <param name="aTileFilename">base name of tile file</param>
    ''' <returns>actual file name of most recent tile including any ETag extension if present</returns>
    ''' <remarks>TODO: delete any older versions of same tile</remarks>
    Private Function LatestTileFileName(ByVal aTileFilename As String) As String
        Dim lFilenameMatch As String = aTileFilename
        Dim lFiledateMatch As Date = Date.MinValue
        Dim lDate As Date
        Dim lDirectory As String = IO.Path.GetDirectoryName(aTileFilename)
        If IO.Directory.Exists(lDirectory) Then
            For Each lFilename As String In IO.Directory.GetFiles(lDirectory, IO.Path.GetFileName(aTileFilename) & ".*")
                If Not lFilename.EndsWith(pPartialDownloadExtension) AndAlso Not lFilename.EndsWith(pLastCheckedExtension) Then
                    lDate = TileLastCheckedDate(lFilename)
                    If lDate > lFiledateMatch Then
                        lFiledateMatch = lDate
                        lFilenameMatch = lFilename
                    End If
                End If
            Next
        End If
        Return lFilenameMatch
    End Function

    Public Overrides Sub ServiceItem(ByVal aQueueItem As clsQueueItem)
        Select Case aQueueItem.ItemType
            Case QueueItemType.TileItem
                DownloadTile(aQueueItem.TilePoint, aQueueItem.Zoom, aQueueItem.ReplaceExisting)
            Case QueueItemType.FileItem, QueueItemType.IconItem, QueueItemType.PointItem
                If aQueueItem.ReplaceExisting OrElse Not IO.File.Exists(aQueueItem.Filename) Then
                    DownloadItem(aQueueItem)
                End If
        End Select
    End Sub

    ''' <summary>
    ''' Download an OSM tile
    ''' </summary>
    ''' <param name="aTilePoint">OSM Tile X and Y</param>
    ''' <param name="aZoom">OSM zoom level (0-17)</param>
    ''' <param name="aReplaceExisting">True to download even if we already have a recently cached tile</param>
    ''' <returns>Filename of downloaded tile image or empty string if not downloaded</returns>
    Public Function DownloadTile(ByVal aTilePoint As Point, _
                                 ByVal aZoom As Integer, _
                                 ByVal aReplaceExisting As Boolean) As String

        Dim lTileServerURL As String = g_TileServerURL
        Dim lFileName As String = TileFilename(aTilePoint, aZoom, False)
        If lFileName.Length > 0 Then
            If EnsureDirForFile(lFileName) Then
                Dim lActualFilename As String = LatestTileFileName(lFileName)
                If aReplaceExisting _
                    OrElse Not IO.File.Exists(lActualFilename) _
                    OrElse (TileCacheOldest > Date.MinValue AndAlso IO.File.GetCreationTime(lActualFilename) < TileCacheOldest) Then
                    'Debug.WriteLine("Downloading Tile " & aZoom & "/" & aTilePoint.X & "/" & aTilePoint.Y)
                    Dim lSuffix As String = lActualFilename.Substring(lFileName.Length).Replace(".png", "")
                    If DownloadFile(MakeImageUrl(g_TileServerType, aTilePoint, aZoom), lFileName, True, lSuffix) Then
                        For Each lListener As IQueueListener In Listeners
                            lListener.DownloadedTile(aTilePoint, aZoom, lFileName, lTileServerURL)
                        Next
                    Else
                        Return ""
                    End If
                End If
            Else
                Return ""
            End If
        End If
        Return lFileName
    End Function

    ''' <summary>
    ''' Download and OSM tile and all tiles in the same area with finer detail
    ''' </summary>
    ''' <param name="aTilePoint">OSM Tile X and Y</param>
    ''' <param name="aZoom">OSM zoom level (0-17)</param>
    Public Function DownloadDescendants(ByVal aTilePoint As Point, _
                                        ByVal aZoom As Integer) As Boolean

        If DownloadTile(aTilePoint, aZoom, False).Length > 0 Then
            For lZoom As Integer = aZoom + 1 To g_ZoomMax
                For childX As Integer = aTilePoint.X * 2 To aTilePoint.X * 2 + 1
                    For childY As Integer = aTilePoint.Y * 2 To aTilePoint.Y * 2 + 1
                        If Not DownloadDescendants(New Point(childX, childY), lZoom) Then Return False
                    Next
                Next
            Next
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' Download a file, icon, or point
    ''' </summary>
    ''' <param name="aItem">Item to get</param>
    ''' <returns>Filename of downloaded file or empty string if not downloaded</returns>
    Private Function DownloadItem(ByVal aItem As clsQueueItem) As Boolean
        If DownloadFile(aItem.URL, aItem.Filename, False, Nothing) Then
            For Each lListener As IQueueListener In Listeners
                lListener.DownloadedItem(aItem)
            Next
            Return True
        Else
            Return False
        End If
    End Function

    ' This function should not be used from outside this class for downloading tiles points, or icons, those should use DownloadTile and DownloadItem above
    Friend Function DownloadFile(ByVal aUrl As String, ByVal aFileName As String, ByVal aIsTile As Boolean, Optional ByVal aSuffix As String = Nothing) As Boolean
        Dim lSuccess As Boolean = False
        If EnsureDirForFile(aFileName) Then
            Dim request As Net.WebRequest
            Dim response As Net.WebResponse = Nothing
            Dim input As IO.Stream = Nothing
            Dim output As IO.FileStream = Nothing
            Dim count As Long = 128 * 1024 ' 128k at a time
            Dim buffer(count - 1) As Byte
            Dim ext As String = Nothing
            Dim lDownloadAs As String = aFileName & pPartialDownloadExtension
            Dim lError As Boolean = False
            Dim lCachedETag As String = ""
            'Debug.WriteLine(strUrl)
            'Dim lNewLastModified As String = Nothing

            Try
                request = Net.WebRequest.Create(aUrl)
                If request IsNot Nothing Then
                    If (aSuffix IsNot Nothing) AndAlso (aSuffix.Length > 0) Then
                        Dim lETagStart As Integer = aSuffix.IndexOf(pETagExt)
                        If lETagStart > -1 Then
                            lCachedETag = aSuffix.Substring(lETagStart + pETagExt.Length)
                            request.Headers.Set("If-None-Match", """" & lCachedETag & """")
                        End If
                    End If
                    response = request.GetResponse()
                    If response IsNot Nothing Then
                        Dim lContentLength As String = response.Headers.Item("Content-Length")
                        If lContentLength = "0" Then ' Nothing can be ok, but zero means there is no tile
                            lError = True
                        Else
                            'lNewLastModified = response.Headers.Item("Last-Modified")
                            'If lNewLastModified Is Nothing Then
                            '    lNewLastModified = Format(Date.UtcNow, "ddd, dd MMM yyyy HH:mm:ss") & " GMT"
                            'End If

                            Dim lNewETag As String = response.Headers.Item("ETag")
                            If lNewETag IsNot Nothing Then
                                lNewETag = lNewETag.Replace("""", "")
                                ext = pETagExt & lNewETag
                                If lNewETag.Length > 1 AndAlso lNewETag = lCachedETag Then
                                    ' Cached ETag matches response ETag, no need to download.
                                    ' Should not reach here, should have caused 304 exception at GetResponse above.
                                    response.Close()
                                    WriteTextFile(aFileName & ext & pLastCheckedExtension, Format(Date.UtcNow, "yyyy-MM-dd HH:mm:ss"))
                                    lSuccess = True
                                End If
                            End If
                            If ext Is Nothing Then ext = ""
                            lDownloadAs = aFileName & ext & pPartialDownloadExtension
                            input = response.GetResponseStream()
                            If input IsNot Nothing Then
                                output = New IO.FileStream(lDownloadAs, IO.FileMode.Create)
                                If output IsNot Nothing Then
                                    Do
                                        count = input.Read(buffer, 0, count)
                                        If count = 0 Then Exit Do 'finished download
                                        output.Write(buffer, 0, count)
                                    Loop
                                End If
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                If ex.Message.IndexOf("(304) Not Modified") > -1 Then
                    'Not an error, this means we don't need to download new file
                    WriteTextFile(aFileName & ext & pLastCheckedExtension, Format(Date.UtcNow, "yyyy-MM-dd HH:mm:ss"))
                    lSuccess = True
                Else
                    lError = True
                    Debug.WriteLine("Error downloading '" & aUrl & "' " & ex.Message)
                End If
            End Try
            If input IsNot Nothing Then input.Close()
            If output IsNot Nothing Then output.Close()
            If response IsNot Nothing Then response.Close()
            If lError Then
                If aIsTile AndAlso lDownloadAs.Length > 0 AndAlso Not IO.File.Exists(aFileName) Then
                    Try
                        IO.File.Create(aFileName).Close()
                    Catch
                    End Try
                    TileRAMcacheAddTile(aFileName, aFileName, Nothing, -1, -1, False)
                End If
                Return False
            ElseIf IO.File.Exists(lDownloadAs) Then
                Dim lStartMoving As DateTime = DateTime.Now
                While IO.File.Exists(aFileName)
                    Try
                        If aIsTile Then
                            DeleteTile(aFileName)
                        Else
                            IO.File.Delete(aFileName)
                        End If
                    Catch
                        If DateTime.Now.Subtract(lStartMoving).TotalSeconds > 2 Then
                            Try 'target tile file busy too long, delete new version and report failure
                                IO.File.Delete(lDownloadAs)
                            Catch
                            End Try
                            Return False
                        End If
                        Application.DoEvents()
                    End Try
                End While
                Try
                    If aIsTile Then
                        Dim lMoveTo As String = lDownloadAs.Substring(0, lDownloadAs.Length - pPartialDownloadExtension.Length)
                        IO.File.Move(lDownloadAs, lMoveTo)
                        WriteTextFile(lMoveTo & pLastCheckedExtension, Format(Date.UtcNow, "yyyy-MM-dd HH:mm:ss"))
                        TileRAMcacheAddTile(aFileName, lMoveTo, Nothing, -1, -1, False)
                    Else
                        IO.File.Move(lDownloadAs, aFileName)
                    End If
                    lSuccess = True
                Catch
                End Try
            End If
        End If
        Return lSuccess
    End Function

    '#If Not Smartphone Then
    '    Public Sub SetWriteTime(ByVal aFileName As String, ByVal aDate As DateTime)
    '        Try
    '            IO.File.SetLastWriteTime(aFileName, aDate) ' touch cached file to show it is current
    '        Catch
    '        End Try
    '    End Sub
    '#Else
    '
    '    ' Compact Framework does not have handy IO.File.SetLastWriteTime but we can build it from API calls
    '
    '    Declare Function SetFileTime Lib "coredll.dll" (ByVal hFile As IntPtr, ByVal lpCreationTime As Byte(), ByVal lpLastAccessTime As Byte(), ByVal lpLastWriteTime As Byte()) As Boolean
    '    Declare Function CreateFileCE Lib "coredll.dll" (ByVal lpFileName As String, ByVal dwDesiredAccess As UInteger, ByVal dwShareMode As UInteger, ByVal lpSecurityAttributes As Integer, ByVal dwCreationDisposition As UInteger, ByVal dwFlagsAndAttributes As UInteger, ByVal hTemplateFile As Integer) As IntPtr
    '    Declare Function CloseHandle Lib "coredll.dll" Alias "CloseHandle" (ByVal hobject As Integer) As Integer

    '    Public Sub SetWriteTime(ByVal aFileName As String, ByVal aDate As DateTime)
    '        Dim ft As New FILETIME(aDate.ToFileTime())

    '        Dim hFile As IntPtr = IntPtr.Zero

    '        hFile = DirectCast(CreateFileCE(aFileName, &H40000000, 2, 0, 3, 0, 0), IntPtr)

    '        If CInt(hFile) = -1 Then
    '            ' Failed to create file handle
    '        Else
    '            If SetFileTime(hFile, Nothing, Nothing, ft) = 0 Then
    '                ' Failed to set time
    '            End If
    '            CloseHandle(hFile)
    '        End If
    '    End Sub

    '    Private Class FILETIME
    '        Private m_data As Byte()

    '        Public Sub New(ByVal FileTime As Long)
    '            m_data = BitConverter.GetBytes(FileTime)
    '        End Sub

    '        Public Sub New()
    '            m_data = New Byte(7) {}
    '        End Sub

    '        Public Property dwLowDateTime() As UInteger
    '            Get
    '                Return BitConverter.ToUInt32(m_data, 0)
    '            End Get
    '            Set(ByVal value As UInteger)
    '                Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_data, 0, 4)
    '            End Set
    '        End Property

    '        Public Property dwHighDateTime() As UInteger
    '            Get
    '                Return BitConverter.ToUInt32(m_data, 4)
    '            End Get
    '            Set(ByVal value As UInteger)
    '                Buffer.BlockCopy(BitConverter.GetBytes(value), 0, m_data, 4, 4)
    '            End Set
    '        End Property

    '        Public Shared Widening Operator CType(ByVal f As FILETIME) As Byte()
    '            Return f.m_data
    '        End Operator
    '    End Class
    '#End If
End Class