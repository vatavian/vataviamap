Imports System.Collections.ObjectModel
Imports System.Text
Imports atcUtility

Module UserVars
    Public Users = New UserCollection
End Module

Public Class UserCollection
    Inherits KeyedCollection(Of String, User)
    Protected Overrides Function GetKeyForItem(ByVal aUser As User) As String
        Return aUser.Name
    End Function

    Public Sub AddReference(ByVal aName As String, ByRef aObject As Object)
        If Not Users.Contains(aName) Then
            Users.Add(New User(aName))
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
        lSB.AppendLine(vbCrLf & "Users:" & Users.Count)
        lSB.AppendLine(vbTab & "User".PadRight(20) & vbTab & "ReferenceCount" & vbTab & "FirstEdit" & vbTab & "LastEdit")
        For Each lUser As User In Users
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
        Dim lCounts As New atcCollection
        For Each lUserObject In Me.Objects
            lCounts.Increment(lUserObject.Value.GetType.ToString.Replace("OsmXmlViewer.", ""), 1)
        Next
        With Me
            lSB.AppendLine(vbTab & .Name.PadRight(20) & _
                           vbTab & .Objects.Count.ToString.PadLeft(8) & _
                           vbTab & .FirstEdit & _
                           vbTab & .LastEdit)
        End With
        For lIndex As Integer = 0 To lCounts.Count - 1
            'TODO: add fractions of totals - not known here
            lSB.AppendLine(vbTab & vbTab & lCounts.Keys(lIndex).ToString.PadRight(12) & _
                                   vbTab & lCounts.Item(lIndex).ToString.PadLeft(8))
        Next
        Return lSB.ToString
    End Function
End Class

