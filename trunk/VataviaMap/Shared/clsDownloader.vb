'This code allow asynchronous downloading of tiles and other files while allowing the user to continue interacting with the user interface

''' <summary>
''' Manage a queue of download requests.
''' .Enabled = True must be set before downloading from the queue will start
''' </summary>
''' <remarks></remarks>
Public Class clsDownloader
    Inherits clsQueueManager

    Public TileCacheOldest As Date = Date.MinValue ' Keep cached tiles up to this age, download replacement only if cached tile is older
    Public DeleteOldTiles As Boolean = True

    Private pTileRAMcacheLimit As Integer = 10
    Private pTileRAMcache As New Generic.Dictionary(Of String, Bitmap)
    Private pTileRAMcacheRecent As New Generic.List(Of String)
    Private pPartialDownloadExtension As String = ".part"
    Private pLastCheckedExtension As String = ".Last-Checked"
    Private pETagExt As String = ".ETag."
    Private pExpiresExt As String = ".Expires."
    Private pPNGmagic() As Byte = {137, 80, 78, 71, 13, 10, 26, 10}
    Private pJPGmagic() As Byte = {255, 216, 255}

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
                If pTileRAMcacheLimit > 0 Then
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
    Private Function TileRAMcacheAddTile(ByVal aTileServer As clsServer, _
                                         ByVal aTileFilename As String, _
                                         ByVal aActualFilename As String, _
                                         ByVal aTilePoint As Point, _
                                         ByVal aZoom As Integer, _
                                         ByVal aPriority As Integer, _
                                         ByVal aReplaceExisting As Boolean) As Boolean

        Dim lBitmap As Bitmap = Nothing
        Dim lCanDownload As Boolean = (aPriority > -1 AndAlso Enabled AndAlso Not aTileServer.CacheOnly)
        If aActualFilename Is Nothing Then 'OrElse Not IO.File.Exists(aActualFilename)
            aActualFilename = GetTileFilename(aTileServer, aTileFilename, aTilePoint, aZoom, aPriority, aReplaceExisting)
        End If
        If IO.File.Exists(aActualFilename) Then
            Try
                If FileSize(aActualFilename) > 0 Then
                    'TODO: check for pPNGmagic or pJPGmagic as first bytes of tile?
                    If pTileRAMcacheLimit = 0 Then Return True 'If we are not caching bitmaps, report success without opening the file
                    lBitmap = New Bitmap(aActualFilename)
                    If lBitmap IsNot Nothing Then 'Have a good bitmap
                        Return TileRAMcacheAddTile(aTileFilename, lBitmap)
                    End If
                End If
            Catch exNewBitmap As Exception
                Try
                    IO.File.Delete(aActualFilename)
                Catch exDelete As Exception
                End Try
            End Try

            'Try tile with error again?
            If lCanDownload AndAlso TileNotJustChecked(aActualFilename) Then
                Enqueue(aTileServer, aTilePoint, aZoom, aPriority, True) 'Request error tile replacement
            End If

        End If

        'Add dummy tile for now so we know we can't draw it, will be replaced soon if enqueued
        TileRAMcacheAddTile(aTileFilename, Nothing)

        Return False
    End Function

    Private Sub TileRAMcacheForgetTile(ByVal aTileFilename As String)
        If pTileRAMcache.ContainsKey(aTileFilename) Then
            Try
                Dim lItem As Bitmap = pTileRAMcache.Item(aTileFilename)
                If lItem IsNot Nothing Then lItem.Dispose()
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
                If Not lFilename.EndsWith(pPartialDownloadExtension) Then
                    IO.File.Delete(lFilename)
                End If
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
    Public Function GetTileBitmap(ByVal aTileServer As clsServer, _
                                  ByVal aTileFilename As String, _
                                  ByVal aTilePoint As Point, _
                                  ByVal aZoom As Integer, _
                                  ByVal aPriority As Integer, _
                                  ByVal aReplaceExisting As Boolean) As Bitmap
        Dim lBitmap As Bitmap = Nothing
        Dim lTriedLoad As Boolean = False
CheckCache:
        If pTileRAMcache.ContainsKey(aTileFilename) Then
            lBitmap = pTileRAMcache.Item(aTileFilename)
            ' Move this name to most recently used position
            pTileRAMcacheRecent.Remove(aTileFilename)
            pTileRAMcacheRecent.Insert(0, aTileFilename)
        ElseIf Not lTriedLoad Then 'Try loading bitmap from cached file
            lTriedLoad = True
            If TileRAMcacheAddTile(aTileServer, _
                                   aTileFilename, Nothing, _
                                   aTilePoint, _
                                   aZoom, _
                                   aPriority, _
                                   aReplaceExisting) Then GoTo CheckCache
        End If
        Return lBitmap
    End Function

    Public Function GetTileFilename(ByVal aTileServer As clsServer, _
                                    ByVal aTileFilename As String, _
                                    ByVal aTilePoint As Point, _
                                    ByVal aZoom As Integer, _
                                    ByVal aPriority As Integer, _
                                    ByVal aReplaceExisting As Boolean) As String
        If aTileFilename Is Nothing Then Return ""
        Dim lCanDownload As Boolean = (aPriority > -1 AndAlso Enabled AndAlso  Not aTileServer.CacheOnly)
        Dim lActualFilename As String = LatestTileFileName(aTileFilename)
        If IO.File.Exists(lActualFilename) Then

            'Request a newer version of this tile if it is stale
            'Checking (TileCacheOldest > Date.MinValue) saves file system calls if we are not expiring
            If lCanDownload AndAlso _
              (aReplaceExisting OrElse _
               (TileCacheOldest > Date.MinValue _
                AndAlso TileLastCheckedDate(aTileFilename) < TileCacheOldest _
                AndAlso TileLastCheckedDate(lActualFilename) < TileCacheOldest)) Then
                Enqueue(aTileServer, aTilePoint, aZoom, aPriority + 1, True) 'Request stale tile replacement with lower priority
            End If

            Return lActualFilename

        ElseIf lCanDownload AndAlso TileNotJustChecked(lActualFilename) Then
            Enqueue(aTileServer, aTilePoint, aZoom, aPriority) 'Request missing tile
        End If
        Return ""
    End Function

    Private Function TileNotJustChecked(ByVal aActualFilename As String) As Boolean
        Return TileLastCheckedDate(aActualFilename).AddMinutes(15).ToUniversalTime < Date.UtcNow
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
        ElseIf IO.File.Exists(aActualFilename) Then
#If Smartphone Then
            Return IO.Directory.GetLastWriteTime(aActualFilename).ToUniversalTime
#Else
            Return IO.Directory.GetLastWriteTimeUtc(aActualFilename)
#End If
        Else
            Return Date.MinValue
        End If
    End Function

    Private Sub TileLastCheckedSet(ByVal aTileFilename As String, ByVal aLastCheckedDate As String)
        Dim lExpires As String = FilenameDecodeExt(aTileFilename, pExpiresExt)
        Dim lDate As Date
        If DateTryParse(lExpires, lDate) AndAlso lDate > Date.Now Then
            'Already have a future expiration date, don't need to save when it was last checked
        Else
            'Dim lSavedModified As Boolean = False
            'If lReplacingFilename.Length > 0 Then
            '    If lReplacingFilename.IndexOf(pExpiresExt) > -1 Then
            '        lSavedModified = True
            '    Else
            '        lMoveTo = lReplacingFilename
            '        Try
            '            IO.File.AppendText(lMoveTo).Close()
            '            'Does not work on Phone: IO.File.SetCreationTimeUtc(lMoveTo, Date.Parse(lNewLastModified)) 'TODO: replace use of .LastModified file with creation time
            '            'Dbg("Changed Creation Time of " & lMoveTo & " to " & lNewLastModified)
            '            lSavedModified = True
            '        Catch eSCT As Exception
            '        End Try
            '    End If
            'End If
            'If Not lSavedModified Then
            WriteTextFile(aTileFilename & pLastCheckedExtension, aLastCheckedDate) 'TODO: save date last checked somewhere else, tiny files waste storage space
            'End If
        End If

    End Sub

    ''' <summary>
    ''' Find the most recent tile file fitting the given file name key
    ''' If DeleteOldTiles=True, also delete any older versions of same tile
    ''' </summary>
    ''' <param name="aTileFilename">base name of tile file</param>
    ''' <returns>actual file name of most recent tile including any extensions (ETag, Expires)</returns>
    Public Function LatestTileFileName(ByVal aTileFilename As String) As String
        Static lNotTileSuffixes() As String = {".prj", ".xml", pLastCheckedExtension.ToLower, pPartialDownloadExtension.ToLower}
        Static lNotTileNotPartialSuffixes() As String = {".prj", ".xml", pLastCheckedExtension.ToLower}
        Dim lFilenameMatch As String = aTileFilename
        Dim lFiledateMatch As Date = Date.MinValue
        Dim lDate As Date
        Dim lDirectory As String = IO.Path.GetDirectoryName(aTileFilename)
        If IO.Directory.Exists(lDirectory) Then
            Dim lMatchingFilenames As String() = IO.Directory.GetFiles(lDirectory, IO.Path.GetFileName(aTileFilename) & ".*")
            If lMatchingFilenames.Length > 0 Then
                Dim lTileFilenames As New Generic.List(Of String)
                Dim lFilename As String
                For Each lFilename In lMatchingFilenames
                    For Each lSuffix As String In lNotTileSuffixes
                        If lFilename.ToLower.EndsWith(lSuffix) Then
                            GoTo SkipNotTile
                        End If
                    Next
                    lTileFilenames.Add(lFilename)

                    lDate = TileLastCheckedDate(lFilename)
                    If lDate > lFiledateMatch Then
                        lFiledateMatch = lDate
                        lFilenameMatch = lFilename
                    End If
SkipNotTile:
                Next

                If DeleteOldTiles Then
                    For Each lFilename In lTileFilenames
                        If lFilename <> lFilenameMatch Then
                            Try
                                IO.File.Delete(lFilename)
                                For Each lSuffix As String In lNotTileNotPartialSuffixes
                                    If IO.File.Exists(lFilename & lSuffix) Then
                                        IO.File.Delete(lFilename & lSuffix)
                                    End If
                                Next
                            Catch ex As Exception
                                Debug.WriteLine("Unable to delete " & lFilename)
                            End Try
                        End If
                    Next
                End If
            End If
        End If
        Return lFilenameMatch
    End Function

    Public Overrides Sub ServiceItem(ByVal aQueueItem As clsQueueItem)
        Select Case aQueueItem.ItemType
            Case QueueItemType.TileItem
                DownloadTile(aQueueItem.TileServer, aQueueItem.TilePoint, aQueueItem.Zoom, aQueueItem.ReplaceExisting)
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
    Public Function DownloadTile(ByVal aTileServer As clsServer, _
                                 ByVal aTilePoint As Point, _
                                 ByVal aZoom As Integer, _
                                 ByVal aReplaceExisting As Boolean) As String

        Dim lTileServerURL As String = aTileServer.TilePattern
        Dim lFileName As String = aTileServer.TileFilename(aTilePoint, aZoom, False)
        If lFileName.Length > 0 Then
            If EnsureDirForFile(lFileName) Then
                Dim lActualFilename As String = LatestTileFileName(lFileName)
                If aReplaceExisting _
                    OrElse Not IO.File.Exists(lActualFilename) _
                    OrElse (TileCacheOldest > Date.MinValue AndAlso TileLastCheckedDate(lActualFilename) < TileCacheOldest) Then
                    Dbg("Downloading Tile " & aZoom & "/" & aTilePoint.X & "/" & aTilePoint.Y)
                    If DownloadFile(aTileServer, aTileServer.BuildTileURL(aTilePoint, aZoom), lFileName, lActualFilename) Then
                        For Each lListener As IQueueListener In Listeners
                            lListener.DownloadedTile(aTilePoint, aZoom, lActualFilename, lTileServerURL)
                        Next
                        lFileName = lActualFilename
                    Else
                        lFileName = ""
                    End If
                End If
            Else
                lFileName = ""
            End If
        End If
        Return lFileName
    End Function

    ''' <summary>
    ''' Download an OSM tile and all tiles in the same area with finer detail
    ''' </summary>
    ''' <param name="aTilePoint">OSM Tile X and Y</param>
    ''' <param name="aZoom">OSM zoom level (0 to g_TileServer.ZoomMax)</param>
    ''' <param name="aZoomMax">Maximum zoom level to download (0 to g_TileServer.ZoomMax)</param>
    ''' <returns>True if all descendants were downloaded, False if any could not be downloaded</returns>
    ''' <remarks>If any descendant cannot be downloaded, no further descendants are attempted</remarks>
    Public Function DownloadDescendants(ByVal aTileServer As clsServer, _
                                        ByVal aTilePoint As Point, _
                                        ByVal aZoom As Integer, _
                                        ByVal aZoomMax As Integer, _
                               Optional ByVal aReplaceExisting As Boolean = False) As Boolean

        If DownloadTile(aTileServer, aTilePoint, aZoom, aReplaceExisting).Length > 0 Then
            For lZoom As Integer = aZoom + 1 To aZoomMax
                For childX As Integer = aTilePoint.X * 2 To aTilePoint.X * 2 + 1
                    For childY As Integer = aTilePoint.Y * 2 To aTilePoint.Y * 2 + 1
                        If Not DownloadDescendants(aTileServer, New Point(childX, childY), lZoom, aZoomMax, aReplaceExisting) Then Return False
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
        Dim lActualFilename As String = Nothing
        If DownloadFile(Nothing, aItem.URL, aItem.Filename, lActualFilename) Then
            'TODO: If IO.File.Exists(lActualFilename) Then aItem.Filename = lActualFilename ?
            For Each lListener As IQueueListener In Listeners
                lListener.DownloadedItem(aItem)
            Next
            Return True
        Else
            Return False
        End If
    End Function

    ' This function should not be used from outside this class for downloading tiles points, or icons, those should use DownloadTile and DownloadItem above
    Friend Function DownloadFile(ByVal aTileServer As clsServer, _
                                 ByVal aUrl As String, _
                                 ByVal aFileName As String, _
                                 ByRef aActualFilename As String) As Boolean
        Dim lSuccess As Boolean = False

        Dim lReplacingFilename As String
        If IO.File.Exists(aActualFilename) AndAlso FileSize(aActualFilename) > 0 Then
            lReplacingFilename = aActualFilename

            'Do not download new version if existing one has not yet expired
            'TODO: move this to before Enqueue?
            Dim lExpiresString As String = FilenameDecodeExt(lReplacingFilename, pExpiresExt)
            Dim lDateExpires As Date
            If DateTryParse(lExpiresString, lDateExpires) Then 'If expiration date is in filename, skip downloading if has not yet expired
                If Date.UtcNow < lDateExpires Then
                    Return False
                End If
            End If
        Else
            lReplacingFilename = ""
        End If

        aActualFilename = ""
        Try
            If EnsureDirForFile(aFileName) Then
                Dim lResponse As Net.WebResponse = Nothing
                Dim lInput As IO.Stream = Nothing
                Dim lOutput As IO.FileStream = Nothing
                Dim lBufferSize As Long = 128 * 1024 ' 128k at a time
                Dim lBytesRead As Long
                Dim lBuffer(lBufferSize - 1) As Byte
                Dim lDownloadAs As String = aFileName & pPartialDownloadExtension
                Dim lError As Boolean = False
                Dim lSkipped As Boolean = False
                Dim lCachedETag As String = FilenameDecodeExt(lReplacingFilename, pETagExt)
                Dim lMoveTo As String = aFileName
                Dbg("Download '" & aUrl & "' to '" & aFileName & "'")
                Dim lNewLastChecked As String = Format(Date.UtcNow, "R") ' "yyyy-MM-dd HH:mm:ss") 'Format(Date.UtcNow, "ddd, dd MMM yyyy HH:mm:ss") & " GMT"
                Dim lNewLastModified As String = lNewLastChecked.Clone

                Try
                    Dim request As Net.HttpWebRequest = Net.WebRequest.Create(aUrl)
                    If request IsNot Nothing Then
                        request.UserAgent = "VataviaMap/1.0"
                        request.Proxy.Credentials = Net.CredentialCache.DefaultCredentials
                        If lCachedETag.Length > 0 Then
                            request.Headers.Set("If-None-Match", """" & lCachedETag & """")
                        ElseIf lReplacingFilename.Length > 0 Then
                            request.IfModifiedSince = TileLastCheckedDate(lReplacingFilename)
                        End If
                        lResponse = request.GetResponse()
                        If lResponse IsNot Nothing Then
                            Dim lContentLength As String = lResponse.Headers.Item("Content-Length")
                            If lContentLength = "0" Then ' Nothing can be ok, but zero means there is no tile
                                lError = True
                            ElseIf aTileServer IsNot Nothing AndAlso lContentLength = aTileServer.BadTileSize Then ' Same size as "unavailable" tile, perhaps should check further but pretty sure this is a bad tile
                                lError = True
                            Else
                                Select Case lResponse.Headers.Item("Content-Type")
                                    Case "image/jpeg" : If lMoveTo.ToLower.IndexOf(".jp") < 0 Then lMoveTo &= ".jpg"
                                    Case "image/png" : If lMoveTo.ToLower.IndexOf(".png") < 0 Then lMoveTo &= ".png"
                                    Case Else
                                        If aTileServer IsNot Nothing Then
                                            lError = True
                                            GoTo AfterDownload
                                        End If
                                End Select
                                Dim lResponseLastModified As String = lResponse.Headers.Item("Last-Modified")
                                If lResponseLastModified IsNot Nothing Then
                                    lNewLastModified = lResponseLastModified
                                End If
                                Dim lNewETag As String = lResponse.Headers.Item("ETag")
                                If lNewETag IsNot Nothing Then
                                    lNewETag = FilenameEncode(lNewETag.Trim(""""))
                                    lMoveTo = IO.Path.ChangeExtension(lMoveTo, pETagExt & lNewETag & IO.Path.GetExtension(lMoveTo))
                                    If lNewETag.Length > 1 AndAlso _
                                        (lNewETag = lCachedETag OrElse _
                                            (FileSize(lMoveTo) > 0)) Then
                                        ' Cached ETag matches response ETag or other existing file, no need to download.
                                        ' Should not reach here, should have caused 304 exception at GetResponse above.
                                        lSuccess = True
                                        lSkipped = True
                                        GoTo AfterDownload
                                    End If
                                End If

                                Dim lNewExpires As String = lResponse.Headers.Item("Expires")
                                If lNewExpires Is Nothing Then 'If they don't tell us when it expires, assume it expires in a week
                                    lNewExpires = Format(Date.UtcNow.AddDays(7), "R")
                                End If

                                If lNewExpires IsNot Nothing Then
                                    Dim lNewExpiresDate As Date
                                    If DateTryParse(lNewExpires, lNewExpiresDate) AndAlso lNewExpiresDate > Date.Now Then
                                        lMoveTo = IO.Path.ChangeExtension(lMoveTo, pExpiresExt & FilenameEncode(lNewExpires) & IO.Path.GetExtension(lMoveTo))
                                    End If
                                End If

                                lDownloadAs = lMoveTo & pPartialDownloadExtension
                                lInput = lResponse.GetResponseStream()
                                If lInput IsNot Nothing Then
                                    lOutput = New IO.FileStream(lDownloadAs, IO.FileMode.Create)
                                    If lOutput IsNot Nothing Then
                                        Do
                                            lBytesRead = lInput.Read(lBuffer, 0, lBufferSize)
                                            If lBytesRead = 0 Then Exit Do 'finished download
                                            lOutput.Write(lBuffer, 0, lBytesRead)
                                        Loop
                                    End If
                                End If
                            End If
                        End If
                    End If
                Catch ex As Exception
                    If ex.Message.IndexOf("(304) Not Modified") > -1 Then
                        'Not an error, this means we don't need to download new file
                        lMoveTo &= pETagExt & lCachedETag
                        lSuccess = True
                        lSkipped = True
                    Else
                        lError = True
                        Dbg("Error downloading '" & aUrl & "' " & ex.Message)
                    End If
                End Try
AfterDownload:
                If lInput IsNot Nothing Then
                    Try
                        lInput.Close()
                    Catch exInput As Exception
                    End Try
                End If
                If lOutput IsNot Nothing Then
                    Try
                        lOutput.Close()
                    Catch exOutput As Exception
                    End Try
                End If
                If lResponse IsNot Nothing Then
                    Try
                        lResponse.Close()
                    Catch exResponse As Exception
                    End Try
                End If
                If lError Then
                    If aTileServer IsNot Nothing AndAlso lDownloadAs.Length > 0 Then
                        TileLastCheckedSet(aFileName, lNewLastChecked)
                    End If
                    Return False
                ElseIf lSkipped Then
                    If lReplacingFilename.Length > 0 Then
                        TileLastCheckedSet(lReplacingFilename, lNewLastChecked)
                    End If
                ElseIf IO.File.Exists(lDownloadAs) Then
                    Dim lStartMoving As DateTime = DateTime.Now
                    While IO.File.Exists(lMoveTo)
                        Try
                            If aTileServer IsNot Nothing Then
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
                            Windows.Forms.Application.DoEvents()
                        End Try
                    End While
                    Try
                        If aTileServer IsNot Nothing Then
                            TileLastCheckedSet(lMoveTo, lNewLastChecked)
                            If aTileServer.IsBadTile(lDownloadAs) Then
                                IO.File.Delete(lDownloadAs)
                                Return False
                            Else
                                IO.File.Move(lDownloadAs, lMoveTo)
                                If TileRAMcacheAddTile(aTileServer, aFileName, lMoveTo, Nothing, -1, -1, False) Then
                                    aActualFilename = lMoveTo
                                End If
                            End If
                        Else
                            If IO.File.Exists(aFileName) Then IO.File.Delete(aFileName)
                            IO.File.Move(lDownloadAs, aFileName)
                            aActualFilename = aFileName
                        End If
                        lSuccess = True
                    Catch exMove As Exception
                        Debug.WriteLine("Could not move '" & lDownloadAs & "' to '" & aFileName & "' : " & exMove.Message)
                    End Try
                End If
            End If
        Catch exMisc As Exception
            Debug.WriteLine("Exception during DownloadFile '" & aUrl & "' to '" & aFileName & "' : " & exMisc.Message)
        End Try
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