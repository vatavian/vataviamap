Imports System.Collections.ObjectModel

Public Module NodeVars
    Public Nodes As New NodeCollection
End Module

Public Class NodeCollection
    Inherits KeyedCollection(Of String, Node)
    Protected Overrides Function GetKeyForItem(ByVal aNode As Node) As String
        Return "K" & aNode.Id
    End Function
End Class

Public Class Node
    Public Id As String
    Public Lat As Double
    Public Lon As Double
    Public Version As Integer
    Public Changeset As Integer
    Public User As String = ""
    Public Actor As Integer
    Public UId As Integer
    Public Visible As Boolean = True
    Public Timestamp As Date
    Public Tags As New Tags
    Public RelationKeys As New Collection(Of String)

    Public Sub New(ByVal aXmlNode As Xml.XmlNode)
        For Each lAttribute As XmlAttribute In aXmlNode.Attributes
            Select Case lAttribute.Name
                Case "id" : Id = lAttribute.Value
                Case "lon" : Lon = lAttribute.Value
                Case "lat" : Lat = lAttribute.Value
                Case "timestamp" : Timestamp = lAttribute.Value
                Case "user" : User = lAttribute.Value
                Case "actor" : Actor = lAttribute.Value
                Case "versionr", "version" : Version = lAttribute.Value
                Case "visible" : Visible = lAttribute.Value
                Case "uid" : UId = lAttribute.Value
                Case "changeset" : Changeset = lAttribute.Value
                Case Else
                    pIssues.AppendLine("MissingAttribute " & lAttribute.Name & " forNode " & Id)
            End Select
        Next

        For Each lNode As XmlNode In aXmlNode.ChildNodes
            Select Case lNode.Name
                Case "tag"
                    Tags.Add(New Tag(lNode.Attributes))
                Case Else
                    pIssues.AppendLine("MissingXmlTag " & lNode.Name & " forNode " & Id)
            End Select
        Next
    End Sub

    Public Function XML() As Xml.XmlNode
        Dim lXmlDocument As New Xml.XmlDocument
        Dim lXmlNode As Xml.XmlElement = lXmlDocument.CreateElement("node")
        If Id.Length > 0 Then lXmlNode.SetAttribute("id", Id)
        If Lat <> 0 Then lXmlNode.SetAttribute("lat", Lat)
        If Lon <> 0 Then lXmlNode.SetAttribute("lon", Lon)
        If Version <> 0 Then lXmlNode.SetAttribute("version", Version)
        If Changeset <> 0 Then lXmlNode.SetAttribute("changeset", Version)
        If User.Length > 0 Then lXmlNode.SetAttribute("user", User)
        If UId > 0 Then lXmlNode.SetAttribute("uid", UId)
        If Actor > 0 Then lXmlNode.SetAttribute("actor", Actor)
        If Timestamp.ToString.Length > 0 Then lXmlNode.SetAttribute("timestamp", Timestamp)
        For Each lTag As Tag In Tags
            lXmlNode.AppendChild(lTag.XML(lXmlDocument))
        Next
        Return lXmlNode
    End Function
End Class
