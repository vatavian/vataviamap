Imports atcUtility

Module modMain
    Public pSB As New System.Text.StringBuilder
    Public pNodes As New Nodes
    Public pWays As New Ways
    Public pRelations As New Relations
    Public pUsers As New Users

    Sub main()
        IO.Directory.SetCurrentDirectory("..\..\..\osm_data")

        'Dim lXmlFleName As String = "decatur.mdc"
        Dim lXmlFleName As String = "map.osm"

        Dim lXml As New Xml.XmlDocument
        lXml.Load(lXmlFleName)

        Dim lXmlBaseNode As XmlNode
        If lXmlFleName.EndsWith(".mdc") Then
            lXmlBaseNode = CType(lXml.GetElementsByTagName("DrawingMapLayer"), System.Xml.XmlNodeList).Item(0).ChildNodes(0)
        Else
            lXmlBaseNode = lXml.ChildNodes(1)
        End If
        For Each lAttribute As XmlAttribute In lXmlBaseNode.Attributes
            pSB.AppendLine("Attribute:" & lAttribute.Name & ":" & lAttribute.Value)
        Next
        'Dim lMapLayerEnum As IEnumerator = lXmlDrawingMapLayerNodeList.Item(0).ChildNodes(0).ChildNodes.GetEnumerator
        Dim lMapLayerEnum As IEnumerator = lXmlBaseNode.ChildNodes.GetEnumerator
        Dim lNodeTypeCounts As New atcCollection
        While lMapLayerEnum.MoveNext
            Dim lNode As XmlNode = CType(lMapLayerEnum.Current, XmlNode)
            lNodeTypeCounts.Increment(lNode.Name, 1)
            Select Case lNode.Name
                Case "node"
                    pNodes.Add(New Node(lNode))
                Case "way"
                    pWays.Add(New Way(lNode))
                Case "relation"
                    pRelations.Add(New Relation(lNode))
                Case "bound"
                    pSB.AppendLine(vbCrLf & lNode.OuterXml)
                Case Else
                    pSB.AppendLine(vbCrLf & lNode.OuterXml)
            End Select
        End While
        pSB.AppendLine(vbCrLf & "XMLNodeCounts")
        For lIndex As Integer = 0 To lNodeTypeCounts.Count - 1
            pSB.AppendLine(vbTab & lNodeTypeCounts.Keys(lIndex) & ":" & lNodeTypeCounts.ItemByIndex(lIndex))
        Next
        pSB.AppendLine(vbCrLf & "TagNames")
        For lTagIndex As Integer = 0 To Tags.TagNames.Count - 1
            pSB.AppendLine(vbCrLf & Tags.TagNames.Keys(lTagIndex))
            Dim lValueCollection As atcCollection = Tags.TagNames.ItemByIndex(lTagIndex)
            For lValueIndex As Integer = 0 To lValueCollection.Count - 1
                Dim lReferenceCollection As atcCollection = lValueCollection.Item(lValueIndex)
                pSB.AppendLine(vbTab & lValueCollection.Keys(lValueIndex) & " (" & lReferenceCollection.Count & ")")
                Dim lReferenceMax As Integer = Math.Min(lReferenceCollection.Count - 1, 99)
                For lReferenceIndex As Integer = 0 To lReferenceMax
                    Dim lReference As Object = lReferenceCollection.ItemByIndex(lReferenceIndex)
                    pSB.AppendLine(vbTab & vbTab & lReference.GetType.Name & ":" & lReference.id)
                Next
                If lReferenceMax < lReferenceCollection.Count - 1 Then
                    pSB.AppendLine(vbTab & vbTab & "...")
                End If

            Next
        Next
        pSB.AppendLine(vbCrLf & "Ways Id:Tags:Nodes")
        For Each lWay As Way In pWays
            pSB.AppendLine(vbTab & lWay.Id & ":" & lWay.Tags.Count & ":" & lWay.NodeKeys.Count)
            For Each lNodeKey As String In lWay.NodeKeys
                If Not pNodes.Contains(lNodeKey) Then
                    pSB.AppendLine(vbTab & vbTab & "MissingNode " & lNodeKey)
                End If
            Next
        Next

        pSB.AppendLine(vbCrLf & "Relations Id:Tags:Nodes:Ways")
        For Each lRelation As Relation In pRelations
            pSB.AppendLine(vbTab & lRelation.Id & ":" & lRelation.Tags.Count & ":" & _
                                                        lRelation.NodeKeys.Count & ":" & _
                                                        lRelation.WayKeys.Count)
            For Each lNodeKey As String In lRelation.NodeKeys
                If Not pNodes.Contains(lNodeKey) Then
                    pSB.AppendLine(vbTab & vbTab & "MissingNode " & lNodeKey)
                End If
            Next
        Next

        pSB.AppendLine(vbCrLf & "Users:" & pUsers.Count)
        pSB.AppendLine(vbTab & "User".PadRight(20) & vbTab & "ReferenceCount")
        For Each lUser As User In pUsers
            pSB.AppendLine(vbTab & lUser.Name.PadRight(20) & vbTab & lUser.Objects.Count)
        Next

        IO.File.WriteAllText(IO.Path.ChangeExtension(lXmlFleName, "txt"), pSB.ToString)
    End Sub
End Module

