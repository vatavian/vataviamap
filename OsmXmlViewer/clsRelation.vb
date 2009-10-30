Imports System.Collections.ObjectModel
Imports atcUtility

Public Class Relations
    Inherits KeyedCollection(Of String, Relation)
    Protected Overrides Function GetKeyForItem(ByVal aRelation As Relation) As String
        Return "K" & aRelation.Id
    End Function
End Class

Public Class Relation
    Public Id As String
    Public Timestamp As Date
    Public User As String
    Public Actor As Integer
    Public Version As Integer
    Public Tags As New Tags
    Public NodeKeys As New Collection(Of String)
    Public WayKeys As New Collection(Of String)
    Public Visible As Boolean
    Public UId As Integer
    Public Changeset As Integer

    Public Sub New(ByVal aNode As Xml.XmlNode)
        For Each lAttribute As XmlAttribute In aNode.Attributes
            Select Case lAttribute.Name
                Case "id" : Id = lAttribute.Value
                Case "timestamp" : Timestamp = lAttribute.Value
                Case "user" : pUsers.AddReference(lAttribute.Value, Me)
                    'Case "actor" : Actor = lAttribute.Value
                Case "version" : Version = lAttribute.Value
                Case "visible" : Visible = lAttribute.Value
                Case "uid" : UId = lAttribute.Value
                Case "changeset" : Changeset = lAttribute.Value
                Case Else
                    pSB.AppendLine("MissingAttribute " & lAttribute.Name & " forRelation " & Id)
            End Select
        Next
        For Each lXmlNode As XmlNode In aNode.ChildNodes
            Select Case lXmlNode.Name
                Case "member"
                    Dim lType As String = lXmlNode.Attributes("type").Value
                    Dim lKey As String = "K" & lXmlNode.Attributes("ref").Value
                    Select Case lType
                        Case "node"
                            NodeKeys.Add(lKey)
                        Case "way"
                            WayKeys.Add(lKey)
                        Case Else
                            pSB.AppendLine("MissingMemberType " & lType)
                    End Select
                Case "tag"
                    Dim lTag As New Tag(lXmlNode.Attributes, Me)
                    Tags.Add(lTag)
                Case Else
                    pSB.AppendLine("MissingXmlTag " & lXmlNode.Name & " forRelation " & Id)
            End Select
        Next
    End Sub
End Class