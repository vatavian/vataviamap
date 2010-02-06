Imports Microsoft.WindowsMobile.PocketOutlook.MessageInterception
Imports Microsoft.WindowsMobile.Status.SystemState

Module modMain

    Public Const g_AppName As String = "BackTracker"

    ' If an incoming SMS starts with this text, the message will be intercepted as a command
    Public pSMScode As String = GetAppSetting("SMScode", "adgj")

    ' URL of server to upload points to (and get commands from)
    Private pUploadURL As String = GetAppSetting("UploadURL", "http://vatavia.net/cgi-bin/gps?u=unknown&y=#Lat#&x=#Lon#&e=#Alt#&s=#Speed#&h=#Heading#&t=#Time#&l=#Label#&c=#CellID#")

    Private pSMSdestination As String = GetAppSetting("SMSdest", "")
    Private pSMShours As Integer = 48
    Private pSendNextSMS As Date = Date.UtcNow.AddHours(pSMShours)

    Private pAllGetProperties() As String = {"app", "bat", "pow", "cel", "cev", "cna", "pad", "pbs", "pcb", "pgc", "phs", "pio", "pnm", "pns", "pon", "pro", "prp", "pss", "tim", "lic", "cal", "tlk", "who", "gma", "guu", "guw", "gwf"}

    Private pMessageApplication As String = GetAppSetting("MessageApplication", "\Program Files\DataMessage\DataMessage.exe")
    Private pMessageProcess As Process

    ' Run any commands in this file, then rename it Ran
    Private pRunFilename As String = "\My Documents\Run.txt"
    Private pRanFilename As String = "\My Documents\Ran.txt"
    Private pStopFilename As String = "\My Documents\Stop.txt"
    Private pErrorFilename As String = "\My Documents\ErrLog.txt"
    Private pStoppedFilename As String = "\My Documents\Stopped.txt"

    Private GPS_Listen As Boolean = True
    Private WithEvents GPS As GPS_API.GPS

    Private pLastTimeUploaded As DateTime    'When we last checked server with or without reporting location (UTC)
    Private pLastTimeUploadedGPS As DateTime 'When we last uploaded a track point (UTC)
    Private pLastTimeGPS As Date = New Date(1, 1, 1) 'Last time reported by GPS
    Private pLastCellID As String = ""

    ' True to upload whenever we enter a new cell
    Private pUploadNewCells As Boolean = True

    ' Last instructions downloaded, remember them so we don't repeat
    Private pLastInstructions As String = GetAppSetting("LastInstructions", "none")

    ' Wait at most this long for a GPS fix
    Private pWaitForFixMinutes As Integer = 2

    ' Wait this long before trying for another GPS fix if the last try succeeded and battery is High
    Private pUploadWaitMinutesSuccess As Integer = 5

    ' Wait this long before trying for another GPS fix if the last try failed and battery is High
    Private pUploadWaitMinutesFailed As Integer = 15

    Private pUploadWaitMinutesGPS As Integer = pUploadWaitMinutesSuccess 'Wait this long till trying again to upload

    Private pUploadWaitMinutes As Integer = 1

    'Sleep this long before restarting main loop
    Private pSleepMilliseconds As Integer = 30000

    'Private Interceptor As MessageInterceptor

    Sub Main()
        'Interceptor = New MessageInterceptor(InterceptionAction.NotifyAndDelete)
        'Interceptor.MessageCondition = New MessageCondition(MessageProperty.Body, MessagePropertyComparisonType.Contains, pSMScode)
        'AddHandler Interceptor.MessageReceived, AddressOf Interceptor_MessageReceived

        pLastTimeUploaded = New DateTime(1, 1, 1)
        pLastTimeUploadedGPS = pLastTimeUploaded
        Try
            pUploadWaitMinutesSuccess = Integer.Parse(GetAppSetting("UploadMinutes", pUploadWaitMinutesSuccess))
        Catch
        End Try
        Dim lWaitMultiplier As Integer = 1
        While True

            If IO.File.Exists(pRunFilename) Then
                Dim lStream As IO.TextReader = IO.File.OpenText(pRunFilename)
                Dim lInstruction As String = lStream.ReadLine
                Dim lStop As Boolean = False
                While Not lStop AndAlso lInstruction IsNot Nothing
                    If Not lInstruction.StartsWith("#") Then
                        Select Case lInstruction.ToLower
                            Case "stop"
                                lStop = True
                            Case Else
                                ProcessInstruction(lInstruction)
                                lInstruction = lStream.ReadLine
                        End Select
                    End If
                End While
                lStream.Close()
                Try
                    If IO.File.Exists(pRanFilename) Then
                        Try
                            IO.File.Delete(pRanFilename)
                        Catch
                        End Try
                    End If
                    IO.File.Move(pRunFilename, pRanFilename)
                Catch
                End Try
                If lStop Then Exit While
            End If

            If IO.File.Exists(pStopFilename) Then
                Try
                    Dim lStream As IO.TextWriter = IO.File.CreateText(pStoppedFilename)
                    lStream.WriteLine(Now.ToShortDateString & "  " & Now.ToShortTimeString)
                    lStream.Close()
                Catch
                End Try
                Try
                    IO.File.Delete(pStopFilename)
                Catch
                End Try
                Exit While
            End If

            If CanSend() Then
                'If time to wait has elapsed or we upload on new cells and have entered a new cell
                If (DateTime.UtcNow.Subtract(New TimeSpan(0, pUploadWaitMinutesGPS * lWaitMultiplier, 0)) >= pLastTimeUploadedGPS) _
                   OrElse (pUploadNewCells AndAlso InNewCell()) Then
                    StartGPS()
                    System.Threading.Thread.Sleep(pWaitForFixMinutes * 60000)
                    If StopGPS() Then ' GPS was still searching for fix, report home without GPS location
                        SendLocation(Nothing)
                        pUploadWaitMinutesGPS = pUploadWaitMinutesFailed
                    Else
                        pUploadWaitMinutesGPS = pUploadWaitMinutesSuccess
                    End If
                    Select Case PowerBatteryStrength
                        Case Microsoft.WindowsMobile.Status.BatteryLevel.VeryLow : lWaitMultiplier = 20
                        Case Microsoft.WindowsMobile.Status.BatteryLevel.Low : lWaitMultiplier = 10
                        Case Microsoft.WindowsMobile.Status.BatteryLevel.Medium : lWaitMultiplier = 2
                        Case Else : lWaitMultiplier = 1
                    End Select
                ElseIf lWaitMultiplier = 1 AndAlso _
                       DateTime.UtcNow.Subtract(New TimeSpan(0, pUploadWaitMinutes, 0)) >= pLastTimeUploaded Then
                    SendLocation(Nothing)
                End If
            ElseIf pSMSdestination.Length > 0 AndAlso Not PhoneNoService AndAlso DateTime.UtcNow > pSendNextSMS Then
                StartGPS()
                System.Threading.Thread.Sleep(pWaitForFixMinutes * 60000)
                If StopGPS() Then SendLocation(Nothing)
            End If

            System.Threading.Thread.Sleep(pSleepMilliseconds)

        End While
        'RemoveHandler Interceptor.MessageReceived, AddressOf Interceptor_MessageReceived
        'Interceptor.Dispose()
        CloseMessage()
    End Sub

    'True if we have cellular data or ActiveSync or WiFi connection available
    Private Function CanSend() As Boolean
        Return PhoneGprsCoverage OrElse ConnectionsDesktopCount > 0 OrElse ConnectionsNetworkCount > 0
    End Function

    Private Sub LogEx(ByVal e As Exception, Optional ByVal aDetails As String = "")
        Dim lStream As IO.TextWriter = IO.File.CreateText(pErrorFilename)
        lStream.WriteLine(Now.ToShortDateString & "  " & Now.ToShortTimeString)
        If aDetails.Length > 0 Then lStream.WriteLine(aDetails)
        lStream.WriteLine(e.Message)
        lStream.WriteLine(e.StackTrace)
        lStream.Close()
    End Sub

    Private Function GetAppSetting(ByVal aSettingName As String, ByVal aDefaultValue As Object) As Object
        Try
            Dim lSoftwareKey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software")
            If lSoftwareKey IsNot Nothing Then
                Dim lAppKey As Microsoft.Win32.RegistryKey = lSoftwareKey.OpenSubKey(g_AppName)
                If lAppKey IsNot Nothing Then Return lAppKey.GetValue(aSettingName, aDefaultValue)
            End If
        Catch e As Exception
            LogEx(e, "GetAppSetting " & aSettingName)
        End Try
        Return aDefaultValue
    End Function

    Private Sub SaveAppSetting(ByVal aSettingName As String, ByVal aValue As Object)
        Try
            Dim lSoftwareKey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software")
            If lSoftwareKey IsNot Nothing Then
                Dim lAppKey As Microsoft.Win32.RegistryKey = lSoftwareKey.CreateSubKey(g_AppName)
                If lAppKey IsNot Nothing Then lAppKey.SetValue(aSettingName, aValue)
            End If
        Catch e As Exception
            LogEx(e, "SaveAppSetting " & aSettingName)
        End Try
    End Sub

    'True if we are in a new cell different from pLastCellID
    'False if in same cell as pLastCellID or if no valid cell ID is available
    Private Function InNewCell() As Boolean
        Dim lCellID As String = GPS_API.RIL.GetCellTowerString
        Return (lCellID <> pLastCellID) AndAlso (lCellID.Trim("0"c, "."c).Length > 0)
    End Function

    Private Sub StartGPS()
        If GPS Is Nothing Then GPS = New GPS_API.GPS
        If Not GPS.Opened Then
            SetPowerRequirement(DeviceGPS, True)
            GPS.Open()
            GPS_Listen = True
        End If
    End Sub

    ''' <summary>
    ''' Returns True if GPS was stopped during this call, False if GPS was already stopped before
    ''' </summary>
    Private Function StopGPS() As Boolean
        StopGPS = False
        GPS_Listen = False
        Threading.Thread.Sleep(100)

        If GPS IsNot Nothing Then
            If GPS.Opened Then
                GPS.Close()
                StopGPS = True
            End If
            GPS = Nothing
        End If
        SetPowerRequirement(DeviceGPS, False)
    End Function

    Private Sub GPS_DEVICE_LocationChanged(ByVal sender As Object, ByVal args As GPS_API.LocationChangedEventArgs) Handles GPS.LocationChanged
        If GPS_Listen Then
            Dim GPS_POSITION As GPS_API.GpsPosition = args.Position
            Try
                If (GPS.Opened _
                  AndAlso GPS_POSITION IsNot Nothing _
                  AndAlso GPS_POSITION.TimeValid _
                  AndAlso GPS_POSITION.LatitudeValid _
                  AndAlso GPS_POSITION.LongitudeValid _
                  AndAlso GPS_POSITION.SatellitesInSolutionValid _
                  AndAlso GPS_POSITION.SatelliteCount > 2 _
                  AndAlso GPS_POSITION.Time > pLastTimeGPS) Then
                    pLastTimeGPS = GPS_POSITION.Time
                    StopGPS()
                    SendLocation(GPS_POSITION)
                End If
            Catch e As Exception
                LogEx(e, "GPS_DEVICE_LocationChanged")
            End Try
        End If
    End Sub

    Private Sub BuildURL(ByRef aURL As String, ByVal aTag As String, ByVal aReplaceTrue As String, ByVal aReplaceFalse As String, ByVal aTest As Boolean)
        Dim lReplacement As String
        If aTest Then
            lReplacement = aReplaceTrue
        Else
            lReplacement = aReplaceFalse
        End If
        aURL = aURL.Replace("#" & aTag & "#", lReplacement)
    End Sub

    Private Sub SendLocation(ByVal GPS_POSITION As GPS_API.GpsPosition)
        Dim lURL As String = pUploadURL
        If lURL IsNot Nothing AndAlso lURL.Length > 0 Then
            Try
                If GPS_POSITION Is Nothing Then
                    BuildURL(lURL, "Time", "", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fff") & "Z", False)
                    BuildURL(lURL, "Lat", "", "", False)
                    BuildURL(lURL, "Lon", "", "", False)

                    BuildURL(lURL, "Alt", "", "", False)
                    BuildURL(lURL, "Speed", "", "", False)
                    BuildURL(lURL, "Heading", "", "", False)
                Else
                    BuildURL(lURL, "Time", GPS_POSITION.Time.ToString("yyyy-MM-ddTHH:mm:ss.fff") & "Z", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fff") & "Z", GPS_POSITION.TimeValid)
                    BuildURL(lURL, "Lat", GPS_POSITION.Latitude.ToString("#.########"), "", GPS_POSITION.LatitudeValid)
                    BuildURL(lURL, "Lon", GPS_POSITION.Longitude.ToString("#.########"), "", GPS_POSITION.LongitudeValid)

                    BuildURL(lURL, "Alt", GPS_POSITION.SeaLevelAltitude, "", GPS_POSITION.SeaLevelAltitudeValid)
                    BuildURL(lURL, "Speed", GPS_POSITION.Speed, "", GPS_POSITION.SpeedValid)
                    BuildURL(lURL, "Heading", GPS_POSITION.Heading, "", GPS_POSITION.HeadingValid)
                End If

                BuildURL(lURL, "Label", "BackTracker", "", True)
                If lURL.IndexOf("CellID") > -1 Then
                    Dim lCellId As String = GPS_API.RIL.GetCellTowerString
                    pLastCellID = lCellId
                    BuildURL(lURL, "CellID", lCellId, "", True)
                End If

                If DateTime.UtcNow > pSendNextSMS Then
                    Dim lSMS As New Microsoft.WindowsMobile.PocketOutlook.SmsMessage(pSMSdestination, lURL.Substring(lURL.IndexOf("?") + 1))
                    lSMS.Send()
                    pSendNextSMS = Date.UtcNow.AddHours(pSMShours)
                Else
                    SendURL(lURL, True)
                End If
            Catch e As Exception
                LogEx(e, lURL)
            End Try
            pLastTimeUploaded = DateTime.UtcNow
            If GPS_POSITION IsNot Nothing Then
                pLastTimeUploadedGPS = pLastTimeUploaded

            End If
        End If
    End Sub

    Public Function UrlEncode(ByVal input_url As String) As String
        Dim lResult As String = ""
        For Each ch As Char In input_url.ToCharArray
            If Char.IsLetterOrDigit(ch) Then
                lResult &= ch
            Else
                lResult &= "%" & Hex(Asc(ch)).PadLeft(2, "0")
            End If
        Next
        Return lResult
    End Function

    Private Sub SendURL(ByVal aURL As String, ByVal aProcessResults As Boolean)
        Dim lReq As System.Net.WebRequest = System.Net.WebRequest.Create(aURL)
        Try
            lReq.Timeout = 60000 'Milliseconds
            Dim lResp As System.Net.HttpWebResponse = lReq.GetResponse()
            If lResp.StatusCode = Net.HttpStatusCode.OK Then
                Dim lResponseStream As IO.Stream = lResp.GetResponseStream
                Dim lRespStr As String = ""
                Dim lBuffer(1024) As Byte
                Do
                    Dim lRespLen As Integer = lResponseStream.Read(lBuffer, 0, lBuffer.Length)
                    If lRespLen <= 0 Then Exit Do
                    If aProcessResults Then
                        For lIndex As Integer = 0 To lRespLen - 1
                            lRespStr &= Chr(lBuffer(lIndex))
                        Next
                    End If
                Loop
                lResp.Close()
                pSendNextSMS = Date.UtcNow.AddHours(pSMShours)
                'Debug.WriteLine(lRespStr)
                If aProcessResults Then
                    ProcessInstructions(lRespStr)
                End If
            End If
        Catch e As Exception
            Debug.WriteLine(e.Message)
        End Try
    End Sub

    'Save URL as file
    Private Function GetURL(ByVal aURL As String, ByVal aFilename As String) As String
        Dim lReq As System.Net.WebRequest = System.Net.WebRequest.Create(aURL)
        Try
            lReq.Timeout = 60000 'Milliseconds
            Dim lResp As Net.HttpWebResponse = lReq.GetResponse()
            If lResp.StatusCode = Net.HttpStatusCode.OK Then
                Dim targetDirectory As String = IO.Path.GetDirectoryName(aFilename)
                If (targetDirectory.Length > 0) Then IO.Directory.CreateDirectory(targetDirectory)
                Dim ContentStream As New IO.FileStream(aFilename, IO.FileMode.Create)
                Dim lResponseStream As IO.Stream = lResp.GetResponseStream
                Dim lBuffer(1024) As Byte
                Do
                    Dim lRespLen As Integer = lResponseStream.Read(lBuffer, 0, lBuffer.Length)
                    If lRespLen <= 0 Then Exit Do
                    ContentStream.Write(lBuffer, 0, lRespLen)
                Loop
                ContentStream.Close()
                lResp.Close()                
                Return ""
            End If
            Return lResp.StatusCode.ToString
        Catch e As Exception
            Debug.WriteLine(e.Message)
            Return e.Message
        End Try
    End Function

    Private Function TrimToWithinTag(ByVal aSource As String, ByVal aTag As String) As String
        Dim lFind As Integer = aSource.IndexOf("<" & aTag & ">")
        If lFind > 0 Then aSource = aSource.Substring(lFind + aTag.Length + 2)
        lFind = aSource.IndexOf("</" & aTag & ">")
        If lFind > 0 Then aSource = aSource.Substring(0, lFind)
        Return aSource.Trim
    End Function

    Private Sub ProcessInstructions(ByVal aInstructions As String)
        If aInstructions Is Nothing Then Return
        aInstructions = TrimToWithinTag(aInstructions, "html")
        If aInstructions <> pLastInstructions Then
            pLastInstructions = aInstructions
            SaveAppSetting("LastInstructions", pLastInstructions)
            For Each lInstruction As String In aInstructions.Split("|")
                ProcessInstruction(lInstruction.Trim)
            Next
        End If
    End Sub

    Private Function ProcessInstruction(ByVal aInstruction As String) As String
        Dim lMessage As String = ""
        Try
            Dim lDestination As String = aInstruction.Substring(0, 3).ToLower
            Dim lCommand As String = ""
            If aInstruction.Length > 4 Then lCommand = aInstruction.Substring(4, 3)
            Select Case lCommand                    
                Case "---" : lMessage = ""
                Case "app" : lMessage = ActiveApplication
                Case "bat" : lMessage = PowerBatteryStrength
                Case "pow" : lMessage = PowerBatteryState.ToString
                Case "cel" : lMessage = GPS_API.RIL.GetCellTowerString
                Case "cev" : lMessage = EventDetails(CalendarEvent)
                Case "cna" : lMessage = EventDetails(CalendarNextAppointment)
                Case "dir" : lMessage = String.Join(vbCrLf, IO.Directory.GetFileSystemEntries(aInstruction.Substring(8).Trim))
                Case "pad" : lMessage = PhoneActiveDataCall
                Case "pbs" : lMessage = PhoneBlockedSim
                Case "pcb" : lMessage = PhoneCellBroadcast
                Case "pgc" : lMessage = PhoneGprsCoverage
                Case "pho"
                    If Not PhoneRadioPresent Then
                        lMessage = "NoRadio"
                    ElseIf PhoneRadioOff Then
                        lMessage = "RadioOff"
                    ElseIf PhoneNoSim Then
                        lMessage = "NoSim"
                    ElseIf PhoneNoService Then
                        lMessage = "NoService"
                    Else
                        lMessage = PhoneOperatorName & ":" & GPS_API.RIL.GetCellTowerString _
                           & "s" & PhoneSignalStrength & " " _
                           & PhoneCellBroadcast & " " _
                           & ProcessInstruction("cal") & " - " _
                           & ProcessInstruction("tlk") _
                           & " last= " & ProcessInstruction("lic")
                    End If
                Case "phs" : lMessage = PhoneHomeService
                Case "pio" : lMessage = PhoneRingerOff
                Case "pnm" : lMessage = PhoneNoSim
                Case "pns" : lMessage = PhoneNoService
                Case "pon" : lMessage = PhoneOperatorName
                Case "pro" : lMessage = PhoneRadioOff
                Case "prp" : lMessage = PhoneRadioPresent
                Case "pss" : lMessage = PhoneSignalStrength
                Case "tim" : lMessage = Time

                Case "lic" : lMessage = PhoneLastIncomingCallerName & " " & PhoneLastIncomingCallerNumber
                Case "cal" : lMessage = PhoneIncomingCallerName & " " & PhoneIncomingCallerNumber
                Case "tlk" : lMessage = PhoneTalkingCallerName & " " & PhoneTalkingCallerNumber
                Case "who" : lMessage = OwnerName & vbCrLf & OwnerEmail & vbCrLf & OwnerPhoneNumber & vbCrLf & OwnerNotes

                Case "get"
                    Dim lURL As String = aInstruction.Substring(8).Trim
                    Dim lFilename As String = "\My Documents\download"
                    Dim lDelim As Integer = lURL.IndexOf("::")
                    If lDelim > 0 Then
                        lFilename = lURL.Substring(lDelim + 2)
                        lURL = lURL.Substring(0, lDelim)
                    End If
                    lMessage = GetURL(lURL, lFilename)

                Case "run"
                    Dim newProcess As New Process
                    Dim lFilename As String = aInstruction.Substring(8).Trim
                    Dim lDelim As Integer = lFilename.IndexOf("::")
                    If lDelim > 0 Then
                        newProcess.StartInfo.Arguments = lFilename.Substring(lDelim + 2)
                        lFilename = lFilename.Substring(0, lDelim)
                    End If
                    newProcess.StartInfo.FileName = lFilename
                    newProcess.Start()

                Case "sma" : pMessageApplication = aInstruction.Substring(8).Trim
                Case "gma" : lMessage = pMessageApplication

                Case "sp+" : SetPowerRequirement(aInstruction.Substring(8).Trim, True)
                Case "sp-" : SetPowerRequirement(aInstruction.Substring(8).Trim, False)
                Case "sp0" : SetPowerRequirement(aInstruction.Substring(8).Trim, PowerState.D0)
                Case "sp1" : SetPowerRequirement(aInstruction.Substring(8).Trim, PowerState.D1)
                Case "sp2" : SetPowerRequirement(aInstruction.Substring(8).Trim, PowerState.D2)
                Case "sp3" : SetPowerRequirement(aInstruction.Substring(8).Trim, PowerState.D3)
                Case "sp4" : SetPowerRequirement(aInstruction.Substring(8).Trim, PowerState.D4)
                Case "gad"
                    For Each lDeviceName As String In AllDeviceNames()
                        lMessage &= lDeviceName & vbCrLf
                    Next

                Case "suu" : pUploadURL = aInstruction.Substring(8).Trim
                Case "guu" : lMessage = pUploadURL

                Case "sus" : pUploadWaitMinutesSuccess = Integer.Parse(lDestination)
                Case "gus" : lMessage = pUploadWaitMinutesSuccess

                Case "suf" : pUploadWaitMinutesFailed = Integer.Parse(lDestination)
                Case "guf" : lMessage = pUploadWaitMinutesFailed

                Case "swf" : pWaitForFixMinutes = Integer.Parse(lDestination)
                Case "gwf" : lMessage = pWaitForFixMinutes

                Case "stp"
                    Dim lStream As IO.TextWriter = IO.File.CreateText(pStopFilename)
                    lStream.WriteLine(Now.ToShortDateString & "  " & Now.ToShortTimeString)
                    lStream.Close()

                Case "all"
                    For Each lProperty As String In pAllGetProperties
                        Try
                            lMessage &= lProperty & " = " & ProcessInstruction(lProperty) & vbCrLf
                        Catch e As Exception
                            lMessage &= lProperty & " error: " & e.Message
                        End Try
                    Next
                Case Else
                    If aInstruction.Length > 4 Then
                        lMessage = aInstruction.Substring(4).Trim
                    End If
            End Select

            Try
                Select Case lDestination
                    Case "msg"
                        If aInstruction.Length > 8 Then lMessage = aInstruction.Substring(8) & vbCrLf & lMessage
                        If IO.File.Exists(pMessageApplication) Then
                            CloseMessage()
                            pMessageProcess = Process.Start(pMessageApplication, UrlEncode(lMessage))
                        End If
                    Case "sms"
                        Dim lSendTo As String = aInstruction.Substring(8, 10)
                        If aInstruction.Length > 19 Then lMessage = aInstruction.Substring(19) & " " & lMessage
                        Dim lSMS As New Microsoft.WindowsMobile.PocketOutlook.SmsMessage(lSendTo, lMessage)
                        lSMS.Send()
                    Case "url"
                        'Don't process results of this URL, because that would defeat remembering pLastInstructions
                        SendURL(aInstruction.Substring(8).Trim & UrlEncode(lMessage), False)
                End Select
            Catch exDest As Exception
                lMessage = aInstruction & ":" & vbCrLf & exDest.Message
                Debug.WriteLine(lMessage)
            End Try

        Catch ex As Exception
            lMessage = aInstruction & vbCrLf & ex.Message
            Debug.WriteLine(lMessage)
        End Try
        Return lMessage
    End Function

    Private Sub CloseMessage()
        If pMessageProcess IsNot Nothing Then
            Try
                pMessageProcess.Kill()
            Catch
            End Try
        End If
    End Sub

    Private Function EventDetails(ByVal aEvent As Microsoft.WindowsMobile.PocketOutlook.Appointment) As String
        Dim lDetails As String = ""
        If aEvent IsNot Nothing Then
            With aEvent
                On Error Resume Next
                lDetails &= .Subject.Trim & " "
                If .Location.Length > 0 Then lDetails &= "at " & .Location & " "
                If .AllDayEvent Then lDetails &= "(all day) "
                lDetails &= Format(.Start, "yyyy-MM-dd") & " " & Format(.Start, "HH-mm") & " to " & _
                            Format(.End, "yyyy-MM-dd") & " " & Format(.End, "HH-mm ")
                lDetails &= .Categories.Trim & " "
                If .AllDayEvent Then lDetails &= "(recurring) "
                lDetails &= .Body.Trim & " "
            End With
        End If
        Return lDetails
    End Function

    'Private Sub Interceptor_MessageReceived(ByVal sender As Object, ByVal e As Microsoft.WindowsMobile.PocketOutlook.MessageInterception.MessageInterceptorEventArgs)
    '    Try
    '        Dim lMessage As Microsoft.WindowsMobile.PocketOutlook.SmsMessage = e.Message
    '        Try
    '            Dim lStream As IO.TextWriter = IO.File.AppendText("\My Documents\Msg.txt")
    '            Dim lTimestamp As String = Now.ToShortDateString & "  " & Now.ToShortTimeString & "  "
    '            lStream.WriteLine(lTimestamp & lMessage.Body)
    '            lStream.WriteLine(lTimestamp & lMessage.From.Address)
    '            lStream.WriteLine(lTimestamp & lMessage.From.Name)
    '            lStream.Close()
    '        Catch
    '        End Try
    '        ProcessInstructions(lMessage.Body.Substring(pSMScode.Length))
    '    Catch
    '    End Try
    'End Sub

#Region "PowerManagement"

    Private Declare Function GetIdleTime Lib "coredll" () As Integer
    Private Declare Function SetPowerRequirement Lib "coredll.dll" (ByVal pvDevice As String, ByVal DeviceState As PowerState, ByVal DeviceFlags As Integer, ByVal pvSystemState As IntPtr, ByVal StateFlags As Integer) As IntPtr
    Private Declare Function ReleasePowerRequirement Lib "coredll.dll" (ByVal handle As IntPtr) As Integer

    Private Enum PowerState
        PwrDeviceUnspecified = -1
        D0 = 0 'full on
        D1 = 1 'low power
        D2 = 2 'standby
        D3 = 3 'sleep
        D4 = 4 'off
        PwrDeviceMaximum = 5
    End Enum

    Private pPowerHandle As New Generic.Dictionary(Of String, IntPtr)

    'Private Const POWER_FORCE As Integer = 4096

    Friend DeviceBacklight As String = "BKL1:"
    Friend DeviceGPS As String = "GPD0:"

    Friend Sub SetPowerRequirement(ByVal aDeviceName As String, ByVal aNeeded As Boolean)
        Dim lHandle As IntPtr
        Dim lHaveHandle As Boolean = pPowerHandle.TryGetValue(aDeviceName, lHandle)
        If aNeeded Then
            If Not lHaveHandle Then
                Try
                    pPowerHandle.Add(aDeviceName, SetPowerRequirement(aDeviceName, PowerState.D0, 4097, IntPtr.Zero, 0))
                Catch e As Exception
                    LogEx(e, "SetPowerRequirementaDeviceName " & aDeviceName)
                End Try
            End If
        Else
            If lHaveHandle AndAlso lHandle.ToInt32() <> 0 Then
                ReleasePowerRequirement(lHandle)
                pPowerHandle.Remove(aDeviceName)
            End If
        End If
    End Sub

    Friend Function AllDeviceNames() As List(Of String)
        Dim driverKeyRoot As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("Drivers\Active")
        Dim keyName As String() = driverKeyRoot.GetSubKeyNames()

        Dim deviceNameList As New List(Of String)()
        For i As Integer = 0 To keyName.Length - 1
            'Get the name of the hardware and add it to the list
            Dim currentKey As Microsoft.Win32.RegistryKey = driverKeyRoot.OpenSubKey(keyName(i))
            Dim deviceName As String = TryCast(currentKey.GetValue("Name"), String)
            If deviceName IsNot Nothing Then
                Try
                    Debug.WriteLine(Format(i + 1, "00 ") & deviceName & " " & TryCast(currentKey.GetValue("Key"), String))
                Catch
                    Debug.WriteLine(Format(i + 1, "00 ") & deviceName)
                End Try
                deviceNameList.Add(deviceName)
            End If
        Next
        'Sort the list
        deviceNameList.Sort()
        Return deviceNameList
    End Function

#End Region

End Module
