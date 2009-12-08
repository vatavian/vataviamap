Imports System.Collections.ObjectModel
Imports atcUtility

Public Class Tags
    Inherits KeyedCollection(Of String, Tag)
    Protected Overrides Function GetKeyForItem(ByVal aTag As Tag) As String
        Return aTag.Key
    End Function
    Public Shared TagNames As New SortedList

    Public Shared Sub AddTags(ByVal aTags As Tags, ByRef aObject As Object)
        For Each lTag As Tag In aTags
            If Not Tags.TagNames.Contains(lTag.Key) Then
                Tags.TagNames.Add(lTag.Key, New atcCollection)
            End If

            If Tags.TagNames(lTag.Key).indexfromkey(lTag.Value) = -1 Then
                Tags.TagNames(lTag.Key).Add(lTag.Value, New atcCollection)
            End If
            CType(CType(Tags.TagNames(lTag.Key), atcCollection).ItemByKey(lTag.Value), atcCollection).Add(aObject)
        Next
    End Sub
End Class

Public Class Tag
    Public Key As String
    Public Value As String

    Public Sub New(ByVal aTagAttributes As XmlAttributeCollection)
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
    End Sub

    Public Function XML(ByVal aXmlDocument As XmlDocument) As Xml.XmlNode
        Dim lXmlTag As Xml.XmlElement = aXmlDocument.CreateElement("tag")
        lXmlTag.SetAttribute("k", Me.Key)
        lXmlTag.SetAttribute("v", Me.Value)
        Return lXmlTag
    End Function
End Class
