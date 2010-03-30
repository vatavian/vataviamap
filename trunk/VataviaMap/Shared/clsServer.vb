Public Class clsServer
    Public Name As String
    Public Link As String
    Public TilePattern As String
    Public WebmapPattern As String
    Public Copyright As String

    Public Sub New()
    End Sub

    Public Sub New(ByVal aName As String, _
          Optional ByVal aLink As String = Nothing, _
          Optional ByVal aTilePattern As String = Nothing, _
          Optional ByVal aWebmapPattern As String = Nothing, _
          Optional ByVal aCopyright As String = Nothing)
        Name = aName
        Link = aLink
        TilePattern = aTilePattern
        WebmapPattern = aWebmapPattern
        Copyright = aCopyright.Replace("(C)", "©")

        If Link Is Nothing AndAlso TilePattern Is Nothing AndAlso WebmapPattern Is Nothing Then
            Dim lFields() As String = Name.Split("|")
            If lFields.Length > 1 Then
                SetFields(lFields)
            End If
        End If
    End Sub

    Public Sub SetFields(ByVal aFields() As String)
        If aFields.Length > 0 Then Name = aFields(0)
        If aFields.Length > 1 Then Link = aFields(1)
        If aFields.Length > 2 Then TilePattern = aFields(2)
        If aFields.Length > 3 Then WebmapPattern = aFields(3)
        If aFields.Length > 4 Then Copyright = aFields(4).Replace("(C)", "©")
    End Sub

    Public Shared Function FromFields(ByVal aFields() As String) As clsServer
        Dim lServer As clsServer = Nothing
        If aFields.Length > 1 Then
            lServer = New clsServer
            lServer.SetFields(aFields)
        End If
        Return lServer
    End Function

    Public Overrides Function ToString() As String
        Dim lBuilder As New System.Text.StringBuilder
        If Name IsNot Nothing Then lBuilder.Append(Name)
        lBuilder.Append("|")
        If Link IsNot Nothing Then lBuilder.Append(Link)
        lBuilder.Append("|")
        If TilePattern IsNot Nothing Then lBuilder.Append(TilePattern)
        lBuilder.Append("|")
        If WebmapPattern IsNot Nothing Then lBuilder.Append(WebmapPattern)
        lBuilder.Append("|")
        If Copyright IsNot Nothing Then lBuilder.Append(Copyright)
        Return lBuilder.ToString
    End Function

    Public Shared Function ReadServers(ByVal aHTML As String) As Generic.Dictionary(Of String, clsServer)
        Dim lServers As New Generic.Dictionary(Of String, clsServer)
        Dim lServer As clsServer
        For Each lServerRow As String In SplitByTag(aHTML, "tr")
            lServer = clsServer.FromFields(SplitByTag(lServerRow, "td").ToArray)
            If lServer IsNot Nothing Then
                lServers.Add(lServer.Name, lServer)
            End If
        Next
        Return lServers
    End Function

    Private Shared Function SplitByTag(ByVal aHTML As String, ByVal aTag As String) As Generic.List(Of String)
        Dim lList As New Generic.List(Of String)
        Dim lStartTag As String = "<" & aTag & ">"
        Dim lStart As Integer = aHTML.IndexOf(lStartTag)
        While lStart >= 0 AndAlso lStart < aHTML.Length
            lStart += lStartTag.Length
            Dim lEnd As Integer = aHTML.IndexOf("</" & aTag & ">", lStart)
            If lEnd < 0 Then lEnd = aHTML.IndexOf(lStartTag, lStart)
            If lEnd < 0 Then
                lList.Add(aHTML.Substring(lStart))
                lStart = aHTML.Length
            Else
                lList.Add(aHTML.Substring(lStart, lEnd - lStart).Trim)
                lStart = aHTML.IndexOf(lStartTag, lEnd)
            End If
        End While
        Return lList
    End Function

    Public Function BuildTileURL(ByVal aTilePoint As Drawing.Point, ByVal aZoom As Integer) As String
        Dim lURL As String = TilePattern.Replace("{Zoom}", aZoom) _
                                        .Replace("{X}", aTilePoint.X) _
                                        .Replace("{Y}", aTilePoint.Y) _
                                        .Replace("{abc}", "a") 'TODO: spread load by returning different server letters
        If lURL.IndexOf("{YY}") > 0 Then lURL = lURL.Replace("{YY}", (((1 << aZoom) >> 1) - 1 - aTilePoint.Y))
        If lURL.IndexOf("{Zoom+1}") > 0 Then lURL = lURL.Replace("{Zoom+1}", aZoom + 1)
        If lURL.IndexOf("{VersionYahooSatellite") > 0 Then lURL = lURL.Replace("{VersionYahooSatellite", "1.9")
        Return lURL
    End Function

    'TODO: get a list of sites from a web page, e.g. http://frvipofm.net/osm/mapjumper/
    Public Function BuildWebmapURL(ByVal aCenterLatitude As Double, ByVal aCenterLongitude As Double, _
                               ByVal aZoom As Integer, _
                               ByRef aNorth As Double, ByRef aWest As Double, _
                               ByRef aSouth As Double, ByRef aEast As Double) As String
        Dim lFormat As String = "#.#####"
        Dim lLon As String = Format(aCenterLongitude, lFormat)
        Dim lLat As String = Format(aCenterLatitude, lFormat)
        Dim lHeight As String = Format(aNorth - aSouth, lFormat)
        Dim lWidth As String = Format(aEast - aWest, lFormat)
        Dim lLeft As String = Format(aWest, lFormat)
        Dim lRight As String = Format(aEast, lFormat)
        Dim lTop As String = Format(aNorth, lFormat)
        Dim lBottom As String = Format(aSouth, lFormat)

        Dim lURL As String = WebmapPattern.Replace("{Lat}", lLat) _
                                          .Replace("{Lon}", lLon) _
                                          .Replace("{Zoom}", aZoom)
        If lURL.IndexOf("{Zoom+1}") > 0 Then lURL = lURL.Replace("{Zoom+1}", aZoom + 1)
        If lURL.IndexOf("{Height}") > 0 Then lURL = lURL.Replace("{Height}", lHeight)
        If lURL.IndexOf("{Width}") > 0 Then lURL = lURL.Replace("{Width}", lWidth)
        If lURL.IndexOf("{Bottom}") > 0 Then lURL = lURL.Replace("{Bottom}", lBottom)
        If lURL.IndexOf("{Top}") > 0 Then lURL = lURL.Replace("{Top}", lTop)
        If lURL.IndexOf("{Left}") > 0 Then lURL = lURL.Replace("{Left}", lLeft)
        If lURL.IndexOf("{Right}") > 0 Then lURL = lURL.Replace("{Right}", lRight)

        Return lURL
    End Function

    Public Shared Function ParseWebmapURL(ByVal aURL As String, _
                                          ByRef aCenterLatitude As Double, ByRef aCenterLongitude As Double, _
                                          ByRef aZoom As Integer, _
                                          ByRef aNorth As Double, ByRef aWest As Double, _
                                          ByRef aSouth As Double, ByRef aEast As Double) As Boolean
        Try
            Dim lURL As String = aURL.ToLower
            Dim lArgs() As String = lURL.Split("&"c, "?"c)
            For Each lArg As String In lArgs
                Dim lArgPart() As String = lArg.Split("=")
                If lArgPart.Length = 2 Then
                    Select Case lArgPart(0)
                        Case "latitude", "lat", "mlat"
                            aCenterLatitude = Double.Parse(lArgPart(1))
                        Case "longitude", "lon", "mlon", "lng", "mlng"
                            aCenterLongitude = Double.Parse(lArgPart(1))
                        Case "zoom", "z"
                            aZoom = Integer.Parse(lArgPart(1))
                        Case "ll"
                            Dim ll() As String = lArgPart(1).Split(",")
                            If ll.Length = 2 AndAlso IsNumeric(ll(0)) AndAlso IsNumeric(ll(1)) Then
                                'aSite = SiteEnum.GoogleMaps
                                aCenterLatitude = Double.Parse(ll(0))
                                aCenterLongitude = Double.Parse(ll(1))
                            End If
                        Case "spn"
                            'TODO: parse Google's height,width into zoom
                        Case "cp"
                            'aSite = SiteEnum.Bing
                            Dim ll() As String = lArgPart(1).Split("~")
                            If ll.Length = 2 AndAlso IsNumeric(ll(0)) AndAlso IsNumeric(ll(1)) Then
                                aCenterLatitude = Double.Parse(ll(0))
                                aCenterLongitude = Double.Parse(ll(1))
                            End If
                        Case "lvl" : aZoom = lArgPart(1)
                        Case "starttop" : aNorth = Double.Parse(lArgPart(1))
                        Case "startbottom" : aSouth = Double.Parse(lArgPart(1))
                        Case "startleft" : aWest = Integer.Parse(lArgPart(1))
                        Case "startright" : aEast = Integer.Parse(lArgPart(1))

                    End Select
                End If
            Next
            If lArgs(0).IndexOf("seamless.usgs.gov") >= 0 Then
                aCenterLatitude = (aNorth + aSouth) / 2
                aCenterLongitude = (aWest + aEast) / 2
                'TODO: compute zoom from (aNorth - aSouth) and/or (aWest - aEast)                    
            ElseIf lArgs(0).StartsWith("geo:") Then
                Dim ll() As String = lArgs(0).Substring(4).Split(",")
                If ll.Length > 1 AndAlso IsNumeric(ll(0)) AndAlso IsNumeric(ll(1)) Then
                    aCenterLatitude = Double.Parse(ll(0))
                    aCenterLongitude = Double.Parse(ll(1))
                End If
            End If
            Return aCenterLatitude <> 0 AndAlso aCenterLongitude <> 0 AndAlso aZoom >= g_ZoomMin AndAlso aZoom <= g_ZoomMax
        Catch
            Return False
        End Try
    End Function

End Class
