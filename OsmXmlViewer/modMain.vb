Imports System.Text
Imports System.Collections

Module modMain
    Public pLocation As String = "decatur" '"intownATL" '"dekalb", "metroATL", "intownATL" 
    Public pTerse As Boolean = True
    Public pFolder As String = "C:\OSM\extracts\20110624\quick"
    Public pCreationString As String = "" 'Format(IO.File.GetCreationTime(pXmlFileName), "yyyy-MM-dd-hh-mm")
    Public pReferenceMax As Integer = 100000

    Public pWriteShapes As String = "amenity" '"highway"
    Public pWriteShapesValue As String = "bicycle_parking"
    Public pTagsWrite As New List(Of String) From {"highway", "name", "maxspeed", "width", "surface",
                                                   "amenity", "access", "area", "bicycle", "bridge", "bridge:type",
                                                   "capacity", "class:bicycle", "covered", "crossing", "cycleway", "FIXME", "foot", "ford",
                                                   "junction", "lanes", "name_1", "name_2", "name_3", "note", "oneway", "public_transport",
                                                   "railway", "service", "shelter", "supervised", "towards", "tunnel", "website"}
    Public pTagsSkip As New List(Of String) From {"tiger:tlid", "tiger:name_base", "tiger:name_base_1", "tiger:name_base_2", "tiger:name_base_3", "tiger:name_base_4", "tiger:name_base_5",
                                                  "DeKalb:id",
                                                  "NHD:ComID", "NHD:way_id", "NHD:ReachCode", "NHD:Elevation", "NHD:GNIS_ID"}

    Public pSB As New StringBuilder
    Public pIssues As New StringBuilder

    Public pBounds As Bounds
    Public pMinLat As Double = 90
    Public pMaxLat As Double = -90
    Public pMinLng As Double = 180
    Public pMaxLng As Double = -180
    Public pProcessAll As Boolean = False
    Public pXmlFileName As String
    Public pXmlNodeTypeCounts As New Dictionary(Of String, Integer)

    Sub Initialize()
        IO.Directory.SetCurrentDirectory(pFolder)

        Select Case pLocation
            Case "dekalb"
                pXmlFileName = "georgia.osm"
                pMinLat = 33.61
                pMaxLat = 33.97
                pMinLng = -84.34
                pMaxLng = -84.03
            Case "decatur"
                pXmlFileName = "decatur.osm"
                pProcessAll = True
            Case "intownATL" '(inside 285)
                pXmlFileName = "georgia.osm"
                pMinLat = 33.58
                pMaxLat = 33.95
                pMinLng = -84.54
                pMaxLng = -84.19
            Case "metroATL"
                pXmlFileName = "georgia.osm"
                pMinLat = 33.0
                pMaxLat = 34.5
                pMinLng = -85.0
                pMaxLng = -83.75
        End Select
    End Sub

    Sub main()
        pSB.AppendLine(Now & " Start")
        Initialize()
        pSB.AppendLine(Now & " Curdir " & CurDir())

        Dim lXml As New Xml.XmlDocument
        Dim lSettings As New XmlReaderSettings()
        lSettings.IgnoreWhitespace = True
        Dim lXmlReader As XmlReader = XmlReader.Create(pXmlFileName, lSettings)
        lXmlReader.MoveToContent()
  
        pSB.AppendLine(Now & " Processing '" & pXmlFileName & "' in '" & IO.Directory.GetCurrentDirectory & "'")

        If pProcessAll Then
            pSB.AppendLine(Now & " NoNodeFilter")
        Else
            pSB.AppendLine(Now & " NodeFilter " & pMinLat & ":" & pMaxLat & " " & pMinLng & ":" & pMaxLng)
        End If

        Dim lXmlReadCount As Integer = 0
        Dim lStartTime As Date = Now
        While Not lXmlReader.EOF
            If lXmlReader.Name = "osm" Then
                lXmlReader.Read()
            Else
                Dim lXmlNode As XmlNode = lXml.ReadNode(lXmlReader)
                If lXmlReadCount Mod 10000 = 0 Then
                    Debug.Print(Now & " Elapsed " & CInt(10 * (Now - lStartTime).TotalMinutes) / 10 & " Done " & lXmlReadCount & " " & _
                                Nodes.Count & " " & Ways.Count & " " & Relations.Count & " " & MemUsage())
                End If
                lXmlReadCount += 1
                Dim lKey As String = lXmlNode.Name
                If Not pXmlNodeTypeCounts.ContainsKey(lKey) Then
                    pXmlNodeTypeCounts.Add(lKey, 0)
                End If
                pXmlNodeTypeCounts.Item(lKey) = pXmlNodeTypeCounts.Item(lKey) + 1

                Select Case lXmlNode.Name
                    Case "node"
                        Dim lNode As Node = New Node(lXmlNode)
                        With lNode
                            If pProcessAll OrElse _
                               .Lat > pMinLat AndAlso .Lat < pMaxLat AndAlso _
                               .Lon > pMinLng AndAlso .Lon < pMaxLng Then
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

        Dim lStr As String = Now & " ReadCompleteWithElapsedTime " & CInt(10 * (Now - lStartTime).TotalMinutes) / 10 & " Done " & lXmlReadCount & " " & _
            Nodes.Count & " " & Ways.Count & " " & Relations.Count & " " & MemUsage()
        Debug.Print(lStr)
        pSB.AppendLine(lStr)

        pSB.AppendLine(vbCrLf & "XMLNodeCounts")
        For lIndex As Integer = 0 To pXmlNodeTypeCounts.Count - 1
            pSB.Append(vbTab & (pXmlNodeTypeCounts.Keys(lIndex) & ":").ToString.PadRight(10) & _
                                pXmlNodeTypeCounts.Values(lIndex).ToString.PadLeft(12))
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
        If pBounds Is Nothing Then
            pSB.AppendLine("Missing Bounds")
        Else
            pSB.AppendLine(pBounds.Summary)
        End If

        Dim lOutFileName As String = pLocation & pCreationString & "_Summary.txt"
        IO.File.WriteAllText(lOutFileName, pSB.ToString)

        If pWriteShapes.Length > 0 Then WriteShapes()

        If pIssues.Length > 0 Then
            lOutFileName = pLocation & pCreationString & "_Issues.txt"
            IO.File.WriteAllText(lOutFileName, pIssues.ToString)
        End If
    End Sub

    Private Sub WriteShapes()
        Dim lSB As New StringBuilder
        lSB.AppendLine(vbCrLf & "TagNames " & Tags.TagNames.Count)
        Dim lXmlSB As New StringBuilder
        Dim lShapeSB As New StringBuilder

        Dim lNodeCount As Integer = 0
        Dim lWayCount As Integer = 0
        Dim lNodeTagCounts As New SortedList(Of String, Integer)
        Dim lNodeTagValues As New SortedList(Of String, SortedList)
        Dim lNodes As New List(Of Node)
        Dim lWayTagCounts As New SortedList(Of String, Integer)
        Dim lWayTagValues As New SortedList(Of String, SortedList)
        Dim lWays As New List(Of Way)

        For Each lTagName As KeyValuePair(Of String, SortedList) In Tags.TagNames
            Dim lValueCollection As SortedList = lTagName.Value
            lSB.AppendLine(vbCrLf & lTagName.Key & "(" & lValueCollection.Count & ")")
            If Not pTagsSkip.Contains(lTagName.Key) Then
                For Each lValueEntry As DictionaryEntry In lValueCollection
                    Dim lReferenceCollection As SortedList = lValueEntry.Value
                    lSB.AppendLine(vbTab & lValueEntry.Key & " (" & lReferenceCollection.Count & ")")
                    If Not pTerse OrElse lTagName.Key = pWriteShapes Then
                        Dim lReferenceMax As Integer = Math.Min(lReferenceCollection.Count - 1, pReferenceMax)
                        For lReferenceIndex As Integer = 0 To lReferenceMax
                            Dim lReference As Object = lReferenceCollection.Values(lReferenceIndex)
                            lShapeSB.AppendLine(lReference.GetType.Name & ":" & lReference.id)
                            Select Case lReference.GetType.Name
                                Case "Node"
                                    Dim lNode As Node = lReference
                                    lNodes.Add(lNode)
                                    lXmlSB.AppendLine(lNode.XML.OuterXml)
                                    lNodeCount += 1
                                    For Each lTag In lNode.Tags
                                        If Not pTagsSkip.Contains(lTag.Key) Then
                                            If Not lNodeTagCounts.ContainsKey(lTag.Key) Then
                                                lNodeTagCounts.Add(lTag.Key, 0)
                                                lNodeTagValues.Add(lTag.Key, New SortedList)
                                            End If
                                            lNodeTagCounts(lTag.Key) = lNodeTagCounts(lTag.Key) + 1
                                            If Not lNodeTagValues(lTag.Key).ContainsKey(lTag.Value) Then
                                                lNodeTagValues(lTag.Key).Add(lTag.Value, 0)
                                            End If
                                            lNodeTagValues(lTag.Key).Item(lTag.Value) += 1
                                        End If
                                    Next
                                Case "Way"
                                    Dim lWay As Way = lReference
                                    lWays.Add(lWay)
                                    lXmlSB.AppendLine(lWay.XML.OuterXml)
                                    lWayCount += 1
                                    For Each lTag In lWay.Tags
                                        If Not pTagsSkip.Contains(lTag.Key) Then
                                            If Not lWayTagCounts.ContainsKey(lTag.Key) Then
                                                lWayTagCounts.Add(lTag.Key, 0)
                                                lWayTagValues.Add(lTag.Key, New SortedList)
                                            End If
                                            lWayTagCounts(lTag.Key) = lWayTagCounts(lTag.Key) + 1
                                            If Not lWayTagValues(lTag.Key).ContainsKey(lTag.Value) Then
                                                lWayTagValues(lTag.Key).Add(lTag.Value, 0)
                                            End If
                                            lWayTagValues(lTag.Key).Item(lTag.Value) += 1
                                        End If
                                    Next
                                    For lNodeIndex As Integer = 0 To lWay.NodeKeys.Count - 1
                                        Dim lNodeKey As String = lWay.NodeKeys(lNodeIndex)
                                        If Nodes.Contains(lNodeKey) Then
                                            Dim lNode As Node = Nodes(lNodeKey)
                                            lXmlSB.AppendLine(lNode.XML.OuterXml)
                                        Else
                                            pSB.AppendLine("MissingNode " & lNodeKey)
                                        End If
                                    Next
                                Case "Relation"
                                    'TODO: something here
                                Case Else
                                    Debug.Print("Why!")
                            End Select
                        Next
                        If lReferenceMax < lReferenceCollection.Count - 1 Then
                            lSB.AppendLine(vbTab & vbTab & "...")
                        End If
                    End If
                Next
            End If
        Next
        Dim lTagSummarySB As New StringBuilder
        lTagSummarySB.AppendLine("NodeCount " & lNodeCount & " Tags " & lNodeTagCounts.Count)
        For Each lNodeTag As KeyValuePair(Of String, Integer) In lNodeTagCounts
            lTagSummarySB.AppendLine(vbTab & lNodeTag.Key & "(" & lNodeTag.Value & ")")
            Dim lNodeTagValuesNow As SortedList = lNodeTagValues(lNodeTag.Key)
            For Each lNodeTagValue As DictionaryEntry In lNodeTagValuesNow
                lTagSummarySB.AppendLine(vbTab & vbTab & lNodeTagValue.Key & "(" & lNodeTagValue.Value & ")")
            Next
        Next
        lTagSummarySB.AppendLine("")
        lTagSummarySB.AppendLine("WayCount " & lWayCount & " Tags " & lWayTagCounts.Count)
        For Each lWayTag As KeyValuePair(Of String, Integer) In lWayTagCounts
            lTagSummarySB.AppendLine(vbTab & lWayTag.Key & "(" & lWayTag.Value & ")")
            Dim lWayTagValuesNow As SortedList = lWayTagValues(lWayTag.Key)
            For Each lWayTagValue As DictionaryEntry In lWayTagValuesNow
                lTagSummarySB.AppendLine(vbTab & vbTab & lWayTagValue.Key & "(" & lWayTagValue.Value & ")")
            Next
        Next
        IO.File.WriteAllText(pLocation & pCreationString & "_" & pWriteShapes & "_TagDetails.txt", lTagSummarySB.ToString)

        Dim lOutFileName As String = pLocation & pCreationString & "_Tags.txt"
        IO.File.WriteAllText(lOutFileName, lSB.ToString)
        If lXmlSB.Length > 0 Then
            lOutFileName = pLocation & pCreationString & "_" & pWriteShapes & ".xml"
            IO.File.WriteAllText(lOutFileName, lXmlSB.ToString)
        End If

        Dim lNodeFeatureSet As New DotSpatial.Data.FeatureSet(DotSpatial.Topology.FeatureType.Point)
        Dim lNodeField As New System.Data.DataColumn("Id")
        lNodeFeatureSet.DataTable.Columns.Add(lNodeField)
        For Each lTag As String In pTagsWrite
            If lNodeTagValues.ContainsKey(lTag) Then
                lNodeField = New System.Data.DataColumn(lTag)
                If lTag = "capacity" Then 'TODO: make more generic, insure numeric!
                    lNodeField.DataType = GetType(Integer)
                End If
                lNodeFeatureSet.DataTable.Columns.Add(lNodeField)
            End If
        Next

        Dim lNodeHasValues As New Dictionary(Of String, Boolean)
        For Each lNode As Node In lNodes
            If lNode.Tags(pWriteShapes).Value = pWriteShapesValue Then
                Dim lNodePoint As New DotSpatial.Topology.Point(lNode.Lon, lNode.Lat)
                Dim lNodeFeature As DotSpatial.Data.IFeature = lNodeFeatureSet.AddFeature(lNodePoint)
                lNodeFeature.DataRow("Id") = lNode.Id
                For Each lNodeTag In lNode.Tags
                    If pTagsWrite.Contains(lNodeTag.Key) Then
                        If lNodeTag.Value.Length > 0 Then
                            If lNodeTag.Key = "capacity" Then
                                If IsNumeric(lNodeTag.Value) Then
                                    lNodeFeature.DataRow(lNodeTag.Key) = lNodeTag.Value
                                Else
                                    Debug.Print("Problem with numeric field!")
                                End If
                            Else
                                lNodeFeature.DataRow(lNodeTag.Key) = lNodeTag.Value
                            End If

                            If Not lNodeHasValues.ContainsKey(lNodeTag.Key) Then
                                lNodeHasValues.Add(lNodeTag.Key, True)
                            End If
                        End If
                    End If
                Next
            End If
        Next

        For lNodeFieldIndex As Integer = lNodeFeatureSet.DataTable.Columns.Count - 1 To 0 Step -1
            lNodeField = lNodeFeatureSet.DataTable.Columns(lNodeFieldIndex)
            If Not lNodeHasValues.ContainsKey(lNodeField.ColumnName) Then
                lNodeFeatureSet.DataTable.Columns.Remove(lNodeField)
            End If
        Next
        Dim lNodeFeatureSetFileName As String = String.Empty
        If pWriteShapesValue.Length = 0 Then
            lNodeFeatureSetFileName = pLocation & pCreationString & "_" & pWriteShapes & "_Nodes.shp"
        Else
            lNodeFeatureSetFileName = pLocation & pCreationString & "_" & pWriteShapes & "_" & pWriteShapesValue & "_Nodes.shp"
        End If
        lNodeFeatureSet.SaveAs(lNodeFeatureSetFileName, True)

        Dim lWayFeatureSet As New DotSpatial.Data.FeatureSet(DotSpatial.Topology.FeatureType.Line)
        Dim lWayField As New System.Data.DataColumn("Id")
        lWayFeatureSet.DataTable.Columns.Add(lWayField)
        For Each lTag As String In pTagsWrite
            If lWayTagValues.ContainsKey(lTag) Then
                lWayField = New System.Data.DataColumn(lTag)
                lWayFeatureSet.DataTable.Columns.Add(lWayField)
            End If
        Next
        For Each lWay As Way In lWays
            Dim lWayNodes As New List(Of DotSpatial.Topology.Coordinate)
            For Each lNodeKey As String In lWay.NodeKeys
                If Nodes.Contains(lNodeKey) Then
                    Dim lNode As Node = Nodes.Item(lNodeKey)
                    lWayNodes.Add(New DotSpatial.Topology.Coordinate(lNode.Lon, lNode.Lat))
                End If
            Next
            If lWayNodes.Count > 1 Then
                Dim lWayGeometry As New DotSpatial.Topology.LineString(lWayNodes)
                Dim lWayFeature As DotSpatial.Data.IFeature = lWayFeatureSet.AddFeature(lWayGeometry)
                lWayFeature.DataRow("Id") = lWay.Id
                For Each lWayTag In lWay.Tags
                    If pTagsWrite.Contains(lWayTag.Key) Then
                        lWayFeature.DataRow(lWayTag.Key) = lWayTag.Value
                    End If
                Next
            Else
                lSB.AppendLine("SkipWay " & lWay.Id & " NotEnoughNodes")
            End If
        Next
        lWayFeatureSet.SaveAs(IO.Directory.GetCurrentDirectory & "\" & pLocation & pCreationString & "_" & pWriteShapes & "_Ways.shp", True)
    End Sub

    Private Function MemUsage() As String
        System.GC.WaitForPendingFinalizers()
        Return "MemoryUsage(MB):" & CInt(Process.GetCurrentProcess.PrivateMemorySize64 / (2 ^ 20)) & _
                    " Local(MB):" & CInt(System.GC.GetTotalMemory(True) / (2 ^ 20))
    End Function
End Module

