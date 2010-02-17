Imports System.Text
Imports System.Collections

Imports atcUtility

Module modMain
    Public pLocation As String = "dekalb" '"intownATL" '"decatur", "metroATL", "intownATL" 
    Public pTerse As Boolean = True
    Public pReferenceMax As Integer = 100000

    Public pSB As New StringBuilder
    Public pBounds As Bounds
    Public pMinLat As Double = 90
    Public pMaxLat As Double = -90
    Public pMinLon = 180
    Public pMaxLon = -180
    Public pXmlFileName As String
    Public pXmlNodeTypeCounts As New atcCollection
    Public pTagsSkip() As String = {"tiger:tlid", "tiger:name_base", "tiger:name_base_1", "tiger:name_base_2", "tiger:name_base_3", "tiger:name_base_4", "tiger:name_base_5", _
                                    "gnis:feature_id", "gnis:id", _
                                    "NHD:ComID", "NHD:way_id", "NHD:ReachCode", "NHD:Elevation", "NHD:GNIS_ID"}

    Sub Initialize()
        'IO.Directory.SetCurrentDirectory("..\..\..\osm_data")
        IO.Directory.SetCurrentDirectory("C:\mapnik_0_6_1\decatur\download")

        Select Case pLocation
            Case "dekalb"
                pXmlFileName = "georgia.osm"
                pMinLat = 33.61
                pMaxLat = 33.97
                pMinLon = -84.34
                pMaxLon = -84.03
            Case "decatur"
                pXmlFileName = "decatur.osm"
                pMinLat = 33.58
                pMaxLat = 33.95
                pMinLon = -84.54
                pMaxLon = -84.19
            Case "intownATL" '(inside 285)
                pXmlFileName = "georgia.osm"
                pMinLat = 33.58
                pMaxLat = 33.95
                pMinLon = -84.54
                pMaxLon = -84.19
            Case "metroATL"
                pXmlFileName = "georgia.osm"
                pMinLat = 33.0
                pMaxLat = 34.5
                pMinLon = -85.0
                pMaxLon = -83.75
        End Select
    End Sub

    Sub main()
        Initialize()

        Dim lXml As New Xml.XmlDocument
        Dim lSettings As New XmlReaderSettings()
        lSettings.IgnoreWhitespace = True
        Dim lXmlReader As XmlReader = XmlReader.Create(pXmlFileName, lSettings)
        lXmlReader.MoveToContent()
        Dim lXmlReadCount As Integer = 0
        Dim lStartTime As Date = Now
        While Not lXmlReader.EOF
            If lXmlReader.Name = "osm" Then
                lXmlReader.Read()
            Else
                Dim lXmlNode As XmlNode = lXml.ReadNode(lXmlReader)
                If lXmlReadCount Mod 10000 = 0 Then
                    Debug.Print(Now & " Elapsed " & CInt((Now - lStartTime).TotalMinutes) & " Done " & lXmlReadCount & " " & _
                                Nodes.Count & " " & Ways.Count & " " & Relations.Count & " " & MemUsage())
                End If
                lXmlReadCount += 1
                pXmlNodeTypeCounts.Increment(lXmlNode.Name, 1)
                Select Case lXmlNode.Name
                    Case "node"
                        Dim lNode As Node = New Node(lXmlNode)
                        With lNode
                            If .Lat > pMinLat AndAlso .Lat < pMaxLat AndAlso _
                               .Lon > pMinLon AndAlso .Lon < pMaxLon Then
                                Nodes.Add(lNode)
                                Tags.AddTags(.Tags, lNode)
                                If .User.Length > 0 Then
                                    Users.AddReference(.User, lNode)
                                End If
                            End If
                        End With
                    Case "way"
                        Dim lWay As Way = New Way(lXmlNode)
                        With lWay
                            If .NodeKeys.Count > 0 Then
                                Ways.Add(lWay)
                                Tags.AddTags(.Tags, lWay)
                                If .User.Length > 0 Then
                                    Users.AddReference(.User, lWay)
                                End If
                            End If
                        End With
                    Case "relation"
                        Dim lRelation As Relation = New Relation(lXmlNode)
                        With lRelation
                            If .NodeKeys.Count > 0 OrElse .WayKeys.Count > 0 Then
                                Relations.Add(lRelation)
                                Tags.AddTags(.Tags, lRelation)
                                If .User.Length > 0 Then
                                    Users.AddReference(.User, lRelation)
                                End If
                            End If
                        End With
                    Case "bounds", "bound"
                        pBounds = New Bounds(lXmlNode)
                    Case Else
                        pSB.AppendLine(vbCrLf & lXmlNode.OuterXml)
                End Select
            End If
        End While

        Debug.Print(Now & " Elapsed " & CInt((Now - lStartTime).TotalMinutes) & " Done " & lXmlReadCount & " " & _
            Nodes.Count & " " & Ways.Count & " " & Relations.Count & " " & MemUsage())

        pSB.AppendLine(vbCrLf & "XMLNodeCounts")
        For lIndex As Integer = 0 To pXmlNodeTypeCounts.Count - 1
            pSB.Append(vbTab & (pXmlNodeTypeCounts.Keys(lIndex) & ":").ToString.PadRight(10) & _
                                pXmlNodeTypeCounts.ItemByIndex(lIndex).ToString.PadLeft(12))
            Select Case (pXmlNodeTypeCounts.Keys(lIndex))
                Case "node" : pSB.Append(Nodes.Count.ToString.PadLeft(12))
                Case "way" : pSB.Append(Ways.Count.ToString.PadLeft(12))
                Case "relation" : pSB.Append(Relations.Count.ToString.PadLeft(12))
            End Select
            pSB.AppendLine()
        Next

        pSB.AppendLine(vbCrLf & "TagNames " & Tags.TagNames.Count)
        For Each lTagName As KeyValuePair(Of String, SortedList) In Tags.TagNames
            Dim lValueCollection As SortedList = lTagName.Value
            pSB.AppendLine(vbTab & lTagName.Key & "(" & lValueCollection.Count & ")")
        Next

        pSB.Append(Ways.Summary)
        pSB.Append(Relations.Summary)
        pSB.Append(Users.Summary)
        pSB.AppendLine(pBounds.Summary)

        Dim lOutFileName As String = pLocation & Format(IO.File.GetCreationTime(pXmlFileName), "yyyy-MM-dd-hh-mm") & "Summary.txt"
        IO.File.WriteAllText(lOutFileName, pSB.ToString)

        pSB = Nothing
        pSB = New StringBuilder
        pSB.AppendLine(vbCrLf & "TagNames " & Tags.TagNames.Count)
        Dim pXmlSB As New StringBuilder
        For Each lTagName As KeyValuePair(Of String, SortedList) In Tags.TagNames
            Dim lValueCollection As SortedList = lTagName.Value
            pSB.AppendLine(vbCrLf & lTagName.Key & "(" & lValueCollection.Count & ")")
            If Not pTagsSkip.Contains(lTagName.Key) Then
                For Each lValueEntry As DictionaryEntry In lValueCollection
                    Dim lReferenceCollection As SortedList = lValueEntry.Value
                    pSB.AppendLine(vbTab & lValueEntry.Key & " (" & lReferenceCollection.Count & ")")
                    If Not pTerse OrElse (lTagName.Key = "building") Then
                        Dim lReferenceMax As Integer = Math.Min(lReferenceCollection.Count - 1, pReferenceMax)
                        For lReferenceIndex As Integer = 0 To lReferenceMax
                            Dim lReference As Object = lReferenceCollection.Values(lReferenceIndex)
                            pSB.AppendLine(vbTab & vbTab & lReference.GetType.Name & ":" & lReference.id)
                            Select Case lReference.GetType.Name
                                Case "Node"
                                    Dim lNode As Node = lReference
                                    pXmlSB.AppendLine(lNode.XML.OuterXml)
                                Case "Way"
                                    Dim lWay As Way = lReference
                                    For lNodeIndex As Integer = 0 To lWay.NodeKeys.Count - 1
                                        Dim lNode As Node = Nodes(lWay.NodeKeys(lNodeIndex))
                                        pXmlSB.AppendLine(lNode.XML.OuterXml)
                                    Next
                                Case "Relation"
                                    'TODO: something here
                                Case Else
                                    Debug.Print("Why!")
                            End Select
                        Next
                        If lReferenceMax < lReferenceCollection.Count - 1 Then
                            pSB.AppendLine(vbTab & vbTab & "...")
                        End If
                    End If
                Next
            End If
        Next
        lOutFileName = pLocation & Format(IO.File.GetCreationTime(pXmlFileName), "yyyy-MM-dd-hh-mm") & "Tags.txt"
        IO.File.WriteAllText(lOutFileName, pSB.ToString)
        lOutFileName = pLocation & Format(IO.File.GetCreationTime(pXmlFileName), "yyyy-MM-dd-hh-mm") & "Buildings.xml"
        IO.File.WriteAllText(lOutFileName, pXmlSB.ToString)
    End Sub

    Private Function MemUsage() As String
        System.GC.WaitForPendingFinalizers()
        Return "MemoryUsage(MB):" & Process.GetCurrentProcess.PrivateMemorySize64 / (2 ^ 20) & _
                    " Local(MB):" & System.GC.GetTotalMemory(True) / (2 ^ 20)
    End Function
End Module

