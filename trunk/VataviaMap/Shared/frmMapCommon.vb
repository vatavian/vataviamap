'Shared between VataviaMap Desktop and Mobile
Partial Class frmMap
    Implements IQueueListener

    Public CenterLat As Double = 33.8
    Public CenterLon As Double = -84.3

    Public LatHeight As Double 'Height of map display area in latitude degrees
    Public LonWidth As Double  'Width of map display area in longitude degrees
    'bounds of map display area
    Public LatMin As Double, LatMax As Double, LonMin As Double, LonMax As Double

    Private pZoom As Integer = 10 'varies from g_ZoomMin to g_ZoomMax

    'Top-level tile cache folder. 
    'We name a folder inside here after the tile server, then zoom\x\y.png
    'Changing the tile server also changes g_TileCacheFolder
    Private pTileCacheFolder As String = ""

    Private pShowTileImages As Boolean = True
    Private pShowTileOutlines As Boolean = False
    Private pShowTileNames As Boolean = False
    Private pShowGPSdetails As Boolean = False
    Private pUseMarkedTiles As Boolean = False
    Private pShowTransparentTiles As Boolean = False

    Private pClickWaypoint As Boolean = False ' True means a waypoint will be created when the map is clicked

    Private pControlsUse As Boolean = False ' True means that on-screen controls are in use so clicks in control areas are treated as control presses
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
    Private pShowDate As Boolean = False

    Private pMouseDragging As Boolean = False
    Private pMouseDragStartLocation As Point
    Private pMouseDownLat As Double
    Private pMouseDownLon As Double

    Private pMouseWheelAction As EnumWheelAction = EnumWheelAction.Zoom

    Private pLastKeyDown As Integer = 0

    Private pBitmap As Bitmap
    Private pBitmapMutex As New Threading.Mutex()

    Public Layers As New Generic.List(Of clsLayer)

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
    Private pBuddies As New Generic.Dictionary(Of String, clsBuddy)

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
    Private pUploadTrackOnStop As Boolean = False   ' True to upload track when GPS stops
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

        LoadCachedIcons()

        ' The main form registers itself as a listener so Me.DownloadedTile will be called when each tile is finished.
        ' Me.FinishedQueue and Me.DownloadedPoint are also called when appropriate
        pDownloader.Listeners.Add(Me)

        'Start the download and upload queue runners
        pDownloader.Enabled = True
        pUploader.Enabled = True

        pFormVisible = True
    End Sub

    Public Property TileCacheFolder() As String
        Get
            Return pTileCacheFolder
        End Get
        Set(ByVal value As String)
            If value Is Nothing Then value = ""
            If value.Length > 0 AndAlso Not value.EndsWith(g_PathChar) Then
                value &= g_PathChar
            End If
            pTileCacheFolder = value
            SetCacheFolderFromTileServer()
        End Set
    End Property


    Private Sub LoadCachedIcons()
        Dim lIconFileExts() As String = {"gif", "png", "jpg", "bmp"}
        Dim lCacheIconFolder As String = pTileCacheFolder & "Icons"
        If IO.Directory.Exists(lCacheIconFolder) Then
            For Each lFolder As String In IO.Directory.GetDirectories(lCacheIconFolder)
                Dim lFolderName As String = IO.Path.GetFileName(lFolder).ToLower
                For Each lExt As String In lIconFileExts
                    For Each lFilename As String In IO.Directory.GetFiles(lFolder, "*." & lExt)
                        Try
                            Dim lBitmap As New Drawing.Bitmap(lFilename)
                            g_WaypointIcons.Add(lFolderName & "|" & IO.Path.GetFileNameWithoutExtension(lFilename).ToLower, lBitmap)
                        Catch
                        End Try
                    Next
                Next
            Next
        End If
        'TODO: move this to only get geocaching icons when they are absent and user wants them
        If Not g_WaypointIcons.ContainsKey("geocache|webcam cache") Then
            'get geocaching icons from http://www.geocaching.com/about/cache_types.aspx
            Dim lBaseURL As String = "http://www.geocaching.com/images/"
            Dim lTypeURL As String = lBaseURL & "WptTypes/"
            lCacheIconFolder &= g_PathChar & "Geocache" & g_PathChar
            With pDownloader
                .Enqueue(lTypeURL & "2.gif", lCacheIconFolder & "Traditional Cache.gif", QueueItemType.IconItem, 2, False)
                .Enqueue(lTypeURL & "3.gif", lCacheIconFolder & "Multi-cache.gif", QueueItemType.IconItem, 2, False)
                .Enqueue(lTypeURL & "8.gif", lCacheIconFolder & "Unknown Cache.gif", QueueItemType.IconItem, 2, False)
                .Enqueue(lTypeURL & "5.gif", lCacheIconFolder & "Letterbox Hybrid.gif", QueueItemType.IconItem, 2, False)
                .Enqueue(lTypeURL & "1858.gif", lCacheIconFolder & "Wherigo Cache.gif", QueueItemType.IconItem, 2, False)
                .Enqueue(lTypeURL & "6.gif", lCacheIconFolder & "Event Cache.gif", QueueItemType.IconItem, 2, False)
                .Enqueue(lTypeURL & "mega.gif", lCacheIconFolder & "Mega-Event Cache.gif", QueueItemType.IconItem, 2, False)
                .Enqueue(lTypeURL & "13.gif", lCacheIconFolder & "Cache In Trash Out Event.gif", QueueItemType.IconItem, 2, False)
                .Enqueue(lTypeURL & "earthcache.gif", lCacheIconFolder & "Earthcache.gif", QueueItemType.IconItem, 2, False)
                .Enqueue(lTypeURL & "4.gif", lCacheIconFolder & "Virtual Cache.gif", QueueItemType.IconItem, 2, False)
                .Enqueue(lTypeURL & "1304.gif", lCacheIconFolder & "GPS Adventures Exhibit.gif", QueueItemType.IconItem, 2, False)
                .Enqueue(lTypeURL & "9.gif", lCacheIconFolder & "Project APE Cache.gif", QueueItemType.IconItem, 2, False)
                .Enqueue(lTypeURL & "11.gif", lCacheIconFolder & "Webcam Cache.gif", QueueItemType.IconItem, 2, False)
                Dim lIconURL As String = lBaseURL & "icons/"
                .Enqueue(lIconURL & "icon_smile.gif", lCacheIconFolder & "icon_smile.gif", QueueItemType.IconItem, 2, False)
                .Enqueue(lIconURL & "icon_sad.gif", lCacheIconFolder & "icon_sad.gif", QueueItemType.IconItem, 2, False)
                .Enqueue(lIconURL & "icon_note.gif", lCacheIconFolder & "icon_note.gif", QueueItemType.IconItem, 2, False)
                .Enqueue(lIconURL & "icon_maint.gif", lCacheIconFolder & "icon_maint.gif", QueueItemType.IconItem, 2, False)
                .Enqueue(lIconURL & "icon_needsmaint.gif", lCacheIconFolder & "icon_needsmaint.gif", QueueItemType.IconItem, 2, False)
                .Enqueue(lIconURL & "icon_disabled.gif", lCacheIconFolder & "icon_disabled.gif", QueueItemType.IconItem, 2, False)
                .Enqueue(lIconURL & "icon_enabled.gif", lCacheIconFolder & "icon_enabled.gif", QueueItemType.IconItem, 2, False)
                .Enqueue(lIconURL & "icon_greenlight.gif", lCacheIconFolder & "icon_greenlight.gif", QueueItemType.IconItem, 2, False)
                .Enqueue(lIconURL & "coord_update.gif", lCacheIconFolder & "coord_update.gif", QueueItemType.IconItem, 2, False)
                .Enqueue(lIconURL & "icon_rsvp.gif", lCacheIconFolder & "icon_rsvp.gif", QueueItemType.IconItem, 2, False)
            End With
        End If
    End Sub

    Private Sub GetSettings()
        pTileServers = New Dictionary(Of String, String)
        Dim lSoftwareKey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software")
        If lSoftwareKey IsNot Nothing Then
            Dim lAppKey As Microsoft.Win32.RegistryKey = lSoftwareKey.OpenSubKey(g_AppName)
            If lAppKey IsNot Nothing Then
                With lAppKey
                    On Error Resume Next 'Maybe a registry setting is not numeric but needs to be, skip bad settings

                    Dim lKeyIndex As Integer = 1
                    Do
                        Dim lServerNameUrl() As String = CStr(.GetValue("TileServer" & lKeyIndex, "")).Split("|")
                        If lServerNameUrl.Length = 2 Then
                            pTileServers.Add(lServerNameUrl(0), lServerNameUrl(1))
                            lKeyIndex += 1
                        Else
                            Exit Do
                        End If
                    Loop
                    lKeyIndex = 1
                    Do
                        Dim lBuddyNameUrl() As String = CStr(.GetValue("Buddy" & lKeyIndex, "")).Split("|")
                        If lBuddyNameUrl.Length = 2 Then
                            Dim lNewBuddy As New clsBuddy
                            lNewBuddy.Name = lBuddyNameUrl(0)
                            lNewBuddy.LocationURL = lBuddyNameUrl(1)
                            pBuddies.Add(lNewBuddy.Name, lNewBuddy)
                            lKeyIndex += 1
                        Else
                            Exit Do
                        End If
                    Loop

                    'TileCacheFolder gets set earlier
                    g_TileServerName = .GetValue("TileServer", g_TileServerName)
                    g_UploadPointURL = .GetValue("UploadPointURL", g_UploadPointURL)
                    g_UploadTrackURL = .GetValue("UploadTrackURL", g_UploadTrackURL)

                    'Tiles older than this will be downloaded again as needed. TileCacheDays = 0 to never refresh old tiles
                    'TODO: different expiration time for different tiles - satellite photos seldom updated, OSM tiles often
                    Dim lTileCacheDays As Integer = .GetValue("TileCacheDays", 0)
                    If lTileCacheDays > 0 Then
                        pDownloader.TileCacheOldest = DateTime.Now.ToUniversalTime.AddDays(-lTileCacheDays)
                    Else
                        pDownloader.TileCacheOldest = Date.MinValue
                    End If

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
                    pUploadTrackOnStop = .GetValue("UploadTrackOnStop", pUploadTrackOnStop)
                    pUploadPeriodic = .GetValue("UploadPeriodic", pUploadPeriodic)
                    pUploadMinInterval = New TimeSpan(0, 0, .GetValue("UploadPeriodicSeconds", pUploadMinInterval.TotalSeconds))
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

                    Dim lKeyIndex As Integer = 1
                    For Each lName As String In pTileServers.Keys
                        .SetValue("TileServer" & lKeyIndex, lName & "|" & pTileServers.Item(lName))
                        lKeyIndex += 1
                    Next
                    lKeyIndex = 1
                    For Each lName As String In pBuddies.Keys
                        .SetValue("Buddy" & lKeyIndex, lName & "|" & pBuddies.Item(lName).LocationURL)
                        lKeyIndex += 1
                    Next

                    If IO.Directory.Exists(pTileCacheFolder) Then
                        .SetValue("TileCacheFolder", pTileCacheFolder)
                    End If
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
                    .SetValue("UploadTrackOnStop", pUploadTrackOnStop)
                    .SetValue("UploadPeriodic", pUploadPeriodic)
                    .SetValue("UploadPeriodicSeconds", pUploadMinInterval.TotalSeconds)
                    .SetValue("UploadPointURL", g_UploadPointURL)
                    .SetValue("UploadTrackURL", g_UploadTrackURL)
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
                g_TileServerType = MapType.OpenStreetMap
                If Not TileServerUrl.IndexOf("openstreetmap") > 0 AndAlso _
                   Not TileServerUrl.IndexOf("cloudmade") > 0 AndAlso _
                   Not TileServerUrl.IndexOf("toposm.com") > 0 AndAlso _
                   Not TileServerUrl.IndexOf("opentiles.appspot.com") > 0 Then
                    Try
                        g_TileServerType = System.Enum.Parse(GetType(MapType), g_TileServerName, False)
                    Catch ex As Exception
                    End Try
                End If
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

            pDownloader.ClearQueue(QueueItemType.TileItem, -1)

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
        g_TileCacheFolder = pTileCacheFolder & SafeFilename(g_TileServerURL.Replace("http://", "")) & g_PathChar
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
                pDownloader.ClearQueue(QueueItemType.TileItem, -1)
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

    Private Function GetBitmapGraphics() As Graphics
        Try
            pBitmapMutex.WaitOne()
            If pBitmap Is Nothing Then
                pBitmapMutex.ReleaseMutex()
                Return Nothing
            Else
                Dim lGraphics As Graphics = Graphics.FromImage(pBitmap)
                lGraphics.Clip = New Drawing.Region(New Drawing.Rectangle(0, 0, pBitmap.Width, pBitmap.Height))
                Return lGraphics
            End If
        Catch e As Exception
            Return Nothing
        End Try
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
        SanitizeCenterLatLon()

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
        Dim lOffsetToCenter As Point
        Dim lTopLeft As Point
        Dim lBotRight As Point

        FindTileBounds(g.ClipBounds, lOffsetToCenter, lTopLeft, lBotRight)

        Dim lTilePoint As Point
        Dim lOffsetFromWindowCorner As Point

        'TODO: check available memory before deciding how many tiles to keep in RAM
        pDownloader.TileRAMcacheLimit = (lBotRight.X - lTopLeft.X + 1) * (lBotRight.Y - lTopLeft.Y + 1) * 2

        Dim lTilePoints As New SortedList(Of Single, Point)
        Dim x, y As Integer
        Dim lMidX As Integer = (lTopLeft.X + lBotRight.X) / 2
        Dim lMidY As Integer = (lTopLeft.Y + lBotRight.Y) / 2
        Dim lDistance As Single

        'Loop through each visible tile
        For x = lTopLeft.X To lBotRight.X
            For y = lTopLeft.Y To lBotRight.Y
                Try
                    lDistance = (x - lMidX) ^ 2 + (y - lMidY) ^ 2
                    While lTilePoints.ContainsKey(lDistance)
                        lDistance += 0.1
                    End While
                    lTilePoints.Add(lDistance, New Point(x, y))
                Catch
                    lDistance = 11
                    While lTilePoints.ContainsKey(lDistance)
                        lDistance += 0.1
                    End While
                    lTilePoints.Add(lDistance, New Point(x, y))
                End Try
            Next
        Next

        For Each lTilePoint In lTilePoints.Values
            If pRedrawPending Then Exit Sub

            x = lTilePoint.X
            y = lTilePoint.Y

            lOffsetFromWindowCorner.X = (x - lTopLeft.X) * g_TileSize + lOffsetToCenter.X
            lOffsetFromWindowCorner.Y = (y - lTopLeft.Y) * g_TileSize + lOffsetToCenter.Y

            Dim lDrewTile As Boolean = DrawTile(lTilePoint, pZoom, g, lOffsetFromWindowCorner, 0)

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
                Dim lZoomMin As Integer = Math.Max(g_ZoomMin, pZoom - 4)
                For lZoom = pZoom - 1 To lZoomMin Step -1
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

                    If DrawTile(New Drawing.Point(lNextX, lNextY), lZoom, g, lRectOrigTile, _
                                New Rectangle(lFromX, lFromY, lZoomedTilePortion, lZoomedTilePortion), -1) Then
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
                    If Not DrawTile(lFineTilePoint, lZoom, g, lRectDestination, g_TileSizeRect, -1) Then
                        'TODO: four tiles at next finer zoom level, at this and other three DrawTile below
                    End If

                    ' upper right tile
                    lFineTilePoint.X += 1
                    lRectDestination.X = lOffsetFromWindowCorner.X + g_HalfTile
                    DrawTile(lFineTilePoint, lZoom, g, lRectDestination, g_TileSizeRect, -1)

                    ' lower right tile
                    lFineTilePoint.Y += 1
                    lRectDestination.Y = lOffsetFromWindowCorner.Y + g_HalfTile
                    DrawTile(lFineTilePoint, lZoom, g, lRectDestination, g_TileSizeRect, -1)

                    ' lower left tile
                    lFineTilePoint.X = lDoubleX
                    lRectDestination.X = lOffsetFromWindowCorner.X
                    DrawTile(lFineTilePoint, lZoom, g, lRectDestination, g_TileSizeRect, -1)
                End If
            End If
        Next

        If pRedrawPending Then Exit Sub

        If pControlsShow Then
            g.DrawLine(pPenBlack, pControlsMargin, 0, pControlsMargin, pBitmap.Height)
            g.DrawLine(pPenBlack, 0, pControlsMargin, pBitmap.Width, pControlsMargin)
            g.DrawLine(pPenBlack, pBitmap.Width - pControlsMargin, 0, pBitmap.Width - pControlsMargin, pBitmap.Height)
            g.DrawLine(pPenBlack, 0, pBitmap.Height - pControlsMargin, pBitmap.Width, pBitmap.Height - pControlsMargin)
        End If

        If pGPXShow Then DrawLayers(g, lTopLeft, lOffsetToCenter)

        If pShowCopyright AndAlso pShowTileImages Then g.DrawString(g_TileCopyright, pFontCopyright, pBrushCopyright, 3, pBitmap.Height - 20)

        If pShowDate Then g.DrawString(DateTime.Now.ToString("yyyy-MM-dd HH:mm"), pFontTileLabel, pBrushBlack, 3, 3)

        If pRedrawPending Then Exit Sub

        DrewTiles(g, lTopLeft, lOffsetToCenter)
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
    ''' <param name="aTilePoint">which tile</param>
    ''' <param name="aZoom">Zoom level of downloaded tile</param>
    ''' <param name="aTileFilename"></param>
    ''' <remarks></remarks>
    Public Sub DownloadedTile(ByVal aTilePoint As Point, ByVal aZoom As Integer, ByVal aTileFilename As String, ByVal aTileServerURL As String) Implements IQueueListener.DownloadedTile
        If aTileServerURL = g_TileServerURL Then
            Dim lGraphics As Graphics = GetBitmapGraphics()
            If lGraphics IsNot Nothing Then
                If aZoom = pZoom Then
                    Dim lOffsetToCenter As Point
                    Dim lTopLeft As Point
                    Dim lBotRight As Point

                    FindTileBounds(lGraphics.ClipBounds, lOffsetToCenter, lTopLeft, lBotRight)

                    Dim lOffsetFromWindowCorner As Point
                    lOffsetFromWindowCorner.X = (aTilePoint.X - lTopLeft.X) * g_TileSize + lOffsetToCenter.X
                    lOffsetFromWindowCorner.Y = (aTilePoint.Y - lTopLeft.Y) * g_TileSize + lOffsetToCenter.Y
                    DrawTile(aTilePoint, aZoom, lGraphics, lOffsetFromWindowCorner, -1)
                Else
                    'TODO: draw tiles at different zoom levels? 
                    'Would be nice when doing DownloadDescendants to see progress, but would also be confusing when tile download completes after zoom
                End If
                ReleaseBitmapGraphics()

                Try 'This method will be running in another thread, so we need to call Refresh in a complicated way
                    Me.Invoke(pRefreshCallback)
                Catch
                    'Ignore if we could not refresh, probably because form is closing
                End Try
            End If
        End If
    End Sub

    Private Sub RequestBuddyPoint(ByVal o As Object)
        If pBuddies Is Nothing OrElse pBuddies.Count = 0 Then
            MsgBox("Add Buddies Before Finding them", MsgBoxStyle.OkOnly, "No Buddies Found")
        Else
            For Each lBuddy As clsBuddy In pBuddies.Values
                If lBuddy.Selected Then pDownloader.Enqueue(lBuddy.LocationURL, IO.Path.GetTempPath & SafeFilename(lBuddy.Name), QueueItemType.PointItem, 0, True, lBuddy)
            Next
        End If
    End Sub

    Public Sub DownloadedItem(ByVal aItem As clsQueueItem) Implements IQueueListener.DownloadedItem
        Select Case aItem.ItemType
            Case QueueItemType.IconItem
                Dim lCacheIconFolder As String = (pTileCacheFolder & "Icons").ToLower & g_PathChar
                If aItem.Filename.ToLower.StartsWith(lCacheIconFolder) Then 'Geocache or other icon that lives in "Icons" folder in pTileCacheFolder
                    Try
                        Dim lBitmap As New Drawing.Bitmap(aItem.Filename)
                        Dim lIconName As String = IO.Path.ChangeExtension(aItem.Filename, "").TrimEnd(".").Substring(lCacheIconFolder.Length).Replace(g_PathChar, "|")
                        g_WaypointIcons.Add(lIconName, lBitmap)
                    Catch e As Exception
                    End Try
                Else
                    Try
                        Dim lBitmap As New Drawing.Bitmap(aItem.Filename)
                        g_WaypointIcons.Add(aItem.Filename.ToLower, lBitmap)
                    Catch e As Exception
                    End Try
                End If
            Case QueueItemType.PointItem
                Try
                    Dim lFileNameOnly As String = IO.Path.GetFileNameWithoutExtension(aItem.Filename)
                    Dim lBuddy As clsBuddy = aItem.ItemObject
                    If lBuddy IsNot Nothing Then
                        If lBuddy.LoadFile(aItem.Filename) Then
                            If lBuddy.IconFilename.Length > 0 AndAlso Not IO.File.Exists(lBuddy.IconFilename) AndAlso lBuddy.IconURL.Length > 0 Then
                                pDownloader.Enqueue(lBuddy.IconURL, lBuddy.IconFilename, QueueItemType.IconItem, , False, lBuddy)
                            End If
                            NeedRedraw()
                            If pBuddyAlarmEnabled AndAlso pBuddyAlarmMeters > 0 Then
                                If MetersBetweenLatLon(pBuddyAlarmLat, pBuddyAlarmLon, lBuddy.Waypoint.lat, lBuddy.Waypoint.lon) < pBuddyAlarmMeters Then
                                    Windows.Forms.MessageBox.Show(lBuddy.Name & " approaches", "Nearby Buddy", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1)
                                    pBuddyAlarmEnabled = False
                                End If
                            End If
                        End If
                    End If
                Catch ex As Exception 'Maybe we could not parse lat and lon?
                    Dbg("DownloadedPoint:Exception:" & ex.Message)
                End Try
        End Select
    End Sub

    Public Sub NeedRedraw()
        'This method will be running in another thread, so we need to call Refresh in a complicated way
        Try
            If Me.InvokeRequired Then
                Me.Invoke(pRedrawCallback)
            Else
                Redraw()
            End If
        Catch
            'Ignore if we could not refresh, probably because form is closing
        End Try
    End Sub

    Sub FinishedQueue(ByVal aQueueIndex As Integer) Implements IQueueListener.FinishedQueue
        NeedRedraw()
    End Sub

    Public Function LatLonInView(ByVal aLat As Double, ByVal aLon As Double) As Boolean
        Return (aLat < LatMax AndAlso _
                aLat > LatMin AndAlso _
                aLon < LonMax AndAlso _
                aLon > LonMin)
    End Function

    Private Sub DrawLayers(ByVal g As Graphics, ByVal aTopLeftTile As Point, ByVal aOffsetToCenter As Point)
        If Layers IsNot Nothing AndAlso Layers.Count > 0 Then
            For Each lLayer As clsLayer In Layers
                If pRedrawPending Then Exit Sub
                lLayer.Render(g, aTopLeftTile, aOffsetToCenter)
            Next
        End If
        If pBuddies IsNot Nothing AndAlso pBuddies.Count > 0 Then
            Dim lUtcNow As Date = Date.UtcNow
            Dim lWaypoints As New Generic.List(Of clsGPXwaypoint)
            For Each lBuddy As clsBuddy In pBuddies.Values
                If lBuddy.Selected AndAlso lBuddy.Waypoint IsNot Nothing Then
                    Dim lWaypoint As clsGPXwaypoint = lBuddy.Waypoint.Clone
                    If lWaypoint.timeSpecified Then
                        lWaypoint.name &= " " & TimeSpanString(lUtcNow.Subtract(lWaypoint.time))
                    End If
                    lWaypoints.Add(lWaypoint)
                End If
            Next
            If lWaypoints.Count > 0 Then
                Dim lLayer As New clsLayerGPX(lWaypoints, Me)
                lLayer.LabelField = "name"
                lLayer.Render(g, aTopLeftTile, aOffsetToCenter)
            End If
        End If
    End Sub

    ''' <summary>
    ''' Draw a tile into the graphics context at the offset
    ''' </summary>
    ''' <param name="aTilePoint">tile to draw</param>
    ''' <param name="aZoom">which zoom level</param>
    ''' <param name="g">Graphics context to draw in</param>
    ''' <param name="aOffset">distance from top left of Graphics to draw tile</param>
    ''' <param name="aPriority">download priority for getting tile if not present, -1 to skip download, 0 for highest priority</param>
    ''' <returns>True if tile was drawn or did not need to be drawn, False if needed but not drawn</returns>
    ''' <remarks></remarks>
    Private Function DrawTile(ByVal aTilePoint As Point, ByVal aZoom As Integer, ByVal g As Graphics, ByVal aOffset As Point, ByVal aPriority As Integer) As Boolean
        Try
            Dim lDrewImage As Boolean = False
            If pShowTileImages Then
                Dim lTileImage As Bitmap = TileBitmap(aTilePoint, aZoom, aPriority)
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
            If pShowTileOutlines Then g.DrawRectangle(pPenBlack, aOffset.X, aOffset.Y, g_TileSize, g_TileSize)
            If pShowTileNames Then
                Dim lTileFileName As String = TileFilename(aTilePoint, aZoom, False)
                If lTileFileName.Length > 0 Then
                    g.DrawString(lTileFileName.Substring(g_TileCacheFolder.Length).Replace(g_TileExtension, ""), _
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
    ''' Draw a tile into the graphics context, scaled/positioned with the given destination and source rectangles
    ''' </summary>
    ''' <param name="aTilePoint">tile to draw</param>
    ''' <param name="aZoom">which zoom level</param>
    ''' <param name="g">Graphics context to draw in</param>
    ''' <param name="aDrawRect">rectangle to fill with tile</param>
    ''' <param name="aImageRect">subset of tile image to draw</param>
    ''' <param name="aPriority">download priority for getting tile if not present, -1 to skip download, 0 for highest priority</param>
    ''' <returns>True if tile was drawn or did not need to be drawn, False if needed but not drawn</returns>
    ''' <remarks></remarks>
    Private Function DrawTile(ByVal aTilePoint As Point, ByVal aZoom As Integer, ByVal g As Graphics, ByVal aDrawRect As Rectangle, ByVal aImageRect As Rectangle, ByVal aPriority As Integer) As Boolean
        Try
            If Not pShowTileImages Then Return True

            Dim lTileImage As Bitmap = TileBitmap(aTilePoint, aZoom, aPriority)

            If lTileImage IsNot Nothing Then
                g.DrawImage(lTileImage, aDrawRect, aImageRect, GraphicsUnit.Pixel)
                Return True
            End If
        Catch
        End Try
        Return False
    End Function

    ''' <summary>
    ''' Get a tile image from pDownloader. 
    ''' </summary>
    ''' <param name="aTilePoint">tile to draw</param>
    ''' <param name="aZoom">which zoom level</param>
    ''' <param name="aPriority">download priority for getting tile if not present, -1 to skip download, 0 for highest priority</param>
    ''' <returns>Bitmap of tile or Nothing if not yet available</returns>
    ''' <remarks>
    ''' If tile is found not found in local cache, Nothing is returned. If aPriority > -1, download of tile is queued
    ''' returns unmarked tile if marked was requested but not available
    ''' </remarks>
    Private Function TileBitmap(ByVal aTilePoint As Point, ByVal aZoom As Integer, ByVal aPriority As Integer) As Bitmap
        Dim lTileFileName As String = TileFilename(aTilePoint, aZoom, pUseMarkedTiles)
        If lTileFileName.Length = 0 Then
            Return Nothing
        Else
            Dim lTileImage As Bitmap = pDownloader.GetTile(lTileFileName, aTilePoint, aZoom, aPriority, False)

            'Fall back to using unmarked tile if we have it but not a marked version
            If pUseMarkedTiles AndAlso lTileImage Is Nothing Then
                lTileFileName = TileFilename(aTilePoint, pZoom, False)
                lTileImage = pDownloader.GetTile(lTileFileName, aTilePoint, aZoom, aPriority, False)
            End If
            Return lTileImage
        End If
    End Function

    Private Sub SanitizeCenterLatLon()

        If Math.Abs(CenterLat) > 1000 OrElse Math.Abs(CenterLon) > 1000 Then
            CenterLat = 33.8
            CenterLon = -84.3
        End If

        While CenterLon > 180
            CenterLon -= 360
        End While

        While CenterLon < -180
            CenterLon += 360
        End While

        If CenterLat > g_LatMax Then CenterLat = g_LatMax
        If CenterLat < g_LatMin Then CenterLat = g_LatMin

        LatMin = CenterLat - LatHeight / 2
        LatMax = CenterLat + LatHeight / 2
        LonMin = CenterLon - LonWidth / 2
        LonMax = CenterLon + LonWidth / 2
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
        Diagnostics.Process.GetCurrentProcess.Kill() 'Try to kill our own process, making sure all threads stop
    End Sub

    Private Sub frmMap_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
        If Not pBitmap Is Nothing Then
            pBitmapMutex.WaitOne()
            With MapRectangle()
                e.Graphics.DrawImage(pBitmap, .Left, .Top)
            End With
            pBitmapMutex.ReleaseMutex()
        End If
    End Sub

    ' Prevent flickering when default implementation redraws background
    Protected Overrides Sub OnPaintBackground(ByVal e As PaintEventArgs)
    End Sub

    Private Sub frmMap_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize
        With MapRectangle()
            If .Width > 0 AndAlso .Height > 0 Then
                pBitmapMutex.WaitOne()
                pBitmap = New Bitmap(.Width, .Height, Drawing.Imaging.PixelFormat.Format24bppRgb)
                pControlsMargin = Math.Min(pBitmap.Width, pBitmap.Height) / 4
                pBitmapMutex.ReleaseMutex()
            End If
        End With
        Redraw()
    End Sub

    Private Sub OpenFiles(ByVal aFilenames() As String)
        Dim lGPXPanTo As Boolean = pGPXPanTo
        Dim lGPXZoomTo As Boolean = pGPXZoomTo
        If aFilenames.Length > 1 Then
            pGPXPanTo = False
            pGPXZoomTo = False
        End If
        Dim lSaveTitle As String = Me.Text
        Dim lNumFiles As Integer = aFilenames.Length
        Dim lCurFile As Integer = 1
        For Each lFilename As String In aFilenames
            Me.Text = "Loading " & lCurFile & "/" & lNumFiles & " '" & IO.Path.GetFileNameWithoutExtension(lFilename) & "'"
            Select Case IO.Path.GetExtension(lFilename).ToLower
                Case ".cell" : OpenCell(lFilename)
                Case ".jpg", ".jpeg" : OpenPhoto(lFilename)
                Case Else : OpenGPX(lFilename)
            End Select
            lCurFile += 1
        Next
        Me.Text = lSaveTitle
        If aFilenames.Length > 1 Then
            pGPXPanTo = lGPXPanTo
            pGPXZoomTo = lGPXZoomTo
            If pGPXZoomTo Then ZoomToAll()
        End If

        If Not pGPXPanTo AndAlso Not pGPXZoomTo Then Redraw()
    End Sub

    Public Sub OpenCell(ByVal aFilename As String)
        Dim lLayer As New clsCellLayer(aFilename, Me)
        Layers.Add(lLayer)
        NeedRedraw()
    End Sub

    Public Sub OpenPhoto(ByVal aFilename As String)
#If Not Smartphone Then
        Dim lExif As New ExifWorks(aFilename)
        Dim lLatitude As Double = lExif.Latitude
        If Double.IsNaN(lLatitude) Then Exit Sub
        Dim lLongitude As Double = lExif.Longitude
        If Double.IsNaN(lLongitude) Then Exit Sub

        Dim lWaypoints As New Generic.List(Of clsGPXwaypoint)
        Dim lWaypoint As New clsGPXwaypoint("wpt", lLatitude, lLongitude)
        lWaypoint.name = IO.Path.GetFileNameWithoutExtension(aFilename)
        lWaypoint.sym = aFilename
        lWaypoint.url = aFilename
        lWaypoints.Add(lWaypoint)

        Dim lLayer As New clsLayerGPX(lWaypoints, Me)
        lLayer.Filename = aFilename
        lLayer.LabelField = "name"
        Dim lBounds As New clsGPXbounds()
        With lBounds
            .maxlat = lLatitude
            .minlat = lLatitude
            .maxlon = lLongitude
            .minlon = lLongitude
        End With
        lLayer.Bounds = lBounds

        Me.Layers.Add(lLayer)

        If pGPXPanTo Then
            CenterLat = lLatitude
            CenterLon = lLongitude
            SanitizeCenterLatLon()
            Redraw()
        Else
            NeedRedraw()
        End If
#End If
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
                    Layers.Insert(aInsertAt, lNewLayer)
                Else
                    Layers.Add(lNewLayer)
                End If
                If pGPXPanTo OrElse pGPXZoomTo AndAlso lNewLayer.Bounds IsNot Nothing Then
                    If pGPXPanTo Then
                        PanTo(lNewLayer.Bounds)
                    End If
                    If pGPXZoomTo AndAlso _
                       lNewLayer.Bounds.minlat < lNewLayer.Bounds.maxlat AndAlso _
                       lNewLayer.Bounds.minlon < lNewLayer.Bounds.maxlon Then
                        Zoom = FindZoom(lNewLayer.Bounds)
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
    ''' Center the view on the center of the given bounds
    ''' </summary>
    ''' <param name="aBounds">Bounds to center on</param>
    ''' <remarks>Does not redraw</remarks>
    Private Sub PanTo(ByVal aBounds As clsGPXbounds)
        With aBounds
            CenterLat = .minlat + (.maxlat - .minlat) / 2
            CenterLon = .minlon + (.maxlon - .minlon) / 2
            SanitizeCenterLatLon()
        End With
    End Sub

    ''' <summary>
    ''' Find zoom that would include the given layer without changing the center point
    ''' </summary>
    ''' <param name="aBounds">Bounds to zoom to</param>
    ''' <param name="aZoomIn">True to zoom in to best fit layer</param>
    ''' <param name="aZoomOut">True to zoom out to fit layer</param>
    ''' <remarks>
    ''' Defaults to zooming in or out as needed to best fit layer
    ''' Does not redraw or set the zoom level of the map
    ''' </remarks>
    Private Function FindZoom(ByVal aBounds As clsGPXbounds, _
                     Optional ByVal aZoomIn As Boolean = True, _
                     Optional ByVal aZoomOut As Boolean = True) As Integer
        With aBounds
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
            Return lDesiredZoom
        End With
    End Function

    Private Sub ZoomToAll()
        Dim lFirstLayer As Boolean = True 'Zoom in on first layer, then zoom out to include all layers
        Dim lAllBounds As clsGPXbounds = Nothing
        For Each lLayer As clsLayer In Layers
            If lLayer.Bounds IsNot Nothing Then
                If lFirstLayer Then
                    lAllBounds = lLayer.Bounds.Clone
                    lFirstLayer = False
                Else
                    lAllBounds.Expand(lLayer.Bounds.minlat, lLayer.Bounds.minlon)
                    lAllBounds.Expand(lLayer.Bounds.maxlat, lLayer.Bounds.maxlon)
                End If
            End If
        Next
        If lAllBounds IsNot Nothing Then
            PanTo(lAllBounds)
            Zoom = FindZoom(lAllBounds)
        End If
    End Sub

    ''' <summary>
    ''' Close the existing file if already open
    ''' </summary>
    ''' <param name="aFilename">Name of file to close</param>
    ''' <returns>True if layer matching file name was found and closed</returns>
    Private Function CloseLayer(ByVal aFilename As String) As Boolean
        Dim lLayer As clsLayer
        aFilename = aFilename.ToLower
        For Each lLayer In Layers
            If aFilename.Equals(lLayer.Filename.ToLower) Then
                CloseLayer(lLayer)
                Return True
            End If
        Next
        Return False
    End Function

    Private Sub CloseLayer(ByVal aLayer As clsLayer)
        aLayer.Clear()
        Layers.Remove(aLayer)
    End Sub

    Private Sub CloseAllLayers()
        For Each lLayer As clsLayer In Layers
            lLayer.Clear()
        Next
        Layers.Clear()
    End Sub

    ''' <summary>
    ''' Start timer that refreshes buddy positions
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub StartBuddyTimer()
        If pBuddyTimer Is Nothing Then
            pBuddyTimer = New System.Threading.Timer(New Threading.TimerCallback(AddressOf RequestBuddyPoint), Nothing, 0, 60000)
        End If
    End Sub
    Private Sub StopBuddyTimer()
        If pBuddyTimer IsNot Nothing Then
            Try
                pBuddyTimer.Dispose()
            Catch
            End Try
            pBuddyTimer = Nothing
        End If
    End Sub

End Class
