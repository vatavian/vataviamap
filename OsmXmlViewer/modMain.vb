Imports System.Text
Imports System.Collections

Imports atcUtility

Module modMain
    Public pSB As New StringBuilder
    Public pBounds As Bounds
    'intown ATL (inside 285)
    'Public pMinLat As Double = 33.58
    'Public pMaxLat As Double = 33.95
    'Public pMinLon = -84.54
    'Public pMaxLon = -84.19
    'more metro ATL
    Public pMinLat As Double = 33.0
    Public pMaxLat As Double = 34.5
    Public pMinLon = -85.0
    Public pMaxLon = -83.75

    Sub main()
        'IO.Directory.SetCurrentDirectory("..\..\..\osm_data")
        IO.Directory.SetCurrentDirectory("C:\mapnik_0_6_1\decatur\download")

        Dim lXmlNodeTypeCounts As New atcCollection

        'Dim lXmlFleName As String = "decatur.mdc"
        'Dim lXmlFleName As String = "decatur.osm"
        'Dim lXmlFleName As String = "osm.xml"
        Dim lXmlFleName As String = "georgia.osm"

        Dim lXml As New Xml.XmlDocument
        Dim lSettings As New XmlReaderSettings()
        lSettings.IgnoreWhitespace = True
        Dim lXmlReader As XmlReader = XmlReader.Create(lXmlFleName, lSettings)
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
                lXmlNodeTypeCounts.Increment(lXmlNode.Name, 1)
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
        For lIndex As Integer = 0 To lXmlNodeTypeCounts.Count - 1
            pSB.Append(vbTab & (lXmlNodeTypeCounts.Keys(lIndex) & ":").ToString.PadRight(10) & _
                                lXmlNodeTypeCounts.ItemByIndex(lIndex).ToString.PadLeft(12))
            Select Case (lXmlNodeTypeCounts.Keys(lIndex))
                Case "node" : pSB.Append(Nodes.Count.ToString.PadLeft(12))
                Case "way" : pSB.Append(Ways.Count.ToString.PadLeft(12))
                Case "relation" : pSB.Append(Relations.Count.ToString.PadLeft(12))
            End Select
            pSB.AppendLine()
        Next

        pSB.AppendLine(vbCrLf & "TagNames " & Tags.TagNames.Count)
        For Each lTagName As KeyValuePair(Of String, SortedList) In Tags.TagNames
            If lTagName.Key <> "tiger:tlid" Then
                Dim lValueCollection As SortedList = lTagName.Value
                pSB.AppendLine(vbCrLf & lTagName.Key & "(" & lValueCollection.Count & ")")
                For Each lValueEntry As DictionaryEntry In lValueCollection
                    Dim lReferenceCollection As SortedList = lValueEntry.Value
                    pSB.AppendLine(vbTab & lValueEntry.Key & " (" & lReferenceCollection.Count & ")")
                    Dim lReferenceMax As Integer = Math.Min(lReferenceCollection.Count - 1, 20)
                    For lReferenceIndex As Integer = 0 To lReferenceMax
                        Dim lReference As Object = lReferenceCollection.Values(lReferenceIndex)
                        pSB.AppendLine(vbTab & vbTab & lReference.GetType.Name & ":" & lReference.id)
                    Next
                    If lReferenceMax < lReferenceCollection.Count - 1 Then
                        pSB.AppendLine(vbTab & vbTab & "...")
                    End If
                Next
            End If
        Next

        pSB.Append(Ways.Summary)
        pSB.Append(Relations.Summary)
        pSB.Append(Users.Summary)

        IO.File.WriteAllText(IO.Path.ChangeExtension(lXmlFleName, "txt"), pSB.ToString)
    End Sub

    Private Function MemUsage() As String
        System.GC.WaitForPendingFinalizers()
        Return "MemoryUsage(MB):" & Process.GetCurrentProcess.PrivateMemorySize64 / (2 ^ 20) & _
                    " Local(MB):" & System.GC.GetTotalMemory(True) / (2 ^ 20)
    End Function
End Module

