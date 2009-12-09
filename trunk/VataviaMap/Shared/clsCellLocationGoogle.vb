Imports System.io
Imports System.Net
Imports GPS_API.RIL

Class clsCellLocationGoogle
    Inherits clsCellLocationProvider

    Private Const Google_Mobile_Service_Uri As String = "http://www.google.com/glm/mmap"

    Public Sub New()
        Me.pProviderCode = "Google"
    End Sub

    Public Overrides Function GetCellLocation(ByVal aCell As clsCell) As Boolean
        Try
            ' Translate cell tower data into http post parameter data
            Dim formData As Byte() = GetFormPostData(aCell)

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
                    aCell.Latitude = GetCode(responseBytes, 7) / 1000000
                    aCell.Longitude = GetCode(responseBytes, 11) / 1000000
                    Return True
                End If
            End If
        Catch e As Exception
            Dbg("GetCellLocation: " & e.Message)
        End Try
        Return False
    End Function 'GetLocation

    Private Shared Function GetFormPostData(ByVal aCell As clsCell) As Byte()
        Dim pd(55) As Byte
        pd(1) = 14 '0x0e;
        pd(16) = 27 '0x1b;
        pd(47) = 255 '0xff;
        pd(48) = 255 '0xff;
        pd(49) = 255 '0xff;
        pd(50) = 255 '0xff;
        ' GSM uses 4 digits while UTMS used 6 digits (hex)
        If CType(aCell.ID, Int64) > 65536 Then
            pd(28) = 5
        Else
            pd(28) = 3
        End If
        Shift(pd, 17, aCell.MNC)
        Shift(pd, 21, aCell.MCC)
        Shift(pd, 31, aCell.ID)
        Shift(pd, 35, aCell.LAC)
        Shift(pd, 39, aCell.MNC)
        Shift(pd, 43, aCell.MCC)

        Return pd
    End Function 'GetFormPostData

    '/ <summary>
    '/ Shifts specified data in the byte array starting at the specified array index.
    '/ </summary>
    '/ <param name="data">The data.</param>
    '/ <param name="startIndex">The start index.</param>
    '/ <param name="leftOperand">The left operand.</param>
    Private Shared Sub Shift(ByVal data() As Byte, ByVal startIndex As Integer, ByVal leftOperand As Integer)
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
    Private Shared Function GetCode(ByVal data() As Byte, ByVal startIndex As Integer) As Double
        Return CDbl((CInt(data(startIndex)) << 24) Or (CInt(data(startIndex + 1)) << 16) Or (CInt(data(startIndex + 2)) << 8) Or CInt(data(startIndex + 3)))
    End Function
End Class

