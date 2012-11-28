Public Class ctlMap
    Implements IQueueListener

    Public CenterLat As Double = 33.8
    Public CenterLon As Double = -84.3

    ''' <summary>
    ''' Tile server for opaque background map
    ''' </summary>
    ''' <remarks></remarks>
    Public TileServer As New clsServer("OpenStreetMap", _
                                       "http://openstreetmap.org/", _
                                       "http://{abc}.tile.openstreetmap.org/{Zoom}/{X}/{Y}.png", _
                                       "http://www.openstreetmap.org/?lat={Lat}&lon={Lon}&zoom={Zoom}", _
                                       "© OpenStreetMap")

    Public TransparentTileServers As New Generic.List(Of clsServer)
    Public LabelServer As clsServer = Nothing

    Public LatHeight As Double 'Height of map display area in latitude degrees
    Public LonWidth As Double  'Width of map display area in longitude degrees
    'bounds of map display area
    Public LatMin As Double, LatMax As Double, LonMin As Double, LonMax As Double

    Public Event OpenedLayer(ByVal aLayer As clsLayer)
    Public Event ClosedLayer()
    Public Event Zoomed()
    Public Event Panned()
    Public Event CenterBehaviorChanged()
    Public Event TileServerChanged()
    Public Event StatusChanged(ByVal aStatusMessage As String)

    Private pMagnify As Double = 1 ' Stretch the image by this much: 1=no stretch, 2=double size
    Private pZoom As Integer = 10 'varies from g_TileServer.ZoomMin to g_TileServer.ZoomMax

    Public Enum EnumWheelAction
        Zoom = 0
        TileServer = 1
        Layer = 2
    End Enum
    Public MouseWheelAction As EnumWheelAction = EnumWheelAction.Zoom

    'Top-level tile cache folder. 
    'We name a folder inside here after the tile server, then zoom/x/y.png
    Private pTileCacheFolder As String = ""

    Private pShowTileImages As Boolean = True
    Private pShowTileNames As Boolean = False
    Private pShowTileOutlines As Boolean = False
    Private pShowGPSdetails As Boolean = False
    Private pUseMarkedTiles As Boolean = False
    Private pShowTransparentTiles As Boolean = False

    Private pClickWaypoint As Boolean = False ' True means a waypoint will be created when the map is clicked

    Public ControlsUse As Boolean = False ' True means that on-screen controls are in use so clicks in control areas are treated as control presses
    Public ControlsShow As Boolean = False
    Private pControlsMargin As Integer = 40 'This gets set on resize

    Public ClickedTileFilename As String = ""

    Private pBrushBlack As New SolidBrush(Color.Black)
    Private pBrushWhite As New SolidBrush(Color.White)

    Private pPenBlack As New Pen(Color.Black)
    Private pPenCursor As New Pen(Color.Red)

    Private pFontTileLabel As New Font("Arial", 10, FontStyle.Regular)
    Private pFontGpsDetails As New Font("Arial", 10, FontStyle.Regular)
    Private pFontCopyright As New Font(FontFamily.GenericSansSerif, 7, FontStyle.Regular)
    Private pBrushCopyright As New SolidBrush(Color.Black)
    Private pShowCopyright As Boolean = True
    Private pShowDate As Boolean = False

    Public MouseDragging As Boolean = False
    Public MouseDragStartLocation As Point
    Public MouseDownLat As Double
    Public MouseDownLon As Double

    Private pLastKeyDown As Integer = 0

    Private pBitmap As Bitmap
    Private pBitmapMutex As New Threading.Mutex()

    Public Layers As New Generic.List(Of clsLayer)
    Public LayersImportedByTime As New Generic.List(Of clsLayer)

    Public GPXPanTo As Boolean = True
    Public GPXZoomTo As Boolean = True
    Public GPXShow As Boolean = True
    Public GPXFolder As String
    Public GPXLabelField As String = "name"

    Private pBuddyTimer As System.Threading.Timer  ' Check for new buddy information

    ' Alert when buddy location is within pBuddyAlarmMeters of (pBuddyAlarmLat, pBuddyAlarmLon)
    Private pBuddyAlarmEnabled As Boolean = False
    Private pBuddyAlarmLat As Double = 0
    Private pBuddyAlarmLon As Double = 0
    Private pBuddyAlarmMeters As Double = 0 'Must be greater than zero to get buddy alerts
    Public Buddies As New Generic.Dictionary(Of String, clsBuddy)

    Delegate Function OpenCallback(ByVal aFilename As String, ByVal aInsertAt As Integer) As clsLayerGPX
    Private pOpenGPXCallback As New OpenCallback(AddressOf OpenGPX)

    ' True to automatically start GPS when application starts
    Private pGPSAutoStart As Boolean = True

    ' 0=don't follow, 1=re-center only when GPS moves off screen, 2=keep centered, 3=zoom out if needed but keep current area visible
    Private pGPSCenterBehavior As Integer = 2

    ' Symbol at GPS position
    Public GPSSymbolSize As Integer = 10

    ' Symbol at each track point
    Public TrackSymbolSize As Integer = 3

    ' True to record track information while GPS is on
    Public RecordTrack As Boolean = False

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
    Public UploadMinInterval As New TimeSpan(0, 2, 0)

    Public UploadOnStart As Boolean = False  ' True to upload point when GPS starts
    Public UploadOnStop As Boolean = False   ' True to upload point when GPS stops
    Public UploadTrackOnStop As Boolean = False   ' True to upload track when GPS stops
    Public UploadPeriodic As Boolean = False ' True to upload point periodically

    Private pActive As Boolean = False      ' False to disable all Redraw of control
    Private pDark As Boolean = False        ' True to draw simple black background instead of map

    Delegate Sub RefreshCallback()
    Private pRefreshCallback As New RefreshCallback(AddressOf Refresh)
    Private pRedrawCallback As New RefreshCallback(AddressOf Redraw)
    Private pRedrawing As Boolean = False
    Private pRedrawPending As Boolean = False
    Private pRedrawWhenFinishedQueue As Boolean = False

    Private pRedrawTimer As System.Threading.Timer ' Redraw every so often to be sure any dates displayed are recent
    Private pLastRedrawTime As Date = Now

    ' Set to false when we have a first-guess background (e.g. when zooming from tiles we have to ones we don't)
    Private pClearDelayedTiles As Boolean = True

    Public Uploader As New clsUploader
    Public Downloader As New clsDownloader

    Public Servers As New Generic.Dictionary(Of String, clsServer)

    Private pImportOffsetFromUTC As TimeSpan = DateTime.UtcNow.Subtract(DateTime.Now)

    'Private Declare Function GetAsyncKeyState Lib "user32" (ByVal vKey As Integer) As Integer

    Public Sub SharedNew(ByVal aTileCacheFolder As String)
        TileCacheFolder = aTileCacheFolder
        GPXFolder = g_PathChar & "My Documents" & g_PathChar & "gps"
        GetSettings()

        LoadCachedIcons()

        ' The main form registers itself as a listener so Me.DownloadedTile will be called when each tile is finished.
        ' Me.FinishedQueue and Me.DownloadedPoint are also called when appropriate
        Downloader.Listeners.Add(Me)

        'Start the download queue runner
        Downloader.Enabled = True
        'TODO: background refresh without disturbing other windows: pRedrawTimer = New System.Threading.Timer(New Threading.TimerCallback(AddressOf RedrawTimeout), Nothing, 0, 10000)
    End Sub

    ''' <summary>
    ''' Keep the device awake, also move map to a cell tower location if desired 
    ''' </summary>
    ''' <remarks>Periodically called by pKeepAwakeTimer</remarks>
    Private Sub RedrawTimeout(ByVal o As Object)
        If Now.Subtract(pLastRedrawTime).TotalSeconds >= 10 Then
            NeedRedraw()
        End If
    End Sub

    Public Property ImportOffsetFromUTC() As TimeSpan
        Get
            Return pImportOffsetFromUTC
        End Get
        Set(ByVal value As TimeSpan)
            If Math.Floor(value.TotalSeconds) <> Math.Floor(pImportOffsetFromUTC.TotalSeconds) Then
                pImportOffsetFromUTC = value
                ReImportLayersByDate()
                NeedRedraw()
            End If
        End Set
    End Property

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
            For Each lServer As clsServer In Me.Servers.Values
                SetServerCacheFolder(lServer)
            Next
        End Set
    End Property

    Public Property ShowDate() As Boolean
        Get
            Return pShowDate
        End Get
        Set(ByVal value As Boolean)
            If pShowDate <> value Then
                pShowDate = value
                NeedRedraw()
            End If
        End Set
    End Property

    Public Property ShowTileImages() As Boolean
        Get
            Return pShowTileImages
        End Get
        Set(ByVal value As Boolean)
            If pShowTileImages <> value Then
                pShowTileImages = value
                NeedRedraw()
            End If
        End Set
    End Property

    Public Property ShowTileNames() As Boolean
        Get
            Return pShowTileNames
        End Get
        Set(ByVal value As Boolean)
            If pShowTileNames <> value Then
                pShowTileNames = value
                NeedRedraw()
            End If
        End Set
    End Property

    Public Property ShowTileOutlines() As Boolean
        Get
            Return pShowTileOutlines
        End Get
        Set(ByVal value As Boolean)
            If pShowTileOutlines <> value Then
                pShowTileOutlines = value
                NeedRedraw()
            End If
        End Set
    End Property

    Public Property Dark() As Boolean
        Get
            Return pDark
        End Get
        Set(ByVal value As Boolean)
            pDark = value
#If Smartphone Then
            If pDark Then
                StopKeepAwake()
                pFontGpsDetails = New Font(pFontGpsDetails.Name, 20, pFontGpsDetails.Style)
            Else
                StartKeepAwake()
                pFontGpsDetails = New Font(pFontGpsDetails.Name, 10, pFontGpsDetails.Style)
            End If
