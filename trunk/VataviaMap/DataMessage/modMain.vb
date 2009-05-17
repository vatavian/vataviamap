Module modMain
    Dim pForm As frmMain
    Sub Main(ByVal args() As String)
        pForm = New frmMain
        For Each lArg As String In args
            pForm.txtReceive.Text &= UrlDecode(args(0)) & vbCrLf
        Next
        pForm.ShowDialog()
    End Sub

    Friend Function UrlEncode(ByVal input_url As String) As String
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

    Private Function HexCharToInteger(ByVal ch As Char) As Integer
        Dim lCode As Integer = 0
        If Char.IsDigit(ch) Then
            lCode += (Asc(ch) - 48)
        ElseIf Char.IsUpper(ch) Then
            lCode += (Asc(ch) - 55)
        Else
            lCode += (Asc(ch) - 87)
        End If
        Return lCode
    End Function

    Public Function UrlDecode(ByVal input_url As String) As String
        Dim lResult As String = ""
        Dim lIndex As Integer = 0
        While lIndex < input_url.Length
            Dim ch As Char = input_url(lIndex)
            If ch = "%"c Then
                Dim lCode As Integer = 16 * HexCharToInteger(input_url(lIndex + 1))
                lCode += HexCharToInteger(input_url(lIndex + 2))
                lResult &= Chr(lCode)
                lIndex += 3
            Else
                lResult &= ch
                lIndex += 1
            End If
        End While
        Return lResult
    End Function

    'Friend Function Encode64(ByVal aEncodeThis As String) As String
    '    Return Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(aEncodeThis))
    'End Function

    'Friend Function Decode64(ByVal aEncoded As String) As String
    '    Dim byt() As Byte = System.Convert.FromBase64String(aEncoded)
    '    Return System.Text.Encoding.ASCII.GetString(byt, 0, byt.Length)
    'End Function

    Friend Function SendURL(ByVal aURL As String) As String
        Dim lReq As System.Net.WebRequest = System.Net.WebRequest.Create(aURL)
        Try
            lReq.Timeout = 60000 'Milliseconds
            'lReq.BeginGetResponse(Nothing, Nothing)
            Dim lResp As System.Net.WebResponse = lReq.GetResponse

            Dim lBuffer(1024) As Byte
            Dim lRespLen As Integer = lResp.GetResponseStream.Read(lBuffer, 0, lBuffer.Length)
            Return System.Text.Encoding.ASCII.GetString(lBuffer, 0, lRespLen)
        Catch e As Exception
            Return e.Message
        End Try
    End Function

    'Dim SMSservers() As String = {"itelemigcelular.com.br", _
    '                              "message.alltel.com", _
    '                              "message.pioneerenidcellular.com", _
    '                              "messaging.cellone-sf.com", _
    '                              "messaging.centurytel.net", _
    '                              "messaging.sprintpcs.com", _
    '                              "mobile.att.net", _
    '                              "mobile.cell1se.com", _
    '                              "mobile.celloneusa.com", _
    '                              "mobile.dobson.net", _
    '                              "mobile.mycingular.com", _
    '                              "mobile.mycingular.net", _
    '                              "mobile.surewest.com", _
    '                              "msg.acsalaska.com", _
    '                              "msg.clearnet.com", _
    '                              "msg.mactel.com", _
    '                              "msg.myvzw.com", _
    '                              "msg.telus.com", _
    '                              "mycellular.com", _
    '                              "mycingular.com", _
    '                              "mycingular.net", _
    '                              "mycingular.textmsg.com", _
    '                              "o2.net.br", _
    '                              "ondefor.com", _
    '                              "pcs.rogers.com", _
    '                              "personal-net.com.ar", _
    '                              "personal.net.py", _
    '                              "portafree.com", _
    '                              "qwest.com", _
    '                              "qwestmp.com", _
    '                              "sbcemail.com", _
    '                              "sms.bluecell.com", _
    '                              "sms.cwjamaica.com", _
    '                              "sms.edgewireless.com", _
    '                              "sms.hickorytech.com", _
    '                              "sms.net.nz", _
    '                              "sms.pscel.com", _
    '                              "smsc.vzpacifica.net", _
    '                              "speedmemo.com", _
    '                              "suncom1.com", _
    '                              "sungram.com", _
    '                              "telesurf.com.py", _
    '                              "teletexto.rcp.net.pe", _
    '                              "text.houstoncellular.net", _
    '                              "text.telus.com", _
    '                              "timnet.com", _
    '                              "timnet.com.br", _
    '                              "tms.suncom.com", _
    '                              "tmomail.net", _
    '                              "tsttmobile.co.tt", _
    '                              "txt.bellmobility.ca", _
    '                              "typetalk.ruralcellular.com", _
    '                              "unistar.unifon.com.ar", _
    '                              "uscc.textmsg.com", _
    '                              "voicestream.net", _
    '                              "vtext.com", _
    '                              "wireless.bellsouth.com"}

End Module
