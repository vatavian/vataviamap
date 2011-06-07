Imports System.Collections.ObjectModel
Imports System.Text

Module UserVars
    Public Users = New UserCollection
End Module

Public Class UserCollection
    Inherits SortedList(Of String, User)

    Public Sub AddReference(ByVal aName As String, ByRef aObject As Object)
        If Users.IndexOfKey(aName) = -1 Then
            Users.Add(aName, New User(aName))
        End If
        aObject.User = aName
        Dim lDate As Date = aObject.timestamp
        With CType(Users.Item(aName), User)
            If Not .Objects.Contains(aObject.id) Then
                .Objects.Add(aObject.Id, aObject)
            End If
            If .FirstEdit > lDate Then
                .FirstEdit = lDate
            End If
            If .LastEdit < lDate Then
                .LastEdit = lDate
            End If
        End With
    End Sub

    Public Function Summary() As String
        Dim lSB As New StringBuilder
        lSB.AppendLine(vbCrLf & "Users " & Users.Count)
        lSB.Append(vbTab & "User".PadRight(20) & vbTab & "RefCnt".PadLeft(8) & vbTab & "FirstEdit".PadLeft(24) & vbTab & "LastEdit".PadLeft(24))
        For Each lXmlNodeType As String In pXmlNodeTypeCounts.Keys
            If lXmlNodeType <> "bounds" Then
                lSB.Append(vbTab & lXmlNodeType.PadLeft(8))
            End If
        Next
        lSB.AppendLine()
        For Each lUser As User In Users.Values
            lSB.AppendLine(lUser.Summary)
        Next
        Return lSB.ToString
    End Function
End Class

Public Class User
    Public Name As String
    Public Objects As New SortedList
    Public FirstEdit As Date = "2100/01/01"
    Public LastEdit As Date = "1900/01/01"

    Public Sub New(ByVal aName As String)
        Name = aName
    End Sub

    Public Function Summary() As String
        Dim lSB As New StringBuilder
        Dim lCounts As New SortedList(Of String, Integer)
        For Each lUserObject In Me.Objects
            Dim lKey As String = lUserObject.Value.GetType.ToString.Replace("OsmXmlViewer.", "").ToLower
            If Not lCounts.ContainsKey(lKey) Then
                lCounts.Add(lKey, 0)
            End If
            lCounts(lKey) = lCounts(lKey) + 1
        Next
        With Me
            lSB.Append(vbTab & .Name.PadRight(20) & _
                       vbTab & .Objects.Count.ToString.PadLeft(8) & _
                       vbTab & .FirstEdit.ToString.PadLeft(24) & _
                       vbTab & .LastEdit.ToString.PadLeft(24))
            For Each lXmlNodeType As String In pXmlNodeTypeCounts.Keys
                If lXmlNodeType <> "bounds" Then
                    Dim lCount As Integer = 0
                    If lCounts.Keys.Contains(lXmlNodeType) Then
                        lCount = lCounts.Item(lXmlNodeType)
                    End If
                    lSB.Append(vbTab & lCount.ToString.PadLeft(8))
                End If
            Next
        End With
        Return lSB.ToString
    End Function
End Class

