''' <summary>
''' Interface for any object that wants to hear when items are finished downloading
''' </summary>
''' <remarks></remarks>
Public Interface IQueueListener
    Sub DownloadedItem(ByVal aItem As clsQueueItem)
    Sub DownloadedTile(ByVal aTilePoint As Point, ByVal aZoom As Integer, ByVal aFilename As String, ByVal aTileServerURL As String)
    Sub FinishedQueue(ByVal aQueueIndex As Integer)
End Interface

Public Enum QueueItemType
    AnyItem
    FileItem
    IconItem
    PointItem
    TileItem
End Enum

''' <summary>
''' Structure to hold a request for a download or upload
''' </summary>
Public Class clsQueueItem
    Public ItemType As QueueItemType
    Public TilePoint As Point
    Public Zoom As Integer
    Public ReplaceExisting As Boolean = False
    Public URL As String
    Public Filename As String
    Public ItemObject As Object

    Public Overrides Function ToString() As String
        If ItemType = QueueItemType.TileItem Then
            Return Zoom & " " & TilePoint.ToString
        Else
            Return URL
        End If
    End Function
End Class

Public Class clsQueueManager

    Public Listeners As Generic.List(Of IQueueListener)

    Private pQueues As Generic.List(Of Generic.Queue(Of clsQueueItem))
    Private pQueueMutex As Threading.Mutex
    Private pQueueQuit As Boolean
    Private pQueueRunner As Threading.Thread

    Public Sub New()
        Listeners = New Generic.List(Of IQueueListener)
        pQueues = New Generic.List(Of Generic.Queue(Of clsQueueItem))
        pQueueMutex = New Threading.Mutex
        pQueueQuit = False
    End Sub

    Public Property Enabled() As Boolean
        Get
            Return pQueueRunner IsNot Nothing
        End Get
        Set(ByVal value As Boolean)
            If value Then
                pQueueQuit = False
                If pQueueRunner Is Nothing Then
                    pQueueRunner = New Threading.Thread(AddressOf RunQueue)
                    pQueueRunner.Start()
                End If
            Else
                pQueueQuit = True
                If pQueueRunner IsNot Nothing Then
                    'pQueueRunner.Join()
                    pQueueRunner = Nothing
                End If
            End If
        End Set
    End Property

    ''' <summary>
    ''' Keep track of the queue. Service the next queue item when the previous one is finished
    ''' </summary>
    ''' <remarks>
    ''' Only one thread, pQueueRunner, should be running RunQueue. 
    ''' No other code should call Dequeue
    ''' </remarks>
    Private Sub RunQueue()
        Dim lQueueIndex As Integer
        Dim lQueueItem As clsQueueItem
        Dim lFinishedQueue As Boolean
        Dim lQueue As Generic.Queue(Of clsQueueItem)
        While Not pQueueQuit
            pQueueMutex.WaitOne()
            lQueueIndex = 0
            ' Check queues in order 
            For Each lQueue In pQueues
                If lQueue.Count > 0 Then ' select next item in lowest queue with items                   
                    lQueueItem = lQueue.Peek
                    pQueueMutex.ReleaseMutex()

                    ServiceItem(lQueueItem)

                    pQueueMutex.WaitOne()
                    ' May not have to dequeue if ClearQueue has been called
                    ' Only qualifies as finishing the queue if ClearQueue was not called
                    If lQueue.Count > 0 AndAlso lQueueItem.Equals(lQueue.Peek) Then
                        lQueue.Dequeue()
                        If lQueue.Count = 0 Then lFinishedQueue = True
                    End If
                    pQueueMutex.ReleaseMutex()

                    If lFinishedQueue Then
                        lFinishedQueue = False
                        For Each lListener As IQueueListener In Listeners
                            lListener.FinishedQueue(lQueueIndex)
                        Next
                    End If

                    GoTo NextUpload ' Skip checking lower priority queues and sleeping
                End If
                lQueueIndex += 1
            Next
            ' Did not find any non-empty queues, release and sleep before checking again
            pQueueMutex.ReleaseMutex()
            Threading.Thread.Sleep(100)