#End If
            NeedRedraw()
        End Set
    End Property

    Private Sub LoadCachedIcons()
        Try
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
        Catch e As Exception
            MsgBox("Exception opening " & pTileCacheFolder & "Icons: " & e.Message, MsgBoxStyle.OkOnly, "LoadCachedIcons")
        End Try
        'TODO: move this to only get geocaching icons when they are absent and user wants them
        'If Not g_WaypointIcons.ContainsKey("geocache|webcam cache") Then
        '    'get geocaching icons from http://www.geocaching.com/about/cache_types.aspx
        '    Dim lBaseURL As String = "http://www.geocaching.com/images/"
        '    Dim lTypeURL As String = lBaseURL & "WptTypes/"
        '    lCacheIconFolder &= g_PathChar & "Geocache" & g_PathChar
        '    With Downloader
        '        .Enqueue(lTypeURL & "2.gif", lCacheIconFolder & "Traditional Cache.gif", QueueItemType.IconItem, 2, False)
        '        .Enqueue(lTypeURL & "3.gif", lCacheIconFolder & "Multi-cache.gif", QueueItemType.IconItem, 2, False)
        '        .Enqueue(lTypeURL & "8.gif", lCacheIconFolder & "Unknown Cache.gif", QueueItemType.IconItem, 2, False)
        '        .Enqueue(lTypeURL & "5.gif", lCacheIconFolder & "Letterbox Hybrid.gif", QueueItemType.IconItem, 2, False)
        '        .Enqueue(lTypeURL & "1858.gif", lCacheIconFolder & "Wherigo Cache.gif", QueueItemType.IconItem, 2, False)
        '        .Enqueue(lTypeURL & "6.gif", lCacheIconFolder & "Event Cache.gif", QueueItemType.IconItem, 2, False)
        '        .Enqueue(lTypeURL & "mega.gif", lCacheIconFolder & "Mega-Event Cache.gif", QueueItemType.IconItem, 2, False)
        '        .Enqueue(lTypeURL & "13.gif", lCacheIconFolder & "Cache In Trash Out Event.gif", QueueItemType.IconItem, 2, False)
        '        .Enqueue(lTypeURL & "earthcache.gif", lCacheIconFolder & "Earthcache.gif", QueueItemType.IconItem, 2, False)
        '        .Enqueue(lTypeURL & "4.gif", lCacheIconFolder & "Virtual Cache.gif", QueueItemType.IconItem, 2, False)
        '        .Enqueue(lTypeURL & "1304.gif", lCacheIconFolder & "GPS Adventures Exhibit.gif", QueueItemType.IconItem, 2, False)
        '        .Enqueue(lTypeURL & "9.gif", lCacheIconFolder & "Project APE Cache.gif", QueueItemType.IconItem, 2, False)
        '        .Enqueue(lTypeURL & "11.gif", lCacheIconFolder & "Webcam Cache.gif", QueueItemType.IconItem, 2, False)
        '        Dim lIconURL As String = lBaseURL & "icons/"
        '        .Enqueue(lIconURL & "icon_smile.gif", lCacheIconFolder & "icon_smile.gif", QueueItemType.IconItem, 2, False)
        '        .Enqueue(lIconURL & "icon_sad.gif", lCacheIconFolder & "icon_sad.gif", QueueItemType.IconItem, 2, False)
        '        .Enqueue(lIconURL & "icon_note.gif", lCacheIconFolder & "icon_note.gif", QueueItemType.IconItem, 2, False)
        '        .Enqueue(lIconURL & "icon_maint.gif", lCacheIconFolder & "icon_maint.gif", QueueItemType.IconItem, 2, False)
        '        .Enqueue(lIconURL & "icon_needsmaint.gif", lCacheIconFolder & "icon_needsmaint.gif", QueueItemType.IconItem, 2, False)
        '        .Enqueue(lIconURL & "icon_disabled.gif", lCacheIconFolder & "icon_disabled.gif", QueueItemType.IconItem, 2, False)
        '        .Enqueue(lIconURL & "icon_enabled.gif", lCacheIconFolder & "icon_enabled.gif", QueueItemType.IconItem, 2, False)
        '        .Enqueue(lIconURL & "icon_greenlight.gif", lCacheIconFolder & "icon_greenlight.gif", QueueItemType.IconItem, 2, False)
        '        .Enqueue(lIconURL & "coord_update.gif", lCacheIconFolder & "coord_update.gif", QueueItemType.IconItem, 2, False)
        '        .Enqueue(lIconURL & "icon_rsvp.gif", lCacheIconFolder & "icon_rsvp.gif", QueueItemType.IconItem, 2, False)
        '    End With
        'End If
    End Sub

    Private Sub GetSettings()
        On Error Resume Next 'skip bad settings such as one that is not numeric but needs to be
        Dim lTileServerName As String = ""
        SetDefaultTileServers()
        Dim lSoftwareKey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software")
        If lSoftwareKey IsNot Nothing Then
            Dim lAppKey As Microsoft.Win32.RegistryKey = lSoftwareKey.OpenSubKey(g_AppName)
            If lAppKey IsNot Nothing Then
                With lAppKey
                    Dim lKeyIndex As Integer = 1
                    'Not currently saving servers in registry, saving in text file instead which is read in SetDefaultTileServers
                    'Do
                    '    Dim lServerNameUrl() As String = CStr(.GetValue("Server" & lKeyIndex, "")).replace(vbcr, "").Split(vblf)
                    '    If lServerNameUrl.Length > 1 Then
                    '        Servers.Add(lServerNameUrl(0), clsServer.FromFields(lServerNameUrl))
                    '        lKeyIndex += 1
                    '    Else
                    '        Exit Do
                    '    End If
                    'Loop
                    lKeyIndex = 1
                    Do
                        Dim lBuddyNameUrl() As String = CStr(.GetValue("Buddy" & lKeyIndex, "")).Split("|")
                        If lBuddyNameUrl.Length = 2 Then
                            Dim lNewBuddy As New clsBuddy
                            lNewBuddy.Name = lBuddyNameUrl(0)
                            lNewBuddy.LocationURL = lBuddyNameUrl(1)
                            Buddies.Add(lNewBuddy.Name, lNewBuddy)
                            lKeyIndex += 1
                        Else
                            Exit Do
                        End If
                    Loop

                    'TileCacheFolder gets set earlier
                    lTileServerName = .GetValue("TileServer", TileServer.Name)
                    TileServerName = lTileServerName
                    g_UploadPointURL = .GetValue("UploadPointURL", g_UploadPointURL)
                    g_UploadTrackURL = .GetValue("UploadTrackURL", g_UploadTrackURL)

                    'Tiles older than this will be downloaded again as needed. TileCacheDays = 0 to never refresh old tiles
                    'TODO: different expiration time for different tiles - satellite photos seldom updated, OSM tiles often
                    Dim lTileCacheDays As Integer = .GetValue("TileCacheDays", 0)
                    If lTileCacheDays > 0 Then
                        Downloader.TileCacheOldest = DateTime.Now.ToUniversalTime.AddDays(-lTileCacheDays)
                    Else
                        Downloader.TileCacheOldest = Date.MinValue
                    End If

                    Zoom = .GetValue("Zoom", Zoom)

                    GPXShow = .GetValue("GPXShow", GPXShow)
                    GPXPanTo = .GetValue("GPXPanTo", GPXPanTo)
                    GPXZoomTo = .GetValue("GPXZoomTo", GPXZoomTo)
                    GPXFolder = .GetValue("GPXFolder", GPXFolder)
                    GPXLabelField = .GetValue("GPXLabelField", GPXLabelField)

                    Dark = .GetValue("Dark", pDark)
                    pShowTileImages = .GetValue("TileImages", pShowTileImages)
                    pShowTileNames = .GetValue("TileNames", pShowTileNames)
                    pShowTileOutlines = .GetValue("TileOutlines", pShowTileOutlines)
                    pShowGPSdetails = .GetValue("GPSdetails", pShowGPSdetails)
                    pShowCopyright = .GetValue("ShowCopyright", pShowCopyright)

                    ControlsUse = .GetValue("ControlsUse", ControlsUse)
                    ControlsShow = .GetValue("ControlsShow", ControlsShow)
                    'TODO: pControlsMargin, but in pixels or percent?

                    CenterLat = .GetValue("CenterLat", CenterLat)
                    CenterLon = .GetValue("CenterLon", CenterLon)
                    SanitizeCenterLatLon()

                    pGPSAutoStart = .GetValue("GPSAutoStart", pGPSAutoStart)
                    pGPSCenterBehavior = .GetValue("GPSFollow", pGPSCenterBehavior)
                    GPSSymbolSize = .GetValue("GPSSymbolSize", GPSSymbolSize)
                    TrackSymbolSize = .GetValue("TrackSymbolSize", TrackSymbolSize)

                    RecordTrack = .GetValue("RecordTrack", RecordTrack)
                    pRecordCellID = .GetValue("RecordCellID", pRecordCellID)
                    pOpenCellIdKey = .GetValue("OpenCellIdKey", pOpenCellIdKey)
                    pDisplayTrack = .GetValue("DisplayTrack", pDisplayTrack)
                    UploadOnStart = .GetValue("UploadOnStart", UploadOnStart)
                    UploadOnStop = .GetValue("UploadOnStop", UploadOnStop)
                    UploadTrackOnStop = .GetValue("UploadTrackOnStop", UploadTrackOnStop)
                    UploadPeriodic = .GetValue("UploadPeriodic", UploadPeriodic)
                    UploadMinInterval = New TimeSpan(0, 0, .GetValue("UploadPeriodicSeconds", UploadMinInterval.TotalSeconds))
                End With
            End If
        End If
        TileServerName = lTileServerName
    End Sub

    Public Sub SetDefaultTileServers()
        Dim lTileServersFilename As String = TileServersFilename()
        Dim lServersHTML As String = ReadTextFile(lTileServersFilename)
        If lServersHTML.ToLower.IndexOf("<table>") > 0 Then
            Servers = clsServer.ReadServers(lServersHTML)
        Else
            'TODO: embed default version of servers.html?
        End If
        If Servers.Count = 0 Then
            Dim pDefaultTileServers() As String = {"OpenStreetMap", "http://tile.openstreetmap.org/mapnik/", _
                                                   "Maplint", "http://tah.openstreetmap.org/Tiles/maplint/", _
                                                   "Osmarender", "http://tah.openstreetmap.org/Tiles/tile/", _
                                                   "Cycle Map", "http://andy.sandbox.cloudmade.com/tiles/cycle/", _
                                                   "No Name", "http://tile.cloudmade.com/fd093e52f0965d46bb1c6c6281022199/3/256/"}
            Servers = New Dictionary(Of String, clsServer)
            For lIndex As Integer = 0 To pDefaultTileServers.Length - 1 Step 2
                Servers.Add(pDefaultTileServers(lIndex), New clsServer(pDefaultTileServers(lIndex), , pDefaultTileServers(lIndex + 1)))
            Next

            '    Windows Mobile does not support Enum.GetValues or Enum.GetName, so we hard-code names to add below
            '    For Each lMapType As MapType In System.Enum.GetValues(GetType(MapType))
            '        Dim lMapTypeName As String = System.Enum.GetName(GetType(MapType), lMapType)
            '        If lMapTypeName.IndexOf("OpenStreet") < 0 Then 'Only add non-OSM types
            '            pTileServers.Add(lMapTypeName, MakeImageUrl(lMapType, New Point(1, 2), 3))
            '        End If
            '    Next

            'Servers.Add("YahooMap", New clsServer("YahooMap", , MakeImageUrl(MapType.YahooMap, New Point(1, 2), 3)))
            'Servers.Add("YahooSatellite", New clsServer("YahooSatellite", , MakeImageUrl(MapType.YahooSatellite, New Point(1, 2), 3)))
            'Servers.Add("YahooLabels", MakeImageUrl(MapType.YahooLabels, New Point(1, 2), 3))

            'Servers.Add("VirtualEarthMap", MakeImageUrl(MapType.VirtualEarthMap, New Point(1, 2), 3))
            'Servers.Add("VirtualEarthSatellite", MakeImageUrl(MapType.VirtualEarthSatellite, New Point(1, 2), 3))
            'Servers.Add("VirtualEarthHybrid", MakeImageUrl(MapType.VirtualEarthHybrid, New Point(1, 2), 3))
        End If
        TileCacheFolder = pTileCacheFolder
    End Sub

    Private Function TileServersFilename() As String
        Dim lTileServersFilename As String = pTileCacheFolder & "servers.html"
        If IO.File.Exists(lTileServersFilename) OrElse _
           Downloader.DownloadFile(Nothing, "http://vatavia.net/mark/VataviaMap/servers.html", lTileServersFilename, lTileServersFilename) Then
            Return lTileServersFilename
        End If
        Return ""
    End Function

    Public Sub SaveSettings()
        On Error Resume Next
        Dim lSoftwareKey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software")
        If lSoftwareKey IsNot Nothing Then
            Dim lAppKey As Microsoft.Win32.RegistryKey = lSoftwareKey.CreateSubKey(g_AppName)
            If lAppKey IsNot Nothing Then
                With lAppKey
                    'TODO: .SetValue("TileCacheDays", lCacheDays)

                    Dim lKeyIndex As Integer = 1

                    Dim lTileServersFilename As String = TileServersFilename()
                    If Not String.IsNullOrEmpty(lTileServersFilename) Then
                        Dim lNewServersHTML As String = clsServer.WriteServers(Servers)
                        If lNewServersHTML <> ReadTextFile(lTileServersFilename) Then
                            WriteTextFile(lTileServersFilename, lNewServersHTML)
                        End If
                    End If

                    'For Each lName As String In Servers.Keys
                    '    .SetValue("Server" & lKeyIndex, Servers.Item(lName).ToString)
                    '    lKeyIndex += 1
                    'Next
                    'lKeyIndex = 1
                    For Each lName As String In Buddies.Keys
                        .SetValue("Buddy" & lKeyIndex, lName & "|" & Buddies.Item(lName).LocationURL)
                        lKeyIndex += 1
                    Next

                    If IO.Directory.Exists(pTileCacheFolder) Then
                        .SetValue("TileCacheFolder", pTileCacheFolder)
                    End If
                    .SetValue("TileServer", TileServer.Name)
                    .SetValue("Zoom", Zoom)

                    .SetValue("GPXShow", GPXShow)
                    .SetValue("GPXPanTo", GPXPanTo)
                    .SetValue("GPXZoomTo", GPXZoomTo)
                    .SetValue("GPXFolder", GPXFolder)
                    .SetValue("GPXLabelField", GPXLabelField)

                    .SetValue("Dark", pDark)
                    .SetValue("TileImages", pShowTileImages)
                    .SetValue("TileNames", pShowTileNames)
                    .SetValue("TileOutlines", pShowTileOutlines)
                    .SetValue("GPSdetails", pShowGPSdetails)

                    .SetValue("ControlsUse", ControlsUse)
                    .SetValue("ControlsShow", ControlsShow)
                    'TODO: pControlsMargin, but in pixels or percent?

                    .SetValue("CenterLat", CenterLat)
                    .SetValue("CenterLon", CenterLon)

                    .SetValue("GPSAutoStart", pGPSAutoStart)
                    .SetValue("GPSFollow", pGPSCenterBehavior)
                    .SetValue("GPSSymbolSize", GPSSymbolSize)
                    .SetValue("TrackSymbolSize", TrackSymbolSize)
                    .SetValue("RecordTrack", RecordTrack)
                    .SetValue("RecordCellID", pRecordCellID)
                    .SetValue("OpenCellIdKey", pOpenCellIdKey)
                    .SetValue("DisplayTrack", pDisplayTrack)
                    .SetValue("UploadOnStart", UploadOnStart)
                    .SetValue("UploadOnStop", UploadOnStop)
                    .SetValue("UploadTrackOnStop", UploadTrackOnStop)
                    .SetValue("UploadPeriodic", UploadPeriodic)
                    .SetValue("UploadPeriodicSeconds", UploadMinInterval.TotalSeconds)
                    .SetValue("UploadPointURL", g_UploadPointURL)
                    .SetValue("UploadTrackURL", g_UploadTrackURL)
                End With
            End If
        End If
    End Sub

    Public Property TileServerName() As String
        Get
            Return TileServer.Name
        End Get
        Set(ByVal value As String)
            If Servers IsNot Nothing AndAlso Servers.ContainsKey(value) Then
                Dim lServer As clsServer = Servers(value)
                SetServerCacheFolder(lServer)
                TileServer = lServer
                If Downloader IsNot Nothing Then Downloader.ClearQueue(QueueItemType.TileItem, -1)

                'If new server does not cover the area we were looking at, pan to center of area new server covers
                If CenterLat > TileServer.LatMax OrElse _
                   CenterLat < TileServer.LatMin Then
                    CenterLat = TileServer.LatMin + (TileServer.LatMax - TileServer.LatMin) / 2
                End If

                If CenterLon > TileServer.LonMax OrElse _
                   CenterLon < TileServer.LonMin Then
                    CenterLon = TileServer.LonMin + (TileServer.LonMax - TileServer.LonMin) / 2
                End If
                SanitizeCenterLatLon()
 
                Dim lTitle As String = g_AppName & " " & TileServer.Name
                For Each lServer In TransparentTileServers
                    lTitle &= ", " & lServer.Name
                Next
                Me.Text = lTitle
                RaiseEvent TileServerChanged()
            End If
        End Set
    End Property

    Public Sub EnableTransparentTileServer(ByVal aServerName As String, ByVal aEnable As Boolean)
        Dim lServer As clsServer = Servers(aServerName)
        If lServer IsNot Nothing Then
            SetServerCacheFolder(TileServer)
            If aEnable Then
                If Not TransparentTileServers.Contains(lServer) Then
                    TransparentTileServers.Add(lServer)
                    FitToServer(lServer)
                End If
            Else
                If TransparentTileServers.Contains(lServer) Then
                    TransparentTileServers.Remove(lServer)
                End If
            End If
            RaiseEvent TileServerChanged()
        End If
    End Sub

    Private Sub SetServerCacheFolder(ByVal aTileServer As clsServer)
        aTileServer.CacheFolder = pTileCacheFolder & SafeFilename(aTileServer.Name.Replace(" ", "")) & g_PathChar
        aTileServer.BadTileSize = FileSize(aTileServer.CacheFolder & "unavailable") 'Look for example "bad tile" named "unavailable" in server's cache folder
    End Sub

    Public Function FollowWebsiteURL(ByVal aURL As String) As Boolean
        If aURL.IndexOf(":") >= 0 Then
            Dim lLat As Double = 0
            Dim lLon As Double = 0
            Dim lZoom As Integer = Zoom 'Default to current zoom in case we don't get a zoom value from URL
            Dim lNorth As Double = 0
            Dim lWest As Double = 0
            Dim lSouth As Double = 0
            Dim lEast As Double = 0

            If TileServer.ParseWebmapURL(aURL, lLat, lLon, lZoom, lNorth, lWest, lSouth, lEast) Then
                CenterLat = lLat
                CenterLon = lLon
                SanitizeCenterLatLon()
                'pShowURL = aURL
                If lZoom <> Zoom Then
                    Zoom = lZoom
                Else
                    NeedRedraw()
                End If
                FollowWebsiteURL = True
            Else
                MsgBox("Did not recognize map website link:" & vbLf & aURL, MsgBoxStyle.OkOnly, "Web Map URL")
            End If
        Else
            MsgBox("Copy a map website permanent link to the clipboard before using this menu", MsgBoxStyle.OkOnly, "Web Map URL")
        End If
    End Function

    Public Property Magnify() As Double
        Get
            Return pMagnify
        End Get
        Set(ByVal value As Double)
            pMagnify = value
            NeedRedraw()
        End Set
    End Property

    ''' <summary>
    ''' OSM zoom level currently being displayed on the form
    ''' </summary>
    ''' <remarks>varies from g_TileServer.ZoomMin to g_TileServer.ZoomMax</remarks>
    Public Property Zoom() As Integer
        Get
            Return pZoom
        End Get
        Set(ByVal value As Integer)
            If value <> pZoom Then
                Downloader.ClearQueue(QueueItemType.TileItem, -1)
                If value > TileServer.ZoomMax Then
                    pZoom = TileServer.ZoomMax
                ElseIf value < TileServer.ZoomMin Then
                    pZoom = TileServer.ZoomMin
                Else
                    'ZoomPreview(value)
                    pZoom = value
                End If
                NeedRedraw()
                RaiseEvent Zoomed()
                pClearDelayedTiles = True
            End If
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
    ''' <remarks>Also sets LatHeight, LonWidth</remarks>
    Private Sub FindTileBounds(ByVal aServer As clsServer, _
                               ByVal aBounds As RectangleF, _
                               ByRef aOffsetToCenter As Point, _
                               ByRef aTopLeft As Point, ByRef aBotRight As Point)
        Dim lOffsetFromTileEdge As Point
        Dim lCentermostTile As Point = CalcTileXY(aServer, CenterLat, CenterLon, pZoom, lOffsetFromTileEdge)
        Dim lNorth As Double, lWest As Double, lSouth As Double, lEast As Double
        CalcLatLonFromTileXY(lCentermostTile, pZoom, lNorth, lWest, lSouth, lEast)
        LatHeight = (lNorth - lSouth) * aBounds.Height / aServer.TileSize
        LonWidth = (lEast - lWest) * aBounds.Width / aServer.TileSize
        SanitizeCenterLatLon()

        aTopLeft = New Point(lCentermostTile.X, lCentermostTile.Y)
        Dim x As Integer = (aBounds.Width / 2) - lOffsetFromTileEdge.X
        'Move west until we find the edge of g, TODO: find faster with mod?
        While x > 0
            aTopLeft.X -= 1
            x -= aServer.TileSize
        End While
        aOffsetToCenter.X = x

        Dim y As Integer = (aBounds.Height / 2) - lOffsetFromTileEdge.Y
        'Move north until we find the edge of g, TODO: find faster with mod?
        While y > 0
            aTopLeft.Y -= 1
            y -= aServer.TileSize
        End While
        aOffsetToCenter.Y = y

        aBotRight = New Point(lCentermostTile.X, lCentermostTile.Y)
        x = (aBounds.Width / 2) - lOffsetFromTileEdge.X + aServer.TileSize
        'Move east until we find the edge of g, TODO: find faster with mod?
        While x < aBounds.Width
            aBotRight.X += 1
            x += aServer.TileSize
        End While

        y = (aBounds.Height / 2) - lOffsetFromTileEdge.Y + aServer.TileSize
        'Move south until we find the edge of g, TODO: find faster with mod?
        While y < aBounds.Height
            aBotRight.Y += 1
            y += aServer.TileSize
        End While
    End Sub

    Private Function TilesPointsInView(ByVal aTopLeft As Point, ByVal aBotRight As Point) As SortedList(Of Single, Point)
        Dim lTilePoints As New SortedList(Of Single, Point)
        Dim x, y As Integer
        Dim lMidX As Integer = (aTopLeft.X + aBotRight.X) / 2
        Dim lMidY As Integer = (aTopLeft.Y + aBotRight.Y) / 2
        Dim lDistance As Single

        'Loop through each visible tile
        For x = aTopLeft.X To aBotRight.X
            For y = aTopLeft.Y To aBotRight.Y
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
        Return lTilePoints
    End Function

    Public Sub ReDownloadAllVisibleTiles()
        Dim lOffsetToCenter As Point
        Dim lTopLeft As Point
        Dim lBotRight As Point

        FindTileBounds(TileServer, New RectangleF(0, 0, Width, Height), lOffsetToCenter, lTopLeft, lBotRight)
        For Each lTilePoint As Point In TilesPointsInView(lTopLeft, lBotRight).Values
            Dim lTileFilename As String = TileServer.TileFilename(lTilePoint, pZoom, False)
            If lTileFilename.Length > 0 Then
                Downloader.DeleteTile(lTileFilename)
                NeedRedraw()
            End If
        Next
    End Sub

    ''' <summary>
    ''' Draw all the visible tiles from all active tile servers
    ''' </summary>
    ''' <param name="g">Graphics object to be drawn into</param>
    Public Sub DrawTiles(ByVal g As Graphics)
        If TileServer.Transparent Then g.Clear(Color.White)
        TileServer.Opacity = 1
        DrawTiles(TileServer, g)

        For Each lServer As clsServer In TransparentTileServers
            If Not lServer.Transparent AndAlso lServer.Opacity = 1 Then lServer.Opacity = 0.5
            DrawTiles(lServer, g)
        Next
    End Sub

    ''' <summary>
    ''' Draw all the visible tiles from the given tile server
    ''' </summary>
    ''' <param name="g">Graphics object to be drawn into</param>
    Private Sub DrawTiles(ByVal aServer As clsServer, ByVal g As Graphics)
        Dim lOffsetToCenter As Point
        Dim lTopLeft As Point
        Dim lBotRight As Point

        FindTileBounds(aServer, g.ClipBounds, lOffsetToCenter, lTopLeft, lBotRight)

        Dim x, y As Integer
        Dim lOffsetFromWindowCorner As Point

        'TODO: check available memory before deciding how many tiles to keep in RAM
        Downloader.TileRAMcacheLimit = (lBotRight.X - lTopLeft.X + 1) * (lBotRight.Y - lTopLeft.Y + 1) * 2

        For Each lTilePoint As Point In TilesPointsInView(lTopLeft, lBotRight).Values
            If pRedrawPending Then Exit Sub

            x = lTilePoint.X
            y = lTilePoint.Y

            lOffsetFromWindowCorner.X = (x - lTopLeft.X) * aServer.TileSize + lOffsetToCenter.X
            lOffsetFromWindowCorner.Y = (y - lTopLeft.Y) * aServer.TileSize + lOffsetToCenter.Y

            Dim lDrewTile As Boolean = DrawTile(aServer, lTilePoint, pZoom, g, lOffsetFromWindowCorner, 0)

            If Not lDrewTile Then ' search for cached tiles at other zoom levels to substitute
                'First try tiles zoomed farther out
                Dim lZoom As Integer
                Dim lX As Integer = x
                Dim lY As Integer = y
                Dim lNextX As Integer
                Dim lNextY As Integer
                Dim lZoomedTilePortion As Integer = aServer.TileSize
                Dim lFromX As Integer = 0
                Dim lFromY As Integer = 0
                Dim lRectOrigTile As New Rectangle(lOffsetFromWindowCorner.X, lOffsetFromWindowCorner.Y, aServer.TileSize, aServer.TileSize)
                Dim lZoomMin As Integer = Math.Max(aServer.ZoomMin, pZoom - 4)
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
                    If lX > lNextX << 1 Then lFromX += aServer.HalfTileSize
                    If lY > lNextY << 1 Then lFromY += aServer.HalfTileSize

                    If DrawTile(aServer, New Drawing.Point(lNextX, lNextY), lZoom, g, lRectOrigTile, _
                                New Rectangle(lFromX, lFromY, lZoomedTilePortion, lZoomedTilePortion), -1) Then
                        Exit For 'found a zoomed out tile to draw, don't keep looking for one zoomed out farther
                    End If
                    lX = lNextX
                    lY = lNextY
                Next

                If pZoom < aServer.ZoomMax Then ' Try to draw tiles within this tile at finer zoom level
                    lZoom = pZoom + 1
                    Dim lDoubleX As Integer = x << 1
                    Dim lFineTilePoint As New Drawing.Point(lDoubleX, y << 1)
                    ' Portion of the tile covered by a finer zoom tile
                    Dim lRectDestination As New Rectangle(lOffsetFromWindowCorner.X, _
                                                          lOffsetFromWindowCorner.Y, _
                                                          aServer.HalfTileSize, aServer.HalfTileSize)

                    ' upper left tile
                    If Not DrawTile(aServer, lFineTilePoint, lZoom, g, lRectDestination, aServer.TileSizeRect, -1) Then
                        'TODO: four tiles at next finer zoom level, at this and other three DrawTile below
                    End If

                    ' upper right tile
                    lFineTilePoint.X += 1
                    lRectDestination.X = lOffsetFromWindowCorner.X + aServer.HalfTileSize
                    DrawTile(aServer, lFineTilePoint, lZoom, g, lRectDestination, aServer.TileSizeRect, -1)

                    ' lower right tile
                    lFineTilePoint.Y += 1
                    lRectDestination.Y = lOffsetFromWindowCorner.Y + aServer.HalfTileSize
                    DrawTile(aServer, lFineTilePoint, lZoom, g, lRectDestination, aServer.TileSizeRect, -1)

                    ' lower left tile
                    lFineTilePoint.X = lDoubleX
                    lRectDestination.X = lOffsetFromWindowCorner.X
                    DrawTile(aServer, lFineTilePoint, lZoom, g, lRectDestination, aServer.TileSizeRect, -1)
                End If
            End If
        Next

        If pRedrawPending Then Exit Sub

        If ControlsShow Then
            g.DrawLine(pPenBlack, pControlsMargin, 0, pControlsMargin, pBitmap.Height)
            g.DrawLine(pPenBlack, 0, pControlsMargin, pBitmap.Width, pControlsMargin)
            g.DrawLine(pPenBlack, pBitmap.Width - pControlsMargin, 0, pBitmap.Width - pControlsMargin, pBitmap.Height)
            g.DrawLine(pPenBlack, 0, pBitmap.Height - pControlsMargin, pBitmap.Width, pBitmap.Height - pControlsMargin)
        End If

        If GPXShow Then DrawLayers(g, lTopLeft, lOffsetToCenter)

        If pShowCopyright AndAlso pShowTileImages AndAlso Not String.IsNullOrEmpty(aServer.Copyright) Then
            g.DrawString(aServer.Copyright, pFontCopyright, pBrushCopyright, 3, pBitmap.Height - 20)
        End If

        If pShowDate OrElse LabelServer IsNot Nothing Then
            Dim lHeader As String = ""
            If pShowDate Then lHeader &= DateTime.Now.ToString("yyyy-MM-dd HH:mm ")
            If LabelServer IsNot Nothing Then lHeader &= LabelServer.BuildWebmapURL(CenterLat, CenterLon, Zoom, LatMax, LonMin, LatMin, LonMax) 'CenterLon.ToString("#.#######") & ", " & CenterLat.ToString("#.#######")
            g.DrawString(lHeader, pFontTileLabel, pBrushBlack, 3, 3)
        End If

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
    Private Function FindTileFilename(ByVal aServer As clsServer, _
                                      ByVal aBounds As RectangleF, _
                             Optional ByVal aX As Integer = -1, _
                             Optional ByVal aY As Integer = -1) As String
        Dim lOffsetToCenter As Point
        Dim lTopLeft As Point
        Dim lBotRight As Point

        FindTileBounds(aServer, aBounds, lOffsetToCenter, lTopLeft, lBotRight)

        Dim TilePoint As Point
        Dim lOffsetFromWindowCorner As Point

        'Loop through each visible tile
        For x As Integer = lTopLeft.X To lBotRight.X
            For y As Integer = lTopLeft.Y To lBotRight.Y
                TilePoint = New Point(x, y)

                lOffsetFromWindowCorner.X = (x - lTopLeft.X) * TileServer.TileSize + lOffsetToCenter.X
                lOffsetFromWindowCorner.Y = (y - lTopLeft.Y) * TileServer.TileSize + lOffsetToCenter.Y

                With lOffsetFromWindowCorner
                    If .X <= aX AndAlso .X + TileServer.TileSize >= aX AndAlso .Y <= aY AndAlso .Y + TileServer.TileSize >= aY Then
                        Return TileServer.TileFilename(TilePoint, pZoom, False)
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
        If aZoom = pZoom Then
            If aTileServerURL = TileServer.TilePattern Then
                NeedRedraw()
            Else
                For Each lServer As clsServer In TransparentTileServers
                    If aTileServerURL = lServer.TilePattern Then
                        NeedRedraw()
                        Exit For
                    End If
                Next
            End If
        End If

        'pRedrawWhenFinishedQueue = True
        'Just draw the tile downloaded, not the whole display
        'Dim lGraphics As Graphics = GetBitmapGraphics()
        'If lGraphics IsNot Nothing Then
        '    If aZoom = pZoom Then
        '        Dim lOffsetToCenter As Point
        '        Dim lTopLeft As Point
        '        Dim lBotRight As Point

        '        FindTileBounds(lGraphics.ClipBounds, lOffsetToCenter, lTopLeft, lBotRight)

        '        Dim lOffsetFromWindowCorner As Point
        '        lOffsetFromWindowCorner.X = (aTilePoint.X - lTopLeft.X) * g_TileServer.TileSize + lOffsetToCenter.X
        '        lOffsetFromWindowCorner.Y = (aTilePoint.Y - lTopLeft.Y) * g_TileServer.TileSize + lOffsetToCenter.Y
        '        DrawTile(aTilePoint, aZoom, lGraphics, lOffsetFromWindowCorner, -1)
        '    Else
        '        'TODO: draw tiles at different zoom levels? 
        '        'Would be nice when doing DownloadDescendants to see progress, but would also be confusing when tile download completes after zoom
        '    End If
        '    ReleaseBitmapGraphics()

        '    Try 'This method will be running in another thread, so we need to call Refresh in a complicated way
        '        Me.Invoke(pRefreshCallback)
        '    Catch
        '        'Ignore if we could not refresh, probably because form is closing
        '    End Try
        'End If
    End Sub

    Private Sub RequestBuddyPoint(ByVal o As Object)
        If Buddies Is Nothing OrElse Buddies.Count = 0 Then
            MsgBox("Add Buddies Before Finding them", MsgBoxStyle.OkOnly, "No Buddies Found")
        Else
            For Each lBuddy As clsBuddy In Buddies.Values
                If lBuddy.Selected Then Downloader.Enqueue(lBuddy.LocationURL, IO.Path.GetTempPath & SafeFilename(lBuddy.Name), QueueItemType.PointItem, 0, True, lBuddy)
            Next
        End If
    End Sub

    Public Sub DownloadedItem(ByVal aItem As clsQueueItem) Implements IQueueListener.DownloadedItem
        Dim lNeedRedrawNow As Boolean = False
        Select Case aItem.ItemType
            Case QueueItemType.IconItem
                Try
                    Dim lBitmap As New Drawing.Bitmap(aItem.Filename)
                    Dim lCacheIconFolder As String = (pTileCacheFolder & "Icons").ToLower & g_PathChar
                    If aItem.Filename.ToLower.StartsWith(lCacheIconFolder) Then 'Geocache or other icon that lives in "Icons" folder in pTileCacheFolder
                        Dim lIconName As String = IO.Path.ChangeExtension(aItem.Filename, "").TrimEnd(".").Substring(lCacheIconFolder.Length).Replace(g_PathChar, "|")
                        g_WaypointIcons.Add(lIconName.ToLower, lBitmap)
                    Else
                        g_WaypointIcons.Add(aItem.Filename.ToLower, lBitmap)
                    End If
                    pRedrawWhenFinishedQueue = True
                Catch e As Exception
                End Try
            Case QueueItemType.PointItem
                Try
                    Dim lBuddy As clsBuddy = aItem.ItemObject
                    If lBuddy IsNot Nothing Then
                        If lBuddy.LoadFile(aItem.Filename, pTileCacheFolder & "Icons" & g_PathChar & "Buddy") Then
                            If lBuddy.IconFilename.Length > 0 AndAlso Not IO.File.Exists(lBuddy.IconFilename) AndAlso lBuddy.IconURL.Length > 0 Then
                                Downloader.Enqueue(lBuddy.IconURL, lBuddy.IconFilename, QueueItemType.IconItem, , False, lBuddy)
                            End If

                            'Zoom out to include this buddy in the view
                            SetCenterFromDevice(lBuddy.Waypoint.lat, lBuddy.Waypoint.lon, 3)

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
        Try
            'If running in another thread, can't call Refresh directly
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
        If pRedrawWhenFinishedQueue Then
            pRedrawWhenFinishedQueue = False
            NeedRedraw()
        End If
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
                lLayer.Render(TileServer, g, aTopLeftTile, aOffsetToCenter)
            Next
        End If
        If Buddies IsNot Nothing AndAlso Buddies.Count > 0 Then
            Dim lUtcNow As Date = Date.UtcNow
            Dim lWaypoints As New Generic.List(Of clsGPXwaypoint)
            For Each lBuddy As clsBuddy In Buddies.Values
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
                lLayer.LabelMinZoom = 0 'Always label buddies
                lLayer.Render(TileServer, g, aTopLeftTile, aOffsetToCenter)
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
    Private Function DrawTile(ByVal aServer As clsServer, ByVal aTilePoint As Point, ByVal aZoom As Integer, ByVal g As Graphics, ByVal aOffset As Point, ByVal aPriority As Integer) As Boolean
        Try
            Dim lDrewImage As Boolean = False
            If pShowTileImages Then
                Dim lDestRect As New Rectangle(aOffset.X, aOffset.Y, aServer.TileSize, aServer.TileSize)
                lDrewImage = DrawTile(aServer, aTilePoint, aZoom, g, lDestRect, aServer.TileSizeRect, 0)
            End If
            If pClearDelayedTiles AndAlso Not lDrewImage AndAlso Not aServer.Transparent Then
                g.FillRectangle(pBrushWhite, aOffset.X, aOffset.Y, aServer.TileSize, aServer.TileSize)
            End If
            If pShowTileOutlines Then g.DrawRectangle(pPenBlack, aOffset.X, aOffset.Y, aServer.TileSize, aServer.TileSize)
            If pShowTileNames Then
                Dim lTileFileName As String = aServer.TileFilename(aTilePoint, aZoom, False)
                If lTileFileName.Length > 0 Then
                    g.DrawString(lTileFileName.Substring(aServer.CacheFolder.Length).Replace(aServer.FileExtension, ""), _
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
    Private Function DrawTile(ByVal aServer As clsServer, ByVal aTilePoint As Point, ByVal aZoom As Integer, ByVal g As Graphics, ByVal aDrawRect As Rectangle, ByVal aImageRect As Rectangle, ByVal aPriority As Integer) As Boolean
        Try
            If Not pShowTileImages Then Return True

            Dim lTileImage As Bitmap = TileBitmap(aServer, aTilePoint, aZoom, aPriority)

            If lTileImage IsNot Nothing AndAlso lTileImage.Width > 1 Then

#If Smartphone Then
                If aServer.Transparent Then
                    Dim imageAttr As New Drawing.Imaging.ImageAttributes
                    imageAttr.SetColorKey(Color.White, Color.White)
                    g.DrawImage(lTileImage, aDrawRect, aImageRect.X, aImageRect.Y, aImageRect.Width, aImageRect.Height, GraphicsUnit.Pixel, imageAttr)
                Else
                    g.DrawImage(lTileImage, aDrawRect, aImageRect, GraphicsUnit.Pixel)
                End If
#Else
                If aServer.Opacity < 1 Then
                    Dim cm As New System.Drawing.Imaging.ColorMatrix
                    cm.Matrix33 = 0.5 ' aServer.Opacity
                    Dim lAttrs As New System.Drawing.Imaging.ImageAttributes
                    lAttrs.SetColorMatrix(cm)
                    g.DrawImage(lTileImage, aDrawRect, aImageRect.X, aImageRect.Y, aImageRect.Width, aImageRect.Height, GraphicsUnit.Pixel, lAttrs)
                Else
                    g.DrawImage(lTileImage, aDrawRect, aImageRect.X, aImageRect.Y, aImageRect.Width, aImageRect.Height, GraphicsUnit.Pixel)
                End If
#End If
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
    Private Function TileBitmap(ByVal aServer As clsServer, ByVal aTilePoint As Point, ByVal aZoom As Integer, ByVal aPriority As Integer) As Bitmap
        Dim lTileFileName As String = aServer.TileFilename(aTilePoint, aZoom, pUseMarkedTiles)
        If lTileFileName.Length = 0 Then
            Return Nothing
        Else
            Dim lTileImage As Bitmap = Downloader.GetTileBitmap(aServer, lTileFileName, aTilePoint, aZoom, aPriority, False)

            'Fall back to using unmarked tile if we have it but not a marked version
            If pUseMarkedTiles AndAlso lTileImage Is Nothing Then
                lTileFileName = aServer.TileFilename(aTilePoint, pZoom, False)
                lTileImage = Downloader.GetTileBitmap(aServer, lTileFileName, aTilePoint, aZoom, aPriority, False)
            End If
            Return lTileImage
        End If
    End Function

    Public Sub SanitizeCenterLatLon()

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

        FitToServer(TileServer)

        LatMin = CenterLat - LatHeight / 2
        LatMax = CenterLat + LatHeight / 2
        LonMin = CenterLon - LonWidth / 2
        LonMax = CenterLon + LonWidth / 2
        RaiseEvent Panned()
    End Sub

    Private Sub FitToServer(ByVal aServer As clsServer)
        If CenterLat > aServer.LatMax Then CenterLat = aServer.LatMax
        If CenterLat < aServer.LatMin Then CenterLat = aServer.LatMin

        If CenterLon > aServer.LonMax Then CenterLon = aServer.LonMax
        If CenterLon < aServer.LonMin Then CenterLon = aServer.LonMin

        If pZoom > aServer.ZoomMax Then Zoom = aServer.ZoomMax
        If pZoom < aServer.ZoomMin Then Zoom = aServer.ZoomMin
    End Sub

    Private Sub MouseDownLeft(ByVal e As System.Windows.Forms.MouseEventArgs)
        MouseDragging = True
        MouseDragStartLocation.X = e.X
        MouseDragStartLocation.Y = e.Y
        MouseDownLat = CenterLat
        MouseDownLon = CenterLon
    End Sub

    Private Sub ctlMap_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles MyBase.KeyDown
        pLastKeyDown = e.KeyCode
        If pLastKeyDown = 229 Then ' ProcessKey means we have to look harder to find actual key pressed
            'For lKeyCode As Integer = 0 To 255
            '    If GetAsyncKeyState(lKeyCode) And Windows.Forms.Keys.KeyCode Then
            '        pLastKeyDown = lKeyCode
            '        Exit Sub ' record the lowest numbered key code currently down, not necessarily the one triggering KeyDown
            '    End If
            'Next
        End If
    End Sub

    Private Sub Event_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles MyBase.KeyUp
        e.Handled = True
        Dim lNeedRedraw As Boolean = True
        Select Case e.KeyCode
            Case Keys.Right : CenterLon += MetersPerPixel(Zoom, TileServer.TileSize) * Me.Width / g_CircumferenceOfEarth * 180
            Case Keys.Left : CenterLon -= MetersPerPixel(Zoom, TileServer.TileSize) * Me.Width / g_CircumferenceOfEarth * 180
            Case Keys.Up
                If pLastKeyDown = 131 Then 'using scroll wheel
                    Zoom += 1 : lNeedRedraw = False
                Else
                    CenterLat += MetersPerPixel(Zoom, TileServer.TileSize) * Me.Height / g_CircumferenceOfEarth * 90
                End If
            Case Keys.Down
                If pLastKeyDown = 131 Then 'using scroll wheel
                    Zoom -= 1 : lNeedRedraw = False
                Else
                    CenterLat -= MetersPerPixel(Zoom, TileServer.TileSize) * Me.Height / g_CircumferenceOfEarth * 90
                End If
            Case Keys.A : Zoom -= 1 : lNeedRedraw = False
            Case Keys.Q : Zoom += 1 : lNeedRedraw = False
            Case Else
                e.Handled = False
                lNeedRedraw = False
        End Select
        If lNeedRedraw Then
            ManuallyNavigated()
        End If
    End Sub

    Private Sub Event_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseUp
        MouseDragging = False
        If ControlsUse AndAlso _
               Math.Abs(MouseDragStartLocation.X - e.X) < pControlsMargin / 2 AndAlso _
               Math.Abs(MouseDragStartLocation.Y - e.Y) < pControlsMargin / 2 Then
            'Count this as a tap/click and navigate if in a control area
            Dim lNeedRedraw As Boolean = True
            If e.X < pControlsMargin Then
                If e.Y < pControlsMargin Then 'Top Left Corner, zoom in
                    Zoom += 1 : lNeedRedraw = False
                ElseIf e.Y > Me.Height - pControlsMargin Then
                    Zoom -= 1 : lNeedRedraw = False 'Bottom Left Corner, zoom out
                Else 'Left Edge, pan left
                    CenterLon -= MetersPerPixel(pZoom, TileServer.TileSize) * Me.Width / g_CircumferenceOfEarth * 180
                End If
            ElseIf e.X > Me.Width - pControlsMargin Then
                If e.Y < pControlsMargin Then 'Top Right Corner, zoom in
                    Zoom += 1 : lNeedRedraw = False
                ElseIf e.Y > Me.Height - pControlsMargin Then
                    Zoom -= 1 : lNeedRedraw = False 'Bottom Right Corner, zoom out
                Else 'Right Edge, pan right
                    CenterLon += MetersPerPixel(pZoom, TileServer.TileSize) * Me.Width / g_CircumferenceOfEarth * 180
                End If
            ElseIf e.Y < pControlsMargin Then 'Top edge, pan up
                CenterLat += MetersPerPixel(pZoom, TileServer.TileSize) * Me.Height / g_CircumferenceOfEarth * 90
            ElseIf e.Y > Me.Height - pControlsMargin Then 'Bottom edge, pan down
                CenterLat -= MetersPerPixel(pZoom, TileServer.TileSize) * Me.Height / g_CircumferenceOfEarth * 90
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

    Private Sub Event_Disposed(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Disposed
        Downloader.Enabled = False
        Uploader.Enabled = False
        Diagnostics.Process.GetCurrentProcess.Kill() 'Try to kill our own process, making sure all threads stop
    End Sub

    Private Sub Event_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
        If pBitmap IsNot Nothing Then
            pBitmapMutex.WaitOne()
            Dim lMapRectangle As System.Drawing.Rectangle = MapRectangle()
            With lMapRectangle
                If pMagnify = 1 Then
                    e.Graphics.DrawImage(pBitmap, .Left, .Top)
                Else
                    Dim lMagnifiedWidth As Integer = .Width * pMagnify
                    Dim lMagnifiedHeight As Integer = .Height * pMagnify
                    Dim lMagnifiedLeft As Integer = .Left - (lMagnifiedWidth - .Width) / 2
                    Dim lMagnifiedTop As Integer = .Top - (lMagnifiedHeight - .Height) / 2
                    e.Graphics.DrawImage(pBitmap, New Rectangle(lMagnifiedLeft, lMagnifiedTop, lMagnifiedWidth, lMagnifiedHeight), lMapRectangle, GraphicsUnit.Pixel)
                End If
            End With
            pBitmapMutex.ReleaseMutex()
        End If
    End Sub

    ' Prevent flickering when default implementation redraws background
    Protected Overrides Sub OnPaintBackground(ByVal e As PaintEventArgs)
    End Sub

    Private Sub Event_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize
        With MapRectangle()
            If .Width > 0 AndAlso .Height > 0 Then
                pBitmapMutex.WaitOne()
                pBitmap = New Bitmap(.Width, .Height, Drawing.Imaging.PixelFormat.Format24bppRgb)
                pControlsMargin = Math.Min(pBitmap.Width, pBitmap.Height) / 4
                pBitmapMutex.ReleaseMutex()
            End If
        End With
        NeedRedraw()
    End Sub

    Public Sub OpenFiles(ByVal aFilenames() As String)
        Dim lGPXPanTo As Boolean = GPXPanTo
        Dim lGPXZoomTo As Boolean = GPXZoomTo
        If aFilenames.Length > 1 Then
            GPXPanTo = False
            GPXZoomTo = False
        End If
        'Dim lSaveTitle As String = Me.Text
        Dim lNumFiles As Integer = aFilenames.Length
        Dim lCurFile As Integer = 1
        For Each lFilename As String In aFilenames
            If IO.Directory.Exists(lFilename) Then
                OpenFiles(IO.Directory.GetFileSystemEntries(lFilename))
            Else
                RaiseEvent StatusChanged("Loading " & lCurFile & "/" & lNumFiles & " '" & IO.Path.GetFileNameWithoutExtension(lFilename) & "'")
                OpenFile(lFilename)
                lCurFile += 1
            End If
        Next
        'Me.Text = lSaveTitle
        If aFilenames.Length > 1 Then
            GPXPanTo = lGPXPanTo
            GPXZoomTo = lGPXZoomTo
            If GPXZoomTo Then ZoomToAll()
        End If

        If Not GPXPanTo AndAlso Not GPXZoomTo Then NeedRedraw()
        RaiseEvent StatusChanged(Nothing)
    End Sub

    Public Function OpenFile(ByVal aFilename As String) As clsLayer
        Dim lLayer As clsLayer = Nothing
        Select Case IO.Path.GetExtension(aFilename).ToLower
            Case ".cell" : lLayer = OpenCell(aFilename)
            Case ".jpg", ".jpeg" : lLayer = OpenPhoto(aFilename)
            Case Else : lLayer = OpenGPX(aFilename)
        End Select
        If lLayer IsNot Nothing Then
            RaiseEvent OpenedLayer(lLayer)
        End If
        Return lLayer
    End Function

    Public Function OpenCell(ByVal aFilename As String) As clsCellLayer
        Dim lLayer As New clsCellLayer(aFilename, Me)
        Layers.Add(lLayer)
        NeedRedraw()
        Return lLayer
    End Function

    Public Function OpenPhoto(ByVal aFilename As String) As clsLayer
        Dim lLayer As clsLayerGPX = Nothing
#If Not Smartphone Then
        Dim lExif As New ExifWorks(aFilename)
        Dim lNeedToSearchTracks As Boolean = False
        Dim lLatitude As Double = lExif.Latitude
        Dim lLongitude As Double = lExif.Longitude
        Dim lStr As String = Nothing, lStrMinutes As String = Nothing, lStrSeconds As String = Nothing

        If Double.IsNaN(lLatitude) OrElse Double.IsNaN(lLongitude) Then
            lNeedToSearchTracks = True
        Else
            DegreesMinutesSeconds(lLatitude, lStr, lStrMinutes, lStrSeconds)
            lStr &= " " & lStrMinutes & "' " & lStrSeconds & """"
            If Not lStrSeconds.Contains(".") Then lNeedToSearchTracks = True
        End If

        Dim lPhotoDate As DateTime = lExif.GPSDateTime
        If lPhotoDate.Year < 2 Then
            lPhotoDate = lExif.DateTimeOriginal.Add(pImportOffsetFromUTC)
        End If

        If lNeedToSearchTracks Then
            Dim lClosestPoint As clsGPXwaypoint = ClosestPoint(lPhotoDate)

            If lClosestPoint IsNot Nothing Then
                Dim lMsg As String = ""
                If Not Double.IsNaN(lLatitude) Then
                    DegreesMinutesSeconds(lLatitude, lStr, lStrMinutes, lStrSeconds)
                    lMsg &= "ExifLat = " & lStr & " " & lStrMinutes & "' " & lStrSeconds & """"
                End If

                lLatitude = lClosestPoint.lat
                DegreesMinutesSeconds(lLatitude, lStr, lStrMinutes, lStrSeconds)
                lMsg &= "GPXLat = " & lStr & " " & lStrMinutes & "' " & lStrSeconds & """"

                If Not Double.IsNaN(lLongitude) Then
                    DegreesMinutesSeconds(lLongitude, lStr, lStrMinutes, lStrSeconds)
                    lMsg &= "ExifLon = " & lStr & " " & lStrMinutes & "' " & lStrSeconds & """"
                End If

                lLongitude = lClosestPoint.lon
                DegreesMinutesSeconds(lLongitude, lStr, lStrMinutes, lStrSeconds)
                lMsg &= "GPXLon = " & lStr & " " & lStrMinutes & "' " & lStrSeconds & """"
                Dbg(lMsg)
            End If
        End If
        If Not Double.IsNaN(lLatitude) AndAlso Not Double.IsNaN(lLongitude) Then
            Dim lWaypoints As New Generic.List(Of clsGPXwaypoint)
            Dim lWaypoint As New clsGPXwaypoint("wpt", lLatitude, lLongitude)
            With lWaypoint
                .name = IO.Path.GetFileNameWithoutExtension(aFilename)
                .sym = aFilename
                .url = aFilename
                .time = lPhotoDate
                .SetExtension("LocalTime", Format(lExif.DateTimeOriginal, "yyyy-MM-dd HH:mm:ss"))
            End With

            lWaypoints.Add(lWaypoint)

            lLayer = New clsLayerGPX(lWaypoints, Me)
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
            If lNeedToSearchTracks Then LayersImportedByTime.Add(lLayer)

            If GPXPanTo Then
                CenterLat = lLatitude
                CenterLon = lLongitude
                SanitizeCenterLatLon()
            End If
            NeedRedraw()
        End If
#End If
        Return lLayer
    End Function

    Public Sub ReImportLayersByDate()
        For Each lLayer As clsLayerGPX In LayersImportedByTime
            lLayer.Bounds = New clsGPXbounds
            For Each lWaypoint As clsGPXwaypoint In lLayer.GPX.wpt
                Dim lLocalTime As Date = Date.Parse(lWaypoint.GetExtension("LocalTime") & "Z")
                Dim lUTC As Date = lLocalTime.Add(pImportOffsetFromUTC)
                Dim lClosestPoint As clsGPXwaypoint = ClosestPoint(lUTC)
                If lClosestPoint IsNot Nothing Then
                    lWaypoint.lat = lClosestPoint.lat
                    lWaypoint.lon = lClosestPoint.lon
                    lLayer.Bounds.Expand(lWaypoint.lat, lWaypoint.lon)
                End If
            Next
        Next
    End Sub

    ''' <summary>
    ''' Find the GPS location closest to the given date
    ''' </summary>
    ''' <param name="aDate">Date to search for</param>
    ''' <returns></returns>
    ''' <remarks>searches all currently loaded GPX layers</remarks>
    Private Function ClosestPoint(ByVal aDate As Date) As clsGPXwaypoint
        Dim lTargetTicks As Long = aDate.Ticks
        Dim lClosestTicks As Long = Long.MaxValue
        Dim lClosestPoint As clsGPXwaypoint = Nothing
        If Layers IsNot Nothing AndAlso Layers.Count > 0 Then
            For Each lTrackLayer As clsLayer In Layers
                If lTrackLayer.GetType.Name.Equals("clsLayerGPX") Then
                    Dim lGPX As clsLayerGPX = lTrackLayer
                    For Each lGpxTrack As clsGPXtrack In lGPX.GPX.trk
                        For Each lGpxTrackSeg As clsGPXtracksegment In lGpxTrack.trkseg
                            For Each lGpxTrackPoint As clsGPXwaypoint In lGpxTrackSeg.trkpt
                                If lGpxTrackPoint.timeSpecified Then
                                    Dim lTicksDiff As Long = Math.Abs(lGpxTrackPoint.time.Ticks - lTargetTicks)
                                    If lTicksDiff < lClosestTicks Then
                                        lClosestTicks = lTicksDiff
                                        lClosestPoint = lGpxTrackPoint
                                    End If
                                End If
                            Next
                        Next
                    Next
                End If
            Next
        End If
        Return lClosestPoint
    End Function

    Private Function OpenGPX(ByVal aFilename As String, Optional ByVal aInsertAt As Integer = -1) As clsLayerGPX
        Dim lNewLayer As clsLayerGPX = Nothing
        If IO.File.Exists(aFilename) Then
            CloseLayer(aFilename)
            Try
                lNewLayer = New clsLayerGPX(aFilename, Me)
                With lNewLayer
                    .LabelField = GPXLabelField
                    If Not GPXPanTo AndAlso Not GPXZoomTo AndAlso lNewLayer.GPX.bounds IsNot Nothing Then
                        With lNewLayer.GPX.bounds 'Skip loading this one if it is not in view
                            If .minlat > LatMax OrElse _
                            .maxlat < LatMin OrElse _
                            .minlon > LonMax OrElse _
                            .maxlon < LonMin Then
                                Return Nothing
                            End If
                        End With
                    End If
                End With
                If aInsertAt >= 0 Then
                    Layers.Insert(aInsertAt, lNewLayer)
                Else
                    Layers.Add(lNewLayer)
                End If
                If GPXPanTo OrElse GPXZoomTo AndAlso lNewLayer.Bounds IsNot Nothing Then
                    If GPXPanTo Then
                        PanTo(lNewLayer.Bounds)
                    End If
                    If GPXZoomTo AndAlso _
                       lNewLayer.Bounds.minlat < lNewLayer.Bounds.maxlat AndAlso _
                       lNewLayer.Bounds.minlon < lNewLayer.Bounds.maxlon Then
                        Zoom = FindZoom(lNewLayer.Bounds)
                    Else
                        NeedRedraw()
                    End If
                    Application.DoEvents()
                End If
            Catch e As Exception
                MsgBox(e.Message, MsgBoxStyle.Critical, "Could not open '" & aFilename & "'")
            End Try
        End If
        Return lNewLayer
    End Function

    ''' <summary>
    ''' Center the view on the center of the given bounds
    ''' </summary>
    ''' <param name="aBounds">Bounds to center on</param>
    ''' <remarks>Does not redraw</remarks>
    Public Sub PanTo(ByVal aBounds As clsGPXbounds)
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
                While lDesiredZoom < TileServer.ZoomMax AndAlso _
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
                While lDesiredZoom > TileServer.ZoomMin AndAlso _
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

    ''' <summary>
    ''' Pan and zoom the view to best contain the given bounds
    ''' </summary>
    ''' <param name="aBounds">Bounds to center on and zoom to</param>
    ''' <remarks>Redraws map</remarks>
    Public Sub ZoomTo(ByVal aBounds As clsGPXbounds, _
             Optional ByVal aZoomIn As Boolean = True, _
             Optional ByVal aZoomOut As Boolean = True)
        PanTo(aBounds)
        Dim lZoom As Integer = FindZoom(aBounds, aZoomIn, aZoomOut)
        If lZoom = Zoom Then
            NeedRedraw() 'Don't need to change zoom, just redraw
        Else
            Zoom = lZoom 'Changing zoom will also redraw
        End If
    End Sub

    Public Sub ZoomToAll()
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
            ZoomTo(lAllBounds)
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
        RaiseEvent ClosedLayer()
    End Sub

    Public Sub CloseAllLayers()
        For Each lLayer As clsLayer In Layers
            lLayer.Clear()
        Next
        Layers.Clear()
        RaiseEvent ClosedLayer()
    End Sub

    ''' <summary>
    ''' True to check for buddies, False to stop checking
    ''' </summary>
    ''' <value></value>
    Public Property FindBuddy() As Boolean
        Get
            Return pBuddyTimer IsNot Nothing
        End Get
        Set(ByVal value As Boolean)
            If value Then
                StartBuddyTimer()
            Else
                StopBuddyTimer()
            End If
        End Set
    End Property

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

    ''' <summary>
    ''' Set the map to make sure given location is in view
    ''' </summary>
    ''' <param name="aLatitude">Device Latitude</param>
    ''' <param name="aLongitude">Device Longitude</param>
    ''' <param name="aCenterBehavior">Whether / how to change view to include the given location</param>
    ''' <returns>True if map center was updated</returns>
    ''' <remarks></remarks>
    Private Function SetCenterFromDevice(ByVal aLatitude As Double, ByVal aLongitude As Double, ByVal aCenterBehavior As Integer) As Boolean
        If CenterLat <> aLatitude OrElse CenterLon <> aLongitude Then
            Select Case aCenterBehavior
                Case 0 'Do nothing
                Case 1 'Follow, only move center if it is off screen
                    If Not LatLonInView(aLatitude, aLongitude) Then
                        CenterLat = aLatitude
                        CenterLon = aLongitude
                        SanitizeCenterLatLon()
                        Return True
                    End If
                Case 2 'Always center
                    CenterLat = aLatitude
                    CenterLon = aLongitude
                    SanitizeCenterLatLon()
                    Return True
                Case 3 'Zoom out until location is in view
                    While Zoom > 0 AndAlso Not LatLonInView(aLatitude, aLongitude)
                        Zoom -= 1
                    End While
            End Select
        End If
        Return False
    End Function

    Public Function ClosestGeocache(ByVal aLatitude As Double, ByVal aLongitude As Double) As clsGPXwaypoint
        Dim lMiddlemostCache As clsGPXwaypoint = Nothing
        If Layers IsNot Nothing AndAlso Layers.Count > 0 Then
            Dim lClosestDistance As Double = Double.MaxValue
            Dim lThisDistance As Double
            For Each lLayer As clsLayer In Layers
                Try
                    Dim lGPXLayer As clsLayerGPX = lLayer
                    Dim lDrawThisOne As Boolean = True
                    If lGPXLayer.GPX.bounds IsNot Nothing Then
                        With lGPXLayer.GPX.bounds 'Skip if it is not in view
                            If .minlat > CenterLat + LatHeight / 2 OrElse _
                                .maxlat < CenterLat - LatHeight / 2 OrElse _
                                .minlon > CenterLon + LonWidth / 2 OrElse _
                                .maxlon < CenterLon - LonWidth / 2 Then
                                lDrawThisOne = False
                            End If
                        End With
                    End If
                    If lDrawThisOne Then
                        For Each lWaypoint As clsGPXwaypoint In lGPXLayer.GPX.wpt
                            lThisDistance = (lWaypoint.lat - CenterLat) ^ 2 + (lWaypoint.lon - CenterLon) ^ 2
                            If lThisDistance < lClosestDistance Then
                                lMiddlemostCache = lWaypoint
                                lClosestDistance = lThisDistance
                            End If
                        Next
                    End If
                Catch
                End Try
            Next
        End If
        Return lMiddlemostCache
    End Function

    Public Sub ShowGeocacheHTML(ByVal aCache As clsGPXwaypoint)
        If aCache IsNot Nothing Then
            Dim lTempPath As String = IO.Path.GetTempPath
            With aCache
                Windows.Forms.Clipboard.SetDataObject(.name)
                Dim lText As String = "<html><head><title>" & .name & "</title></head><body>"

                lText &= "<b><a href=""" & .url & """>" & .name & "</a></b> " _
                      & FormattedDegrees(.lat, g_DegreeFormat) & ", " _
                      & FormattedDegrees(.lon, g_DegreeFormat) & "<br>"

                If .desc IsNot Nothing AndAlso .desc.Length > 0 Then
                    lText &= .desc
                ElseIf .urlname IsNot Nothing AndAlso .urlname.Length > 0 Then
                    lText &= .urlname
                    If .cache IsNot Nothing Then
                        lText &= " (" & Format(.cache.difficulty, "#.#") & "/" _
                                      & Format(.cache.terrain, "#.#") & ")"
                    End If
                End If

                Dim lType As String = Nothing
                If .type IsNot Nothing AndAlso .type.Length > 0 Then
                    lType = .type
                    If lType.StartsWith("Geocache|") Then lType = lType.Substring(9)
                End If
                If lType Is Nothing AndAlso .cache IsNot Nothing AndAlso .cache.cachetype IsNot Nothing AndAlso .cache.cachetype.Length > 0 Then
                    lType = .cache.cachetype
                End If
                If lType IsNot Nothing AndAlso lText.IndexOf(lType) = -1 Then
                    lText &= " <b>" & .type & "</b>"
                End If

                If .cache IsNot Nothing AndAlso .cache.container IsNot Nothing AndAlso .cache.container.Length > 0 Then lText &= " <b>" & .cache.container & "</b>"
                lText &= "<br>"

                If .cmt IsNot Nothing AndAlso .cmt.Length > 0 Then lText &= "<b>comment:</b> " & .cmt & "<br>"

                If .cache IsNot Nothing Then
                    If .cache.archived Then lText &= "<h2>ARCHIVED</h2><br>"

                    If .cache.short_description IsNot Nothing AndAlso .cache.short_description.Length > 0 Then
                        lText &= .cache.short_description & "<br>"
                    End If
                    If .cache.long_description IsNot Nothing AndAlso .cache.long_description.Length > 0 Then
                        lText &= .cache.long_description & "<br>"
                    End If

                    If .cache.encoded_hints IsNot Nothing AndAlso .cache.encoded_hints.Length > 0 Then
                        lText &= "<b>Hint:</b> " & .cache.encoded_hints & "<br>"
                    End If

                    If .cache.logs IsNot Nothing AndAlso .cache.logs.Count > 0 Then
                        'T19:00:00 is the time for all logs? remove it and format the entries a bit
                        Dim lIconPath As String = pTileCacheFolder & "Icons" & g_PathChar & "Geocache" & g_PathChar
                        Dim lIconFilename As String
                        lText &= "<b>Logs:</b><br>"
                        For Each lLog As clsGroundspeakLog In .cache.logs
                            Select Case lLog.logtype
                                Case "Found it" : lIconFilename = "icon_smile.gif"
                                Case "Didn't find it" : lIconFilename = "icon_sad.gif"
                                Case "Write note" : lIconFilename = "icon_note.gif"
                                Case "Enable Listing" : lIconFilename = "icon_enabled.gif"
                                Case "Temporarily Disable Listing" : lIconFilename = "icon_disabled.gif"
                                Case "Needs Maintenance" : lIconFilename = "icon_needsmaint.gif"
                                Case "Owner Maintenance" : lIconFilename = "icon_maint.gif"
                                Case "Publish Listing" : lIconFilename = "icon_greenlight.gif"
                                Case "Update Coordinates" : lIconFilename = "coord_update.gif"
                                Case "Will Attend" : lIconFilename = "icon_rsvp.gif"
                                Case Else : lIconFilename = ""
                            End Select
                            If IO.File.Exists(lIconPath & lIconFilename) Then
                                Dim lTempIcon As String = IO.Path.Combine(lTempPath, lIconFilename)
                                If Not IO.File.Exists(lTempIcon) Then
                                    IO.File.Copy(lIconPath & lIconFilename, lTempIcon)
                                End If
                                lText &= "<img src=""" & lIconFilename & """>"
                            Else
                                lText &= lLog.logtype
                            End If
                            lText &= "<b>" & lLog.logdate.Replace("T19:00:00", "") & "</b> " & lLog.logfinder & ": " & lLog.logtext & "<br>"
                        Next
                        'Dim lLastTimePos As Integer = 0
                        'Dim lTimePos As Integer = .cache.logs.IndexOf()
                        'While lTimePos > 0
                        '    lText &= .cache.logs.Substring(lLastTimePos, lTimePos - lLastTimePos - 10) & "<br><b>" & .cache.logs.Substring(lTimePos - 10, 10) & "</b> "
                        '    lLastTimePos = lTimePos + 9
                        '    lTimePos = .cache.logs.IndexOf("T19:00:00", lTimePos + 10)
                        'End While
                        'lText &= .cache.logs.Substring(lLastTimePos + 9) & "<p>"
                    End If
                    If .cache.placed_by IsNot Nothing AndAlso .cache.placed_by.Length > 0 Then lText &= "<b>Placed by:</b> " & .cache.placed_by & "<br>"
                    If .cache.owner IsNot Nothing AndAlso .cache.owner.Length > 0 Then lText &= "<b>Owner:</b> " & .cache.owner & "<br>"
                    If .cache.travelbugs IsNot Nothing AndAlso .cache.travelbugs.Length > 0 Then lText &= "<b>Travellers:</b> " & .cache.travelbugs & "<br>"
                End If
                lText &= .extensionsString
                Dim lTempFilename As String = IO.Path.Combine(lTempPath, "cache.html")
                Dim lStream As IO.StreamWriter = IO.File.CreateText(lTempFilename)
                lStream.Write(lText)
                lStream.Write("<br><a href=""http://wap.geocaching.com/"">Official WAP</a><br>")
                lStream.Close()
                OpenFileOrURL(lTempFilename, False)
            End With
        End If
    End Sub

    Private Sub ManuallyNavigated()
        GPSCenterBehavior = 0 'Manual navigation overrides automatic following of GPS
        SanitizeCenterLatLon()
        NeedRedraw()
    End Sub

    Public Property GPSCenterBehavior() As Integer
        Get
            Return pGPSCenterBehavior
        End Get
        Set(ByVal value As Integer)
            'If pGPSCenterBehavior <> value Then
            pGPSCenterBehavior = value
            RaiseEvent CenterBehaviorChanged()
            'End If
        End Set
    End Property

#If Smartphone Then

#Region "Smartphone"
    Private GPS_Listen As Boolean = False
    Private WithEvents GPS As GPS_API.GPS
    Private GPS_DEVICE_STATE As GPS_API.GpsDeviceState
    Private GPS_POSITION As GPS_API.GpsPosition = Nothing

    <CLSCompliant(False)> _
    Public Event LocationChanged(ByVal aPosition As GPS_API.GpsPosition)

    Private pLastCellTower As clsCell
    Private pCellLocationProviders As New Generic.List(Of clsCellLocationProvider)

    ' Synchronize access to track log file
    Private Shared pTrackMutex As New Threading.Mutex()

    Private pTrackWaypoints As New System.Text.StringBuilder()

    ' File name of waypoints currently being written
    Private pWaypointLogFilename As String = Nothing

    ' File name of track log currently being written
    Private pTrackLogFilename As String

    ' When we last uploaded a track point (UTC)
    Private pUploadLastTime As DateTime

    ' True if we want to upload when GPS starts and have not yet done so
    Private pPendingUploadStart As Boolean = False

    ' When we last logged a track point (UTC)
    Private pTrackLastTime As DateTime
    Private pTrackLastLatitude As Double = 0.0  ' Degrees latitude.  North is positive
    Private pTrackLastLongitude As Double = 0.0 ' Degrees longitude.  East is positive

    Private pClickRefreshTile As Boolean = False ' Always false on startup, not saved to registry

    Private pCursorLayer As clsLayerGPX

    Private pKeepAwakeTimer As System.Threading.Timer

    Public Sub New()
        InitializeComponent()

        'Default to using these for mobile even though default to not in frmMapCommon
        ControlsUse = True
        ControlsShow = True

        pTrackLastTime = DateTime.UtcNow.Subtract(pTrackMinInterval)
        pUploadLastTime = New DateTime(1, 1, 1)

        updateDataHandler = New EventHandler(AddressOf UpdateData)

        pTileCacheFolder = GetAppSetting("TileCacheFolder", g_PathChar & "My Documents" & g_PathChar & "tiles" & g_PathChar)

        If pTileCacheFolder.Length > 0 AndAlso Not pTileCacheFolder.EndsWith(g_PathChar) Then
            pTileCacheFolder &= g_PathChar
        End If

        GPXFolder = IO.Path.GetDirectoryName(pTileCacheFolder)
        Try
            IO.Directory.CreateDirectory(pTileCacheFolder)
            IO.Directory.CreateDirectory(pTileCacheFolder & "WriteTest")
            IO.Directory.Delete(pTileCacheFolder & "WriteTest")
        Catch e As Exception
            pTileCacheFolder = IO.Path.Combine(IO.Path.GetTempPath, "tiles")
        End Try

        SharedNew(pTileCacheFolder)
        Uploader.Enabled = True

        pCellLocationProviders.Add(New clsCellLocationOpenCellID)
        pCellLocationProviders.Add(New clsCellLocationGoogle)

        pCursorLayer = New clsLayerGPX("cursor", Me)
        pCursorLayer.SymbolPen = New Pen(Color.Red, 2) 'TODO: make width user-configurable, allow drawing icon as cursor instead of arrow
        pCursorLayer.SymbolSize = GPSSymbolSize
    End Sub

    Public Property Active() As Boolean
        Get
            Return pActive
        End Get
        Set(ByVal value As Boolean)
            pActive = value
            If pActive Then Me.NeedRedraw()
        End Set
    End Property

    Private Sub Redraw()
        If pRedrawing Then
            pRedrawPending = True
        Else
            pRedrawing = True
            Dim lGraphics As Graphics = Nothing
            Dim lDetailsBrush As Brush = pBrushBlack

RestartRedraw:
            If Active Then
                lGraphics = GetBitmapGraphics()
                If lGraphics IsNot Nothing Then
                    If Dark Then
                        lGraphics.Clear(Color.Black)
                        lDetailsBrush = pBrushWhite
                    Else
                        DrawTiles(lGraphics)
                    End If
                    If pShowGPSdetails Then
                        Dim lMaxWidth As Single = lGraphics.ClipBounds.Right - 10
                        Dim lDetails As String = ""
                        If GPS Is Nothing Then
                            lDetails = "GPS not initialized"
                        ElseIf Not GPS.Opened Then
                            lDetails = "GPS not opened"
                        ElseIf GPS_POSITION Is Nothing Then
                            lDetails = "No position"
                        ElseIf Not GPS_POSITION.LatitudeValid OrElse Not GPS_POSITION.LongitudeValid Then
                            lDetails = "Position not valid"
                        Else
                            lDetails = FormattedDegrees(GPS_POSITION.Latitude, g_DegreeFormat) & ","
                            AppendStringSplitToFitWidth(lGraphics, pFontGpsDetails, lMaxWidth, lDetails, FormattedDegrees(GPS_POSITION.Longitude, g_DegreeFormat))
                            If (GPS_POSITION.SeaLevelAltitudeValid) Then AppendStringSplitToFitWidth(lGraphics, pFontGpsDetails, lMaxWidth, lDetails, Format(GPS_POSITION.SeaLevelAltitude * g_FeetPerMeter, "#,##0") & "ft")
                            If (GPS_POSITION.SpeedValid AndAlso GPS_POSITION.Speed > 0) Then AppendStringSplitToFitWidth(lGraphics, pFontGpsDetails, lMaxWidth, lDetails, CInt(GPS_POSITION.Speed * g_MilesPerKnot) & "mph")
                            If GPS_POSITION.TimeValid Then
                                lDetails &= vbLf & GPS_POSITION.Time.ToString("yyyy-MM-dd")
                                AppendStringSplitToFitWidth(lGraphics, pFontGpsDetails, lMaxWidth, lDetails, GPS_POSITION.Time.ToString("HH:mm:ss") & "Z")
                                Dim lAgeOfPosition As TimeSpan = DateTime.UtcNow - GPS_POSITION.Time
                                Dim lAgeString As String = ""
                                If (Math.Abs(lAgeOfPosition.TotalSeconds) > 5) Then
                                    If Math.Abs(lAgeOfPosition.Days) > 1 Then
                                        lAgeString = Format(lAgeOfPosition.TotalDays, "0.#") & " days"
                                    Else
                                        If Math.Abs(lAgeOfPosition.Days) > 0 Then lAgeString = lAgeOfPosition.Days & "day"
                                        If Math.Abs(lAgeOfPosition.Hours) > 0 Then lAgeString &= lAgeOfPosition.Hours & "h"
                                        If Math.Abs(lAgeOfPosition.Minutes) > 0 Then lAgeString &= lAgeOfPosition.Minutes & "m"
                                        If lAgeOfPosition.Hours = 0 Then lAgeString &= lAgeOfPosition.Seconds & "s"
                                    End If
                                    AppendStringSplitToFitWidth(lGraphics, pFontGpsDetails, lMaxWidth, lDetails, "(" & lAgeString & " ago)")
                                End If
                            End If
                        End If
                        If pRecordCellID Then
                            Dim lCurrentCellInfo As New clsCell(GPS_API.RIL.GetCellTowerInfo)
                            If lCurrentCellInfo.IsValid Then
                                lDetails &= vbLf & lCurrentCellInfo.ToString
                            Else
                                lDetails &= vbLf & "no cell"
                            End If
                        End If
                        'If pUsedCacheCount + pAddedCacheCount > 0 Then
                        '    lDetails &= " Cache " & Format(pUsedCacheCount / (pUsedCacheCount + pAddedCacheCount), "0.0 %") & " " & pDownloader.TileRAMcacheLimit
                        'End If
                        If Not RecordTrack Then lDetails &= vbLf & "Logging Off"
                        lGraphics.DrawString(lDetails, pFontGpsDetails, lDetailsBrush, 5, 5)
                    End If
                    ReleaseBitmapGraphics()
                    Refresh()
                    pLastRedrawTime = Now
                End If
            End If
            Application.DoEvents()
            If pRedrawPending Then
                pRedrawPending = False
                GoTo RestartRedraw
            End If
            pRedrawing = False
        End If
    End Sub

    Private Sub AppendStringSplitToFitWidth(ByVal aGraphics As Graphics, _
        ByVal aFont As Font, ByVal lMaxWidth As Single, _
        ByRef aString As String, ByVal aAppend As String)
        Dim lLastNewline As Integer = aString.LastIndexOf(vbLf)
        Dim lLastLine As String = aString.Substring(lLastNewline + 1)
        Dim lWidth As Single = aGraphics.MeasureString(lLastLine & " " & aAppend, aFont).Width
        If lWidth > lMaxWidth Then
            aString &= vbLf & aAppend
        Else
            aString &= " " & aAppend
        End If
    End Sub

    Private Function EnsureCurrentTrack() As clsLayerGPX
        If Layers.Count = 0 OrElse Layers(0).Filename <> "current" Then
            Dim lCurrentTrack As New clsLayerGPX("current", Me)
            lCurrentTrack.Filename = "current"
            With lCurrentTrack.GPX
                .trk = New Generic.List(Of clsGPXtrack)
                .trk.Add(New clsGPXtrack("current"))
                .trk(0).trkseg.Add(New clsGPXtracksegment())
            End With
            Layers.Insert(0, lCurrentTrack)
        End If
        Return Layers(0)
    End Function

    Private Function MapRectangle() As Rectangle
        Return ClientRectangle
    End Function

    Private Sub DrewTiles(ByVal g As Graphics, ByVal aTopLeft As Point, ByVal aOffsetToCenter As Point)
        If GPS IsNot Nothing AndAlso GPS.Opened AndAlso GPS_POSITION IsNot Nothing AndAlso GPS_POSITION.LatitudeValid AndAlso GPS_POSITION.LongitudeValid Then
            If GPSSymbolSize > 0 Then
                pCursorLayer.SymbolSize = GPSSymbolSize
                Dim lWaypoint As New clsGPXwaypoint("wpt", GPS_POSITION.Latitude, GPS_POSITION.Longitude)
                lWaypoint.sym = "cursor"
                lWaypoint.course = GPS_POSITION.Heading
                pCursorLayer.DrawTrackpoint(TileServer, g, lWaypoint, aTopLeft, aOffsetToCenter, -1, -1)
            End If
        End If
    End Sub

    Private Sub Event_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseDown
        Select Case e.Button
            Case Windows.Forms.MouseButtons.Left
                If pClickRefreshTile Then
                    Dim lBounds As New RectangleF(0, 0, pBitmap.Width, pBitmap.Height)
                    ClickedTileFilename = FindTileFilename(TileServer, lBounds, e.X, e.Y)
                    Downloader.DeleteTile(ClickedTileFilename)
                    NeedRedraw()
                ElseIf pClickWaypoint Then
                    AddWaypoint(CenterLat - (e.Y - Me.ClientRectangle.Height / 2) * LatHeight / Me.ClientRectangle.Height, _
                                CenterLon + (e.X - Me.ClientRectangle.Width / 2) * LonWidth / Me.ClientRectangle.Width, _
                                Date.UtcNow, Now.ToString("yyyy-MM-dd HH:mm:ss"))
                Else
                    MouseDownLeft(e)
                End If
        End Select
    End Sub

    Private Sub Event_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseMove
        If MouseDragging Then
            If Not ControlsUse OrElse _
               (e.X > pControlsMargin AndAlso e.X < (Me.Width - pControlsMargin)) OrElse _
               (e.Y > pControlsMargin AndAlso e.Y < (Me.Height - pControlsMargin)) OrElse _
               Math.Abs(MouseDragStartLocation.X - e.X) > pControlsMargin / 2 OrElse _
               Math.Abs(MouseDragStartLocation.Y - e.Y) > pControlsMargin / 2 Then

                CenterLat = MouseDownLat + (e.Y - MouseDragStartLocation.Y) * LatHeight / Me.ClientRectangle.Height
                CenterLon = MouseDownLon - (e.X - MouseDragStartLocation.X) * LonWidth / Me.ClientRectangle.Width
                ManuallyNavigated()
            End If
        End If
    End Sub

    Public Sub StartGPS()
        Application.DoEvents()
        If pGPSCenterBehavior = 0 Then
            GPSCenterBehavior = 2 'Center at GPS location when starting GPS
        End If
        If GPS Is Nothing Then GPS = New GPS_API.GPS
        If Not GPS.Opened Then
            If Not Dark Then StartKeepAwake()

            SetPowerRequirement(DeviceGPS, True)

            NewLogFilename()

            Application.DoEvents()
            GPS.Open()
            GPS_Listen = True
            pPendingUploadStart = UploadOnStart
        End If
    End Sub

    Private Sub NewLogFilename(Optional ByVal aNeedMutex As Boolean = True)
        If aNeedMutex Then pTrackMutex.WaitOne()
        CloseLog(pWaypointLogFilename, "</gpx>" & vbLf)
        CloseLog(pTrackLogFilename, "</trkseg>" & vbLf & "</trk>" & vbLf & "</gpx>" & vbLf)
        Dim lBaseFilename As String = IO.Path.Combine(GPXFolder, DateTime.Now.ToString("yyyy-MM-dd_HH-mm"))
        pTrackLogFilename = lBaseFilename & ".gpx"

        Dim lLogIndex As Integer = 1
        While System.IO.File.Exists(pTrackLogFilename)
            pTrackLogFilename = lBaseFilename & "_" & ++lLogIndex & ".gpx"
        End While
        pWaypointLogFilename = IO.Path.ChangeExtension(pTrackLogFilename, ".wpt.gpx")
        If aNeedMutex Then pTrackMutex.ReleaseMutex()
    End Sub

    Public Sub StopGPS()
        GPS_Listen = False
        Threading.Thread.Sleep(100)
        Me.SaveSettings() 'In case of crash, at least we can start again with the same settings

        'Try
        '    mnuStartStopGPS.Text = "Finishing Log..."
        '    Application.DoEvents()
        'Catch
        'End Try

        pTrackMutex.WaitOne()
        CloseLog(pWaypointLogFilename, "</gpx>" & vbLf)
        CloseLog(pTrackLogFilename, "</trkseg>" & vbLf & "</trk>" & vbLf & "</gpx>" & vbLf)
        pTrackMutex.ReleaseMutex()

        If GPS IsNot Nothing Then
            If GPS.Opened Then
                'Try
                '    mnuStartStopGPS.Text = "Closing GPS..."
                '    Application.DoEvents()
                'Catch
                'End Try
                If UploadOnStop Then UploadGpsPosition()
                GPS.Close()
            End If
            GPS = Nothing
        End If
        SetPowerRequirement(DeviceGPS, False)
        StopKeepAwake()
    End Sub

    ''' <summary>
    ''' Close a GPX file
    ''' </summary>
    ''' <param name="aFilename">File to close</param>
    ''' <param name="aAppend">Closing tags to append at end of file</param>
    ''' <remarks>Only call between pTrackMutex.WaitOne and pTrackMutex.ReleaseMutex</remarks>
    Private Sub CloseLog(ByRef aFilename As String, ByVal aAppend As String)
        Try
            If IO.File.Exists(aFilename) Then
                If aAppend IsNot Nothing AndAlso aAppend.Length > 0 Then
                    Dim lFile As System.IO.StreamWriter = System.IO.File.AppendText(aFilename)
                    lFile.Write(aAppend)
                    lFile.Close()
                End If
                If UploadTrackOnStop AndAlso Uploader.Enabled Then
                    Uploader.Enqueue(g_UploadTrackURL, aFilename, QueueItemType.FileItem, 0)
                End If
            End If
            aFilename = ""
        Catch ex As System.IO.IOException
        End Try
    End Sub

    Private updateDataHandler As EventHandler

    Private Sub GPS_DEVICE_LocationChanged(ByVal sender As Object, ByVal args As GPS_API.LocationChangedEventArgs) Handles GPS.LocationChanged
        If GPS_Listen Then
            GPS_POSITION = args.Position
            Invoke(updateDataHandler)
        End If
    End Sub

    Private Sub UpdateData(ByVal sender As Object, ByVal args As System.EventArgs)
        Try
            If (GPS.Opened) Then
                Try
                    RaiseEvent LocationChanged(GPS_POSITION)
                Catch
                End Try
                If (GPS_POSITION IsNot Nothing AndAlso GPS_POSITION.LatitudeValid AndAlso GPS_POSITION.LongitudeValid) Then
                    Dim lNeedRedraw As Boolean = SetCenterFromDevice(GPS_POSITION.Latitude, GPS_POSITION.Longitude, pGPSCenterBehavior)

                    If GPS_Listen AndAlso (RecordTrack OrElse pDisplayTrack) AndAlso DateTime.UtcNow.Subtract(pTrackMinInterval) >= pTrackLastTime Then
                        TrackAddPoint()
                        pTrackLastTime = DateTime.UtcNow
                    End If

                    If GPSSymbolSize > 0 OrElse lNeedRedraw Then
                        NeedRedraw()
                    End If

                    If pPendingUploadStart OrElse (UploadPeriodic AndAlso DateTime.UtcNow.Subtract(UploadMinInterval) >= pUploadLastTime) Then
                        UploadGpsPosition()
                    End If
                End If
            End If
        Catch e As Exception
            'MsgBox(e.Message & vbLf & e.StackTrace)
        End Try
    End Sub

    Private Function SetCenterFromCellLocation() As Boolean
        Dim lCurrentCellInfo As New clsCell(GPS_API.RIL.GetCellTowerInfo)
        If lCurrentCellInfo.IsValid AndAlso (pLastCellTower Is Nothing OrElse lCurrentCellInfo.ID <> pLastCellTower.ID) Then
            pLastCellTower = lCurrentCellInfo
            With lCurrentCellInfo
                If GetCellLocation(pTileCacheFolder & "cells", lCurrentCellInfo) Then
                    If SetCenterFromDevice(lCurrentCellInfo.Latitude, lCurrentCellInfo.Longitude, pGPSCenterBehavior) Then
                        Me.Invoke(pRedrawCallback)
                        Return True
                    End If
                End If
            End With
        End If
        Return False
    End Function

    Private Function GetCellLocation(ByVal aCellCacheFolder As String, ByVal aCell As clsCell) As Boolean
        'Check for a cached location first
        For Each lLocationProvider As clsCellLocationProvider In pCellLocationProviders
            If lLocationProvider.GetCachedLocation(aCellCacheFolder, aCell) Then
                Return True
            End If
        Next

        'Query for a new location and cache if found
        For Each lLocationProvider As clsCellLocationProvider In pCellLocationProviders
            If lLocationProvider.GetCellLocation(aCell) Then
                lLocationProvider.SaveCachedLocation(aCellCacheFolder, aCell)
                Return True
            End If
        Next
        Return False
    End Function

    Private Sub TrackAddPoint()
        Dim lTrackPoint As clsGPXwaypoint = LatestPositionWaypoint("trkpt")
        If lTrackPoint IsNot Nothing Then
            ' If subsequent track points vary this much, we don't believe it, don't log till they are closer
            If Math.Abs((GPS_POSITION.Latitude - pTrackLastLatitude)) + Math.Abs((GPS_POSITION.Longitude - pTrackLastLongitude)) < 0.5 Then
                If RecordTrack Then
                    AppendLog(pTrackLogFilename, lTrackPoint.ToString, "<?xml version=""1.0"" encoding=""UTF-8""?>" & vbLf & "<gpx xmlns=""http://www.topografix.com/GPX/1/1"" version=""1.1"" creator=""" & g_AppName & """ xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd http://www.topografix.com/GPX/gpx_overlay/0/3 http://www.topografix.com/GPX/gpx_overlay/0/3/gpx_overlay.xsd http://www.topografix.com/GPX/gpx_modified/0/1 http://www.topografix.com/GPX/gpx_modified/0/1/gpx_modified.xsd"">" & vbLf & "<trk>" & vbLf & "<name>" & g_AppName & " Log " & System.IO.Path.GetFileNameWithoutExtension(pTrackLogFilename) & " </name>" & vbLf & "<type>GPS Tracklog</type>" & vbLf & "<trkseg>" & vbLf)
                End If
                pTrackLastTime = DateTime.UtcNow

                If pDisplayTrack Then
                    EnsureCurrentTrack.GPX.trk(0).trkseg(0).trkpt.Add(lTrackPoint)
                End If
            End If
            pTrackLastLatitude = GPS_POSITION.Latitude
            pTrackLastLongitude = GPS_POSITION.Longitude
        End If
    End Sub 'TrackAddPoint

    ''' <summary>
    ''' Create a GPX waypoint for the latest GPS position
    ''' </summary>
    ''' <param name="aTag">trkpt or wpt depending on whether this is part of a track or an independent waypoint</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function LatestPositionWaypoint(ByVal aTag As String) As clsGPXwaypoint
        Dim lTrackPoint As clsGPXwaypoint = Nothing
        With GPS_POSITION
            If GPS_POSITION IsNot Nothing _
               AndAlso .TimeValid _
               AndAlso .LatitudeValid _
               AndAlso .LongitudeValid _
               AndAlso .SatellitesInSolutionValid _
               AndAlso .SatelliteCount > 2 Then
                Dim lGPXheader As String = Nothing

                lTrackPoint = New clsGPXwaypoint(aTag, GPS_POSITION.Latitude, GPS_POSITION.Longitude)

                If GPS_POSITION.SeaLevelAltitudeValid Then lTrackPoint.ele = GPS_POSITION.SeaLevelAltitude
                lTrackPoint.time = GPS_POSITION.Time
                lTrackPoint.sat = GPS_POSITION.SatelliteCount

                If GPS_POSITION.SpeedValid AndAlso GPS_POSITION.Speed > 0.01 Then lTrackPoint.speed = GPS_POSITION.Speed
                If GPS_POSITION.HeadingValid Then lTrackPoint.course = GPS_POSITION.Heading

                If pRecordCellID Then
                    Dim lCurrentCellInfo As New clsCell(GPS_API.RIL.GetCellTowerInfo)
                    If lCurrentCellInfo.IsValid Then
                        lTrackPoint.SetExtension("cellid", lCurrentCellInfo.ToString)
                        Dim lSignalStrength As Integer = Microsoft.WindowsMobile.Status.SystemState.PhoneSignalStrength
                        If lSignalStrength > 0 Then
                            lTrackPoint.SetExtension("phonesignal", lSignalStrength)
                        End If
                    End If
                End If
            End If
        End With
        Return lTrackPoint
    End Function

    Private Sub AppendLog(ByVal aFilename As String, ByVal aAppend As String, ByVal aHeader As String)
        pTrackMutex.WaitOne()
        Dim lTries As Integer = 0
TryAgain:
        Try
            Dim lNeedHeader As Boolean = Not IO.File.Exists(aFilename)
            If lNeedHeader Then IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(aFilename))
            Dim lFile As System.IO.StreamWriter = System.IO.File.AppendText(aFilename)
            If lNeedHeader AndAlso aHeader IsNot Nothing Then lFile.Write(aHeader)
            lFile.Write(aAppend)
            lFile.Close()
        Catch ex As Exception
            'Windows.Forms.MessageBox.Show("Could not save " & vbLf & pTrackLogFilename & vbLf & ex.Message)
            If lTries < 1 Then
                lTries += 1
                NewLogFilename()
                GoTo TryAgain
            End If
        End Try
        pTrackMutex.ReleaseMutex()
    End Sub

    Public Sub UploadGpsPosition()
        If Uploader.Enabled _
           AndAlso GPS_POSITION IsNot Nothing _
           AndAlso GPS_POSITION.TimeValid _
           AndAlso GPS_POSITION.LatitudeValid _
           AndAlso GPS_POSITION.LongitudeValid _
           AndAlso GPS_POSITION.SatellitesInSolutionValid _
           AndAlso GPS_POSITION.SatelliteCount > 2 Then
            Try
                Dim lURL As String = g_UploadPointURL
                If lURL IsNot Nothing AndAlso lURL.Length > 0 Then
                    BuildURL(lURL, "Time", timeZ(GPS_POSITION.Time), "", GPS_POSITION.TimeValid)
                    BuildURL(lURL, "Lat", GPS_POSITION.Latitude.ToString("#.########"), "", GPS_POSITION.LatitudeValid)
                    BuildURL(lURL, "Lon", GPS_POSITION.Longitude.ToString("#.########"), "", GPS_POSITION.LongitudeValid)

                    BuildURL(lURL, "Alt", GPS_POSITION.SeaLevelAltitude, "", GPS_POSITION.SeaLevelAltitudeValid)
                    BuildURL(lURL, "Speed", GPS_POSITION.Speed, "", GPS_POSITION.SpeedValid)
                    BuildURL(lURL, "Heading", GPS_POSITION.Heading, "", GPS_POSITION.HeadingValid)
                    If pPendingUploadStart Then
                        BuildURL(lURL, "Label", g_AppName & "-Start", "", True)
                    Else
                        BuildURL(lURL, "Label", g_AppName, "", True)
                    End If
                    If lURL.IndexOf("CellID") > -1 Then
                        Dim lCurrentCellInfo As New clsCell(GPS_API.RIL.GetCellTowerInfo)
                        BuildURL(lURL, "CellID", lCurrentCellInfo.ToString, "", True)
                    End If

                    Uploader.ClearQueue(0)
                    Uploader.Enqueue(lURL, "", QueueItemType.PointItem, 0)
                    pUploadLastTime = DateTime.UtcNow
                    pPendingUploadStart = False
                End If
            Catch
            End Try
        End If
    End Sub 'UploadPoint

    Private Sub BuildURL(ByRef aURL As String, ByVal aTag As String, ByVal aReplaceTrue As String, ByVal aReplaceFalse As String, ByVal aTest As Boolean)
        Dim lReplacement As String
        If aTest Then
            lReplacement = aReplaceTrue
        Else
            lReplacement = aReplaceFalse
        End If
        aURL = aURL.Replace("#" & aTag & "#", lReplacement)
    End Sub

    Public Property DisplayTrack() As Boolean
        Get
            Return pDisplayTrack
        End Get
        Set(ByVal value As Boolean)
            pDisplayTrack = value
            If Layers.Count > 0 AndAlso Layers(0).Filename = "current" Then
                Layers.RemoveAt(0)
            End If
            If pDisplayTrack Then
                pTrackMutex.WaitOne()
                If System.IO.File.Exists(pTrackLogFilename) Then
                    Dim lLayer As clsLayerGPX = OpenGPX(pTrackLastTime, 0)
                    If lLayer IsNot Nothing Then
                        lLayer.SymbolSize = TrackSymbolSize
                    End If
                End If
                pTrackMutex.ReleaseMutex()
            End If
        End Set
    End Property

    Public Property ShowGPSdetails() As Boolean
        Get
            Return pShowGPSdetails
        End Get
        Set(ByVal value As Boolean)
            pShowGPSdetails = value
            NeedRedraw()
        End Set
    End Property

    Public Property ViewTileOutlines() As Boolean
        Get
            Return pShowTileOutlines
        End Get
        Set(ByVal value As Boolean)
            pShowTileOutlines = value
            NeedRedraw()
        End Set
    End Property

    Public Property ViewTileNames() As Boolean
        Get
            Return pShowTileNames
        End Get
        Set(ByVal value As Boolean)
            pShowTileNames = value
            NeedRedraw()
        End Set
    End Property

    Public Property ClickRefreshTile() As Boolean
        Get
            Return pClickRefreshTile
        End Get
        Set(ByVal value As Boolean)
            pClickRefreshTile = value
        End Set
    End Property

    ''' <summary>
    ''' Start timer that keeps system idle from turning off device
    ''' </summary>
    Private Sub StartKeepAwake()
        SystemIdleTimerReset()
        If pKeepAwakeTimer Is Nothing Then
            pKeepAwakeTimer = New System.Threading.Timer(New Threading.TimerCallback(AddressOf IdleTimeout), Nothing, 0, 50000)
        End If
    End Sub

    ''' <summary>
    ''' Stop timer that keeps system idle from turning off device
    ''' </summary>
    Private Sub StopKeepAwake()
        If pKeepAwakeTimer IsNot Nothing Then
            pKeepAwakeTimer.Dispose()
            pKeepAwakeTimer = Nothing
        End If
    End Sub

    ''' <summary>
    ''' Keep the device awake, also move map to a cell tower location if desired 
    ''' </summary>
    ''' <remarks>Periodically called by pKeepAwakeTimer</remarks>
    Private Sub IdleTimeout(ByVal o As Object)
        SystemIdleTimerReset()

        'If the user requested some centering from GPS
        'and the GPS has not given us a good location within the last minute
        'then try to center on a cell tower location
        If pGPSCenterBehavior > 0 _
            AndAlso (Not GPS_Listen _
                     OrElse GPS_POSITION Is Nothing _
                     OrElse Not GPS_POSITION.TimeValid _
                     OrElse Date.UtcNow.Subtract(GPS_POSITION.Time).TotalMinutes > 1) Then
            SetCenterFromCellLocation()
        End If
    End Sub

    Public Property AutoStart() As Boolean
        Get
            Return pGPSAutoStart
        End Get
        Set(ByVal value As Boolean)
            pGPSAutoStart = value
        End Set
    End Property

    Public Property ViewMapTiles() As Boolean
        Get
            Return pShowTileImages
        End Get
        Set(ByVal value As Boolean)
            pShowTileImages = value
            NeedRedraw()
        End Set
    End Property

    Public Property ViewControls() As Boolean
        Get
            Return ControlsShow
        End Get
        Set(ByVal value As Boolean)
            ControlsShow = value
            ControlsUse = value
            NeedRedraw()
        End Set
    End Property

    Public Property ClickMakeWaypoint() As Boolean
        Get
            Return pClickWaypoint
        End Get
        Set(ByVal value As Boolean)
            pClickWaypoint = value
        End Set
    End Property

    Private Sub AddWaypoint(ByVal aLatitude As Double, ByVal aLongitude As Double, ByVal aTime As Date, ByVal aName As String)
        Dim lWayPoint As New clsGPXwaypoint("wpt", aLatitude, aLongitude)
        lWayPoint.time = aTime
        lWayPoint.name = aName
        pTrackMutex.WaitOne()
        If pWaypointLogFilename Is Nothing OrElse pWaypointLogFilename.Length = 0 Then
            pWaypointLogFilename = IO.Path.Combine(GPXFolder, DateTime.Now.ToString("yyyy-MM-dd_HH-mm")) & ".wpt.gpx"
        End If
        pTrackMutex.ReleaseMutex()
        AppendLog(pWaypointLogFilename, lWayPoint.ToString, "<?xml version=""1.0"" encoding=""UTF-8""?>" & vbLf & "<gpx xmlns=""http://www.topografix.com/GPX/1/1"" version=""1.1"" creator=""" & g_AppName & """ xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.topografix.com/GPX/1/1 http://www.topografix.com/GPX/1/1/gpx.xsd http://www.topografix.com/GPX/gpx_overlay/0/3 http://www.topografix.com/GPX/gpx_overlay/0/3/gpx_overlay.xsd http://www.topografix.com/GPX/gpx_modified/0/1 http://www.topografix.com/GPX/gpx_modified/0/1/gpx_modified.xsd"">" & vbLf)
    End Sub
#End Region 'Smartphone

#Else

#Region "Desktop"
    Friend WithEvents RightClickMenu As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents RefreshFromServerToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ClosestGeocacheToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents TileCacheFolderToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem

    Public Sub CopyToClipboard()
        pBitmapMutex.WaitOne()
        My.Computer.Clipboard.SetImage(pBitmap)
        pBitmapMutex.ReleaseMutex()
    End Sub

    Private Function ImageWorldFilename(ByVal aImageFilename As String) As String
        Dim lImageExt As String = IO.Path.GetExtension(aImageFilename)
        If lImageExt.Length > 1 Then
            lImageExt = lImageExt.Substring(1, 1) & lImageExt.Substring(lImageExt.Length - 1, 1) & "w"
        End If
        Return IO.Path.ChangeExtension(aImageFilename, lImageExt)
    End Function

    Public Sub SaveImageAs(ByVal aFilename As String)
        Dim lImageFormat As Imaging.ImageFormat = Imaging.ImageFormat.Png
        Select Case IO.Path.GetExtension(aFilename).ToLower
            Case ".bmp" : lImageFormat = Imaging.ImageFormat.Bmp
            Case ".emf" : lImageFormat = Imaging.ImageFormat.Emf
            Case ".gif" : lImageFormat = Imaging.ImageFormat.Gif
            Case ".ico" : lImageFormat = Imaging.ImageFormat.Icon
            Case ".jpg", ".jpeg" : lImageFormat = Imaging.ImageFormat.Jpeg
            Case ".tif", ".tiff" : lImageFormat = Imaging.ImageFormat.Tiff
            Case ".wmf" : lImageFormat = Imaging.ImageFormat.Wmf
        End Select
        pBitmapMutex.WaitOne()
        pBitmap.Save(aFilename, lImageFormat)
        'SaveTiles(IO.Path.GetDirectoryName(.FileName) & g_PathChar & IO.Path.GetFileNameWithoutExtension(.FileName) & g_PathChar)
        pBitmapMutex.ReleaseMutex()
        'CreateGeoReferenceFile(LatitudeToMeters(pCenterLat - pLatHeight * 1.66), _
        CreateGeoReferenceFile(LatitudeToMeters(LatMax), _
                               LongitudeToMeters(LonMin), _
                               pZoom, ImageWorldFilename(aFilename))
        IO.File.WriteAllText(IO.Path.ChangeExtension(aFilename, "prj"), g_TileProjection)
        'IO.File.WriteAllText(IO.Path.ChangeExtension(.FileName, "prj"), "PROJCS[""unnamed"", GEOGCS[""unnamed ellipse"", DATUM[""unknown"", SPHEROID[""unnamed"",6378137,0]], PRIMEM[""Greenwich"",0], UNIT[""degree"",0.0174532925199433]], PROJECTION[""Mercator_2SP""], PARAMETER[""standard_parallel_1"",0], PARAMETER[""central_meridian"",0], PARAMETER[""false_easting"",0], PARAMETER[""false_northing"",0], UNIT[""Meter"",1], EXTENSION[""PROJ4"",""+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +k=1.0 +units=m +nadgrids=@null +wktext +no_defs""]]")
        ''IO.File.WriteAllText(IO.Path.ChangeExtension(.FileName, "prj"), "PROJCS[""Mercator"",GEOGCS[""unnamed ellipse"",DATUM[""D_unknown"",SPHEROID[""Unknown"",6371000,0]],PRIMEM[""Greenwich"",0],UNIT[""Degree"",0.017453292519943295]],PROJECTION[""Mercator""],PARAMETER[""standard_parallel_1"",0],PARAMETER[""central_meridian"",0],PARAMETER[""scale_factor"",1],PARAMETER[""false_easting"",0],PARAMETER[""false_northing"",0],UNIT[""Meter"",1]]")
        ''IO.File.WriteAllText(IO.Path.ChangeExtension(.FileName, "prj2"), "PROJCS[""Mercator Spheric"", GEOGCS[""WGS84basedSpheric_GCS"", DATUM[""WGS84basedSpheric_Datum"", SPHEROID[""WGS84based_Sphere"", 6378137, 0], TOWGS84[0, 0, 0, 0, 0, 0, 0]], PRIMEM[""Greenwich"", 0, AUTHORITY[""EPSG"", ""8901""]], UNIT[""degree"", 0.0174532925199433, AUTHORITY[""EPSG"", ""9102""]], AXIS[""E"", EAST], AXIS[""N"", NORTH]], PROJECTION[""Mercator""], PARAMETER[""False_Easting"", 0], PARAMETER[""False_Northing"", 0], PARAMETER[""Central_Meridian"", 0], PARAMETER[""Latitude_of_origin"", 0], UNIT[""metre"", 1, AUTHORITY[""EPSG"", ""9001""]], AXIS[""East"", EAST], AXIS[""North"", NORTH]]")
        ''IO.File.WriteAllText(IO.Path.ChangeExtension(.FileName, "prj3"), "PROJCS[""WGS84 / Simple Mercator"",GEOGCS[""WGS 84"",DATUM[""WGS_1984"",SPHEROID[""WGS_1984"", 6378137.0, 298.257223563]],PRIMEM[""Greenwich"", 0.0],UNIT[""degree"", 0.017453292519943295],AXIS[""Longitude"", EAST],AXIS[""Latitude"", NORTH]],PROJECTION[""Mercator_1SP_Google""],PARAMETER[""latitude_of_origin"", 0.0],PARAMETER[""central_meridian"", 0.0],PARAMETER[""scale_factor"", 1.0],PARAMETER[""false_easting"", 0.0],PARAMETER[""false_northing"", 0.0],UNIT[""m"", 1.0],AXIS[""x"", EAST],AXIS[""y"", NORTH],AUTHORITY[""EPSG"",""900913""]]")
        ''IO.File.WriteAllText(IO.Path.ChangeExtension(.FileName, "prj4"), "PROJCS[""Google Mercator"",GEOGCS[""WGS 84"",DATUM[""WGS_1984"",SPHEROID[""WGS84"",6378137,298.2572235630016,AUTHORITY[""EPSG"",""7030""]],AUTHORITY[""EPSG"",""6326""]],PRIMEM[""Greenwich"",0],UNIT[""degree"",0.0174532925199433],AUTHORITY[""EPSG"",""4326""]],PROJECTION[""Mercator_1SP""],PARAMETER[""central_meridian"",0],PARAMETER[""scale_factor"",1],PARAMETER[""false_easting"",0],PARAMETER[""false_northing"",0],UNIT[""metre"",1,AUTHORITY[""EPSG"",""9001""]]]")

        ''+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +k=1.0 +units=m +nadgrids=@null +no_defs
        ''+proj=merc +a=6378137 +b=6378137 +lat_ts=0.0 +lon_0=0.0 +x_0=0.0 +y_0=0 +k=1.0 +units=m +nadgrids=@null +no_defs +over
    End Sub

    Public Function CreateGeoReferenceFile(ByVal aNorthEdge As Double, _
                                           ByVal aWestEdge As Double, _
                                           ByVal aZoom As Integer, _
                                           ByVal aSaveAsFileName As String) As Boolean
        'http://en.wikipedia.org/wiki/World_file
        'http://support.esri.com/index.cfm?fa=knowledgebase.techarticles.articleShow&d=17489
        ' and thanks to Bostjan for the idea

        Dim lFormat As String = "0.00000000000000000"
        Dim lMetersPerPixel As Double = MetersPerPixel(aZoom, TileServer.TileSize)
        Dim lFileWriter As New IO.StreamWriter(aSaveAsFileName)
        If lFileWriter IsNot Nothing Then
            With lFileWriter
                .WriteLine(Format(lMetersPerPixel, lFormat)) ' size of pixel in x direction
                .WriteLine(lFormat)
                .WriteLine(lFormat)
                .WriteLine(Format(-lMetersPerPixel, lFormat)) ' size of pixel in y direction (same x to be square, but negative)
                .WriteLine(Format(aWestEdge, lFormat))
                .WriteLine(Format(aNorthEdge, lFormat))
                .Close()
                Return True
            End With
        End If
        Return False
    End Function

    Private Sub DrewTiles(ByVal g As Graphics, ByVal aTopLeft As Point, ByVal aOffsetToCenter As Point)
        'TODO:
        '    If pBuddyAlarmEnabled OrElse pBuddyAlarmForm IsNot Nothing Then
        '        Dim lWaypoint As New clsGPXwaypoint("wpt", pBuddyAlarmLat, pBuddyAlarmLon)
        '        lWaypoint.sym = "circle"
        '        Dim lBuddyAlarmLayer As clsLayerGPX = New clsLayerGPX("cursor", Me)
        '        lBuddyAlarmLayer.SymbolPen = New Pen(Color.Red)
        '        lBuddyAlarmLayer.SymbolSize = pBuddyAlarmMeters / MetersPerPixel(pZoom)
        '        lBuddyAlarmLayer.DrawTrackpoint(g, lWaypoint, aTopLeft, aOffsetToCenter, -1, -1)
        '    End If
    End Sub

    Private Function MapRectangle() As Rectangle
        'Dim lMenuHeight As Integer = 27
        'If Me.MainMenuStrip IsNot Nothing Then lMenuHeight = MainMenuStrip.Height
        'With ClientRectangle
        'Return New Rectangle(.X, .Y + lMenuHeight, .Width, .Height - lMenuHeight)
        'End With
        Return ClientRectangle
    End Function

    Public Sub Redraw()
        'If Me.Visible Then 'TODO: AndAlso WindowState <> FormWindowState.Minimized Then
        Dim lGraphics As Graphics = GetBitmapGraphics()
        If lGraphics IsNot Nothing Then
            DrawTiles(lGraphics)
            ReleaseBitmapGraphics()
            Refresh()
            pLastRedrawTime = Now
            'TODO: If pCoordinatesForm IsNot Nothing Then pCoordinatesForm.Show(Me)
        End If
        Application.DoEvents()
        'End If
    End Sub

    Private Sub Event_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseDown
        'If pBuddyAlarmForm IsNot Nothing Then
        '    pBuddyAlarmLat = CenterLat - (e.Y - ClientRectangle.Height / 2) * LatHeight / ClientRectangle.Height
        '    pBuddyAlarmLon = CenterLon + (e.X - ClientRectangle.Width / 2) * LonWidth / ClientRectangle.Width
        '    pBuddyAlarmForm.AskUser(pBuddyAlarmLat, pBuddyAlarmLon)
        '    MouseDownLeft(e)
        'Else
        Select Case e.Button
            Case Windows.Forms.MouseButtons.Left
                MouseDownLeft(e)
            Case Windows.Forms.MouseButtons.Right
                ClickedTileFilename = FindTileFilename(TileServer, pBitmap.GetBounds(Drawing.GraphicsUnit.Pixel), e.X, e.Y)
                If ClickedTileFilename.Length > 0 Then
                    If RightClickMenu Is Nothing Then
                        RefreshFromServerToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem("Refresh From Server")
                        ClosestGeocacheToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem("Closest Geocache")
                        TileCacheFolderToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem("Open Cache Folder")
                        RightClickMenu = New System.Windows.Forms.ContextMenuStrip()
                        RightClickMenu.Items.AddRange(New System.Windows.Forms.ToolStripItem() _
                            {RefreshFromServerToolStripMenuItem, ClosestGeocacheToolStripMenuItem, TileCacheFolderToolStripMenuItem}) ', GetAllDescendantsToolStripMenuItem})
                    End If
                    RightClickMenu.Show(Me, e.Location)
                End If
        End Select
        'End If
    End Sub

    Private Sub RefreshFromServerToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RefreshFromServerToolStripMenuItem.Click
        If ClickedTileFilename.Length > 0 Then
            Try
                Debug.WriteLine("Refreshing  '" & ClickedTileFilename & "'")
                Downloader.DeleteTile(ClickedTileFilename)
                NeedRedraw()
            Catch ex As Exception
                MsgBox("Could not refresh '" & ClickedTileFilename & "'" & vbCrLf & ex.Message, MsgBoxStyle.Critical, "Error Refreshing Tile")
            End Try
            ClickedTileFilename = ""
        End If
    End Sub

    Private Sub ClosestGeocacheToolStripMenuItem_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles ClosestGeocacheToolStripMenuItem.Click
        'TODO: use clicked location not center
        ShowGeocacheHTML(ClosestGeocache(CenterLat, CenterLon))
    End Sub

    Private Sub TileCacheFolderToolStripMenuItem_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles TileCacheFolderToolStripMenuItem.Click
        If ClickedTileFilename.Length > 0 Then
            modGlobal.OpenFileOrURL(IO.Path.GetDirectoryName(ClickedTileFilename), False)
        Else
            modGlobal.OpenFileOrURL(pTileCacheFolder, False)
        End If
    End Sub

    Private Sub Event_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseMove
        If MouseDragging Then
            'If pBuddyAlarmForm IsNot Nothing Then
            '    pBuddyAlarmMeters = MetersBetweenLatLon(pBuddyAlarmLat, pBuddyAlarmLon, _
            '                                            pBuddyAlarmLat + (e.Y - pMouseDragStartLocation.Y) * LatHeight / Me.ClientRectangle.Height, _
            '                                            pBuddyAlarmLon - (e.X - pMouseDragStartLocation.X) * LonWidth / Me.ClientRectangle.Width)
            '    pBuddyAlarmForm.SetDistance(pBuddyAlarmMeters)
            'Else
            'GPSFollow = 0
            CenterLat = MouseDownLat + (e.Y - MouseDragStartLocation.Y) * LatHeight / Height
            CenterLon = MouseDownLon - (e.X - MouseDragStartLocation.X) * LonWidth / Width
            SanitizeCenterLatLon()
            'End If
            NeedRedraw()
        End If
    End Sub

    Private Sub Event_MouseWheel(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseWheel
        Select Case MouseWheelAction
            Case EnumWheelAction.Zoom
                If e.Delta > 0 Then
                    Zoom += 1
                Else
                    Zoom -= 1
                End If
            Case EnumWheelAction.TileServer
                Dim lTileServerNames As New Generic.List(Of String)
                For Each lServer As clsServer In Servers.Values
                    If Not String.IsNullOrEmpty(lServer.TilePattern) Then
                        lTileServerNames.Add(lServer.Name)
                    End If
                Next
                If lTileServerNames.Count > 0 Then
                    Dim lNextServerIndex As Integer = lTileServerNames.IndexOf(TileServer.Name)
                    If e.Delta > 0 Then
                        lNextServerIndex += 1
                    Else
                        lNextServerIndex -= 1
                    End If

                    If lNextServerIndex >= lTileServerNames.Count Then lNextServerIndex = 0
                    If lNextServerIndex < 0 Then lNextServerIndex = lTileServerNames.Count - 1

                    TileServerName = lTileServerNames(lNextServerIndex)
                End If
            Case EnumWheelAction.Layer
                Dim lVisibleIndex As Integer = -1
                For lVisibleIndex = 0 To Layers.Count - 1
                    If Layers(lVisibleIndex).Visible Then
                        Exit For
                    End If
                Next
                If e.Delta > 0 Then
                    lVisibleIndex += 1
                    If lVisibleIndex >= Layers.Count Then lVisibleIndex = 0
                Else
                    lVisibleIndex -= 1
                    If lVisibleIndex < 0 Then lVisibleIndex = Layers.Count - 1
                End If

                For lIndex As Integer = 0 To Layers.Count - 1
                    If (lIndex = lVisibleIndex) Then
                        Layers(lIndex).Visible = True
                    Else
                        Layers(lIndex).Visible = False
                    End If
                Next
                NeedRedraw()
        End Select
    End Sub

#End Region 'Desktop
#End If
End Class
