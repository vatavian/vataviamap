Imports System.Collections.ObjectModel
Imports atcUtility

Public Class Users
    Inherits KeyedCollection(Of String, User)
    Protected Overrides Function GetKeyForItem(ByVal aUser As User) As String
        Return aUser.Name
    End Function

    Public Sub AddReference(ByVal aName As String, ByVal aObject As Object)
        If Not pUsers.Contains(aName) Then
            pUsers.Add(New User(aName))
        End If
        pUsers.Item(aName).Objects.Add(aObject)
        aObject.User = aName
    End Sub
End Class

Public Class User
        Public Name As String
        Public Objects As New atcCollection

    Public Sub New(ByVal aName As String)
        Name = aName
    End Sub
End Class