NextUpload:
        End While
    End Sub

    Public Overridable Sub ServiceItem(ByVal aQueueItem As clsQueueItem)
        'Inheriting class will want to override
    End Sub

    Public Sub Enqueue(ByVal aTilePoint As Point, _
                       ByVal aZoom As Integer, _
              Optional ByVal aPriority As Integer = 0, _
              Optional ByVal aReplaceExisting As Boolean = False)
        If ValidTilePoint(aTilePoint, aZoom) Then
            Dim lFoundMatch As Boolean = False
            pQueueMutex.WaitOne()
            While aPriority >= pQueues.Count
                pQueues.Add(New Generic.Queue(Of clsQueueItem))
            End While
            Dim lQueue As Generic.Queue(Of clsQueueItem) = pQueues(aPriority)
            For Each lQueuedArg As clsQueueItem In lQueue
                With lQueuedArg
                    If .ItemType = QueueItemType.TileItem AndAlso .TilePoint.X = aTilePoint.X AndAlso .TilePoint.Y = aTilePoint.Y AndAlso .Zoom = aZoom Then
                        'Debug.WriteLine("Not adding duplicate request to queue")
                        lFoundMatch = True
                        Exit For
                    End If
                End With
            Next
            If Not lFoundMatch Then
                Dim lQueueItem As New clsQueueItem
                With lQueueItem
                    .ItemType = QueueItemType.TileItem
                    .ReplaceExisting = aReplaceExisting
                    .TilePoint = aTilePoint
                    .Zoom = aZoom
                End With
                Dbg("Enqueue " & lQueueItem.ToString)
                lQueue.Enqueue(lQueueItem)
            End If
            pQueueMutex.ReleaseMutex()
        End If
    End Sub

    Public Sub Enqueue(ByVal aURL As String, _
                       ByVal aFilename As String, _
                       ByVal aItemType As QueueItemType, _
              Optional ByVal aPriority As Integer = 0, _
              Optional ByVal aReplaceExisting As Boolean = True, _
              Optional ByVal aObject As Object = Nothing)
        Dim lFoundMatch As Boolean = False
        pQueueMutex.WaitOne()
        While aPriority >= pQueues.Count 'Create any queues that do not yet exist at or above this priority
            pQueues.Add(New Generic.Queue(Of clsQueueItem))
        End While
        Dim lQueue As Generic.Queue(Of clsQueueItem) = pQueues(aPriority)
        For Each lQueuedArg As clsQueueItem In lQueue
            With lQueuedArg
                If .URL = aURL Then
                    'Debug.WriteLine("Not adding duplicate request to queue")
                    lFoundMatch = True
                    Exit For
                End If
            End With
        Next
        If Not lFoundMatch Then
            Dim lQueueItem As New clsQueueItem
            With lQueueItem
                .ItemType = aItemType
                .URL = aURL
                .Filename = aFilename
                .ReplaceExisting = aReplaceExisting
                .ItemObject = aObject
            End With
            Dbg("Enqueue " & lQueueItem.ToString)
            lQueue.Enqueue(lQueueItem)
        End If
        pQueueMutex.ReleaseMutex()
    End Sub

    ''' <summary>
    ''' Clear the queue of items at the given priority, or all queued items
    ''' </summary>
    ''' <param name="aItemType">Type of items to clear from queue</param>
    ''' <param name="aPriority">Priority of queue to clear, -1 clears all queues</param>
    Public Sub ClearQueue(Optional ByVal aItemType As QueueItemType = QueueItemType.AnyItem, Optional ByVal aPriority As Integer = -1)
        pQueueMutex.WaitOne()
        If pQueues IsNot Nothing Then
            If aPriority > -1 Then 'Clear requested queue
                While aPriority >= pQueues.Count
                    pQueues.Add(New Generic.Queue(Of clsQueueItem))
                End While
                ClearQueue(pQueues(aPriority), aItemType)
            Else 'Clear all queues
                For Each lDownloadQueue As Generic.Queue(Of clsQueueItem) In pQueues
                    ClearQueue(lDownloadQueue, aItemType)
                Next
                pQueues.Clear()
            End If
        End If
        pQueueMutex.ReleaseMutex()
    End Sub

    Private Sub ClearQueue(ByVal aQueue As Generic.Queue(Of clsQueueItem), ByVal aItemType As QueueItemType)
        If aItemType = QueueItemType.AnyItem Then
            aQueue.Clear()
        Else
            Dim lItem As clsQueueItem
            Dim lSaveThese As New Generic.List(Of clsQueueItem)
            For Each lItem In aQueue
                If lItem.ItemType <> aItemType Then
                    lSaveThese.Add(lItem)
                End If
            Next
            aQueue.Clear()
            For Each lItem In lSaveThese
                aQueue.Enqueue(lItem)
            Next
        End If
    End Sub
End Class
