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
        Copyright = aCopyright

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
        If aFields.Length > 4 Then Copyright = aFields(4)
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

    Public Function TileURL(ByVal aTilePoint As Drawing.Point, ByVal aZoom As Integer) As String
        Dim lURL As String = TilePattern.Replace("{z}", aZoom) _
                          .Replace("{x}", aTilePoint.X) _
                          .Replace("{y}", aTilePoint.Y) _
                          .Replace("{abc}", "a")
        If lURL.IndexOf("{yy}") > 0 Then lURL = lURL.Replace("{yy}", (((1 << aZoom) >> 1) - 1 - aTilePoint.Y))
        If lURL.IndexOf("{z+1}") > 0 Then lURL = lURL.Replace("{z+1}", aZoom + 1)
        If lURL.IndexOf("{VersionYahooSatellite") > 0 Then lURL = lURL.Replace("{VersionYahooSatellite", "1.9")
        Return lURL
    End Function

    Private Sub BuildURL(ByRef aURL As String, ByVal aTag As String, ByVal aReplaceTrue As String, ByVal aReplaceFalse As String, ByVal aTest As Boolean)
        Dim lReplacement As String
        If aTest Then
            lReplacement = aReplaceTrue
        Else
            lReplacement = aReplaceFalse
        End If
        aURL = aURL.Replace("{" & aTag & "}", lReplacement)
    End Sub

End Class
