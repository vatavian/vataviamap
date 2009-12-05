Imports System.Text
Imports atcUtility

Module modMain
    Public pSB As New StringBuilder
    Public pBounds As Bounds
    'intown ATL
    Public pMinLat As Double = 33.5
    Public pMaxLat As Double = 34.0
    Public pMinLon = -84.75
    Public pMaxLon = -84.0

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
        While Not lXmlReader.EOF
            If lXmlReader.Name = "osm" Then
                lXmlReader.Read()
            Else
                Dim lXmlNode As XmlNode = lXml.ReadNode(lXmlReader)
                If lXmlReadCount Mod 10000 = 0 Then
                    Debug.Print(Now & " Done " & lXmlReadCount & " " & Nodes.Count & " " & Ways.Count & " " & MemUsage())
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
                                If .User.Length > 0 Then
                                    Users.AddReference(.User, lNode)
                                End If
                            End If
                        End With
                    Case "way"
                        Dim lWay As Way = New Way(lXmlNode)
                        With lWay
                            If Not (.LatMin > pMaxLat OrElse .LatMax < pMinLat OrElse _
                                    .LonMin > pMaxLon OrElse .LonMax < pMinLon) Then
                                Ways.Add(lWay)
                                If .User.Length > 0 Then
                                    Users.AddReference(.User, lWay)
                                End If
                            End If
                        End With
                    Case "relation"
                        Dim lRelation As Relation = New Relation(lXmlNode)
                        'TODO: need to check bounds here
                        Relations.Add(lRelation)
                    Case "bounds", "bound"
                        pBounds = New Bounds(lXmlNode)
                    Case Else
                        pSB.AppendLine(vbCrLf & lXmlNode.OuterXml)
                End Select
            End If
        End While

        pSB.AppendLine(vbCrLf & "XMLNodeCounts")
        For lIndex As Integer = 0 To lXmlNodeTypeCounts.Count - 1
            pSB.AppendLine(vbTab & lXmlNodeTypeCounts.Keys(lIndex) & ":" & lXmlNodeTypeCounts.ItemByIndex(lIndex))
        Next

        pSB.AppendLine(vbCrLf & "TagNames")
        For lTagIndex As Integer = 0 To Tags.TagNames.Count - 1
            pSB.AppendLine(vbCrLf & Tags.TagNames.Keys(lTagIndex))
            Dim lValueCollection As atcCollection = Tags.TagNames.ItemByIndex(lTagIndex)
            For lValueIndex As Integer = 0 To lValueCollection.Count - 1
                Dim lReferenceCollection As atcCollection = lValueCollection.Item(lValueIndex)
                pSB.AppendLine(vbTab & lValueCollection.Keys(lValueIndex) & " (" & lReferenceCollection.Count & ")")
                Dim lReferenceMax As Integer = Math.Min(lReferenceCollection.Count - 1, 20)
                For lReferenceIndex As Integer = 0 To lReferenceMax
                    Dim lReference As Object = lReferenceCollection.ItemByIndex(lReferenceIndex)
                    pSB.AppendLine(vbTab & vbTab & lReference.GetType.Name & ":" & lReference.id)
                Next
                If lReferenceMax < lReferenceCollection.Count - 1 Then
                    pSB.AppendLine(vbTab & vbTab & "...")
                End If
            Next
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

