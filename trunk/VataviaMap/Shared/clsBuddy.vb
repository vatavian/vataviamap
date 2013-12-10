Public Class clsBuddy
    Public Name As String
    Public LocationURL As String
    Public IconFilename As String = ""
    Public IconURL As String = ""
    Public Waypoint As clsGPXwaypoint
    Public Selected As Boolean = True

    Public Function LoadFile(ByVal aFilename As String, Optional ByVal aCacheFolder As String = Nothing) As Boolean
        Dim lFileContents As String = ReadTextFile(aFilename)
        If lFileContents.IndexOf("<UserInfo>") > -1 Then
            Return LoadNavizonXML(lFileContents)
        ElseIf lFileContents.IndexOf("navizon.com") > -1 Then
            Return LoadNavizonGears(lFileContents) 'try to find location even though user chose wrong link
        ElseIf lFileContents.IndexOf("<?xml") = 0 Then
            Return LoadLatitudeKML(lFileContents)
        ElseIf lFileContents.IndexOf("google.com/latitude/") > 0 Then
            Return LoadLatitudeJSON(lFileContents, aCacheFolder)
        ElseIf lFileContents.IndexOf("InstaMapper API") = 0 Then
            Return LoadInstaMapperCSV(lFileContents)
        ElseIf lFileContents.IndexOf("device_label") > 0 Then
            Return LoadInstaMapperJSON(lFileContents)
        Else
            Return LoadDMTP(lFileContents)
        End If
    End Function

    Public Function LoadNavizonXML(ByVal aFileContents As String) As Boolean
        Dim lLongitude As Double, lLatitude As Double
        Try
            lLongitude = Double.Parse(GetXmlTagContents(aFileContents, "Longitude"))
            lLatitude = Double.Parse(GetXmlTagContents(aFileContents, "Latitude"))
            Waypoint = New clsGPXwaypoint("wpt", lLatitude, lLongitude)
            With Waypoint
                .name = GetXmlTagContents(aFileContents, "UserName")
                .desc = GetXmlTagContents(aFileContents, "Timestamp")
                .sym = Name
                Return True
            End With
        Catch ex As Exception
            Dbg(ex.Message)
        End Try
        Return False
    End Function

    Public Function LoadNavizonGears(ByVal aFileContents As String) As Boolean
        Dim lLongitude As Double, lLatitude As Double
        Try
            Dim lBeforeLat As Integer = aFileContents.IndexOf(").offsetHeight},")
            If lBeforeLat > 0 Then
                lBeforeLat += 16
                Dim lLatLonComma As Integer = aFileContents.IndexOf(",", lBeforeLat)
                If lLatLonComma > 0 Then
                    Dim lAfterLonComma As Integer = aFileContents.IndexOf(",", lLatLonComma + 1)
                    lLatitude = Double.Parse(aFileContents.Substring(lBeforeLat, lLatLonComma - lBeforeLat))
                    lLongitude = Double.Parse(aFileContents.Substring(lLatLonComma + 1, lAfterLonComma - lLatLonComma - 1))
                    Waypoint = New clsGPXwaypoint("wpt", lLatitude, lLongitude)
                    Return True
                End If
            End If

        Catch ex As Exception
            Dbg(ex.Message)
        End Try
        Return False
    End Function

    ''' <summary>
    ''' Load details from a Google Latitude KML request
    ''' </summary>
    ''' <param name="aFileContents">Contents of downloaded point</param>
    ''' <returns>True if able to parse buddy location</returns>
    ''' <remarks></remarks>
    Public Function LoadLatitudeKML(ByVal aFileContents As String) As Boolean
        Dim lcoords() As String = GetXmlTagContents(aFileContents, "coordinates").Split(",")
        If lcoords.Length > 0 Then
            Dim lLongitude As Double, lLatitude As Double
            Try
                lLongitude = Double.Parse(lcoords(0))
                lLatitude = Double.Parse(lcoords(1))
                Waypoint = New clsGPXwaypoint("wpt", lLatitude, lLongitude)
                With Waypoint
                    .name = Name
                    .desc = GetXmlTagContents(aFileContents, "description")
                    .sym = Name
                    'TODO: parse time/accuracy from description
                    Return True
                End With
            Catch ex As Exception
                Dbg(ex.Message)
            End Try
        End If
        Return False
    End Function

    ''' <summary>
    ''' Load details from a Google Latitude JSON request
    ''' </summary>
    ''' <param name="aFileContents">Contents of downloaded point</param>
    ''' <returns>True if able to parse buddy location</returns>
    ''' <remarks>If buddy does not yet have an icon, also try downloading the buddy icon</remarks>
    Public Function LoadLatitudeJSON(ByVal aFileContents As String, Optional ByVal aCacheFolder As String = Nothing) As Boolean
        Dim lcoords() As String = GetJSONTagContents(aFileContents, "coordinates").TrimStart("[").TrimEnd("]", "}").Split(",")
        If lcoords.Length = 2 Then
            Dim lLongitude As Double, lLatitude As Double
            Try
                lLongitude = Double.Parse(lcoords(0))
                lLatitude = Double.Parse(lcoords(1))
                Waypoint = New clsGPXwaypoint("wpt", lLatitude, lLongitude)
                With Waypoint
                    .desc = GetJSONTagContents(aFileContents, "reverseGeocode")
                    If Not IO.File.Exists(IconFilename) Then
                        IconURL = GetJSONTagContents(aFileContents, "photoUrl")
                        If IconURL.Length > 0 Then
                            If aCacheFolder Is Nothing OrElse aCacheFolder.Length = 0 Then aCacheFolder = IO.Path.GetTempPath
                            IconFilename = IO.Path.Combine(aCacheFolder, Name & ".jpg")
                        End If
                    End If
                    .sym = "buddy|" & Name.ToLower
                    .name = Name

                    Dim lValue As String

                    lValue = GetJSONTagContents(aFileContents, "timeStamp")
                    If IsNumeric(lValue) Then
                        Dim lBaseDate As New Date(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
                        .time = lBaseDate.AddSeconds(Double.Parse(lValue))
                    End If
                    lValue = GetJSONTagContents(aFileContents, "altitude") : If IsNumeric(lValue) Then .ele = lValue
                    lValue = GetJSONTagContents(aFileContents, "speed") : If IsNumeric(lValue) Then .speed = lValue
                    lValue = GetJSONTagContents(aFileContents, "heading") : If IsNumeric(lValue) Then .course = lValue
                    Return True
                End With
            Catch ex As Exception
                Dbg(ex.Message)
            End Try
        End If
        Return False
    End Function

    ''' <summary>
    ''' Load the default (comma-separated) point type returned by InstaMapper API
    ''' </summary>
    ''' <param name="aFileContents">Contents of downloaded point</param>
    ''' <returns>True if able to parse buddy location</returns>
    Public Function LoadInstaMapperCSV(ByVal aFileContents As String) As Boolean
        'Example URL: http://www.instamapper.com/api?action=getPositions&key=584014439054448247&num=1
        'Field names:
        'Device key,Device label,Timestamp in UTC (seconds since 1970-1-1),Latitude,Longitude,Altitude in meters,Speed in meters / second,Heading in degrees
        'Example Result:
        'InstaMapper API v1.00
        '0071543339905,Demo car,1209252229,47.50417,-122.19697,25.0,20.5,344
        Try
            Dim lPosition As String = aFileContents.Substring(aFileContents.IndexOf(vbLf) + 1)
            Dim lFields() As String = lPosition.Split(","c)
            If lFields.Length > 4 Then
                Dim lLat As Double = lFields(3)
                Dim lLon As Double = lFields(4)
                Waypoint = New clsGPXwaypoint("wpt", lLat, lLon)
                With Waypoint
                    Dim lBaseDate As New Date(1970, 1, 1)
                    .name = lFields(1)
                    .time = lBaseDate.AddSeconds(lFields(2))
                    If lFields.Length > 5 AndAlso lFields(5).Length > 0 Then .ele = lFields(5)
                    If lFields.Length > 6 AndAlso lFields(6).Length > 0 Then .speed = lFields(6)
                    If lFields.Length > 7 AndAlso lFields(7).Length > 0 Then .course = lFields(7)
                    Return True
                End With
            End If
        Catch ex As Exception
            Dbg(ex.Message)
        End Try
        Return False
    End Function

    ''' <summary>
    ''' Load JSON formatted result from InstaMapper API
    ''' </summary>
    ''' <param name="aFileContents">Contents of downloaded point</param>
    ''' <returns>True if able to parse buddy location</returns>
    Public Function LoadInstaMapperJSON(ByVal aFileContents As String) As Boolean
        'Example URL: http://www.instamapper.com/api?action=getPositions&key=584014439054448247&num=1&format=json
        'Example Result:
        ' { "version" : "1.0",
        '   "positions" : [
        '     { "device_key" : "0071543339995",
        '       "device_label" : "Demo car",
        '       "timestamp" : 1209252709,
        '       "latitude" : 47.60969,
        '       "longitude" : -122.18833,
        '       "altitude" : 0.0,
        '       "speed" : 25.0,
        '       "heading" : 349 }
        '   ]
        ' }
        Try
            Dim lLat As Double = GetJSONTagContents(aFileContents, "latitude")
            Dim lLon As Double = GetJSONTagContents(aFileContents, "longitude")
            Waypoint = New clsGPXwaypoint("wpt", lLat, lLon)

            With Waypoint
                .name = GetJSONTagContents(aFileContents, "device_label")

                Dim lValue As String

                lValue = GetJSONTagContents(aFileContents, "timestamp")
                If IsNumeric(lValue) Then
                    Dim lBaseDate As New Date(1970, 1, 1)
                    .time = lBaseDate.AddSeconds(lValue)
                End If

                lValue = GetJSONTagContents(aFileContents, "altitude") : If IsNumeric(lValue) Then .ele = lValue
                lValue = GetJSONTagContents(aFileContents, "speed") : If IsNumeric(lValue) Then .speed = lValue
                lValue = GetJSONTagContents(aFileContents, "heading") : If IsNumeric(lValue) Then .course = lValue
            End With
            Return True
        Catch ex As Exception
            Dbg(ex.Message)
        End Try
        Return False
    End Function

    ''' <summary>
    ''' Load details from OpenDMTP-formatted log entry
    ''' </summary>
    ''' <param name="aFileContents">Contents of downloaded point</param>
    ''' <returns>True if able to parse buddy location</returns>
    Public Function LoadDMTP(ByVal aFileContents As String) As Boolean
        Dim lFields() As String = aFileContents.Split(","c)
        If lFields.Length > 4 AndAlso lFields(0).Length > 0 Then
            Dim lLat As Double = lFields(3)
            Dim lLon As Double = lFields(4)

            Waypoint = New clsGPXwaypoint("wpt", lLat, lLon)
            With Waypoint
                If lFields.Length > 5 AndAlso lFields(5).Length > 0 Then .speed = lFields(5)
                If lFields.Length > 6 AndAlso lFields(6).Length > 0 Then .course = lFields(6)
                If lFields.Length > 7 AndAlso lFields(7).Length > 0 Then .ele = lFields(7)
                If lFields.Length > 8 AndAlso lFields(8).Length > 0 Then  'we have UTC from GPS in the record
                    .time = Date.Parse(lFields(8), _
                                       Globalization.CultureInfo.CurrentCulture.DateTimeFormat, _
                                       Globalization.DateTimeStyles.AssumeUniversal).ToUniversalTime
                Else    'use local time from logged point
                    .time = Date.Parse(lFields(0) & " " & lFields(1), _
                                       Globalization.CultureInfo.CurrentCulture.DateTimeFormat, _
                                       Globalization.DateTimeStyles.AssumeLocal).ToUniversalTime()
                End If
                .name = Name
                .sym = Name
            End With
            Return True
        End If
        Return False
    End Function

    Private Function GetXmlTagContents(ByVal aXML As String, ByVal aTag As String) As String
        Dim lStartPos As Integer = aXML.IndexOf("<" & aTag & ">", StringComparison.CurrentCultureIgnoreCase)
        If lStartPos < 0 Then Return ""
        lStartPos += aTag.Length + 2
        Dim lEndPos As Integer = aXML.IndexOf("</" & aTag & ">", lStartPos, StringComparison.CurrentCultureIgnoreCase)
        If lEndPos < 0 Then
            Return aXML.Substring(lStartPos)
        Else
            Return aXML.Substring(lStartPos, lEndPos - lStartPos)
        End If
    End Function

    Private Function GetJSONTagContents(ByVal aSource As String, ByVal aTag As String) As String
        Dim lStartPos As Integer = aSource.IndexOf("""" & aTag & """", StringComparison.CurrentCultureIgnoreCase)
        If lStartPos < 0 Then Return ""
        lStartPos += aTag.Length + 2
        Dim lEndPos As Integer = aSource.IndexOf(vbLf, lStartPos, StringComparison.CurrentCultureIgnoreCase)
        If lEndPos < 0 Then
            aSource = aSource.Substring(lStartPos)
        Else
            aSource = aSource.Substring(lStartPos, lEndPos - lStartPos)
        End If
        Return aSource.TrimStart(" ", ":", """").TrimEnd(" ", """", ",")
    End Function
End Class