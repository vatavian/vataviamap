Imports System.Collections.ObjectModel
Imports System.Text
Imports atcUtility

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
        lSB.AppendLine(vbCrLf & "Ways Id:Tags:Nodes")
        For Each lWay As Way In Ways
            lSB.AppendLine(vbTab & lWay.Id & ":" & lWay.Tags.Count & ":" & lWay.NodeKeys.Count)
            For Each lNodeKey As String In lWay.NodeKeys
                If Not Nodes.Contains(lNodeKey) Then
                    lSB.AppendLine(vbTab & vbTab & "MissingNode " & lNodeKey)
                End If
            Next
        Next
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
    Public LatMin As Double = 90
    Public LatMax As Double = -90
    Public LonMin As Double = 180
    Public LonMax As Double = -180
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
                    pSB.AppendLine("MissingAttribute " & lAttribute.Name & " forWay " & Id)
            End Select
        Next

        For Each lXmlNode As XmlNode In aXmlNode.ChildNodes
            Select Case lXmlNode.Name
                Case "nd"
                    Dim lKey As String = "K" & lXmlNode.Attributes("ref").Value
                    If NodeKeys.Contains(lKey) Then
                        Closed = True
                    Else
                        NodeKeys.Add(lKey)
                        Dim lNode As Node = Nodes.Item(lKey)
                        With lNode
                            If .Lon < LonMin Then LonMin = .Lon
                            If .Lon > LonMax Then LonMax = .Lon
                            If .Lat < LatMin Then LatMin = .Lat
                            If .Lat > LatMax Then LatMax = .Lat
                        End With
                    End If
                Case "tag"
                    Dim lTag As New Tag(lXmlNode.Attributes, Me)
                    Tags.Add(lTag)
                Case Else
                    pSB.AppendLine("MissingXmlTag " & lXmlNode.Name & " forWay " & Id)
            End Select
        Next
    End Sub
End Class
