Imports System.Collections.ObjectModel
Imports atcUtility

Public Class Nodes
    Inherits KeyedCollection(Of String, Node)
    Protected Overrides Function GetKeyForItem(ByVal aNode As Node) As String
        Return "K" & aNode.Id
    End Function
End Class

Public Class Node
    Public Id As String
    Public Lat As Double
    Public Lon As Double
    Public Timestamp As Date
    Public User As String
    Public Actor As Integer
    Public Version As Integer
    Public Tags As New Tags
    Public Visible As Boolean
    Public UId As Integer
    Public Changeset As Integer

    Public Sub New(ByVal aNode As Xml.XmlNode)
        For Each lAttribute As XmlAttribute In aNode.Attributes
            Select Case lAttribute.Name
                Case "id" : Id = lAttribute.Value
                Case "lon" : Lon = lAttribute.Value
                Case "lat" : Lat = lAttribute.Value
                Case "timestamp" : Timestamp = lAttribute.Value
                Case "user" : pUsers.AddReference(lAttribute.Value, Me)
                Case "actor" : Actor = lAttribute.Value
                Case "versionr", "version" : Version = lAttribute.Value
                Case "visible" : Visible = lAttribute.Value
                Case "uid" : UId = lAttribute.Value
                Case "changeset" : Changeset = lAttribute.Value
                Case Else
                    pSB.AppendLine("MissingAttribute " & lAttribute.Name & " forNode " & Id)
            End Select
        Next
        For Each lNode As XmlNode In aNode.ChildNodes
            Select Case lNode.Name
                Case "tag"
                    Dim lTag As New Tag(lNode.Attributes, Me)
                    Tags.Add(lTag)
                Case Else
                    pSB.AppendLine("MissingXmlTag " & lNode.Name & " forNode " & Id)
            End Select
        Next
    End Sub
End Class
