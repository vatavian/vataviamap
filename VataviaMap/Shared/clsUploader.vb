'This code allow asynchronous uploading while allowing the user to continue interacting with the user interface

Public Class clsUploader
    Inherits clsQueueManager

    Public Overrides Sub ServiceItem(ByVal aQueueItem As clsQueueItem)
        Select Case aQueueItem.ItemType
            'Case QueueItemType.TileItem
            '    UploadTile(lQueueItem.TilePoint, lQueueItem.Zoom, lQueueItem.ReplaceExisting)
            Case QueueItemType.PointItem
                UploadPoint(aQueueItem.URL)
                'Case QueueItemType.FileItem
                '    UploadFile(lQueueItem.URL, lQueueItem.Filename)
        End Select
    End Sub

    'Private Shared Sub UploadFile(ByVal aURL As String, ByVal aFilename As String)
    '    'TODO: include file name, file contents, user ID in upload
    '    'Dim lFeedbackCollection As New System.Collections.Specialized.NameValueCollection
    '    'lFeedbackCollection.Add("filename", aFilename)
    '    'lFeedbackCollection.Add("data", io.File.g aFilename)
    '    'lFeedbackCollection.Add("email", Trim(lEmail))
    '    'lFeedbackCollection.Add("message", Trim(lMessage))
    '    'lFeedbackCollection.Add("sysinfo", lFeedback)
    '    'Dim lClient As New System.Net.WebClient
    '    'lClient.UploadValues("http://hspf.com/cgi-bin/feedback-basins4.cgi", "POST", lFeedbackCollection)
    '    Dim lReq As System.Net.WebRequest = System.Net.WebRequest.Create(aURL)
    '    Try
    '        lReq.BeginGetResponse(Nothing, Nothing)
    '        lReq.GetResponse()
    '    Catch e As Exception
    '    End Try
    'End Sub

    Private Shared Sub UploadPoint(ByVal aURL As String)
        Dim lReq As System.Net.WebRequest = System.Net.WebRequest.Create(aURL)
        Try
            lReq.Timeout = 30000 'Milliseconds
            'lReq.BeginGetResponse(Nothing, Nothing)
            lReq.GetResponse()            
        Catch e As Exception
        End Try
    End Sub
End Class
