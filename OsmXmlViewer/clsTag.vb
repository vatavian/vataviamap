Imports System.Collections.ObjectModel
Imports atcUtility

Public Class Tags
    Inherits KeyedCollection(Of String, Tag)
    Protected Overrides Function GetKeyForItem(ByVal aTag As Tag) As String
        Return aTag.Key
    End Function
    Public Shared TagNames As New atcCollection
End Class

Public Class Tag
    Public Key As String
    Public Value As String

    Public Sub New(ByVal aTagAttributes As XmlAttributeCollection, ByRef aObject As Object)
        For Each lTagAttribute As XmlAttribute In aTagAttributes
            Select Case lTagAttribute.Name
                Case "k"
                    Key = lTagAttribute.Value
                Case "v"
                    Value = lTagAttribute.Value
                Case Else
                    Debug.Print("Why?")
            End Select
        Next
        If Tags.TagNames.IndexFromKey(Key) = -1 Then
            Tags.TagNames.Add(Key, New atcCollection)
        End If

        Dim lTagIndex As Integer = Tags.TagNames.IndexFromKey(Key)
        If Tags.TagNames(lTagIndex).indexfromkey(Value) = -1 Then
            Tags.TagNames(lTagIndex).Add(Value, New atcCollection)
        End If
        CType(CType(Tags.TagNames(lTagIndex), atcCollection).ItemByKey(Me.Value), atcCollection).Add(aObject)
    End Sub

    Public Function XML(ByVal aXmlDocument As XmlDocument) As Xml.XmlNode
        Dim lXmlTag As Xml.XmlElement = aXmlDocument.CreateElement("tag")
        lXmlTag.SetAttribute("k", Me.Key)
        lXmlTag.SetAttribute("v", Me.Value)
        Return lXmlTag
    End Function
End Class
