Public Class Bounds
    Public MinLat As Double
    Public MaxLat As Double
    Public MinLon As Double
    Public MaxLon As Double
    Public Sub New(ByVal aXmlNode As Xml.XmlNode)
        For Each lAttribute As XmlAttribute In aXmlNode.Attributes
            Select Case lAttribute.Name
                Case "minlat" : MinLat = lAttribute.Value
                Case "maxlat" : MaxLat = lAttribute.Value
                Case "minlon" : MaxLon = lAttribute.Value
                Case "maxlon" : MinLon = lAttribute.Value
                Case "box"
                    Debug.Print(lAttribute.Name & ":" & lAttribute.Value)
                Case "origin"
                    Debug.Print(lAttribute.Name & ":" & lAttribute.Value)
                Case Else
                    pSB.AppendLine("MissingAttribute " & lAttribute.Name)
            End Select
        Next
    End Sub

    Public Function Summary() As String
        Return vbCrLf & "Bounds " & vbTab & MinLat & vbTab & MaxLat & vbTab & MinLon & vbTab & MaxLon
    End Function
End Class
