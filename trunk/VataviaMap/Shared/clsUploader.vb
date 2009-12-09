'This code allow asynchronous uploading while allowing the user to continue interacting with the user interface

'The portion of this module inside region "Krystalware" is based on code Copyright © 2009 Krystalware, Inc.,
' licensed under a Creative Commons Attribution-Share Alike 3.0 United States License
' http://creativecommons.org/licenses/by-sa/3.0/us/
' Original Krystalware code was found at: http://aspnetupload.com/Upload-File-POST-HttpWebRequest-WebClient-RFC-1867.aspx
' under "Advanced uploading with HttpWebRequest"
' Code downloaded from http://aspnetupload.com/Download-Source.aspx
' zip file at http://aspnetupload.com/AspNetUploadSamples.zip

Public Class clsUploader
    Inherits clsQueueManager

    Public Overrides Sub ServiceItem(ByVal aQueueItem As clsQueueItem)
        Select Case aQueueItem.ItemType
            'Case QueueItemType.TileItem
            '    UploadTile(lQueueItem.TilePoint, lQueueItem.Zoom, lQueueItem.ReplaceExisting)
            Case QueueItemType.PointItem
                UploadPoint(aQueueItem.URL)
            Case QueueItemType.FileItem
                UploadFile(aQueueItem.URL, aQueueItem.Filename)
        End Select
    End Sub

    Private Shared Sub UploadFile(ByVal aURL As String, ByVal aFilename As String)
        Try
            If IO.File.Exists(aFilename) Then
                Dim lFile As New clsFileToUpload(aFilename, "f", "application/octet-stream")
                Dim lFormVariables As New Collections.Specialized.NameValueCollection
                Dim lSplitURL() As String = aURL.Split("?")
                If lSplitURL.Length > 1 Then
                    For Each lArg As String In lSplitURL(1).Split("&")
                        Dim lArgNameValue() As String = lArg.Split("=")
                        lFormVariables.Add(lArgNameValue(0), lArgNameValue(1))
                    Next
                End If
                Dbg("Upload " & aURL)
                Dbg(Upload(lSplitURL(0), New clsFileToUpload() {lFile}, lFormVariables))
            End If
        Catch e As Exception
            Dbg("Exception in UploadFile: " & e.Message)
        End Try
    End Sub

    Private Shared Sub UploadPoint(ByVal aURL As String)
        Dim lReq As System.Net.WebRequest = System.Net.WebRequest.Create(aURL)
        Try
            lReq.Timeout = 30000 'Milliseconds
            lReq.GetResponse()
        Catch e As Exception
            Dbg("Exception in UploadPoint: " & e.Message)
        End Try
    End Sub

#Region "Krystalware"
    Private Shared Function Upload(ByVal url As String, ByVal files As clsFileToUpload(), ByVal form As Collections.Specialized.NameValueCollection) As String
        Dim resp As Net.HttpWebResponse = Upload(Net.WebRequest.Create(url), files, form)
        Using s As IO.Stream = resp.GetResponseStream()
            Using sr As New IO.StreamReader(s)
                Return sr.ReadToEnd()
            End Using
        End Using
    End Function

    Private Shared Function Upload(ByVal req As Net.HttpWebRequest, ByVal files As clsFileToUpload(), ByVal form As Collections.Specialized.NameValueCollection) As Net.HttpWebResponse
        Dim mimeParts As New List(Of clsMime)()

        Try
            For Each key As String In form.AllKeys
                Dim part As New clsMime
                part.Headers("Content-Disposition") = "form-data; name=""" & key & """"
                part.IOStream = New IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(form(key)))
                mimeParts.Add(part)
            Next

            Dim nameIndex As Integer = 0

            For Each file As clsFileToUpload In files
                Dim part As New clsMime

                If String.IsNullOrEmpty(file.FieldName) Then
                    file.FieldName = "file" & System.Math.Max(System.Threading.Interlocked.Increment(nameIndex), nameIndex - 1)
                End If

                part.Headers("Content-Disposition") = ("form-data; name=""" & file.FieldName & """; filename=""") + file.FileName & """"
                part.Headers("Content-Type") = file.ContentType

                part.IOStream = file.IOStream

                mimeParts.Add(part)
            Next

            Dim boundary As String = "HopefullyNoFileContainsThisString"

            req.ContentType = "multipart/form-data; boundary=" & boundary
            req.Method = "POST"

            Dim contentLength As Long = 0

            Dim lFooter As Byte() = System.Text.Encoding.UTF8.GetBytes("--" & boundary & "--" & vbCr & vbLf)

            For Each part As clsMime In mimeParts
                contentLength += part.GenerateHeaderFooterData(boundary)
            Next

            req.ContentLength = contentLength + lFooter.Length

            Dim buffer As Byte() = New Byte(8191) {}
            Dim afterFile As Byte() = System.Text.Encoding.UTF8.GetBytes(vbCr & vbLf)
            Dim read As Integer

            Using s As IO.Stream = req.GetRequestStream()
                For Each part As clsMime In mimeParts
                    s.Write(part.HeadersAsBytes, 0, part.HeadersAsBytes.Length)

                    While (InlineAssignHelper(read, part.IOStream.Read(buffer, 0, buffer.Length))) > 0
                        s.Write(buffer, 0, read)
                    End While

                    part.IOStream.Close() ' .Dispose()
                    part.IOStream = Nothing

                    s.Write(afterFile, 0, afterFile.Length)
                Next

                s.Write(lFooter, 0, lFooter.Length)
            End Using

            Return req.GetResponse
        Catch
            For Each part As clsMime In mimeParts
                If part.IOStream IsNot Nothing Then
                    part.IOStream.Close() ' .Dispose()
                    part.IOStream = Nothing
                End If
            Next

            Throw
        End Try
    End Function

    Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, ByVal value As T) As T
        target = value
        Return value
    End Function

    Public Class clsMime
        Public Headers As New Collections.Specialized.NameValueCollection()
        Public HeadersAsBytes As Byte()
        Public IOStream As IO.Stream

        Public Function GenerateHeaderFooterData(ByVal boundary As String) As Long
            Dim sb As New System.Text.StringBuilder()

            sb.Append("--")
            sb.Append(boundary)
            sb.Append(vbCrLf)
            For Each key As String In Headers.AllKeys
                sb.Append(key)
                sb.Append(": ")
                sb.Append(Headers(key))
                sb.Append(vbCrLf)
            Next
            sb.Append(vbCrLf)

            HeadersAsBytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString())

            Return HeadersAsBytes.Length + IOStream.Length + 2
        End Function
    End Class

    Public Class clsFileToUpload
        Public IOStream As IO.Stream
        Public FieldName As String
        Public FileName As String
        Public ContentType As String

        Public Sub New(ByVal aStream As IO.Stream, ByVal aFieldName As String, ByVal aFileName As String, ByVal aContentType As String)
            IOStream = aStream
            FieldName = aFieldName
            FileName = aFileName
            ContentType = aContentType
        End Sub

        Public Sub New(ByVal fileName As String, ByVal fieldName As String, ByVal contentType As String)
            Me.New(IO.File.OpenRead(fileName), fieldName, IO.Path.GetFileName(fileName), contentType)
        End Sub

        Public Sub New(ByVal fileName As String)
            Me.New(fileName, Nothing, "application/octet-stream")
        End Sub

    End Class
#End Region

End Class
