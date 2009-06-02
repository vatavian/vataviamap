'This code allow asynchronous downloading of tiles and other files while allowing the user to continue interacting with the user interface

''' <summary>
''' Manage a queue of download requests.
''' .Enabled = True must be set before downloading from the queue will start
''' </summary>
''' <remarks></remarks>
Public Class clsDownloader
    Inherits clsQueueManager

    Public TileRAMcacheLimit As Integer = 10
    Public TileRAMcache As New Generic.Dictionary(Of String, Bitmap)
    Public TileRAMcacheRecent As New Generic.List(Of String)

    Public Sub DeleteTile(ByVal aFilename As String)
        If TileRAMcache.ContainsKey(aFilename) Then
            TileRAMcache.Item(aFilename).Dispose()
            TileRAMcache.Remove(aFilename)
            TileRAMcacheRecent.Remove(aFilename)
        End If
        IO.File.Delete(aFilename)
    End Sub

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
    ''' <param name="aReplaceExisting">True to download even if tile is already cached</param>
    ''' <returns>Filename of downloaded tile image or empty string if not downloaded</returns>
    Public Function DownloadTile(ByVal aTilePoint As Point, _
                                 ByVal aZoom As Integer, _
                        Optional ByVal aReplaceExisting As Boolean = False) As String

        Dim lFileName As String = TileFilename(aTilePoint, aZoom, False)
        If lFileName.Length > 0 Then
            If EnsureDirForFile(lFileName) Then
                If aReplaceExisting OrElse Not IO.File.Exists(lFileName) Then
                    'Debug.WriteLine("Downloading Tile " & aZoom & "/" & aTilePoint.X & "/" & aTilePoint.Y)                    
                    If DownloadFile(MakeImageUrl(g_TileServerType, aTilePoint, aZoom), lFileName) Then
                        For Each lListener As IQueueListener In Listeners
                            lListener.DownloadedTile(aTilePoint, aZoom, lFileName)
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
    ''' Download an OSM tile
    ''' </summary>
    ''' <param name="aTilePoint">OSM Tile X and Y</param>
    ''' <param name="aZoom">OSM zoom level (0-17)</param>
    ''' <param name="aOldest">Keep cached tiles up to this age, download replacement if cached tile is older</param>
    ''' <returns>Filename of downloaded tile image or empty string if not downloaded</returns>
    Public Function DownloadTile(ByVal aTilePoint As Point, _
                                 ByVal aZoom As Integer, _
                                 ByVal aOldest As Date) As String

        Dim lFileName As String = TileFilename(aTilePoint, aZoom, False)
        If lFileName.Length > 0 Then
            If EnsureDirForFile(lFileName) Then
                If Not IO.File.Exists(lFileName) OrElse IO.File.GetCreationTime(lFileName) < aOldest Then
                    'Debug.WriteLine("Downloading Tile " & aZoom & "/" & aTilePoint.X & "/" & aTilePoint.Y)                    
                    If DownloadFile(MakeImageUrl(g_TileServerType, aTilePoint, aZoom), lFileName) Then
                        For Each lListener As IQueueListener In Listeners
                            lListener.DownloadedTile(aTilePoint, aZoom, lFileName)
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
    ''' <param name="aOldest">Keep cached tiles up to this age, download replacement if cached tile is older</param>
    Public Function DownloadDescendants(ByVal aTilePoint As Point, _
                                        ByVal aZoom As Integer, _
                                        ByVal aOldest As Date) As Boolean

        If DownloadTile(aTilePoint, aZoom, aOldest).Length > 0 Then
            For lZoom As Integer = aZoom + 1 To g_ZoomMax
                For childX As Integer = aTilePoint.X * 2 To aTilePoint.X * 2 + 1
                    For childY As Integer = aTilePoint.Y * 2 To aTilePoint.Y * 2 + 1
                        If Not DownloadDescendants(New Point(childX, childY), lZoom, aOldest) Then Return False
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
        If DownloadFile(aItem.URL, aItem.Filename) Then
            For Each lListener As IQueueListener In Listeners
                lListener.DownloadedItem(aItem)
            Next
            Return True
        Else
            Return False
        End If
    End Function

    ' This function should not be used from outside this class for downloading tiles points, or icons, those should use DownloadTile and DownloadItem above
    Friend Function DownloadFile(ByVal strUrl As String, ByVal strFileName As String) As Boolean
        If Not EnsureDirForFile(strFileName) Then
            Return False
        Else
            Dim request As Net.WebRequest
            Dim response As Net.WebResponse = Nothing
            Dim input As IO.Stream = Nothing
            Dim output As IO.FileStream = Nothing
            Dim count As Long = 128 * 1024 ' 128k at a time
            Dim buffer(count - 1) As Byte
            'Debug.WriteLine(strUrl)

            Try
                request = Net.WebRequest.Create(strUrl)
                If request IsNot Nothing Then
                    response = request.GetResponse()
                    If response IsNot Nothing Then
                        input = response.GetResponseStream()
                        If input IsNot Nothing Then
                            'If response.ContentLength > 0 Then
                            output = New IO.FileStream(strFileName & ".part", IO.FileMode.Create)
                            If output IsNot Nothing Then
                                Do
                                    count = input.Read(buffer, 0, count)
                                    If count = 0 Then Exit Do 'finished download
                                    output.Write(buffer, 0, count)
                                Loop
                            End If
                            'End If
                        End If
                    End If
                End If
            Catch ex As Exception
                'MsgBox("Error downloading '" & strUrl & "'" & vbLf & ex.ToString, MsgBoxStyle.Critical)
                Debug.WriteLine("Error downloading '" & strUrl & "' " & ex.Message)
            End Try
            If input IsNot Nothing Then input.Close()
            If output IsNot Nothing Then output.Close()
            If response IsNot Nothing Then response.Close()
            If IO.File.Exists(strFileName & ".part") Then 'AndAlso FileLen(strFileName & ".part") > 0 Then
                Dim lStartMoving As DateTime = DateTime.Now
                While IO.File.Exists(strFileName)
                    Try
                        DeleteTile(strFileName)
                    Catch
                        If DateTime.Now.Subtract(lStartMoving).TotalSeconds > 2 Then
                            Try 'target tile file busy too long, delete new version and report failure
                                IO.File.Delete(strFileName & ".part")
                            Catch
                            End Try
                            Return False
                        End If
                        Application.DoEvents()
                    End Try
                End While
                Try
                    IO.File.Move(strFileName & ".part", strFileName)
                    DownloadFile = True
                Catch

                End Try
            End If
        End If
    End Function
End Class