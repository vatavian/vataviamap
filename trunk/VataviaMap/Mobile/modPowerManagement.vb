Module modPowerManagement

    Declare Sub SystemIdleTimerReset Lib "coredll" ()
    Private Declare Function SetPowerRequirement Lib "coredll.dll" (ByVal pvDevice As String, ByVal DeviceState As PowerState, ByVal DeviceFlags As Integer, ByVal pvSystemState As IntPtr, ByVal StateFlags As Integer) As IntPtr
    Private Declare Function ReleasePowerRequirement Lib "coredll.dll" (ByVal handle As IntPtr) As Integer
    'Private Declare Function GetIdleTime Lib "coredll" () As Integer

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

    Private Const POWER_FORCE As Integer = 4096

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
                    Debug.WriteLine(aDeviceName & ": SetPowerRequirement: " & e.Message)
                End Try
            End If
        Else
            If lHaveHandle AndAlso lHandle.ToInt32() <> 0 Then
                ReleasePowerRequirement(lHandle)
                pPowerHandle.Remove(aDeviceName)
            End If
        End If
    End Sub

    'Friend Sub PowerNeedGPS(ByVal aOn As Boolean)
    '    If aOn Then
    '        Try
    '            PowerHandleGPS = SetPowerRequirement("GPD0:", PowerState.D0, 4097, IntPtr.Zero, 0)
    '        Catch e As Exception
    '            Debug.WriteLine("GPD0: SetPowerRequirement: " & e.Message)
    '        End Try
    '        Try
    '            PowerHandleCOM = SetPowerRequirement("COM4:", PowerState.D0, 4097, IntPtr.Zero, 0)
    '        Catch e As Exception
    '            Debug.WriteLine("COM4: SetPowerRequirement: " & e.Message)
    '        End Try
    '    Else
    '        If PowerHandleGPS.ToInt32() <> 0 Then
    '            ReleasePowerRequirement(PowerHandleGPS)
    '            PowerHandleGPS = IntPtr.Zero
    '        End If
    '        If PowerHandleCOM.ToInt32() <> 0 Then
    '            ReleasePowerRequirement(PowerHandleCOM)
    '            PowerHandleCOM = IntPtr.Zero
    '        End If
    '    End If
    'End Sub

    'Friend Sub PowerNeedBacklight(ByVal aOn As Boolean)
    '    If aOn Then
    '        If PowerHandleBKL.ToInt32() = 0 Then
    '            Try
    '                PowerHandleBKL = SetPowerRequirement("BKL1:", PowerState.D0, 4097, IntPtr.Zero, 0)
    '            Catch ex As Exception
    '                Debug.WriteLine("BKL1: SetPowerRequirement: " & ex.Message)
    '            End Try
    '        End If
    '    Else
    '        If PowerHandleBKL.ToInt32() <> 0 Then
    '            ReleasePowerRequirement(PowerHandleBKL)
    '            PowerHandleBKL = IntPtr.Zero
    '        End If
    '    End If
    'End Sub

    ' Get the names of all of the subkeys that refer to hardware on the device.
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

End Module
