Imports System.io
Imports System.Net

Module modGoogleCellLocation

    Private Const Google_Mobile_Service_Uri As String = "http://www.google.com/glm/mmap"

    Friend Function GetCellLocationFromGoogle(ByVal CellCacheFolder As String, ByVal TowerId As Integer, ByVal MobileCountryCode As Integer, ByVal MobileNetworkCode As Integer, ByVal LocationAreaCode As Integer, _
                                              ByRef Latitude As Double, ByRef Longitude As Double) As Boolean
        Try
            Dim lCellLocationFilename As String = IO.Path.Combine(CellCacheFolder, TowerId & "." & MobileCountryCode & "." & MobileNetworkCode & "." & LocationAreaCode)
            If IO.File.Exists(lCellLocationFilename) Then
                Dim lStream As New IO.FileStream(lCellLocationFilename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Dim lReader As IO.StreamReader = IO.File.OpenText(lCellLocationFilename)
                Latitude = Double.Parse(lReader.ReadLine)
                Longitude = Double.Parse(lReader.ReadLine)
                lReader.Close()
                Return True
            Else
                ' Translate cell tower data into http post parameter data
                Dim formData As Byte() = GetFormPostData(TowerId, MobileCountryCode, MobileNetworkCode, LocationAreaCode)

                Dim request As HttpWebRequest = CType(WebRequest.Create(New Uri(Google_Mobile_Service_Uri)), HttpWebRequest)
                request.Method = "POST"
                request.ContentLength = formData.Length
                request.ContentType = "application/binary"

                Dim outputStream As Stream = request.GetRequestStream()

                ' Write the cell data to the http stream
                outputStream.Write(formData, 0, formData.Length)
                outputStream.Close()

                Dim response As HttpWebResponse = CType(request.GetResponse(), HttpWebResponse)
                Dim br As New BinaryReader(response.GetResponseStream())
                Dim responseBytes() As Byte = br.ReadBytes(response.ContentLength)

                ' Check the response
                If response.StatusCode = HttpStatusCode.OK Then
                    Dim successful As Integer = Convert.ToInt32(GetCode(responseBytes, 3))
                    If successful = 0 Then
                        Latitude = GetCode(responseBytes, 7) / 1000000
                        Longitude = GetCode(responseBytes, 11) / 1000000
                        Try
                            IO.Directory.CreateDirectory(IO.Path.GetDirectoryName(lCellLocationFilename))
                            Dim lWriter As IO.StreamWriter = IO.File.CreateText(lCellLocationFilename)
                            lWriter.WriteLine(Latitude)
                            lWriter.WriteLine(Longitude)
                            lWriter.WriteLine(Format(Date.UtcNow, "yyyy-MM-dd HH:mm:ss"))
                            lWriter.WriteLine(Google_Mobile_Service_Uri)
                            lWriter.Close()
                        Catch e As Exception
                        End Try
                        Return True
                    End If
                End If
            End If
        Catch e As Exception
            Debug.WriteLine(e.Message)
        End Try
        Return False
    End Function 'GetLocation

    Private Function GetFormPostData(ByVal cellTowerId As Integer, ByVal mobileCountryCode As Integer, ByVal mobileNetworkCode As Integer, ByVal locationAreaCode As Integer) As Byte()
        Dim pd(55) As Byte
        pd(1) = 14 '0x0e;
        pd(16) = 27 '0x1b;
        pd(47) = 255 '0xff;
        pd(48) = 255 '0xff;
        pd(49) = 255 '0xff;
        pd(50) = 255 '0xff;
        ' GSM uses 4 digits while UTMS used 6 digits (hex)
        If CType(cellTowerId, Int64) > 65536 Then
            pd(28) = 5
        Else
            pd(28) = 3
        End If
        Shift(pd, 17, mobileNetworkCode)
        Shift(pd, 21, mobileCountryCode)
        Shift(pd, 31, cellTowerId)
        Shift(pd, 35, locationAreaCode)
        Shift(pd, 39, mobileNetworkCode)
        Shift(pd, 43, mobileCountryCode)

        Return pd
    End Function 'GetFormPostData

    '/ <summary>
    '/ Shifts specified data in the byte array starting at the specified array index.
    '/ </summary>
    '/ <param name="data">The data.</param>
    '/ <param name="startIndex">The start index.</param>
    '/ <param name="leftOperand">The left operand.</param>
    Private Sub Shift(ByVal data() As Byte, ByVal startIndex As Integer, ByVal leftOperand As Integer)
        Dim rightOperand As Integer = 24
        For i As Integer = 0 To 3
            data(startIndex) = CByte((leftOperand >> rightOperand) And 255)
            startIndex += 1
            rightOperand -= 8
        Next
    End Sub

    '/ <summary>
    '/ Gets the latitude or longitude from the byte array.
    '/ </summary>
    '/ <param name="data">The byte array.</param>
    '/ <param name="startIndex">The start index.</param>
    '/ <returns></returns>
    Private Function GetCode(ByVal data() As Byte, ByVal startIndex As Integer) As Double
        Return CDbl((CInt(data(startIndex)) << 24) Or (CInt(data(startIndex + 1)) << 16) Or (CInt(data(startIndex + 2)) << 8) Or CInt(data(startIndex + 3)))
    End Function
End Module
