Imports System.Text
Imports System.Collections

Module modMain
    Public pLocation As String = "decatur" '"intownATL" '"dekalb", "metroATL", "intownATL" 
    Public pTerse As Boolean = True
    Public pFolder As String = "C:\OSM\extracts\20110702\quick"
    Public pCreationString As String = "" 'Format(IO.File.GetCreationTime(pXmlFileName), "yyyy-MM-dd-hh-mm")
    Public pReferenceMax As Integer = 100000

    Public pWriteShapeInstructions As New List(Of String) From
                                                 {"amenity:bicycle_parking",
                                                  "highway:",
                                                  "state:proposed"}
    Public pTagsWrite As New List(Of String) From {"highway", "name", "maxspeed", "width", "surface",
                                                   "amenity", "access", "area", "bicycle", "bridge", "bridge:type",
                                                   "capacity", "class:bicycle", "colour", "covered", "crossing", "cycleway", "FIXME", "foot", "ford",
                                                   "junction", "lanes", "name_1", "name_2", "name_3", "network", "note", "oneway", "public_transport",
                                                   "railway", "ref", "route", "service", "shelter", "state", "supervised", "towards", "tunnel", "website"}
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
                    Debug.Print(Now & " Elapsed " & (CInt(10 * (Now - lStartTime).TotalMinutes) / 10).ToString.PadLeft(5) &
                                      " Done " & lXmlReadCount.ToString.PadLeft(8) & " " &
                                      Nodes.Count.ToString.PadLeft(8) & " " &
                                      Ways.Count.ToString.PadLeft(8) & " " &
                                      Relations.Count.ToString.PadLeft(8) & " " & MemUsage())
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

        For Each lWriteShape As String In pWriteShapeInstructions
            If lWriteShape.Length > 0 Then WriteShapes(lWriteShape)
        Next

        If pIssues.Length > 0 Then
            lOutFileName = pLocation & pCreationString & "_Issues.txt"
            IO.File.WriteAllText(lOutFileName, pIssues.ToString)
        End If
    End Sub

    Private Sub WriteShapes(ByVal aWriteShapeInstruction As String)
        Dim lWriteShapeFields() As String = aWriteShapeInstruction.Split(":")
        Dim lWriteTag As String = lWriteShapeFields(0)
        Dim lWriteTagValue As String = lWriteShapeFields(1)

        Dim lSB As New StringBuilder
        lSB.AppendLine(vbCrLf & "TagNames " & Tags.TagNames.Count)
        Dim lXmlSB As New StringBuilder
        Dim lShapeSB As New StringBuilder

        Dim lNodeTagCounts As New SortedList(Of String, Integer)
        Dim lNodeTagValues As New SortedList(Of String, SortedList)
        Dim lNodes As New List(Of Node)
        Dim lNodeDebugSB As New StringBuilder

        Dim lWayTagCounts As New SortedList(Of String, Integer)
        Dim lWayTagValues As New SortedList(Of String, SortedList)
        Dim lWays As New List(Of Way)
        Dim lWayDebugSB As New StringBuilder

        For Each lTagName As KeyValuePair(Of String, SortedList) In Tags.TagNames
            Dim lValueCollection As SortedList = lTagName.Value
            lSB.AppendLine(vbCrLf & lTagName.Key & "(" & lValueCollection.Count & ")")
            If Not pTagsSkip.Contains(lTagName.Key) Then
                 For Each lValueEntry As DictionaryEntry In lValueCollection
                    Dim lReferenceCollection As SortedList = lValueEntry.Value
                    lSB.AppendLine(vbTab & lValueEntry.Key & " (" & lReferenceCollection.Count & ")")
                    If Not pTerse OrElse lTagName.Key = lWriteTag Then
                        Dim lReferenceMax As Integer = Math.Min(lReferenceCollection.Count - 1, pReferenceMax)
                        For lReferenceIndex As Integer = 0 To lReferenceMax
                            Dim lReference As Object = lReferenceCollection.Values(lReferenceIndex)
                            lShapeSB.AppendLine(lReference.GetType.Name & ":" & lReference.id)
                            Select Case lReference.GetType.Name
                                Case "Node"
                                    Dim lNode As Node = lReference
                                    SaveNode(lNode, lNodes, lXmlSB, lNodeTagCounts, lNodeTagValues, lNodeDebugSB)
                                Case "Way"
                                    Dim lWay As Way = lReference
                                    SaveWay(lWay, lWays, lXmlSB, lWayTagCounts, lWayTagValues, lWayDebugSB)
                                Case "Relation"
                                    Dim lRelation As Relation = lReference
                                    For Each lNodeKey As String In lRelation.NodeKeys
                                        Dim lNode As Node = Nodes(lNodeKey)
                                        If lNode.Tags.Contains(lWriteTag) OrElse lRelation.Tags.Contains(lWriteTag) Then
                                            SaveNode(lNode, lNodes, lXmlSB, lNodeTagCounts, lNodeTagValues, lNodeDebugSB, lRelation)
                                        End If
                                    Next
                                    For Each lWayKey As String In lRelation.WayKeys
                                        Dim lWay As Way = Ways(lWayKey)
                                        If lWay.Tags.Contains(lWriteTag) OrElse lRelation.Tags.Contains(lWriteTag) Then
                                            SaveWay(lWay, lWays, lXmlSB, lWayTagCounts, lWayTagValues, lWayDebugSB, lRelation)
                                        End If
                                    Next
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
        lTagSummarySB.AppendLine("NodeCount " & lNodes.Count & " Tags " & lNodeTagCounts.Count)
        For Each lNodeTag As KeyValuePair(Of String, Integer) In lNodeTagCounts
            lTagSummarySB.AppendLine(vbTab & lNodeTag.Key & "(" & lNodeTag.Value & ")")
            Dim lNodeTagValuesNow As SortedList = lNodeTagValues(lNodeTag.Key)
            For Each lNodeTagValue As DictionaryEntry In lNodeTagValuesNow
                lTagSummarySB.AppendLine(vbTab & vbTab & lNodeTagValue.Key & "(" & lNodeTagValue.Value & ")")
            Next
        Next
        lTagSummarySB.AppendLine("")

        lTagSummarySB.AppendLine("WayCount " & lWays.Count & " Tags " & lWayTagCounts.Count)
        For Each lWayTag As KeyValuePair(Of String, Integer) In lWayTagCounts
            lTagSummarySB.AppendLine(vbTab & lWayTag.Key & "(" & lWayTag.Value & ")")
            Dim lWayTagValuesNow As SortedList = lWayTagValues(lWayTag.Key)
            For Each lWayTagValue As DictionaryEntry In lWayTagValuesNow
                lTagSummarySB.AppendLine(vbTab & vbTab & lWayTagValue.Key & "(" & lWayTagValue.Value & ")")
            Next
        Next
        Dim lFileName As String = String.Empty
        If lWriteTagValue.Length = 0 Then
            lFileName = pLocation & pCreationString & "_" & lWriteTag & "_TagDetails.txt"
        Else
            lFileName = pLocation & pCreationString & "_" & lWriteTag & "_" & lWriteTagValue & "_TagDetails.txt"
        End If
        IO.File.WriteAllText(lFileName, lTagSummarySB.ToString)
        lFileName = lFileName.Replace("TagDetails", "NodeDebug")
        IO.File.WriteAllText(lFileName, lNodeDebugSB.ToString)
        lFileName = lFileName.Replace("NodeDebug", "WayDebug")
        IO.File.WriteAllText(lFileName, lWayDebugSB.ToString)

        IO.File.WriteAllText(pLocation & pCreationString & "_Tags.txt", lSB.ToString)

        If lXmlSB.Length > 0 Then
            If lWriteTagValue.Length = 0 Then
                lFileName = pLocation & pCreationString & "_" & lWriteTag & ".xml"
            Else
                lFileName = pLocation & pCreationString & "_" & lWriteTag & "_" & lWriteTagValue & ".xml"
            End If
            IO.File.WriteAllText(lFileName, lXmlSB.ToString)
        End If

        If lNodes.Count > 0 Then
            Dim lNodeFeatureSet As New DotSpatial.Data.FeatureSet(DotSpatial.Topology.FeatureType.Point)
            Dim lNodeField As New System.Data.DataColumn("Id")
            lNodeFeatureSet.DataTable.Columns.Add(lNodeField)
            For Each lTag As String In pTagsWrite
                If lNodeTagValues.ContainsKey(lTag) Then
                    lNodeField = New System.Data.DataColumn(lTag)
                    If lTag = "capacity" Then 'TODO: make more generic, ensure numeric!
                        lNodeField.DataType = GetType(Integer)
                    End If
                    lNodeFeatureSet.DataTable.Columns.Add(lNodeField)
                End If
            Next

            Dim lNodeTagHasValues As New Dictionary(Of String, Boolean)
            For Each lNode As Node In lNodes
                Dim lForceWriteFromRelation As Boolean = False
                Dim lRelation As Relation = Nothing
                For Each lRelationKey As String In lNode.RelationKeys
                    lRelation = Relations(lRelationKey)
                    If lRelation.Tags.Contains(lWriteTag) AndAlso lRelation.Tags(lWriteTag).Value = lWriteTagValue Then
                        lForceWriteFromRelation = True
                        Exit For
                    End If
                Next
                If lForceWriteFromRelation OrElse
                   lWriteTagValue.Length = 0 OrElse
                  (lNode.Tags.Contains(lWriteTag) AndAlso lNode.Tags(lWriteTag).Value = lWriteTagValue) Then
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
                                        Debug.Print("Problem with numeric field in '" & lNodeTag.Key & "' in '" & lNode.Id & "' value is " & lNodeTag.Value)
                                    End If
                                Else
                                    lNodeFeature.DataRow(lNodeTag.Key) = lNodeTag.Value
                                End If

                                If Not lNodeTagHasValues.ContainsKey(lNodeTag.Key) Then
                                    lNodeTagHasValues.Add(lNodeTag.Key, True)
                                End If
                            End If
                        End If
                    Next

                    If lForceWriteFromRelation AndAlso lRelation IsNot Nothing Then
                        For Each lTag As Tag In lRelation.Tags
                            If pTagsWrite.Contains(lTag.Key) Then
                                If lTag.Value.Length > 0 Then
                                    If lTag.Key = "capacity" Then
                                        If IsNumeric(lTag.Value) Then
                                            lNodeFeature.DataRow(lTag.Key) = lTag.Value
                                        Else
                                            Debug.Print("Problem with numeric field in '" & lTag.Key & "' in '" & lNode.Id & "' value is " & lTag.Value)
                                        End If
                                    Else
                                        lNodeFeature.DataRow(lTag.Key) = lTag.Value
                                    End If

                                    If Not lNodeTagHasValues.ContainsKey(lTag.Key) Then
                                        lNodeTagHasValues.Add(lTag.Key, True)
                                    End If
                                End If
                            End If
                        Next
                    End If
                End If
            Next
            For lNodeFieldIndex As Integer = lNodeFeatureSet.DataTable.Columns.Count - 1 To 0 Step -1
                lNodeField = lNodeFeatureSet.DataTable.Columns(lNodeFieldIndex)
                If Not lNodeTagHasValues.ContainsKey(lNodeField.ColumnName) Then
                    lNodeFeatureSet.DataTable.Columns.Remove(lNodeField)
                End If
            Next
            If lNodeFeatureSet.NumRows > 0 Then
                Dim lNodeFeatureSetFileName As String = String.Empty
                If lWriteTagValue.Length = 0 Then
                    lNodeFeatureSetFileName = pLocation & pCreationString & "_" & lWriteTag & "_Nodes.shp"
                Else
                    lNodeFeatureSetFileName = pLocation & pCreationString & "_" & lWriteTag & "_" & lWriteTagValue & "_Nodes.shp"
                End If
                lNodeFeatureSet.SaveAs(lNodeFeatureSetFileName, True)
            End If
        End If

        If lWays.Count > 0 Then
            Dim lWayFeatureSet As New DotSpatial.Data.FeatureSet(DotSpatial.Topology.FeatureType.Line)
            Dim lWayField As New System.Data.DataColumn("Id")
            lWayFeatureSet.DataTable.Columns.Add(lWayField)
            For Each lTag As String In pTagsWrite
                If lWayTagValues.ContainsKey(lTag) Then
                    lWayField = New System.Data.DataColumn(lTag)
                    lWayFeatureSet.DataTable.Columns.Add(lWayField)
                End If
            Next

            Dim lWayTagHasValues As New Dictionary(Of String, Boolean)
            For Each lWay As Way In lWays
                Dim lForceWriteFromRelation As Boolean = False
                Dim lRelation As Relation = Nothing
                For Each lRelationKey As String In lWay.RelationKeys
                    lRelation = Relations(lRelationKey)
                    If lRelation.Tags.Contains(lWriteTag) AndAlso lRelation.Tags(lWriteTag).Value = lWriteTagValue Then
                        lForceWriteFromRelation = True
                        Exit For
                    End If
                Next
                If lForceWriteFromRelation OrElse
                   lWriteTagValue.Length = 0 OrElse
                  (lWay.Tags.Contains(lWriteTag) AndAlso lWay.Tags(lWriteTag).Value = lWriteTagValue) Then
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
                            If Not lWayTagHasValues.ContainsKey(lWayTag.Key) Then
                                lWayTagHasValues.Add(lWayTag.Key, True)
                            End If
                        Next
                        If lForceWriteFromRelation AndAlso lRelation IsNot Nothing Then
                            For Each lTag As Tag In lRelation.Tags
                                If pTagsWrite.Contains(lTag.Key) Then
                                    If lTag.Value.Length > 0 Then
                                        lWayFeature.DataRow(lTag.Key) = lTag.Value
                                        If Not lWayTagHasValues.ContainsKey(lTag.Key) Then
                                            lWayTagHasValues.Add(lTag.Key, True)
                                        End If
                                    End If
                                End If
                            Next
                        End If
                    Else
                        lSB.AppendLine("SkipWay " & lWay.Id & " NotEnoughNodes")
                    End If
                End If
            Next
            For lWayFieldIndex As Integer = lWayFeatureSet.DataTable.Columns.Count - 1 To 0 Step -1
                lWayField = lWayFeatureSet.DataTable.Columns(lWayFieldIndex)
                If Not lWayTagHasValues.ContainsKey(lWayField.ColumnName) Then
                    lWayFeatureSet.DataTable.Columns.Remove(lWayField)
                End If
            Next
            If lWayFeatureSet.NumRows > 0 Then
                Dim lWayFeatureSetFileName As String = String.Empty
                If lWriteTagValue.Length = 0 Then
                    lWayFeatureSetFileName = pLocation & pCreationString & "_" & lWriteTag & "_Ways.shp"
                Else
                    lWayFeatureSetFileName = pLocation & pCreationString & "_" & lWriteTag & "_" & lWriteTagValue & "_Ways.shp"
                End If
                lWayFeatureSet.SaveAs(CurDir() & "\" & lWayFeatureSetFileName, True)
            End If
        End If
    End Sub

    Private Sub SaveNode(ByVal aNode As Node, ByRef aNodes As List(Of Node), ByRef aXmlSB As StringBuilder,
                         ByRef aNodeTagCounts As SortedList(Of String, Integer), ByRef aNodeTagValues As SortedList(Of String, SortedList),
                         ByRef aNodeDebugSB As StringBuilder,
                         Optional ByVal aRelation As Relation = Nothing)
        aNodeDebugSB.AppendLine(aNodes.Count & ":" & aNode.Id & ":" & IsNothing(aRelation) & ":" & aNodeTagCounts.Count)
        aNodes.Add(aNode)
        aXmlSB.AppendLine(aNode.XML.OuterXml)
        For Each lTag In aNode.Tags
            If Not pTagsSkip.Contains(lTag.Key) Then
                If Not aNodeTagCounts.ContainsKey(lTag.Key) Then
                    aNodeDebugSB.AppendLine(vbTab & "AddFromNode " & lTag.Key)
                    aNodeTagCounts.Add(lTag.Key, 0)
                    aNodeTagValues.Add(lTag.Key, New SortedList)
                End If
                aNodeTagCounts(lTag.Key) = aNodeTagCounts(lTag.Key) + 1
                If Not aNodeTagValues(lTag.Key).ContainsKey(lTag.Value) Then
                    aNodeTagValues(lTag.Key).Add(lTag.Value, 0)
                End If
                aNodeTagValues(lTag.Key).Item(lTag.Value) += 1
            End If
        Next

        If aRelation IsNot Nothing Then
            For Each lTag In aRelation.Tags
                If Not pTagsSkip.Contains(lTag.Key) Then
                    If Not aNodeTagCounts.ContainsKey(lTag.Key) Then
                        aNodeDebugSB.AppendLine(vbTab & "AddFromRelation " & lTag.Key)
                        aNodeTagCounts.Add(lTag.Key, 0)
                        aNodeTagValues.Add(lTag.Key, New SortedList)
                    End If
                    aNodeTagCounts(lTag.Key) = aNodeTagCounts(lTag.Key) + 1
                    If Not aNodeTagValues(lTag.Key).ContainsKey(lTag.Value) Then
                        aNodeTagValues(lTag.Key).Add(lTag.Value, 0)
                    End If
                    aNodeTagValues(lTag.Key).Item(lTag.Value) += 1
                End If
            Next
        End If
    End Sub

    Sub SaveWay(ByVal aWay As Way, ByRef aWays As List(Of Way), ByRef aXmlSB As StringBuilder,
                ByRef aWayTagCounts As SortedList(Of String, Integer), ByRef aWayTagValues As SortedList(Of String, SortedList),
                ByRef aWayDebugSB As StringBuilder,
                Optional ByVal aRelation As Relation = Nothing)
        aWayDebugSB.AppendLine(aWays.Count & ":" & aWay.Id & ":" & IsNothing(aRelation) & ":" & aWayTagCounts.Count)
        aWays.Add(aWay)
        aXmlSB.AppendLine(aWay.XML.OuterXml)
        For Each lTag In aWay.Tags
            If Not pTagsSkip.Contains(lTag.Key) Then
                If Not aWayTagCounts.ContainsKey(lTag.Key) Then
                    aWayDebugSB.AppendLine(vbTab & "AddFromWay " & lTag.Key)
                    aWayTagCounts.Add(lTag.Key, 0)
                    aWayTagValues.Add(lTag.Key, New SortedList)
                End If
                aWayTagCounts(lTag.Key) = aWayTagCounts(lTag.Key) + 1
                If Not aWayTagValues(lTag.Key).ContainsKey(lTag.Value) Then
                    aWayTagValues(lTag.Key).Add(lTag.Value, 0)
                End If
                aWayTagValues(lTag.Key).Item(lTag.Value) += 1
            End If
        Next
        If aRelation IsNot Nothing Then
            For Each lTag In aRelation.Tags
                If Not pTagsSkip.Contains(lTag.Key) Then
                    If Not aWayTagCounts.ContainsKey(lTag.Key) Then
                        aWayDebugSB.AppendLine(vbTab & "AddFromRelation " & lTag.Key)
                        aWayTagCounts.Add(lTag.Key, 0)
                        aWayTagValues.Add(lTag.Key, New SortedList)
                    End If
                    aWayTagCounts(lTag.Key) = aWayTagCounts(lTag.Key) + 1
                    If Not aWayTagValues(lTag.Key).ContainsKey(lTag.Value) Then
                        aWayTagValues(lTag.Key).Add(lTag.Value, 0)
                    End If
                    aWayTagValues(lTag.Key).Item(lTag.Value) += 1
                End If
            Next
        End If

        For lNodeIndex As Integer = 0 To aWay.NodeKeys.Count - 1
            Dim lNodeKey As String = aWay.NodeKeys(lNodeIndex)
            If Nodes.Contains(lNodeKey) Then
                Dim lNode As Node = Nodes(lNodeKey)
                aXmlSB.AppendLine(lNode.XML.OuterXml)
            Else
                pSB.AppendLine("MissingNode " & lNodeKey)
            End If
        Next
    End Sub

    Private Function MemUsage() As String
        System.GC.WaitForPendingFinalizers()
        Return "MemoryUsage(MB):" & (CInt(Process.GetCurrentProcess.PrivateMemorySize64 / (2 ^ 20))).ToString.PadLeft(5) &
                    " Local(MB):" & (CInt(System.GC.GetTotalMemory(True) / (2 ^ 20))).ToString.PadLeft(5)
    End Function
End Module

