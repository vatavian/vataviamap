Imports System.Collections.ObjectModel
Imports System.Text

Public Module WayVariables
    Public Ways As New WayCollection
End Module

Public Class WayCollection
    Inherits KeyedCollection(Of String, Way)
    Protected Overrides Function GetKeyForItem(ByVal aWay As Way) As String
        Return "K" & aWay.Id
    End Function

    Public Function Summary() As String
        Dim lSB As New StringBuilder
        lSB.AppendLine(vbCrLf & "Ways(" & Ways.Count & ")")
        If Not pTerse Then lSB.AppendLine(vbCrLf & "Id:Tags:Nodes")

        Dim lIssueCount As Integer = 0
        For Each lWay As Way In Ways
            If Not pTerse Then lSB.AppendLine(vbTab & lWay.Id & ":" & lWay.Tags.Count & ":" & lWay.NodeKeys.Count)
            For Each lNodeKey As String In lWay.NodeKeys
                If Not Nodes.Contains(lNodeKey) Then
                    pIssues.AppendLine(vbTab & vbTab & "MissingNode " & lNodeKey & " in Way " & lWay.Id)
                    lIssueCount += 1
                End If
            Next
        Next

        If lIssueCount = 0 Then
            lSB.AppendLine(vbTab & "NoWayIssuesFound")
        Else
            lSB.AppendLine(vbTab & lIssueCount & " WayIssuesFound")
        End If
        Return lSB.ToString
    End Function
End Class

Public Class Way
    Public Id As String
    Public Timestamp As Date
    Public User As String
    Public Actor As Integer
    Public Version As Integer
    Public Closed As Boolean = False
    Public Visible As Boolean
    Public UId As Integer
    Public Changeset As Integer
    Public NodeKeys As New Collection(Of String)
    Public Tags As New Tags

    Public Sub New(ByVal aXmlNode As Xml.XmlNode)
        For Each lAttribute As XmlAttribute In aXmlNode.Attributes
            Select Case lAttribute.Name
                Case "id" : Id = lAttribute.Value
                Case "timestamp" : Timestamp = lAttribute.Value
                Case "user" : User = lAttribute.Value
                Case "actor" : Actor = lAttribute.Value
                Case "version" : Version = lAttribute.Value
                Case "visible" : Visible = lAttribute.Value
                Case "uid" : UId = lAttribute.Value
                Case "changeset" : Changeset = lAttribute.Value
                Case Else
                    pIssues.AppendLine("MissingAttribute " & lAttribute.Name & " forWay " & Id)
            End Select
        Next

        For Each lXmlNode As XmlNode In aXmlNode.ChildNodes
            Select Case lXmlNode.Name
                Case "nd"
                    Dim lKey As String = "K" & lXmlNode.Attributes("ref").Value
                    If NodeKeys.Contains(lKey) Then
                        Closed = True
                    Else
                        If Nodes.Contains(lKey) Then
                            NodeKeys.Add(lKey)
                        End If
                    End If
                Case "tag"
                    Tags.Add(New Tag(lXmlNode.Attributes))
                Case Else
                    pIssues.AppendLine("MissingXmlTag " & lXmlNode.Name & " forWay " & Id)
            End Select
        Next
    End Sub

    Public Function XML() As Xml.XmlNode
        Dim lXmlDocument As New Xml.XmlDocument
        Dim lXmlNode As Xml.XmlElement = lXmlDocument.CreateElement("way")
        If Id.Length > 0 Then lXmlNode.SetAttribute("id", Id)
        If Version <> 0 Then lXmlNode.SetAttribute("version", Version)
        If Changeset <> 0 Then lXmlNode.SetAttribute("changeset", Version)
        If User.Length > 0 Then lXmlNode.SetAttribute("user", User)
        If UId > 0 Then lXmlNode.SetAttribute("uid", UId)
        If Actor > 0 Then lXmlNode.SetAttribute("actor", Actor)
        If Timestamp.ToString.Length > 0 Then lXmlNode.SetAttribute("timestamp", Timestamp)
        For lNodeKeyIndex As Integer = 0 To NodeKeys.Count - 1
            Dim lXmlNodeChild As Xml.XmlElement = lXmlDocument.CreateElement("nd")
            lXmlNodeChild.SetAttribute("ref", NodeKeys(lNodeKeyIndex).Substring(1))
            lXmlNode.AppendChild(lXmlNodeChild)
        Next
        If Closed Then
            Dim lXmlNodeChild As Xml.XmlElement = lXmlDocument.CreateElement("nd")
            lXmlNodeChild.SetAttribute("ref", NodeKeys(0).Substring(1))
            lXmlNode.AppendChild(lXmlNodeChild)
        End If
        For Each lTag As Tag In Tags
            lXmlNode.AppendChild(lTag.XML(lXmlDocument))
        Next
        Return lXmlNode
    End Function
End Class
