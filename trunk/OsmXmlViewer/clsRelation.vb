Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports System.Text

Public Module RelationVariables
    Public Relations As New RelationCollection
End Module

Public Class RelationCollection
    Inherits KeyedCollection(Of String, Relation)
    Protected Overrides Function GetKeyForItem(ByVal aRelation As Relation) As String
        Return "K" & aRelation.Id
    End Function

    Public Function Summary() As String
        Dim lIssues As Boolean = False
        Dim lSB As New StringBuilder
        lSB.AppendLine(vbCrLf & "Relations(" & Relations.Count & ")")
        If Not pTerse Then lSB.AppendLine(vbCrLf & "Id:Tags:Nodes:Ways")
        For Each lRelation As Relation In Relations
            If Not pTerse Then
                lSB.AppendLine(vbTab & lRelation.Id & ":" & lRelation.Tags.Count & ":" & _
                                                            lRelation.NodeKeys.Count & ":" & _
                                                            lRelation.WayKeys.Count)
            End If
            For Each lNodeKey As String In lRelation.NodeKeys
                If Not Nodes.Contains(lNodeKey) Then
                    If Not pTerse Then pIssues.AppendLine(vbTab & vbTab & "MissingNode " & lNodeKey)
                    lIssues = True
                End If
            Next
            For Each lWayKey As String In lRelation.WayKeys
                If Not Ways.Contains(lWayKey) Then
                    If Not pTerse Then pIssues.AppendLine(vbTab & vbTab & "MissingWay " & lWayKey)
                    lIssues = True
                End If
            Next
        Next
        If Not lIssues Then
            lSB.AppendLine(vbTab & "NoIssuesFound")
        End If
        Return lSB.ToString
    End Function
End Class

Public Class Relation
    Public Id As String
    Public Timestamp As Date
    Public User As String
    Public Actor As Integer
    Public Version As Integer
    Public Visible As Boolean = True
    Public UId As Integer
    Public Changeset As Integer
    Public NodeKeys As New Collection(Of String)
    Public WayKeys As New Collection(Of String)
    Public RelationKeys As New Collection(Of String)
    Public Tags As New Tags

    Public Sub New(ByVal aXmlNode As Xml.XmlNode)
        Dim lUser As String = ""
        For Each lAttribute As XmlAttribute In aXmlNode.Attributes
            Select Case lAttribute.Name
                Case "id" : Id = lAttribute.Value
                Case "timestamp" : Timestamp = lAttribute.Value
                Case "user" : lUser = lAttribute.Value
                Case "actor" : Actor = lAttribute.Value
                Case "version" : Version = lAttribute.Value
                Case "visible" : Visible = lAttribute.Value
                Case "uid" : UId = lAttribute.Value
                Case "changeset" : Changeset = lAttribute.Value
                Case Else
                    pIssues.AppendLine("MissingAttribute " & lAttribute.Name & " forRelation " & Id)
            End Select
        Next

        If lUser.Length > 0 Then
            Users.AddReference(lUser, Me)
        End If

        Dim lIssueCount As Integer = 0
        Dim lIssueSB As New StringBuilder
        Dim lIssueUsers As New SortedList(Of String, Integer)
        For Each lXmlNode As XmlNode In aXmlNode.ChildNodes
            Select Case lXmlNode.Name
                Case "member"
                    Dim lType As String = lXmlNode.Attributes("type").Value
                    Dim lKey As String = "K" & lXmlNode.Attributes("ref").Value
                    Select Case lType
                        Case "node"
                            If Nodes.Contains(lKey) Then
                                NodeKeys.Add(lKey)
                                Nodes(lKey).RelationKeys.Add("K" & Id)
                            Else
                                lIssueCount += 1
                                Blame(lIssueUsers, User)
                                lIssueSB.AppendLine("Missing Node for key '" & lKey & "'")
                            End If
                        Case "way"
                            If Ways.Contains(lKey) Then
                                WayKeys.Add(lKey)
                                Ways(lKey).RelationKeys.Add("K" & Id)
                            Else
                                lIssueCount += 1
                                Blame(lIssueUsers, User)
                                lIssueSB.AppendLine("Missing Way for key '" & lKey & "'")
                            End If
                        Case "relation"
                            If Relations.Contains(lKey) Then
                                RelationKeys.Add(lKey)
                                Relations(lKey).RelationKeys.Add("K" & Id)
                            Else
                                lIssueCount += 1
                                Blame(lIssueUsers, User)
                                lIssueSB.AppendLine("Missing Relation for key '" & lKey & "'")
                            End If
                        Case Else
                            lIssueCount += 1
                            Blame(lIssueUsers, User)
                            lIssueSB.AppendLine("MissingMemberType '" & lType & "' with key '" & lKey & "'")
                    End Select
                Case "tag"
                    Tags.Add(New Tag(lXmlNode.Attributes))
                Case Else
                    lIssueCount += 1
                    Blame(lIssueUsers, User)
                    lIssueSB.AppendLine("MissingXmlTag " & lXmlNode.Name & " forRelation " & Id)
            End Select
        Next

        If lIssueCount > 0 Then
            pSB.AppendLine(Now & " IssuesWithRelation " & Id & " Count " & lIssueCount)
            For Each lIssueUser As KeyValuePair(Of String, Integer) In lIssueUsers
                pSB.AppendLine(Now & "   User " & lIssueUser.Key & " " & lIssueUser.Value)
            Next
            If Not pTerse Then
                pIssues.Append(lIssueSB)
            End If
        End If
    End Sub

    Sub Blame(ByVal aIssueUsers As SortedList(Of String, Integer), ByVal aId As String)
        If aIssueUsers.ContainsKey(aId) Then
            aIssueUsers.Item(aId) = aIssueUsers.Item(aId) + 1
        Else
            aIssueUsers.Add(aId, 1)
        End If
    End Sub
End Class