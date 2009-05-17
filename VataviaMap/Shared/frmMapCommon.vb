'Shared between VataviaMap Desktop and Mobile
Partial Class frmMap
    Implements IQueueListener

    Public CenterLat As Double = 33.8
    Public CenterLon As Double = -84.3

    Public LatHeight As Double 'Height of map display area in latitude degrees
    Public LonWidth As Double  'Width of map display area in longitude degrees

    Private pZoom As Integer = 10 'varies from g_ZoomMin to g_ZoomMax

    'Top-level tile cache folder. 
    'We name a folder inside here after the tile server, then zoom\x\y.png
    'Changing the tile server also changes g_TileCacheFolder
    Private pTileCacheFolder As String = ""

    'Tiles older than this will be downloaded again as needed
    Private pOldestCache As DateTime = New DateTime(1900, 1, 1)

    Private pShowTileImages As Boolean = True
    Private pShowTileOutlines As Boolean = False
    Private pShowTileNames As Boolean = False
    Private pShowGPSdetails As Boolean = False
    Private pUseMarkedTiles As Boolean = False
    Private pShowTransparentTiles As Boolean = False

    Private pControlsUse As Boolean = False
    Private pControlsShow As Boolean = False
    Private pControlsMargin As Integer = 40 'This gets set on resize

    Private pClickedTileFilename As String = ""

    Private pBrushBlack As New SolidBrush(Color.Black)
    Private pBrushWhite As New SolidBrush(Color.White)

    Private pPenBlack As New Pen(Color.Black)
    Private pPenCursor As New Pen(Color.Red)

    Private pFontTileLabel As New Font("Arial", 10, FontStyle.Regular)
    Private pFontCopyright As New Font(FontFamily.GenericSansSerif, 7, FontStyle.Regular)
    Private pBrushCopyright As New SolidBrush(Color.Black)
    Private pShowCopyright As Boolean = True

    Private pMouseDragging As Boolean = False
    Private pMouseDragStartLocation As Point
    Private pMouseDownLat As Double
    Private pMouseDownLon As Double

    Private pLastKeyDown As Integer = 0

    Private pBitmap As Bitmap
    Private pBitmapMutex As New Threading.Mutex()

    Private pLayers As New Generic.List(Of clsLayer)

    Private pGPXPanTo As Boolean = True
    Private pGPXZoomTo As Boolean = True
    Private pGPXShow As Boolean = True
    Private pGPXFolder As String
    Private pGPXLabelField As String = "name"

    Private pBuddyTimer As System.Threading.Timer

    ' Alert when buddy location is within pBuddyAlarmMeters of (pBuddyAlarmLat, pBuddyAlarmLon)
    Private pBuddyAlarmEnabled As Boolean = False
    Private pBuddyAlarmLat As Double = 0
    Private pBuddyAlarmLon As Double = 0
    Private pBuddyAlarmMeters As Double = 0 'Must be greater than zero to get buddy alerts

    Delegate Sub OpenCallback(ByVal aFilename As String, ByVal aInsertAt As Integer)
    Private pOpenGPXCallback As New OpenCallback(AddressOf OpenGPX)

    ' True to automatically start GPS when application starts
    Private pGPSAutoStart As Boolean = True

    ' 0=don't follow, 1=re-center only when GPS moves off screen, 2=keep centered
    Private pGPSFollow As Integer = 2

    ' Symbol at GPS position
    Private pGPSSymbolSize As Integer = 10

    ' Symbol at each track point
    Private pTrackSymbolSize As Integer = 3

    ' True to record track information while GPS is on
    Private pRecordTrack As Boolean = False

    ' True to save GSM cell tower identification in recorded track
    Private pRecordCellID As Boolean = True

    ' API key for use of OpenCellId.org, see http://www.opencellid.org/api
    ' Get location with http://www.opencellid.org/cell/get?key=&mcc=&mnc&cellid=&lac=
    ' Send location with http://www.opencellid.org/measure/add?key=&mnc=&mcc=&lac=&cellid=&lat=&lon=
    Private pOpenCellIdKey As String = ""

    ' User name and hash for use of celldb.org, see http://www.celldb.org/aboutapi.php
    ' Get location with http://celldb.org/api/?method=celldb.getcell&username=&hash=&mcc=&mnc=&lac=&cellid=&format=xml
    ' Send location with http://celldb.org/api/?method=celldb.addcell&username=&hash=&mcc=&mnc=&lac=&cellid=&latitude=&longitude=&signalstrength=&format=xml
    Private pCellDbUsername As String = ""
    Private pCellDbHash As String = ""

    ' True to display current track information while GPS is on
    Private pDisplayTrack As Boolean = False

    ' Wait at least this long before logging a new point
    Private pTrackMinInterval As New TimeSpan(0, 0, 2)

    ' Wait at least this long before uploading a new point
    Private pUploadMinInterval As New TimeSpan(0, 2, 0)

    Private pUploadOnStart As Boolean = False  ' True to upload point when GPS starts
    Private pUploadOnStop As Boolean = False   ' True to upload point when GPS stops
    Private pUploadPeriodic As Boolean = False ' True to upload point periodically

    ' Set to false when another form is covering main form, skip redrawing
    Private pFormVisible As Boolean = False
    Private pFormDark As Boolean = False

    Delegate Sub RefreshCallback()
    Private pRefreshCallback As New RefreshCallback(AddressOf Refresh)
    Private pRedrawCallback As New RefreshCallback(AddressOf Redraw)
    Private pRedrawing As Boolean = False
    Private pRedrawPending As Boolean = False

    ' Set to false when we have a first-guess background (when zooming)
    Private pClearDelayedTiles As Boolean = True

    Private pUploader As New clsUploader
    Private pDownloader As New clsDownloader

    Private pTileServers As Dictionary(Of String, String)

    'Private Declare Function GetAsyncKeyState Lib "user32" (ByVal vKey As Integer) As Integer

    Public Sub SharedNew()
        pGPXFolder = "\My Documents\gps"
        GetSettings()

        For Each lFilename As String In IO.Directory.GetFiles(pTileCacheFolder, "*.gif")
            Try
                Dim lBitmap As New Drawing.Bitmap(lFilename)
                g_WaypointIcons.Add("Geocache|" & IO.Path.GetFileNameWithoutExtension(lFilename), lBitmap)
            Catch
            End Try
        Next

        ' The main form registers itself as a listener so Me.DownloadedTile will be called when each tile is finished.
        ' Me.FinishedQueue and Me.DownloadedPoint are also called when appropriate
        pDownloader.Listeners.Add(Me)

        'Start the download and upload queue runners
        pDownloader.Enabled = True
        pUploader.Enabled = True

        pFormVisible = True
    End Sub

    Private Sub GetSettings()
        pTileServers = New Dictionary(Of String, String)
        Dim lSoftwareKey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software")
        If lSoftwareKey IsNot Nothing Then
            Dim lAppKey As Microsoft.Win32.RegistryKey = lSoftwareKey.OpenSubKey(g_AppName)
            If lAppKey IsNot Nothing Then
                With lAppKey
                    On Error Resume Next 'Maybe a registry setting is not numeric but needs to be, skip bad settings
                    For lServerIndex As Integer = 1 To 30
                        Dim lServerNameUrl() As String = CStr(.GetValue("TileServer" & lServerIndex, "")).Split("|")
                        If lServerNameUrl.Length = 2 Then
                            pTileServers.Add(lServerNameUrl(0), lServerNameUrl(1))
                        End If
                    Next

                    'TileCacheFolder gets set earlier
                    g_TileServerName = .GetValue("TileServer", g_TileServerName)
                    g_UploadURL = .GetValue("UploadURL", g_UploadURL)

                    'Dim lCacheDays As Integer = 7
                    'lCacheDays = .GetValue("TileCacheDays", lCacheDays)
                    'pOldestCache = DateTime.Now.AddDays(-lCacheDays)

                    Zoom = .GetValue("Zoom", Zoom)

                    pGPXShow = .GetValue("GPXShow", pGPXShow)
                    pGPXPanTo = .GetValue("GPXPanTo", pGPXPanTo)
                    pGPXZoomTo = .GetValue("GPXZoomTo", pGPXZoomTo)
                    pGPXFolder = .GetValue("GPXFolder", pGPXFolder)
                    pGPXLabelField = .GetValue("GPXLabelField", pGPXLabelField)

                    pShowTileImages = .GetValue("TileImages", pShowTileImages)
                    pShowTileOutlines = .GetValue("TileOutlines", pShowTileOutlines)
                    pShowTileNames = .GetValue("TileNames", pShowTileNames)
                    pShowGPSdetails = .GetValue("GPSdetails", pShowGPSdetails)
                    pShowCopyright = .GetValue("ShowCopyright", pShowCopyright)

                    pControlsUse = .GetValue("ControlsUse", pControlsUse)
                    pControlsShow = .GetValue("ControlsShow", pControlsShow)
                    'TODO: pControlsMargin, but in pixels or percent?

                    CenterLat = .GetValue("CenterLat", CenterLat)
                    CenterLon = .GetValue("CenterLon", CenterLon)
                    SanitizeCenterLatLon()

                    pGPSAutoStart = .GetValue("GPSAutoStart", pGPSAutoStart)
                    pGPSFollow = .GetValue("GPSFollow", pGPSFollow)
                    pGPSSymbolSize = .GetValue("GPSSymbolSize", pGPSSymbolSize)
                    pTrackSymbolSize = .GetValue("TrackSymbolSize", pTrackSymbolSize)

                    pRecordTrack = .GetValue("RecordTrack", pRecordTrack)
                    pRecordCellID = .GetValue("RecordCellID", pRecordCellID)
                    pOpenCellIdKey = .GetValue("OpenCellIdKey", pOpenCellIdKey)
                    pDisplayTrack = .GetValue("DisplayTrack", pDisplayTrack)
                    pUploadOnStart = .GetValue("UploadOnStart", pUploadOnStart)
                    pUploadOnStop = .GetValue("UploadOnStop", pUploadOnStop)
                    pUploadPeriodic = .GetValue("UploadPeriodic", pUploadPeriodic)
                    pUploadMinInterval = New TimeSpan(0, 0, .GetValue("UploadPeriodicSeconds", pUploadMinInterval.TotalSeconds))
                    g_UploadURL = .GetValue("UploadURL", g_UploadURL)
                End With
            End If
        End If
        If pTileServers.Count = 0 Then
            SetDefaultTileServers()
        End If
        TileServerName = g_TileServerName
    End Sub

    Private Sub SetDefaultTileServers()
        Dim pDefaultTileServers() As String = {"Mapnik", "http://tile.openstreetmap.org/mapnik/", _
                                               "Maplint", "http://tah.openstreetmap.org/Tiles/maplint/", _
                                               "Osmarender", "http://tah.openstreetmap.org/Tiles/tile/", _
                                               "Cycle Map", "http://andy.sandbox.cloudmade.com/tiles/cycle/", _
                                               "No Name", "http://tile.cloudmade.com/fd093e52f0965d46bb1c6c6281022199/3/256/"}
        pTileServers = New Dictionary(Of String, String)
        For lIndex As Integer = 0 To pDefaultTileServers.Length - 1 Step 2
            pTileServers.Add(pDefaultTileServers(lIndex), pDefaultTileServers(lIndex + 1))
        Next

        '    Windows Mobile does not support Enum.GetValues or Enum.GetName, so we hard-code names to add below
        '    For Each lMapType As MapType In System.Enum.GetValues(GetType(MapType))
        '        Dim lMapTypeName As String = System.Enum.GetName(GetType(MapType), lMapType)
        '        If lMapTypeName.IndexOf("OpenStreet") < 0 Then 'Only add non-OSM types
        '            pTileServers.Add(lMapTypeName, MakeImageUrl(lMapType, New Point(1, 2), 3))
        '        End If
        '    Next

        pTileServers.Add("YahooMap", MakeImageUrl(MapType.YahooMap, New Point(1, 2), 3))
        pTileServers.Add("YahooSatellite", MakeImageUrl(MapType.YahooSatellite, New Point(1, 2), 3))
        pTileServers.Add("YahooLabels", MakeImageUrl(MapType.YahooLabels, New Point(1, 2), 3))

        pTileServers.Add("VirtualEarthMap", MakeImageUrl(MapType.VirtualEarthMap, New Point(1, 2), 3))
        pTileServers.Add("VirtualEarthSatellite", MakeImageUrl(MapType.VirtualEarthSatellite, New Point(1, 2), 3))
        pTileServers.Add("VirtualEarthHybrid", MakeImageUrl(MapType.VirtualEarthHybrid, New Point(1, 2), 3))
    End Sub

    Private Sub SaveSettings()
        On Error Resume Next
        Dim lSoftwareKey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software")
        If lSoftwareKey IsNot Nothing Then
            Dim lAppKey As Microsoft.Win32.RegistryKey = lSoftwareKey.CreateSubKey(g_AppName)
            If lAppKey IsNot Nothing Then
                With lAppKey
                    'TODO: .SetValue("TileCacheDays", lCacheDays)

                    Dim lServerIndex As Integer = 0
                    For Each lName As String In pTileServers.Keys
                        lServerIndex += 1
                        .SetValue("TileServer" & lServerIndex, lName & "|" & pTileServers.Item(lName))
                    Next

                    .SetValue("TileCacheFolder", pTileCacheFolder)
                    .SetValue("TileServer", g_TileServerName)
                    .SetValue("Zoom", Zoom)

                    .SetValue("GPXShow", pGPXShow)
                    .SetValue("GPXPanTo", pGPXPanTo)
                    .SetValue("GPXZoomTo", pGPXZoomTo)
                    .SetValue("GPXFolder", pGPXFolder)
                    .SetValue("GPXLabelField", pGPXLabelField)

                    .SetValue("TileImages", pShowTileImages)
                    .SetValue("TileOutlines", pShowTileOutlines)
                    .SetValue("TileNames", pShowTileNames)
                    .SetValue("GPSdetails", pShowGPSdetails)

                    .SetValue("ControlsUse", pControlsUse)
                    .SetValue("ControlsShow", pControlsShow)
                    'TODO: pControlsMargin, but in pixels or percent?

                    .SetValue("CenterLat", CenterLat)
                    .SetValue("CenterLon", CenterLon)

                    If Me.WindowState = FormWindowState.Normal Then
                        .SetValue("WindowWidth", Me.Width)
                        .SetValue("WindowHeight", Me.Height)
                    End If

                    .SetValue("GPSAutoStart", pGPSAutoStart)
                    .SetValue("GPSFollow", pGPSFollow)
                    .SetValue("GPSSymbolSize", pGPSSymbolSize)
                    .SetValue("TrackSymbolSize", pTrackSymbolSize)
                    .SetValue("RecordTrack", pRecordTrack)
                    .SetValue("RecordCellID", pRecordCellID)
                    .SetValue("OpenCellIdKey", pOpenCellIdKey)
                    .SetValue("DisplayTrack", pDisplayTrack)
                    .SetValue("UploadOnStart", pUploadOnStart)
                    .SetValue("UploadOnStop", pUploadOnStop)
                    .SetValue("UploadPeriodic", pUploadPeriodic)
                    .SetValue("UploadPeriodicSeconds", pUploadMinInterval.TotalSeconds)
                    .SetValue("UploadURL", g_UploadURL)
                End With
            End If
        End If
    End Sub

    Private Property TileServerName() As String
        Get
            Return g_TileServerName
        End Get
        Set(ByVal value As String)
            If pTileServers.ContainsKey(value) Then
                g_TileServerName = value
                TileServerUrl = pTileServers(value)
                Try
                    g_TileServerType = System.Enum.Parse(GetType(MapType), g_TileServerName, False)
                Catch ex As Exception
                    g_TileServerType = MapType.OpenStreetMap
                End Try
                g_TileCopyright = CopyrightFromMapType(g_TileServerType)
                pBrushCopyright = New SolidBrush(Color.Black)
            End If
        End Set
    End Property

    ''' <summary>
    ''' Base URL of server to request tiles from, must end with trailing slash
    ''' zoom/x/y.png will be appended when requesting tiles
    ''' </summary>
    ''' <remarks>Side effects of setting TileServer: Me.Title and g_TileCacheFolder are updated</remarks>
    Private Property TileServerUrl() As String
        Get
            Return g_TileServerURL
        End Get
        Set(ByVal value As String)

            pDownloader.ClearQueue()

            If value.IndexOf(":/") < 0 Then 'Doesn't look like a URL, try setting by name instead
                TileServerName = value
            Else
                g_TileServerURL = value

                Me.Text = g_AppName & " " & g_TileServerName
                SetCacheFolderFromTileServer()
            End If
        End Set
    End Property

    Private Sub SetCacheFolderFromTileServer()
        g_TileCacheFolder = IO.Path.Combine(pTileCacheFolder, SafeFilename(g_TileServerURL.Replace("http://", ""))) & g_PathChar
    End Sub

    ''' <summary>
    ''' OSM zoom level currently being displayed on the form
    ''' </summary>
    ''' <remarks>varies from g_ZoomMin to g_ZoomMax</remarks>
    Public Property Zoom() As Integer
        Get
            Return pZoom
        End Get
        Set(ByVal value As Integer)
            If value <> pZoom Then
                pDownloader.ClearQueue()
                If value > g_ZoomMax Then
                    pZoom = g_ZoomMax
                ElseIf value < g_ZoomMin Then
                    pZoom = g_ZoomMin
                Else
                    'ZoomPreview(value)
                    pZoom = value
                End If
            End If
            Zoomed()
            pClearDelayedTiles = True
        End Set
    End Property

    Private Sub ZoomPreview(ByVal aNewZoom As Integer)
        If aNewZoom = pZoom + 1 OrElse aNewZoom = pZoom - 1 Then
            Dim lGraphics As Graphics = GetBitmapGraphics()
            If lGraphics IsNot Nothing Then
                With pBitmap
                    Dim lBitmap As New Bitmap(pBitmap)
                    Dim lHalfHeight As Integer = .Height >> 1
                    Dim lHalfWidth As Integer = .Width >> 1
                    Dim lQuarterHeight As Integer = lHalfHeight >> 1
                    Dim lQuarterWidth As Integer = lHalfWidth >> 1
                    Dim lSourceRect As Rectangle
                    Dim lDestRect As Rectangle

                    If aNewZoom = pZoom + 1 Then
                        lSourceRect = New Rectangle(lQuarterWidth, lQuarterHeight, lHalfWidth, lHalfHeight)
                        lDestRect = New Rectangle(0, 0, .Width, .Height)
                    Else
                        lGraphics.Clear(Color.White)
                        lSourceRect = New Rectangle(0, 0, .Width, .Height)
                        lDestRect = New Rectangle(lQuarterWidth, lQuarterHeight, lHalfWidth, lHalfHeight)
                    End If

                    lGraphics.DrawImage(lBitmap, lDestRect, lSourceRect, GraphicsUnit.Pixel)
                    ReleaseBitmapGraphics()
                    pClearDelayedTiles = False 'We have provided something to to see at new zoom, no need to clear
                End With
            End If
        End If
    End Sub

    Private Function GetBitmapGraphics()
        pBitmapMutex.WaitOne()
        If Not pBitmap Is Nothing Then
            Dim lGraphics As Graphics = Graphics.FromImage(pBitmap)
            lGraphics.Clip = New Drawing.Region(New Drawing.Rectangle(0, 0, pBitmap.Width, pBitmap.Height))
            Return lGraphics
        Else
            pBitmapMutex.ReleaseMutex()
            Return Nothing
        End If
    End Function

    Private Sub ReleaseBitmapGraphics()
        pBitmapMutex.ReleaseMutex()
    End Sub

    ''' <summary>
    ''' Calculate the top left and bottom right OSM tiles that will fit within aBounds
    ''' </summary>
    ''' <param name="aBounds">Rectangle containing the drawn tiles</param>
    ''' <param name="aOffsetToCenter">Pixel offset from corner of tile to allow accurate pCenterLat, pCenterLon</param>
    ''' <param name="aTopLeft">OSM coordinates of northwestmost visible tile</param>
    ''' <param name="aBotRight">OSM coordinates of southeastmost visible tile</param>
    ''' <remarks></remarks>
    Private Sub FindTileBounds(ByVal aBounds As RectangleF, _
                               ByRef aOffsetToCenter As Point, _
                               ByRef aTopLeft As Point, ByRef aBotRight As Point)
        Dim lOffsetFromTileEdge As Point
        Dim lCentermostTile As Point = CalcTileXY(CenterLat, CenterLon, pZoom, lOffsetFromTileEdge)
        Dim lNorth As Double, lWest As Double, lSouth As Double, lEast As Double
        CalcLatLonFromTileXY(lCentermostTile, pZoom, lNorth, lWest, lSouth, lEast)
        LatHeight = (lNorth - lSouth) * aBounds.Height / g_TileSize
        LonWidth = (lEast - lWest) * aBounds.Width / g_TileSize

        aTopLeft = New Point(lCentermostTile.X, lCentermostTile.Y)
        Dim x As Integer = (aBounds.Width / 2) - lOffsetFromTileEdge.X
        'Move west until we find the edge of g, TODO: find faster with mod?
        While x > 0
            aTopLeft.X -= 1
            x -= g_TileSize
        End While
        aOffsetToCenter.X = x

        Dim y As Integer = (aBounds.Height / 2) - lOffsetFromTileEdge.Y
        'Move north until we find the edge of g, TODO: find faster with mod?
        While y > 0
            aTopLeft.Y -= 1
            y -= g_TileSize
        End While
        aOffsetToCenter.Y = y

        aBotRight = New Point(lCentermostTile.X, lCentermostTile.Y)
        x = (aBounds.Width / 2) - lOffsetFromTileEdge.X + g_TileSize
        'Move east until we find the edge of g, TODO: find faster with mod?
        While x < aBounds.Width
            aBotRight.X += 1
            x += g_TileSize
        End While

        y = (aBounds.Height / 2) - lOffsetFromTileEdge.Y + g_TileSize
        'Move south until we find the edge of g, TODO: find faster with mod?
        While y < aBounds.Height
            aBotRight.Y += 1
            y += g_TileSize
        End While
    End Sub

    ''' <summary>
    ''' Draw all the visible tiles
    ''' </summary>
    ''' <param name="g">Graphics object to be drawn into</param>
    Private Sub DrawTiles(ByVal g As Graphics)
        If Not pShowTransparentTiles AndAlso g_TileServerURL.EndsWith("/maplint/") Then g.Clear(Color.White)
        If IO.Directory.Exists(pTileCacheFolder) Then
            Dim lOffsetToCenter As Point
            Dim lTopLeft As Point
            Dim lBotRight As Point

            FindTileBounds(g.ClipBounds, lOffsetToCenter, lTopLeft, lBotRight)

            Dim lTilePoint As Point
            Dim lOffsetFromWindowCorner As Point

            'TODO: check available memory before deciding how many tiles to keep in RAM
            pDownloader.TileRAMcacheLimit = (lBotRight.X - lTopLeft.X + 1) * (lBotRight.Y - lTopLeft.Y + 1) * 2

            'Loop through each visible tile
            For x As Integer = lTopLeft.X To lBotRight.X
                For y As Integer = lTopLeft.Y To lBotRight.Y

                    If pRedrawPending Then Exit Sub

                    lTilePoint = New Point(x, y)

                    lOffsetFromWindowCorner.X = (x - lTopLeft.X) * g_TileSize + lOffsetToCenter.X
                    lOffsetFromWindowCorner.Y = (y - lTopLeft.Y) * g_TileSize + lOffsetToCenter.Y

                    Dim lTileFileName As String = TileFilename(lTilePoint, pZoom, pUseMarkedTiles)

                    Dim lOriginalTileFileName As String
                    If pUseMarkedTiles Then
                        lOriginalTileFileName = TileFilename(lTilePoint, pZoom, False)
                        ' Go ahead and use unmarked tile if we have it but not a marked version
                        If Not IO.File.Exists(lTileFileName) Then lTileFileName = lOriginalTileFileName
                    Else
                        lOriginalTileFileName = lTileFileName
                    End If

                    Dim lDrewTile As Boolean = DrawTile(lTileFileName, g, lOffsetFromWindowCorner)
                    If Not lDrewTile Then ' search for cached tiles at other zoom levels to substitute
                        'First try tiles zoomed farther out
                        Dim lZoom As Integer
                        Dim lX As Integer = x
                        Dim lY As Integer = y
                        Dim lNextX As Integer
                        Dim lNextY As Integer
                        Dim lZoomedTilePortion As Integer = g_TileSize
                        Dim lFromX As Integer = 0
                        Dim lFromY As Integer = 0
                        Dim lRectOrigTile As New Rectangle(lOffsetFromWindowCorner.X, lOffsetFromWindowCorner.Y, g_TileSize, g_TileSize)
                        For lZoom = pZoom - 1 To g_ZoomMin Step -1
                            'Tile coordinates of next zoom out are half
                            lNextX = lX >> 1
                            lNextY = lY >> 1

                            'Half as much of next zoomed tile width and height will fit 
                            lZoomedTilePortion >>= 1

                            'Offset within the tile counts half as much in next zoom out
                            lFromX >>= 1
                            lFromY >>= 1

                            'If zoomed from an odd-numbered tile, it was in the right and/or bottom half of next zoom out
                            If lX > lNextX << 1 Then lFromX += g_HalfTile
                            If lY > lNextY << 1 Then lFromY += g_HalfTile

                            If DrawTile(TileFilename(New Drawing.Point(lNextX, lNextY), lZoom, False), g, lRectOrigTile, _
                                    New Rectangle(lFromX, lFromY, lZoomedTilePortion, lZoomedTilePortion)) Then
                                Exit For 'found a zoomed out tile to draw, don't keep looking for one zoomed out farther
                            End If
                            lX = lNextX
                            lY = lNextY
                        Next

                        If pZoom < g_ZoomMax Then ' Try to draw tiles within this tile at finer zoom level
                            lZoom = pZoom + 1
                            Dim lDoubleX As Integer = x << 1
                            Dim lFineTilePoint As New Drawing.Point(lDoubleX, y << 1)
                            ' Portion of the tile covered by a finer zoom tile
                            Dim lRectDestination As New Rectangle(lOffsetFromWindowCorner.X, _
                                                                  lOffsetFromWindowCorner.Y, _
                                                                  g_HalfTile, g_HalfTile)
                            ' upper left tile
                            DrawTile(TileFilename(lFineTilePoint, lZoom, False), g, lRectDestination, g_TileSizeRect)
                            ' upper right tile
                            lFineTilePoint.X += 1
                            lRectDestination.X = lOffsetFromWindowCorner.X + g_HalfTile
                            DrawTile(TileFilename(lFineTilePoint, lZoom, False), g, lRectDestination, g_TileSizeRect)
                            ' lower right tile
                            lFineTilePoint.Y += 1
                            lRectDestination.Y = lOffsetFromWindowCorner.Y + g_HalfTile
                            DrawTile(TileFilename(lFineTilePoint, lZoom, False), g, lRectDestination, g_TileSizeRect)
                            ' lower left tile
                            lFineTilePoint.X = lDoubleX
                            lRectDestination.X = lOffsetFromWindowCorner.X
                            DrawTile(TileFilename(lFineTilePoint, lZoom, False), g, lRectDestination, g_TileSizeRect)
                        End If
                    End If
                    If pDownloader.Enabled Then
                        If Not lDrewTile Then
                            pDownloader.Enqueue(lTilePoint, pZoom, 0, True) 'Request missing tile with highest priority
                        ElseIf IO.File.GetLastWriteTime(lOriginalTileFileName) < pOldestCache Then
                            pDownloader.DeleteTile(lOriginalTileFileName)
                            pDownloader.Enqueue(lTilePoint, pZoom, 1, True) 'Request outdated tile with lower priority
                        End If
                    End If
                Next
            Next

            If pRedrawPending Then Exit Sub

            If pControlsShow Then
                g.DrawLine(pPenBlack, pControlsMargin, 0, pControlsMargin, Me.Height)
                g.DrawLine(pPenBlack, 0, pControlsMargin, Me.Width, pControlsMargin)
                g.DrawLine(pPenBlack, Me.Width - pControlsMargin, 0, Me.Width - pControlsMargin, Me.Height)
                g.DrawLine(pPenBlack, 0, Me.Height - pControlsMargin, Me.Width, Me.Height - pControlsMargin)
            End If

            If pGPXShow Then DrawLayers(g, lTopLeft, lOffsetToCenter)

            If pShowCopyright Then g.DrawString(g_TileCopyright, pFontCopyright, pBrushCopyright, 3, Height - 50)

            If pRedrawPending Then Exit Sub

            DrewTiles(g, lTopLeft, lOffsetToCenter)
        End If
    End Sub

    ''' <summary>
    ''' Find the tile at screen position aX, aY
    ''' </summary>
    ''' <param name="aBounds">Rectangle containing the drawn tiles</param>
    ''' <param name="aX">Screen-based X value for tile to search for</param>
    ''' <param name="aY">Screen-based Y value for tile to search for</param>
    ''' <returns>filename of tile at aX, aY</returns>
    Private Function FindTileFilename(ByVal aBounds As RectangleF, _
                             Optional ByVal aX As Integer = -1, _
                             Optional ByVal aY As Integer = -1) As String
        Dim lOffsetToCenter As Point
        Dim lTopLeft As Point
        Dim lBotRight As Point

        FindTileBounds(aBounds, lOffsetToCenter, lTopLeft, lBotRight)

        Dim TilePoint As Point
        Dim lOffsetFromWindowCorner As Point

        'Loop through each visible tile
        For x As Integer = lTopLeft.X To lBotRight.X
            For y As Integer = lTopLeft.Y To lBotRight.Y
                TilePoint = New Point(x, y)

                lOffsetFromWindowCorner.X = (x - lTopLeft.X) * g_TileSize + lOffsetToCenter.X
                lOffsetFromWindowCorner.Y = (y - lTopLeft.Y) * g_TileSize + lOffsetToCenter.Y

                With lOffsetFromWindowCorner
                    If .X <= aX AndAlso .X + g_TileSize >= aX AndAlso .Y <= aY AndAlso .Y + g_TileSize >= aY Then
                        Return TileFilename(TilePoint, pZoom, False)
                    End If
                End With
            Next
        Next
        Return ""
    End Function

    ''' <summary>
    ''' Callback is called when a tile has just finished downloading, draws the tile into the display
    ''' </summary>
    ''' <param name="aTilePoint"></param>
    ''' <param name="aZoom"></param>
    ''' <param name="aTileFilename"></param>
    ''' <remarks></remarks>
    Public Sub DownloadedTile(ByVal aTilePoint As Point, ByVal aZoom As Integer, ByVal aTileFilename As String) Implements IQueueListener.DownloadedTile
        If pDownloader.TileRAMcache.ContainsKey(aTileFilename) Then
            pDownloader.TileRAMcache.Remove(aTileFilename)
            pDownloader.TileRAMcacheRecent.Remove(aTileFilename)
        End If
        Dim lGraphics As Graphics = GetBitmapGraphics()
        If lGraphics IsNot Nothing Then
            Dim lOffsetToCenter As Point
            Dim lTopLeft As Point
            Dim lBotRight As Point

            FindTileBounds(lGraphics.ClipBounds, lOffsetToCenter, lTopLeft, lBotRight)

            Dim lOffsetFromWindowCorner As Point
            If aZoom = pZoom Then
                lOffsetFromWindowCorner.X = (aTilePoint.X - lTopLeft.X) * g_TileSize + lOffsetToCenter.X
                lOffsetFromWindowCorner.Y = (aTilePoint.Y - lTopLeft.Y) * g_TileSize + lOffsetToCenter.Y
                DrawTile(aTileFilename, lGraphics, lOffsetFromWindowCorner)
            Else
                'TODO: draw tiles at different zoom levels? 
                'Would be nice when doing DownloadDescendants to see progress, but would also be confusing when tile download completes after zoom
            End If
            ReleaseBitmapGraphics()

            'This method will be running in another thread, so we need to call Refresh in a complicated way
            Try
                Me.Invoke(pRefreshCallback)
            Catch
                'Ignore if we could not refresh, probably because form is closing
            End Try
        End If
    End Sub

    Private Sub RequestBuddyPoint(ByVal o As Object)
        pDownloader.Enqueue("http://vatavia.net/cgi-bin/points?u=rachel", IO.Path.GetTempPath & "rachel", 0, True)
    End Sub

    Public Sub DownloadedPoint(ByVal aFilename As String) Implements IQueueListener.DownloadedPoint
        Dim lGPXstring As New System.Text.StringBuilder

        Dim lReader As IO.StreamReader = IO.File.OpenText(aFilename)
        Dim lPointLines() As String = lReader.ReadToEnd().Replace(vbCrLf, vbLf).Replace(vbCr, vbLf).Split(vbLf)
        For Each lLine As String In lPointLines
            Dim lFields() As String = lLine.Split(","c)
            If lFields.Length > 4 AndAlso lFields(0).Length > 0 Then
                Try
                    Dim lLat As Double = lFields(3)
                    Dim lLon As Double = lFields(4)
                    Dim lTimeSince As TimeSpan = Now.Subtract(Date.Parse(lFields(0) & " " & lFields(1)))
                    Dim lTimeSinceString As String = " "
                    If lTimeSince.TotalDays >= 1 Then lTimeSinceString &= lTimeSince.Days & "d "
                    If lTimeSince.TotalHours >= 1 Then lTimeSinceString &= lTimeSince.Hours & "h "
                    If lTimeSince.TotalMinutes > 1 Then lTimeSinceString &= lTimeSince.Minutes & "m "

                    lGPXstring.Append("<trkpt lat=""" & lFields(3) & """ lon=""" & lFields(4) & """>" & vbLf)
                    lGPXstring.Append("<name>" & IO.Path.GetFileNameWithoutExtension(aFilename) & " " & lTimeSinceString & "</name>" & vbLf)
                    If lFields.Length > 7 AndAlso lFields(7).Length > 0 Then lGPXstring.Append("<ele>" & lFields(7) & "</ele>" & vbLf)
                    If lFields.Length > 8 AndAlso lFields(8).Length > 0 Then lGPXstring.Append("<time>" & lFields(8) & "</time>" & vbLf)
                    If lFields.Length > 5 AndAlso lFields(5).Length > 0 Then lGPXstring.Append("<speed>" & lFields(5) & "</speed>" & vbLf)
                    If lFields.Length > 6 AndAlso lFields(6).Length > 0 Then lGPXstring.Append("<course>" & lFields(6) & "</course>" & vbLf)
                    lGPXstring.Append("<sym>Flag, Blue</sym></trkpt>")

                    If pBuddyAlarmEnabled AndAlso pBuddyAlarmMeters > 0 Then
                        If MetersBetweenLatLon(pBuddyAlarmLat, pBuddyAlarmLon, lLat, lLon) < pBuddyAlarmMeters Then
                            Windows.Forms.MessageBox.Show(IO.Path.GetFileNameWithoutExtension(aFilename) & " approaches", "Nearby Buddy", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
                            pBuddyAlarmEnabled = False
                        End If
                    End If
                Catch ex As Exception 'Maybe we could not parse lat and lon?
                    Debug.WriteLine(ex.Message)
                End Try
            End If
        Next
        If lGPXstring.Length > 0 Then
            Dim lGPXfilename As String = aFilename & ".gpx"
            Dim lWriter As New IO.StreamWriter(lGPXfilename)
            lWriter.Write("<?xml version=""1.0"" encoding=""UTF-8""?>" & vbLf _
                        & "<gpx xmlns=""http://www.topografix.com/GPX/1/1"" version=""1.1"" creator=""" & g_AppName & """ xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd http://www.topografix.com/GPX/gpx_overlay/0/3 http://www.topografix.com/GPX/gpx_overlay/0/3/gpx_overlay.xsd http://www.topografix.com/GPX/gpx_modified/0/1 http://www.topografix.com/GPX/gpx_modified/0/1/gpx_modified.xsd"">" & vbLf _
                        & "<trk><name>" & Now.ToShortDateString & "</name><type>GPS Tracklog</type>" & vbLf _
                        & "<trkseg>" & vbLf _
                        & lGPXstring.ToString _
                        & "</trkseg></trk></gpx>")
            lWriter.Close()
            Me.Invoke(pOpenGPXCallback, lGPXfilename, -1)
        End If
    End Sub

    Sub FinishedQueue(ByVal aQueueIndex As Integer) Implements IQueueListener.FinishedQueue
        'This method will be running in another thread, so we need to call Refresh in a complicated way
        Try
            Me.Invoke(pRedrawCallback)
        Catch
            'Ignore if we could not refresh, probably because form is closing
        End Try
    End Sub

    Public Function LatLonInView(ByVal aLat As Double, ByVal aLon As Double) As Boolean
        Return (aLat < CenterLat + LatHeight / 2 AndAlso _
                aLat > CenterLat - LatHeight / 2 AndAlso _
                aLon < CenterLon + LonWidth / 2 AndAlso _
                aLon > CenterLon - LonWidth / 2)
    End Function

    Private Sub DrawLayers(ByVal g As Graphics, ByVal aTopLeftTile As Point, ByVal aOffsetToCenter As Point)
        If pLayers IsNot Nothing AndAlso pLayers.Count > 0 Then
            For Each lLayer As clsLayer In pLayers
                If pRedrawPending Then Exit Sub
                lLayer.Render(g, aTopLeftTile, aOffsetToCenter)
            Next
        End If
    End Sub

    'Private Sub DrawArrow(ByVal g As Graphics, ByVal aXcenter As Integer, ByVal aYcenter As Integer, ByVal aRadians As Double, ByVal aRadius As Integer)
    '    Dim ldx As Integer = Math.Sin(aRadians) * aRadius
    '    Dim ldy As Integer = Math.Cos(aRadians) * aRadius
    '    Dim lHalfDx As Integer = ldx / 2
    '    Dim lHalfDy As Integer = ldy / 2
    '    g.DrawLine(pPenCursor, aXcenter - ldx, aYcenter + ldy, aXcenter + ldx, aYcenter - ldy)
    '    g.DrawLine(pPenCursor, aXcenter - lHalfDy, aYcenter + lHalfDx, aXcenter + ldx, aYcenter - ldy)
    '    g.DrawLine(pPenCursor, aXcenter + lHalfDy, aYcenter - lHalfDx, aXcenter + ldx, aYcenter - ldy)
    'End Sub

    ''' <summary>
    ''' Get the image bitmap from a tile file
    ''' </summary>
    ''' <param name="aTileFilename"></param>
    ''' <returns></returns>
    ''' <remarks>uses pDownloader's TileRAMcache and TileRAMcacheRecent</remarks>
    Private Function TileImage(ByVal aTileFilename As String) As Bitmap
        Dim lTileImage As Bitmap = Nothing
        If pDownloader.TileRAMcache.ContainsKey(aTileFilename) Then
            lTileImage = pDownloader.TileRAMcache.Item(aTileFilename)
            ' Move this name to most recently used position
            pDownloader.TileRAMcacheRecent.Remove(aTileFilename)
            pDownloader.TileRAMcacheRecent.Insert(0, aTileFilename)
        Else
            If IO.File.Exists(aTileFilename) Then 'AndAlso FileLen(aTileFilename) > 0 Then
                Try 'TODO: check for PNG magic numbers? 89 50 4e 47
                    lTileImage = New Bitmap(aTileFilename)
                    If lTileImage IsNot Nothing Then
                        'Make space in tile cache for new tile
                        While pDownloader.TileRAMcache.Count >= pDownloader.TileRAMcacheLimit
                            Dim lVictimFilename As String = pDownloader.TileRAMcacheRecent.Item(pDownloader.TileRAMcacheRecent.Count - 1)
                            pDownloader.TileRAMcacheRecent.RemoveAt(pDownloader.TileRAMcacheRecent.Count - 1)
                            pDownloader.TileRAMcache.Item(lVictimFilename).Dispose()
                            pDownloader.TileRAMcache.Remove(lVictimFilename)
                        End While
                        pDownloader.TileRAMcache.Add(aTileFilename, lTileImage)
                        pDownloader.TileRAMcacheRecent.Insert(0, aTileFilename)
                    End If
                Catch
                    lTileImage = Nothing
                    If IO.File.Exists(aTileFilename & ".error") Then IO.File.Delete(aTileFilename & ".error")
                    IO.File.Move(aTileFilename, aTileFilename & ".error")
                End Try
            Else
                'TODO: search for other existing tile(s) at different zoom levels to scale
            End If
        End If
        Return lTileImage
    End Function

    ''' <summary>
    ''' Draw a tile from a file into the graphics context at the offset
    ''' </summary>
    ''' <param name="aTileFilename">file name of tile to draw</param>
    ''' <param name="g">Graphics context to draw in</param>
    ''' <param name="aOffset">distance from top left of Graphics to draw tile</param>
    ''' <returns>True if tile was drawn or did not need to be drawn, False if needed but not drawn</returns>
    ''' <remarks></remarks>
    Private Function DrawTile(ByVal aTileFilename As String, ByVal g As Graphics, ByVal aOffset As Point) ', ByVal aImageRect As Rectangle) As Boolean
        Try
            Dim lDrewImage As Boolean = False
            If pShowTileImages Then
                Dim lTileImage As Bitmap = TileImage(aTileFilename)
                If lTileImage IsNot Nothing Then
                    Dim lDestRect As New Rectangle(aOffset.X, aOffset.Y, g_TileSize, g_TileSize)
                    Dim lSrcRect As New Rectangle(0, 0, g_TileSize, g_TileSize)
                    g.DrawImage(lTileImage, lDestRect, lSrcRect, GraphicsUnit.Pixel)
                    lDrewImage = True
                End If
            End If
            If pClearDelayedTiles AndAlso Not lDrewImage Then
                g.FillRectangle(pBrushWhite, aOffset.X, aOffset.Y, g_TileSize, g_TileSize)
            End If
            If aTileFilename.Length > 0 Then
                If pShowTileOutlines Then g.DrawRectangle(pPenBlack, aOffset.X, aOffset.Y, g_TileSize, g_TileSize)
                If pShowTileNames Then
                    g.DrawString(aTileFilename.Substring(g_TileCacheFolder.Length).Replace(g_TileExtension, ""), _
                                 pFontTileLabel, _
                                 pBrushBlack, aOffset.X, aOffset.Y)
                End If
            End If
            Return lDrewImage OrElse Not pShowTileImages
        Catch
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Draw a tile from a file into the graphics context at the offset
    ''' </summary>
    ''' <param name="aTileFilename">file name of tile to draw</param>
    ''' <param name="g">Graphics context to draw in</param>
    ''' <param name="aDrawRect">rectangle to fill with tile</param>
    ''' <param name="aImageRect">subset of tile image to draw</param>
    ''' <returns>True if tile was drawn or did not need to be drawn, False if needed but not drawn</returns>
    ''' <remarks></remarks>
    Private Function DrawTile(ByVal aTileFilename As String, ByVal g As Graphics, ByVal aDrawRect As Rectangle, ByVal aImageRect As Rectangle) As Boolean
        Try
            If Not pShowTileImages Then Return True
            Dim lTileImage As Bitmap = TileImage(aTileFilename)
            If lTileImage IsNot Nothing Then
                g.DrawImage(lTileImage, aDrawRect, aImageRect, GraphicsUnit.Pixel)
                Return True
            End If
        Catch
        End Try
        Return False
    End Function

    Private Sub SanitizeCenterLatLon()
        While CenterLon > 180
            CenterLon -= 360
        End While

        While CenterLon < -180
            CenterLon += 360
        End While

        If CenterLat > g_LatMax Then CenterLat = g_LatMax
        If CenterLat < g_LatMin Then CenterLat = g_LatMin
    End Sub

    Private Sub frmMap_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles Me.KeyDown
        pLastKeyDown = e.KeyValue
        If pLastKeyDown = 229 Then ' ProcessKey means we have to look harder to find actual key pressed
            'For lKeyCode As Integer = 0 To 255
            '    If GetAsyncKeyState(lKeyCode) And Windows.Forms.Keys.KeyCode Then
            '        pLastKeyDown = lKeyCode
            '        Exit Sub ' record the lowest numbered key code currently down, not necessarily the one triggering KeyDown
            '    End If
            'Next
        End If
    End Sub

    Private Sub MouseDownLeft(ByVal e As System.Windows.Forms.MouseEventArgs)
        pMouseDragging = True
        pMouseDragStartLocation.X = e.X
        pMouseDragStartLocation.Y = e.Y
        pMouseDownLat = CenterLat
        pMouseDownLon = CenterLon
    End Sub

    Private Sub frmMap_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseUp
        pMouseDragging = False
        If pControlsUse AndAlso _
           Math.Abs(pMouseDragStartLocation.X - e.X) < pControlsMargin / 2 AndAlso _
           Math.Abs(pMouseDragStartLocation.Y - e.Y) < pControlsMargin / 2 Then
            'Count this as a tap/click and navigate if in a control area
            Dim lNeedRedraw As Boolean = True
            If e.X < pControlsMargin Then
                If e.Y < pControlsMargin Then 'Top Left Corner, zoom in
                    Zoom += 1 : lNeedRedraw = False
                ElseIf e.Y > Me.Height - pControlsMargin Then
                    Zoom -= 1 : lNeedRedraw = False 'Bottom Left Corner, zoom out
                Else 'Left Edge, pan left
                    CenterLon -= MetersPerPixel(pZoom) * Me.Width / g_CircumferenceOfEarth * 180
                End If
            ElseIf e.X > Me.Width - pControlsMargin Then
                If e.Y < pControlsMargin Then 'Top Right Corner, zoom in
                    Zoom += 1 : lNeedRedraw = False
                ElseIf e.Y > Me.Height - pControlsMargin Then
                    Zoom -= 1 : lNeedRedraw = False 'Bottom Right Corner, zoom out
                Else 'Right Edge, pan right
                    CenterLon += MetersPerPixel(pZoom) * Me.Width / g_CircumferenceOfEarth * 180
                End If
            ElseIf e.Y < pControlsMargin Then 'Top edge, pan up
                CenterLat += MetersPerPixel(pZoom) * Me.Height / g_CircumferenceOfEarth * 90
            ElseIf e.Y > Me.Height - pControlsMargin Then 'Bottom edge, pan down
                CenterLat -= MetersPerPixel(pZoom) * Me.Height / g_CircumferenceOfEarth * 90
            Else
                lNeedRedraw = False
            End If

            If lNeedRedraw Then
                ManuallyNavigated()
            End If
        Else
            ManuallyNavigated()
        End If
    End Sub

    Private Sub frmMap_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        pDownloader.Enabled = False
        pUploader.Enabled = False
    End Sub

    Private Sub frmMap_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
        If Not pBitmap Is Nothing Then
            pBitmapMutex.WaitOne()
            e.Graphics.DrawImage(pBitmap, 0, 0)
            pBitmapMutex.ReleaseMutex()
        End If
    End Sub

    ' Prevent flickering when default implementation redraws background
    Protected Overrides Sub OnPaintBackground(ByVal e As PaintEventArgs)
    End Sub

    Private Sub frmMap_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize
        pBitmapMutex.WaitOne()
        pBitmap = New Bitmap(Width, Height, Drawing.Imaging.PixelFormat.Format24bppRgb)
        pBitmapMutex.ReleaseMutex()
        pControlsMargin = Math.Min(Me.Width, Me.Height) / 4
        Redraw()
    End Sub

    Private Sub OpenGPXs(ByVal aFilenames() As String)
        Dim lSaveTitle As String = Me.Text
        Dim lNumFiles As Integer = aFilenames.Length
        Dim lCurFile As Integer = 1
        For Each lFilename As String In aFilenames
            Me.Text = "Loading " & lCurFile & "/" & lNumFiles & " '" & IO.Path.GetFileNameWithoutExtension(lFilename) & "'"
            OpenGPX(lFilename)
            lCurFile += 1
        Next
        Me.Text = lSaveTitle

        If Not pGPXPanTo AndAlso Not pGPXZoomTo Then Redraw()
    End Sub

    Private Sub OpenGPX(ByVal aFilename As String, Optional ByVal aInsertAt As Integer = -1)
        If IO.File.Exists(aFilename) Then
            CloseLayer(aFilename)
            Try
                Dim lNewLayer As New clsLayerGPX(aFilename, Me)
                With lNewLayer
                    .LabelField = pGPXLabelField
                End With
                If aInsertAt >= 0 Then
                    pLayers.Insert(aInsertAt, lNewLayer)
                Else
                    pLayers.Add(lNewLayer)
                End If
                If pGPXPanTo OrElse pGPXZoomTo AndAlso lNewLayer.Bounds IsNot Nothing Then
                    If pGPXPanTo Then
                        PanTo(lNewLayer)
                    End If
                    If pGPXZoomTo AndAlso _
                       lNewLayer.Bounds.minlat < lNewLayer.Bounds.maxlat AndAlso _
                       lNewLayer.Bounds.minlon < lNewLayer.Bounds.maxlon Then
                        ZoomTo(lNewLayer)
                    Else
                        Redraw()
                    End If
                    Application.DoEvents()
                End If
            Catch e As Exception
                MsgBox(e.Message, MsgBoxStyle.Critical, "Could not open '" & aFilename & "'")
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Center the view on the center of the given layer
    ''' </summary>
    ''' <param name="aLayer">Layer to center on</param>
    ''' <remarks>Does not redraw</remarks>
    Private Sub PanTo(ByVal aLayer As clsLayer)
        With aLayer.Bounds
            CenterLat = .minlat + (.maxlat - .minlat) / 2
            CenterLon = .minlon + (.maxlon - .minlon) / 2
        End With
    End Sub

    ''' <summary>
    ''' Change zoom (not center point) to include the given layer
    ''' </summary>
    ''' <param name="aLayer">Layer to zoom to</param>
    ''' <param name="aZoomIn">True to zoom in to best fit layer</param>
    ''' <param name="aZoomOut">True to zoom out to fit layer</param>
    ''' <remarks>
    ''' Defaults to zooming in or out as needed to best fit layer
    ''' Does not redraw
    ''' </remarks>
    Private Sub ZoomTo(ByVal aLayer As clsLayer, _
              Optional ByVal aZoomIn As Boolean = True, _
              Optional ByVal aZoomOut As Boolean = True)
        With aLayer.Bounds
            Dim lDesiredZoom As Integer = pZoom
            Dim lNewLatHeight As Double = LatHeight
            Dim lNewLonWidth As Double = LonWidth
            If aZoomIn Then
                While lDesiredZoom < g_ZoomMax AndAlso _
                      .maxlat < CenterLat + lNewLatHeight / 4 AndAlso _
                      .minlat > CenterLat - lNewLatHeight / 4 AndAlso _
                      .maxlon < CenterLon + lNewLonWidth / 4 AndAlso _
                      .minlon > CenterLon - lNewLonWidth / 4
                    lDesiredZoom += 1
                    lNewLonWidth /= 2
                    lNewLatHeight /= 2
                End While
            End If
            If aZoomOut Then
                While lDesiredZoom > g_ZoomMin AndAlso _
                     (.maxlat > CenterLat + lNewLatHeight / 2 OrElse _
                      .minlat < CenterLat - lNewLatHeight / 2 OrElse _
                      .maxlon > CenterLon + lNewLonWidth / 2 OrElse _
                      .minlon < CenterLon - lNewLonWidth / 2)
                    lDesiredZoom -= 1
                    lNewLonWidth *= 2
                    lNewLatHeight *= 2
                End While
            End If
            Zoom = lDesiredZoom
        End With
    End Sub

    Private Sub ZoomToAll()
        Dim lFirstLayer As Boolean = True 'Zoom in on first layer, then zoom out to include all layers
        For Each lLayer As clsLayer In pLayers
            If lFirstLayer Then PanTo(lLayer)
            ZoomTo(lLayer, lFirstLayer)
            lFirstLayer = False
        Next
    End Sub

    ''' <summary>
    ''' Close the existing file if already open
    ''' </summary>
    ''' <param name="aFilename">Name of file to close</param>
    ''' <returns>True if layer matching file name was found and closed</returns>
    Private Function CloseLayer(ByVal aFilename As String) As Boolean
        Dim lLayer As clsLayer
        aFilename = aFilename.ToLower
        For Each lLayer In pLayers
            If aFilename.Equals(lLayer.Filename.ToLower) Then
                CloseLayer(lLayer)
                Return True
            End If
        Next
        Return False
    End Function

    Private Sub CloseLayer(ByVal aLayer As clsLayer)
        aLayer.Clear()
        pLayers.Remove(aLayer)
    End Sub

    Private Sub CloseAllLayers()
        For Each lLayer As clsLayer In pLayers
            lLayer.Clear()
        Next
        pLayers.Clear()
    End Sub

End Class
