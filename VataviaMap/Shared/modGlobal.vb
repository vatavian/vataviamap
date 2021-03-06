Module modGlobal

    Public Const g_AppName As String = "VataviaMap"

    'Assuming earth is a sphere
    Public Const g_RadiusOfEarth As Double = 6378137 'meters
    Public Const g_CircumferenceOfEarth As Double = 2.0 * Math.PI * g_RadiusOfEarth
    Public Const g_HalfCircumferenceOfEarth As Double = g_CircumferenceOfEarth / 2

    Public Const g_MetersPerMile As Double = 1609.344
    Public Const g_FeetPerMeter As Double = 3.2808399
    Public Const g_MilesPerKnot As Double = 1.15077945

    Public g_PathChar As String = IO.Path.DirectorySeparatorChar
    Public g_MarkedPrefix As String = "Marked" & g_PathChar

    Public g_IconMaxSize As Integer = 150

    Private g_LimitY As Double = ProjectF(85.0511)
    Private g_RangeY As Double = 2 * g_LimitY

    Public g_TileServerExampleLabel As String = "URL of server"
    Public g_TileServerExampleFile As String = "http://wiki.openstreetmap.org/wiki/Slippy_map_tilenames#Tiles"


    ' URL of server to upload points to
    Public g_UploadPointURL As String = "http://vatavia.net/cgi-bin/gps?u=unknown&y=#Lat#&x=#Lon#&e=#Alt#&s=#Speed#&h=#Heading#&t=#Time#&l=#Label#&c=#CellID#"
    ' URL of server to upload points to
    Public g_UploadTrackURL As String = "http://vatavia.net/cgi-bin/trk?u=unknown"

    Public g_DegreeFormat As EnumDegreeFormat = EnumDegreeFormat.DegreesDecimalMinutes

    Public g_WaypointIcons As New Generic.Dictionary(Of String, Drawing.Bitmap)

    Public g_Random As New Random

    Public g_TileProjection As String = "PROJCS[@Popular Visualisation CRS / Mercator@,GEOGCS[@Popular Visualisation CRS@,DATUM[@D_Popular_Visualisation_Datum@,SPHEROID[@Popular_Visualisation_Sphere@,6378137,0]],PRIMEM[@Greenwich@,0],UNIT[@Degree@,0.017453292519943295]],PROJECTION[@Mercator@],PARAMETER[@central_meridian@,0],PARAMETER[@scale_factor@,1],PARAMETER[@false_easting@,0],PARAMETER[@false_northing@,0],UNIT[@Meter@,1]]".Replace("@", """")

    Public g_Debug As Boolean = True
    Public g_DebugFilename As String = Nothing
    Public g_DebugWriter As System.IO.TextWriter = Nothing

    Public Enum EnumDegreeFormat
        DecimalDegrees = 0
        DegreesDecimalMinutes = 1
        DegreesMinutesSeconds = 2
    End Enum

    Friend Function FormattedDegrees(ByVal aDegrees As Double, ByVal aFormat As EnumDegreeFormat) As String
        Dim lStr As String = Nothing, lStrMinutes As String = Nothing, lStrSeconds As String = Nothing
        Select Case aFormat
            Case EnumDegreeFormat.DegreesDecimalMinutes
                DegreesMinutes(aDegrees, lStr, lStrMinutes)
                lStr &= " " & lStrMinutes

            Case EnumDegreeFormat.DegreesMinutesSeconds
                DegreesMinutesSeconds(aDegrees, lStr, lStrMinutes, lStrSeconds)
                lStr &= " " & lStrMinutes & "' " & lStrSeconds & """"

            Case Else
                lStr = Format(aDegrees, "0.######")
        End Select
        Return lStr
    End Function

    Public Sub DegreesMinutes(ByVal aDegrees As Double, ByRef aDegreesString As String, ByRef aMinutesString As String)
        Dim lDegreesInt As Integer
        Dim lDoubleMinutes As Double
        If aDegrees < 0 Then
            lDegreesInt = Math.Ceiling(aDegrees)
            lDoubleMinutes = (lDegreesInt - aDegrees) * 60.0
        Else
            lDegreesInt = Math.Floor(aDegrees)
            lDoubleMinutes = (aDegrees - lDegreesInt) * 60.0
        End If
        aDegreesString = CStr(lDegreesInt)
        aMinutesString = Format(lDoubleMinutes, "0.000")
    End Sub

    ''' <summary>
    ''' From given decimal degrees, populate strings with integer degrees and minutes and seconds rounded to one decimal place
    ''' </summary>
    ''' <param name="aDegrees">Decimal degrees</param>
    ''' <param name="aDegreesString">Integer degrees as string</param>
    ''' <param name="aMinutesString">Integer minutes as string</param>
    ''' <param name="aSecondsString">Seconds as string, rounded to one decimal place</param>
    Public Sub DegreesMinutesSeconds(ByVal aDegrees As Double, ByRef aDegreesString As String, ByRef aMinutesString As String, ByRef aSecondsString As String)
        Dim lDegreesInt As Integer
        Dim lDoubleMinutes As Double
        If aDegrees < 0 Then
            lDegreesInt = Math.Ceiling(aDegrees)
            lDoubleMinutes = (lDegreesInt - aDegrees) * 60.0
        Else
            lDegreesInt = Math.Floor(aDegrees)
            lDoubleMinutes = (aDegrees - lDegreesInt) * 60.0
        End If
        Dim lMinutesInt As Integer = Math.Floor(lDoubleMinutes)
        Dim lDoubleSeconds As Double = (lDoubleMinutes - lMinutesInt) * 60.0

        aDegreesString = CStr(lDegreesInt)
        aMinutesString = CStr(lMinutesInt)
        aSecondsString = Format(lDoubleSeconds, "0.#")
    End Sub

    Public Function DegToRad(ByVal aDegrees As Double) As Double
        Return aDegrees * Math.PI / 180.0
    End Function

    Private Function ProjectF(ByVal aLatitude As Double) As Double
        aLatitude = DegToRad(aLatitude)
        Return Math.Log(Math.Tan(aLatitude) + (1 / Math.Cos(aLatitude)))
    End Function

    Private Function ProjectMercatorToLatitude(ByVal aMercator As Double) As Double
        Return 180 / Math.PI * Math.Atan((Math.Exp(aMercator) - Math.Exp(-aMercator)) / 2)
    End Function

    ''' <summary>
    ''' Compute approximate distance in meters between two points on the earth's surface given in decimal degrees
    ''' </summary>
    Public Function MetersBetweenLatLon(ByVal aLatitude1 As Double, ByVal aLongitude1 As Double, _
                                        ByVal aLatitude2 As Double, ByVal aLongitude2 As Double) As Double
        Dim Lat1rad As Double = DegToRad(aLatitude1)
        Dim dy As Double = g_RadiusOfEarth * (Lat1rad - DegToRad(aLatitude2))
        Dim dx As Double = g_RadiusOfEarth * Math.Cos(Lat1rad) * (DegToRad(aLongitude1) - DegToRad(aLongitude2))
        Return Math.Sqrt(dy * dy + dx * dx)
    End Function

    Public Function MetersPerPixel(ByVal aZoom As Integer, ByVal aTileSize As Integer) As Double
        Return g_CircumferenceOfEarth / ((1 << aZoom) * aTileSize)
    End Function

    Public Function LatitudeToMeters(ByVal aLatitude As Double) As Double
        Dim sinLat As Double = Math.Sin(DegToRad(aLatitude))
        Return g_RadiusOfEarth / 2 * Math.Log((1 + sinLat) / (1 - sinLat))
    End Function

    Public Function LongitudeToMeters(ByVal aLongitude As Double) As Double
        Return g_RadiusOfEarth * DegToRad(aLongitude)
    End Function

    'helper function - converts a latitude at a certain zoom into a y pixel
    Private Function LatitudeToYAtZoom(ByVal aLatitude As Double, ByVal aZoom As Integer, ByVal aTileSize As Integer) As Integer
        Dim arc As Double = MetersPerPixel(aZoom, aTileSize)
        Return CInt(Math.Round((g_HalfCircumferenceOfEarth - LatitudeToMeters(aLatitude)) / arc))
    End Function

    'helper function - converts a longitude at a certain zoom into a x pixel
    Private Function LongitudeToXAtZoom(ByVal aLongitude As Double, ByVal aZoom As Integer, ByVal aTileSize As Integer) As Integer
        Dim arc As Double = MetersPerPixel(aZoom, aTileSize)
        Return CInt(Math.Round((g_HalfCircumferenceOfEarth + LongitudeToMeters(aLongitude)) / arc))
    End Function

    ''' <summary>
    ''' Given an OSM tile specified by aTilePoint(X, Y) and aZoom, 
    ''' compute Lat/Long of NorthWest and SouthEast corners of tile
    ''' </summary>
    ''' <param name="aTilePoint">X and Y of OSM tile</param>
    ''' <param name="aZoom">Zoom of OSM tile</param>
    ''' <param name="aNorth">computed Latitude of northwest corner of tile</param>
    ''' <param name="aWest">computed Longitude of northwest corner of tile</param>
    ''' <param name="aSouth">computed Latitude of southeast corner of tile</param>
    ''' <param name="aEast">computed Longitude of southeast corner of tile</param>
    Public Sub CalcLatLonFromTileXY(ByVal aTilePoint As Point, ByVal aZoom As Long, _
                                    ByRef aNorth As Double, ByRef aWest As Double, _
                                    ByRef aSouth As Double, ByRef aEast As Double)
        Dim lNumTiles As Integer = 1 << aZoom

        'Tile X to longitude
        Dim lTileXtoLongitude As Double = 360.0 / lNumTiles
        aWest = -180 + aTilePoint.X * lTileXtoLongitude
        aEast = aWest + lTileXtoLongitude

        'Tile Y to latitude
        Dim lEarthFractionPerTile As Double = 1.0 / lNumTiles
        Dim lNorthFraction As Double = aTilePoint.Y * lEarthFractionPerTile
        Dim lSouthFraction As Double = lNorthFraction + lEarthFractionPerTile

        aNorth = ProjectMercatorToLatitude(g_LimitY - g_RangeY * lNorthFraction)
        aSouth = ProjectMercatorToLatitude(g_LimitY - g_RangeY * lSouthFraction)
    End Sub

    ''' <summary>
    ''' Given an OSM tile specified by aTilePoint(X, Y) and aZoom, 
    ''' compute projected meters of NorthWest and SouthEast corners of tile
    ''' </summary>
    ''' <param name="aTilePoint">X and Y of OSM tile</param>
    ''' <param name="aZoom">Zoom of OSM tile</param>
    ''' <param name="aNorth">computed meters of northwest corner of tile</param>
    ''' <param name="aWest">computed meters of northwest corner of tile</param>
    ''' <param name="aSouth">computed meters of southeast corner of tile</param>
    ''' <param name="aEast">computed meters of southeast corner of tile</param>
    Public Sub CalcMetersFromTileXY(ByVal aTilePoint As Point, ByVal aZoom As Long, _
                                    ByRef aNorth As Double, ByRef aWest As Double, _
                                    ByRef aSouth As Double, ByRef aEast As Double)
        CalcLatLonFromTileXY(aTilePoint, aZoom, aNorth, aWest, aSouth, aEast)

        aWest = LongitudeToMeters(aWest)
        aEast = LongitudeToMeters(aEast)
        aNorth = LatitudeToMeters(aNorth)
        aSouth = LatitudeToMeters(aSouth)

    End Sub

    ''' <summary>
    ''' Compute the OSM tile (X, Y) that contains the given lat/long point and
    ''' compute the pixel offset within the tile of the given point
    ''' </summary>
    ''' <param name="aLatitude">Latitude of desired point</param>
    ''' <param name="aLongitude">Longitude of desired point</param>
    ''' <param name="aZoom">OSM zoom level (0-17)</param>
    ''' <param name="aOffset">ByRef returns pixel offset of given point within the OSM tile</param>
    ''' <returns>Point containing X and Y of OSM tile containing the given lat/long point</returns>
    Public Function CalcTileXY(ByVal aTileServer As clsServer, _
                               ByVal aLatitude As Double, _
                               ByVal aLongitude As Double, _
                               ByVal aZoom As Long, _
                               ByRef aOffset As Point) As Point
        If aLatitude > aTileServer.LatMax Then aLatitude = aTileServer.LatMax
        If aLatitude < aTileServer.LatMin Then aLatitude = aTileServer.LatMin

        Dim lNumTiles As Integer = 1 << aZoom
        Dim x As Double = (aLongitude + 180) / 360 * lNumTiles
        Dim y As Double = (1 - Math.Log(Math.Tan(aLatitude * Math.PI / 180) _
                                  + 1 / Math.Cos(aLatitude * Math.PI / 180)) / Math.PI) / 2 * lNumTiles
        Dim lPoint As New Point(Math.Floor(x), Math.Floor(y))
        aOffset = New Point((x - lPoint.X) * aTileServer.TileSize, (y - lPoint.Y) * aTileServer.TileSize)
        Return lPoint
    End Function

    ''' <summary>
    ''' Remove dangerous characters in filename
    ''' </summary>
    ''' <param name="aOriginalFilename">Filename to be converted</param>
    ''' <param name="aReplaceWith">String to replace non-printable characters with (default=empty string)</param>
    ''' <returns>aOriginalFilename with non-printable and non-allowed file name characters replaced with aReplaceWith</returns>
    Public Function SafeFilename(ByVal aOriginalFilename As String, _
                        Optional ByVal aReplaceWith As String = "") As String
        Dim lRetval As String = "" 'return string
        Dim lChr As String 'individual character in filename
        For lIndex As Integer = 1 To aOriginalFilename.Length
            lChr = Mid(aOriginalFilename, lIndex, 1)
            Select Case Asc(lChr)
                Case 0 : GoTo EndFound
                Case Is < 32, 34, 42, 47, 58, 60, 62, 63, 92, 124, Is > 125 : lRetval &= aReplaceWith
                Case Else : lRetval &= lChr
            End Select
        Next
EndFound:
        Return lRetval.Trim
    End Function

    ''' <summary>
    ''' Encode dangerous characters in filename into %XX hex digits
    ''' </summary>
    ''' <param name="aEncodeMe">Filename to be converted</param>
    ''' <returns>aEncodeMe with non-printable and non-allowed file name characters replaced with %XX hex digits</returns>
    Public Function FilenameEncode(ByVal aEncodeMe As String) As String
        Dim lRetval As New System.Text.StringBuilder
        For Each lChr As Char In aEncodeMe.ToCharArray
            Dim lAsc As Integer = Asc(lChr)
            Select Case lAsc
                Case 0
                    GoTo EndFound
                Case Is < 32, 34, 37, 39, 42, 46, 47, 58, 60, 62, 63, 92, 96, 124, Is > 125
                    Dim lHex As String = Hex(lAsc)
                    lRetval.Append("%")
                    Select Case lHex.Length
                        Case 0 : lRetval.Append("2D") 'dash, Should never happen, lHex.Length should always be at least 1
                        Case 1 : lRetval.Append("0" & lHex) 'Left-pad single hex character with a zero
                        Case 2 : lRetval.Append(lHex)
                        Case Else : lRetval.Append(lHex.Substring(lHex.Length - 2, 2)) 'Only keep last two hex characters
                    End Select
                Case Else
                    lRetval.Append(lChr)
            End Select
        Next
EndFound:
        Return lRetval.ToString
    End Function

    ''' <summary>
    ''' Decode filename encoded by FilenameEncode so all dangerous characters appear in their original form
    ''' </summary>
    ''' <param name="aEncoded">Filename to be converted</param>
    ''' <returns>aEncoded with non-printable and non-allowed file name characters converted from %XX hex digits back to original form</returns>
    Public Function FilenameDecode(ByVal aEncoded As String) As String
        Dim lRetval As New System.Text.StringBuilder
        Dim lIndex As Integer = 0
        While lIndex < aEncoded.Length
            If aEncoded(lIndex) = "%" Then
                If aEncoded.Substring(lIndex + 1, 1) = "_" Then
                    lRetval.Append(" ")
                    lIndex += 2
                Else
                    Try
                        lRetval.Append(Chr(Convert.ToInt32(aEncoded.Substring(lIndex + 1, 2), 16)))
                        lIndex += 3
                    Catch e As exception
                        lRetval.Append(aEncoded(lIndex))
                        lIndex += 1
                    End Try
                End If
            Else
                lRetval.Append(aEncoded(lIndex))
                lIndex += 1
            End If
        End While
        Return lRetval.ToString
    End Function

    ''' <summary>
    ''' Find information in a filename extension
    ''' </summary>
    ''' <param name="aFilename">filename to search</param>
    ''' <param name="aExtension">extension to search for</param>
    ''' <returns>decoded information following the specified extension</returns>
    ''' <remarks>
    ''' Example: FilenameDecodeExt("3277.Expires.Sat, 22 Aug 2020 13%3A35%3A40 GMT.png", "Expires") 
    '''                                       = "Sat, 22 Aug 2020 13:35:40 GMT"
    ''' </remarks>
    Public Function FilenameDecodeExt(ByVal aFilename As String, ByVal aExtension As String) As String
        If aFilename IsNot Nothing Then
            Dim lExtensionStart As Integer = aFilename.IndexOf(aExtension)
            If lExtensionStart > -1 Then
                lExtensionStart += aExtension.Length
                Dim lExtensionEnd As Integer = aFilename.IndexOf(".", lExtensionStart)
                If lExtensionEnd < 1 Then lExtensionEnd = aFilename.Length
                Return FilenameDecode(aFilename.Substring(lExtensionStart, lExtensionEnd - lExtensionStart))
            End If
        End If
        Return ""
    End Function

    Public Function DateTryParse(ByVal aDateString As String, ByRef aDate As Date) As Boolean
        If aDateString IsNot Nothing AndAlso aDateString.Length > 5 AndAlso aDateString <> "Fri, 01 Jan 1990 00:00:00 GMT" Then
            Try 'First check for format that Date.Parse cannot handle: "Wed Apr  6 15:10:37 2011"
                If aDateString.Length = 24 _
                  AndAlso aDateString.Substring(13, 1) = ":" _
                  AndAlso aDateString.Substring(16, 1) = ":" _
                  AndAlso IsNumeric(aDateString.Substring(19)) Then
                    aDate = Date.Parse(aDateString.Substring(0, 10) _
                                     & aDateString.Substring(19) _
                                     & aDateString.Substring(10, 9))
                    Return True
                End If
            Catch
            End Try

            Try
                aDate = Date.Parse(aDateString)
                Return True
            Catch
                Debug.WriteLine("CannotParseDate: " & aDateString)
            End Try
        End If
        Return False
    End Function

    Public Function FileSize(ByVal aFilename As String) As Integer
        Try
            If IO.File.Exists(aFilename) Then Return (New IO.FileInfo(aFilename)).Length 'FileLen(aFilename) 
        Catch
        End Try
        Return -1
    End Function

    Public Function ReadableFromXML(ByVal aXML As String) As String
        Dim lSB As New System.Text.StringBuilder
        Dim lIndex As Integer = 0
        While lIndex < aXML.Length
            Dim lChar As Char = aXML.Chars(lIndex)
            Select Case lChar
                Case "<"
                    Dim lClose As Integer = aXML.IndexOf(">", lIndex + 1)
                    If aXML.Chars(lIndex + 1) = "/" Then
                        lSB.Append(vbLf)
                    Else
                        lSB.Append(aXML.Substring(lIndex + 1, lClose - lIndex - 1) & ": ")
                    End If
                    lIndex = lClose
                Case Else : lSB.Append(lChar)
            End Select
            lIndex += 1
        End While
        Return lSB.ToString.Replace(vbLf & vbLf, vbLf).Replace(vbLf, vbLf)
    End Function

    'Encode or decode text using the ROT13 algorithm
    Public Function ROT13(ByVal InputText As String) As String
        Dim i As Integer
        Dim CurrentCharacter As Char
        Dim CurrentCharacterCode As Integer
        Dim EncodedText As String = ""

        'Iterate through the length of the input parameter  
        For i = 0 To InputText.Length - 1
            'Convert the current character to a char  
            CurrentCharacter = System.Convert.ToChar(InputText.Substring(i, 1))

            'Get the character code of the current character  
            CurrentCharacterCode = Microsoft.VisualBasic.Asc(CurrentCharacter)

            'Modify the character code of the character, - this  
            'so that "a" becomes "n", "z" becomes "m", "N" becomes "Y" and so on  
            If CurrentCharacterCode >= 97 And CurrentCharacterCode <= 109 Then
                CurrentCharacterCode = CurrentCharacterCode + 13

            Else
                If CurrentCharacterCode >= 110 And CurrentCharacterCode <= 122 Then
                    CurrentCharacterCode = CurrentCharacterCode - 13

                Else
                    If CurrentCharacterCode >= 65 And CurrentCharacterCode <= 77 Then
                        CurrentCharacterCode = CurrentCharacterCode + 13

                    Else
                        If CurrentCharacterCode >= 78 And CurrentCharacterCode <= 90 Then
                            CurrentCharacterCode = CurrentCharacterCode - 13
                        End If
                    End If
                End If 'Add the current character to the string to be returned
            End If
            EncodedText = EncodedText + Microsoft.VisualBasic.ChrW(CurrentCharacterCode)
        Next i

        Return EncodedText
    End Function 'ROT13Encode  

    ''' <summary>
    ''' Set a property or field given its name and a new value
    ''' </summary>
    ''' <param name="aObject">Object whose property or field needs to be set</param>
    ''' <param name="aFieldName">Name of property or field to set</param>
    ''' <param name="aValue">New value to set</param>
    Public Function SetSomething(ByRef aObject As Object, ByVal aFieldName As String, ByVal aValue As Object) As Boolean
        Dim lType As Type = aObject.GetType
        SetSomething = True
        Try
            Dim lProperty As Reflection.PropertyInfo = lType.GetProperty(aFieldName)
            If lProperty IsNot Nothing Then
                Select Case Type.GetTypeCode(lProperty.PropertyType)
                    Case TypeCode.Boolean : lProperty.SetValue(aObject, Boolean.Parse(aValue), Nothing)
                    Case TypeCode.Byte : lProperty.SetValue(aObject, Byte.Parse(aValue, System.Globalization.NumberStyles.Integer), Nothing)
                    Case TypeCode.Char : lProperty.SetValue(aObject, CChar(aValue), Nothing)
                    Case TypeCode.DateTime : lProperty.SetValue(aObject, Date.Parse(aValue, System.Globalization.CultureInfo.InvariantCulture), Nothing)
                    Case TypeCode.Decimal : lProperty.SetValue(aObject, Decimal.Parse(aValue, System.Globalization.CultureInfo.InvariantCulture), Nothing)
                    Case TypeCode.Double : lProperty.SetValue(aObject, Double.Parse(aValue, System.Globalization.CultureInfo.InvariantCulture), Nothing)
                    Case TypeCode.Int16 : lProperty.SetValue(aObject, Short.Parse(aValue, System.Globalization.NumberStyles.Integer), Nothing)
                    Case TypeCode.Int32 : lProperty.SetValue(aObject, Integer.Parse(aValue, System.Globalization.CultureInfo.InvariantCulture), Nothing)
                    Case TypeCode.Int64 : lProperty.SetValue(aObject, Long.Parse(aValue, System.Globalization.NumberStyles.Integer), Nothing)
                    Case TypeCode.SByte : lProperty.SetValue(aObject, SByte.Parse(aValue, System.Globalization.NumberStyles.Integer), Nothing)
                    Case TypeCode.Single : lProperty.SetValue(aObject, Single.Parse(aValue, System.Globalization.CultureInfo.InvariantCulture), Nothing)
                    Case TypeCode.String : lProperty.SetValue(aObject, CStr(aValue), Nothing)
                    Case TypeCode.UInt16 : lProperty.SetValue(aObject, UShort.Parse(aValue, System.Globalization.NumberStyles.Integer), Nothing)
                    Case TypeCode.UInt32 : lProperty.SetValue(aObject, UInteger.Parse(aValue, System.Globalization.NumberStyles.Integer), Nothing)
                    Case TypeCode.UInt64 : lProperty.SetValue(aObject, ULong.Parse(aValue, System.Globalization.NumberStyles.Integer), Nothing)
                    Case Else
                        'Logger.Dbg("Unable to set " & lType.Name & "." & aFieldName & ": unknown type " & lProperty.PropertyType.Name)
                        SetSomething = False
                End Select
            Else
                Dim lField As Reflection.FieldInfo = lType.GetField(aFieldName)
                If lField IsNot Nothing Then
                    Select Case Type.GetTypeCode(lField.FieldType)
                        Case TypeCode.Boolean : lField.SetValue(aObject, Boolean.Parse(aValue))
                        Case TypeCode.Byte : lField.SetValue(aObject, Byte.Parse(aValue, System.Globalization.NumberStyles.Integer))
                        Case TypeCode.Char : lField.SetValue(aObject, CChar(aValue))
                        Case TypeCode.DateTime : lField.SetValue(aObject, Date.Parse(aValue, System.Globalization.CultureInfo.InvariantCulture))
                        Case TypeCode.Decimal : lField.SetValue(aObject, Decimal.Parse(aValue, System.Globalization.CultureInfo.InvariantCulture))
                        Case TypeCode.Double : lField.SetValue(aObject, Double.Parse(aValue, System.Globalization.CultureInfo.InvariantCulture))
                        Case TypeCode.Int16 : lField.SetValue(aObject, Short.Parse(aValue, System.Globalization.NumberStyles.Integer))
                        Case TypeCode.Int32 : lField.SetValue(aObject, Integer.Parse(aValue, System.Globalization.CultureInfo.InvariantCulture))
                        Case TypeCode.Int64 : lField.SetValue(aObject, Long.Parse(aValue, System.Globalization.NumberStyles.Integer))
                        Case TypeCode.SByte : lField.SetValue(aObject, SByte.Parse(aValue, System.Globalization.NumberStyles.Integer))
                        Case TypeCode.Single : lField.SetValue(aObject, Single.Parse(aValue, System.Globalization.CultureInfo.InvariantCulture))
                        Case TypeCode.String : lField.SetValue(aObject, CStr(aValue))
                        Case TypeCode.UInt16 : lField.SetValue(aObject, UShort.Parse(aValue, System.Globalization.NumberStyles.Integer))
                        Case TypeCode.UInt32 : lField.SetValue(aObject, UInteger.Parse(aValue, System.Globalization.NumberStyles.Integer))
                        Case TypeCode.UInt64 : lField.SetValue(aObject, ULong.Parse(aValue, System.Globalization.NumberStyles.Integer))
                        Case Else
                            'Logger.Dbg("Unable to set " & lType.Name & "." & aFieldName & ": unknown type " & lField.FieldType.Name)
                            SetSomething = False
                    End Select
                Else
                    'Logger.Dbg("Unable to set " & lType.Name & "." & aFieldName & ": unknown field or property")
                    SetSomething = False
                End If
            End If
        Catch ex As Exception
            Dbg("SetSomething Exception: " & ex.Message)
            SetSomething = False
        End Try
    End Function

    Public Function GetAppSetting(ByVal aSettingName As String, ByVal aDefaultValue As Object) As Object
        Dim lSoftwareKey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software")
        If lSoftwareKey IsNot Nothing Then
            Dim lAppKey As Microsoft.Win32.RegistryKey = lSoftwareKey.OpenSubKey(g_AppName)
            If lAppKey IsNot Nothing Then Return lAppKey.GetValue(aSettingName, aDefaultValue)
        End If
        Return aDefaultValue
    End Function

    Public Sub SaveAppSetting(ByVal aSettingName As String, ByVal aValue As Object)
        Try
            Dim lSoftwareKey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software")
            If lSoftwareKey IsNot Nothing Then
                Dim lAppKey As Microsoft.Win32.RegistryKey = lSoftwareKey.CreateSubKey(g_AppName)
                If lAppKey IsNot Nothing Then lAppKey.SetValue(aSettingName, aValue)
            End If
        Catch e As Exception
            Dbg("Exception setting '" & aSettingName & "': " & e.Message)
        End Try
    End Sub

    'Open a file or URL using the default method the system would have used if it was double-clicked
    Public Sub OpenFileOrURL(ByVal aFileOrURL As String, ByVal aWait As Boolean)
        Dim newProcess As New Process
        Try
            If aFileOrURL <> "" Then
                'Use a .NET process() to launch the file or URL
                newProcess.StartInfo.FileName = aFileOrURL
                newProcess.Start()
                If aWait Then newProcess.WaitForExit()
            End If
        Catch ex As Exception
            Dbg("Exception opening '" & aFileOrURL & "': " & ex.Message)
        End Try
    End Sub

    Public Function EnsureDirForFile(ByVal aFilename As String) As Boolean
        Try
            Dim lDirectory As String = IO.Path.GetDirectoryName(aFilename)
            If lDirectory.Length > 1 Then
                If Not IO.Directory.Exists(lDirectory) Then
                    IO.Directory.CreateDirectory(lDirectory)
                    If Not IO.Directory.Exists(lDirectory) Then
                        Return False
                    End If
                End If
            End If
            Return True
        Catch
            Return False
        End Try
    End Function

    Public Sub DrawArrow(ByVal g As Graphics, ByVal aPen As Pen, ByVal aXcenter As Integer, ByVal aYcenter As Integer, ByVal aRadians As Double, ByVal aRadius As Integer)
        Dim ldx As Integer = Math.Sin(aRadians) * aRadius
        Dim ldy As Integer = Math.Cos(aRadians) * aRadius
        DrawArrow(g, aPen, aXcenter - ldx, aYcenter + ldy, aXcenter, aYcenter, aRadius)
    End Sub

    Public Sub DrawArrow(ByVal g As Graphics, ByVal aPen As Pen, _
                         ByVal aTailX As Integer, ByVal aTailY As Integer, _
                         ByVal aHeadX As Integer, ByVal aHeadY As Integer, ByVal aHeadLength As Double)
        'main line of arrow is easy
        'g.DrawLine(aPen, aTailX, aTailY, aHeadX, aHeadY)

        Dim aLeaf1X, aLeaf1Y, aLeaf2X, aLeaf2Y As Integer
        ArrowLeaves(aTailX, aTailY, aHeadX, aHeadY, aHeadLength, aLeaf1X, aLeaf1Y, aLeaf2X, aLeaf2Y)
        g.DrawLine(aPen, aLeaf1X, aLeaf1Y, aHeadX, aHeadY)
        g.DrawLine(aPen, aLeaf2X, aLeaf2Y, aHeadX, aHeadY)
    End Sub

    Public Sub ArrowLeaves(ByVal aTailX As Integer, ByVal aTailY As Integer, _
                           ByVal aHeadX As Integer, ByVal aHeadY As Integer, _
                           ByVal aHeadLength As Double, _
                           ByRef aLeaf1X As Integer, ByRef aLeaf1Y As Integer, _
                           ByRef aLeaf2X As Integer, ByRef aLeaf2Y As Integer)

        Dim psi As Double 'the angle of the vector from the tip to the start
        If aTailY = aHeadY Then
            If aTailX > aHeadX Then
                psi = Math.PI / 2
            Else
                psi = 3 * Math.PI / 2
            End If
        Else
            psi = Math.Atan2(aTailX - aHeadX, aTailY - aHeadY)
        End If
        Dim psi1 As Double = psi + Math.PI / 10
        Dim psi2 As Double = psi - Math.PI / 10
        aLeaf1X = CInt(aHeadX + aHeadLength * Math.Sin(psi1))
        aLeaf1Y = CInt(aHeadY + aHeadLength * Math.Cos(psi1))
        aLeaf2X = CInt(aHeadX + aHeadLength * Math.Sin(psi2))
        aLeaf2Y = CInt(aHeadY + aHeadLength * Math.Cos(psi2))
    End Sub

    Public Function TimeSpanString(ByVal aTimeSpan As TimeSpan) As String
        If aTimeSpan.TotalDays > 1 Then
            Return Math.Floor(aTimeSpan.TotalDays) & "d"
        ElseIf aTimeSpan.TotalHours > 1 Then
            Return Math.Floor(aTimeSpan.TotalHours) & "h"
        ElseIf aTimeSpan.TotalMinutes > 1 Then
            Return Math.Floor(aTimeSpan.TotalMinutes) & "m"
        Else
            Return ""
        End If
    End Function

    Public Function ReadTextFile(ByVal aFilename As String) As String
        Try
            If Not IO.File.Exists(aFilename) Then Return ""
#If Smartphone Then
            Dim lReader As IO.StreamReader = IO.File.OpenText(aFilename)
            ReadTextFile = lReader.ReadToEnd()
            lReader.Close()
#Else
        Return IO.File.ReadAllText(aFilename)
#End If
        Catch
            Return ""
        End Try
    End Function

    Public Sub WriteTextFile(ByVal aFilename As String, ByVal aContents As String)
#If Smartphone Then
        Dim lFile As IO.StreamWriter = IO.File.CreateText(aFilename)
        lFile.Write(aContents)
        lFile.Close()
#Else
        IO.File.WriteAllText(aFilename, aContents)
#End If
    End Sub

    Public Function DoubleTryParse(ByVal aString As String, ByRef aDouble As Double) As Boolean
#If Smartphone Then
        Try 'do not have TryParse available
            aDouble = Double.Parse(aString)
            Return True
        Catch ex As Exception
        End Try
#Else   'Use TryParse to avoid exception
        If Double.TryParse(aString, aDouble) Then Return True
#End If
        'See if we can find a Double by only using first part of aString
        Dim lCharIndex As Integer = 0
        Dim lChar As String
        While lCharIndex < aString.Length
            lChar = aString.Substring(lCharIndex, 1)
            If Not IsNumeric(lChar) Then
                Select Case lChar
                    Case "-", "."
                    Case Else : Exit While
                End Select
            End If
            lCharIndex += 1
        End While

        If lCharIndex > 0 AndAlso lCharIndex < aString.Length Then
            Return DoubleTryParse(aString.Substring(0, lCharIndex), aDouble)
        End If

        Return False
    End Function

    Public Function IntegerTryParse(ByVal aString As String, ByRef aInteger As Integer) As Boolean
#If Smartphone Then
        Try
            aInteger = Integer.Parse(aString)
            Return True
        Catch ex As Exception
        End Try
#Else
        If Integer.TryParse(aString, aInteger) Then Return True
#End If
        'See if we can find a number by only using first part of aString
        Dim lCharIndex As Integer = 0
        Dim lChar As String
        While lCharIndex < aString.Length
            lChar = aString.Substring(lCharIndex, 1)
            If Not IsNumeric(lChar) AndAlso lChar <> "-" Then
                Exit While
            End If
            lCharIndex += 1
        End While

        If lCharIndex > 0 AndAlso lCharIndex < aString.Length Then
            Return IntegerTryParse(aString.Substring(0, lCharIndex), aInteger)
        End If

        Return False
    End Function

    ' Return a random RGB color.
    Public Function RandomRGBColor(Optional ByVal aAlpha As Integer = 255) As Color
#If Smartphone Then 'Window Mobile does not have FromArgb with Alpha
        Return Color.FromArgb(g_Random.Next(0, 255), _
                              g_Random.Next(0, 255), _
                              g_Random.Next(0, 255))
#Else
        Return Color.FromArgb(aAlpha, _
                              g_Random.Next(0, 255), _
                              g_Random.Next(0, 255), _
                              g_Random.Next(0, 255))
#End If
    End Function

    Private pDebugMutex As New Threading.Mutex()
    Public Sub Dbg(ByVal aMessage As String)
        If g_Debug Then
            pDebugMutex.WaitOne()
            Debug.WriteLine(aMessage)
            Try
                If g_DebugWriter Is Nothing Then
                    g_DebugFilename = IO.Path.GetTempPath & "VataviaMapLog" & DateTime.Now.ToString("yyyy-MM-dd_HH-mm.ss") & ".txt"
                    g_DebugWriter = IO.StreamWriter.Synchronized(IO.File.AppendText(g_DebugFilename))
                End If
                g_DebugWriter.WriteLine(DateTime.Now.ToString("HH:mm:ss ") & aMessage)
                g_DebugWriter.Flush()
            Catch e As Exception
                'MsgBox("Could not log message to '" & g_DebugFilename & "'" & vbLf & e.Message & vbLf & aMessage)
            End Try
            pDebugMutex.ReleaseMutex()
        End If
    End Sub

End Module