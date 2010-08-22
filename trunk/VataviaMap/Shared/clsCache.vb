Public Class clsCache
    Private pFolder As String
    Private pFolderInfo As clsFileInfo

    Public Sub New(ByVal aFolder As String)
        pFolder = aFolder
        pFolderInfo = New clsFileInfo(aFolder, True, True)
    End Sub

    Public Function Summary(Optional ByVal aCriteria As clsCriteria = Nothing) As String
        Dim lSummary As New System.Text.StringBuilder
        If Not IO.Directory.Exists(pFolder) Then
            lSummary.Append("Folder does not exist: " & pFolder)
        Else
            If aCriteria Is Nothing Then aCriteria = New clsCriteria 'Default to base clsCriteria which includes all files
            Dim lClassifications As New clsClassifications
            SearchCriteria(pFolderInfo, aCriteria, lClassifications)
            Dim lSortedSummaries As New SortedDictionary(Of String, clsSummary)
            For Each lCritSummary As clsSummary In lClassifications
                lSortedSummaries.Add(lCritSummary.Name, lCritSummary)
            Next
            For Each lCritSummary As clsSummary In lSortedSummaries.Values
                lSummary.AppendLine(lCritSummary.ToString & vbCrLf & vbCrLf)
            Next
        End If

        Return lSummary.ToString
    End Function

    Public Sub SearchCriteria(ByVal aFile As clsFileInfo, ByVal aCriteria As clsCriteria, ByRef aClassifications As clsClassifications)
        If aClassifications Is Nothing Then aClassifications = New clsClassifications
        If aFile.DirectoryEntries Is Nothing Then
            aClassifications.AddFileToSummary(aFile, aCriteria)
        Else
            For Each lFile As clsFileInfo In aFile.DirectoryEntries
                SearchCriteria(lFile, aCriteria, aClassifications)
            Next
        End If
    End Sub

    Public Sub DeleteCriteria(ByVal aFile As clsFileInfo, ByVal aCriteria As clsCriteria, ByVal aClassificationName As String)
        If aFile.DirectoryEntries Is Nothing Then
            If aCriteria.ClassificationName(aFile) = aClassificationName Then
                Try
                    IO.File.Delete(aFile.Name)
                    aFile.Exists = False
                Catch eCouldNotDelete As Exception
                End Try
            End If
        Else
            For Each lFile As clsFileInfo In aFile.DirectoryEntries
                DeleteCriteria(lFile, aCriteria, aClassificationName)
            Next
        End If
    End Sub
End Class

Public Class clsFileInfo
        Public Name As String
        Public CreationDate As Date
        Public NumBytes As Long = 0
        Public DirectoryEntries As Generic.List(Of clsFileInfo)
        Public Exists As Boolean

    Public Sub New(ByVal aFilename As String, ByVal aRecursive As Boolean, ByVal aExists As Boolean)
        Name = aFilename
        Exists = aExists
        If Exists Then
            If IO.File.Exists(aFilename) Then
                CreationDate = IO.File.GetCreationTime(aFilename)
                NumBytes = FileLen(aFilename)
            ElseIf IO.Directory.Exists(aFilename) Then
                CreationDate = IO.File.GetCreationTime(aFilename)
                If aRecursive Then
                    DirectoryEntries = New Generic.List(Of clsFileInfo)
                    For Each lEntry As String In IO.Directory.GetFileSystemEntries(aFilename)
                        Dim lNewEntry As New clsFileInfo(lEntry, True, True)
                        DirectoryEntries.Add(lNewEntry)
                        NumBytes += lNewEntry.NumBytes
                    Next
                End If
            Else
                Exists = False
            End If
        End If
    End Sub
End Class

Public Class clsSummary
    Public Name As String
    Public NumFiles As Long = 0
    Public NumBytes As Long = 0

    Public Sub New(ByVal aName As String)
        Name = aName
    End Sub

    Public Sub AddFile(ByVal aFilename As String)
        NumFiles += 1
        NumBytes += FileLen(aFilename)
    End Sub

    Public Sub AddFile(ByVal aFile As clsFileInfo)
        NumFiles += 1
        NumBytes += aFile.NumBytes
    End Sub

    Public Overrides Function ToString() As String
        Return Name & vbCrLf _
            & "Files: " & Format(NumFiles, "#,###") & vbCrLf _
            & "Bytes: " & Format(NumBytes, "#,###")
    End Function
End Class

Public Class clsClassifications
    Inherits System.Collections.ObjectModel.KeyedCollection(Of String, clsSummary)

    Protected pAll As New clsSummary("All")

    Sub New()
        Me.Add(pAll)
    End Sub

    Protected Overrides Function GetKeyForItem(ByVal item As clsSummary) As String
        Return item.Name
    End Function

    Public Overridable Sub AddFileToSummary(ByVal aFile As clsFileInfo, ByVal aCriteria As clsCriteria)
        pAll.AddFile(aFile)
        Dim lSummaryName As String = aCriteria.ClassificationName(aFile)
        Dim lSummary As clsSummary
        If Me.Contains(lSummaryName) Then
            lSummary = Me.Item(lSummaryName)
        Else
            lSummary = New clsSummary(lSummaryName)
            Me.Add(lSummary)
        End If
        lSummary.AddFile(aFile)
    End Sub
End Class

Public Class clsCriteria
    Public Overridable Function ClassificationName(ByVal aFile As clsFileInfo) As String
        Return "All"
    End Function
End Class

Public Class clsCriteriaDateThreshold
    Inherits clsCriteria
    Private pDate As Date

    Private pOlderName As String
    Private pNewerName As String

    Public Sub New(ByVal aDate As Date)
        pDate = aDate
        pOlderName = "Older than " & aDate.ToShortDateString
        pNewerName = "Newer than " & aDate.ToShortDateString
    End Sub

    Public Overrides Function ClassificationName(ByVal aFile As clsFileInfo) As String
        If aFile.CreationDate < pDate Then
            Return pOlderName
        Else
            Return pNewerName
        End If
    End Function
End Class

Public Class clsCriteriaDateAge
    Inherits clsCriteria
    Private pDate As Date
    Private pDaysPerChunk As Double

    Public Sub New(ByVal aNow As Date, ByVal aDaysPerChunk As Double)
        pDate = aNow
        pDaysPerChunk = aDaysPerChunk
    End Sub

    Public Overrides Function ClassificationName(ByVal aFile As clsFileInfo) As String
        Return CStr(CInt(pDate.Subtract(aFile.CreationDate).TotalDays / pDaysPerChunk)).PadLeft(3, "0")
    End Function
End Class

Public Class clsCriteriaFolder
    Inherits clsCriteria
    Private pBaseFolder As String
    Private pPadLen As Integer
    Private pPadChar As String

    Public Sub New(ByVal aBaseFolder As String, Optional ByVal aPadLen As Integer = 0, Optional ByVal aPadChar As String = " ")
        pBaseFolder = aBaseFolder
        pPadLen = aPadLen
        pPadChar = aPadChar
    End Sub

    Public Overrides Function ClassificationName(ByVal aFile As clsFileInfo) As String
        If pPadLen > 0 Then
            Return aFile.Name.Substring(pBaseFolder.Length).Split(IO.Path.DirectorySeparatorChar)(0).PadLeft(pPadLen, pPadChar)
        Else
            Return aFile.Name.Substring(pBaseFolder.Length).Split(IO.Path.DirectorySeparatorChar)(0)
        End If
    End Function
End Class
