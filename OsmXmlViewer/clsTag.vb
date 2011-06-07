Imports System.Collections.ObjectModel

Public Class Tags
    Inherits KeyedCollection(Of String, Tag)
    Protected Overrides Function GetKeyForItem(ByVal aTag As Tag) As String
        Return aTag.Key
    End Function
    Public Shared TagNames As New SortedList(Of String, SortedList)

    Public Shared Sub AddTags(ByVal aTags As Tags, ByRef aObject As Object)
        For Each lTag As Tag In aTags
            If Not TagNames.ContainsKey(lTag.Key) Then
                TagNames.Add(lTag.Key, New SortedList)
            End If

            Dim lTagNameList As SortedList = TagNames(lTag.Key)
            If lTagNameList.IndexOfKey(lTag.Value) = -1 Then
                lTagNameList.Add(lTag.Value, New SortedList)
            End If
            Dim lTagNameValues As SortedList = lTagNameList.Item(lTag.Value)
            lTagNameValues.Add(aObject.Id, aObject)
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
