Public Class frmOpenCellID

    Private pMapForm As frmMap
    Private pDownloader As clsDownloader

    Public Sub AskUser(ByVal aMapForm As frmMap, ByVal aDownloader As clsDownloader)
        pMapForm = aMapForm
        pDownloader = aDownloader
        lblWebSite.Text = clsCellLocationOpenCellID.WebsiteURL
        lblRawData.Text = clsCellLocationOpenCellID.RawDatabaseURL
        lstMCC.Items.Clear()
        Me.Show()
        Try
            Dim lMCCfilename As String = IO.Path.Combine(IO.Path.GetDirectoryName(Reflection.Assembly.GetEntryAssembly.Location), "mcc.txt")
            If IO.File.Exists(lMCCfilename) Then
                Dim lReader As IO.StreamReader = IO.File.OpenText(lMCCfilename)
                Dim lInputLine As String
                While Not lReader.EndOfStream
                    lInputLine = lReader.ReadLine()
                    If lInputLine.Contains(vbTab) Then lstMCC.Items.Add(lInputLine)
                End While
            End If
        Catch
        End Try
        If lstMCC.Items.Count = 0 Then
            lstMCC.Visible = False
        End If
    End Sub

    Private Sub btnWebSite_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnWebSite.Click
        OpenFile(clsCellLocationOpenCellID.WebsiteURL)
    End Sub

    Private Sub btnDownloadRaw_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDownloadRaw.Click
        Dim lSaveDialog As New Windows.Forms.SaveFileDialog
        With lSaveDialog
            .Title = "Save cells.txt as..."
            .FileName = "cells.txt"
            If .ShowDialog = Windows.Forms.DialogResult.OK Then
                If pDownloader.DownloadFile(clsCellLocationOpenCellID.RawDatabaseURL, .FileName & ".gz", False) Then
                    gunzip(.FileName & ".gz", .FileName)
                    SaveAppSetting("OpenCellIDRaw", .FileName)
                    MsgBox("Downloaded to " & .FileName, , "OpenCellID")
                Else
                    MsgBox("Was not able to download from" & vbLf & clsCellLocationOpenCellID.RawDatabaseURL, , "OpenCellID")
                End If
            End If
        End With
    End Sub

    Private Sub btnImportRaw_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnImportRaw.Click
        Dim lRawDataFilename As String = GetAppSetting("OpenCellIDRaw", Nothing)

        Dim lOpenDialog As New Windows.Forms.OpenFileDialog
        With lOpenDialog
            .Title = "Open raw data file"
            If lRawDataFilename IsNot Nothing AndAlso IO.File.Exists(lRawDataFilename) Then
                .FileName = lRawDataFilename
            Else
                .FileName = "cells.txt"
            End If
            .Filter = "*.txt|*.txt|*.txt.gz|*.txt.gz|*.*|*.*"
            If .ShowDialog = Windows.Forms.DialogResult.OK Then
                ImportRaw(.FileName, IO.Path.ChangeExtension(.FileName, ".cell"), txtMCC.Text.Split(","))
            End If
        End With
    End Sub

    Private Sub ImportRaw(ByVal aRawDataFilename As String, _
                          ByVal aConvertedDataFilename As String, _
                          ByVal aMCCs() As String)
        If aRawDataFilename IsNot Nothing AndAlso IO.File.Exists(aRawDataFilename) Then
            If aRawDataFilename.EndsWith("gz") Then
                Dim lUnzippedFilename As String = IO.Path.GetFileNameWithoutExtension(aRawDataFilename)
                gunzip(aRawDataFilename, lUnzippedFilename)
                aRawDataFilename = lUnzippedFilename
            End If
            Dim lCells As New clsCellLayer(pMapForm)
            If lCells.LoadRawCSV(aRawDataFilename, aMCCs) Then
                If lCells.SaveBinary(aConvertedDataFilename) Then
                    SaveAppSetting("OpenCellIDbinary", aConvertedDataFilename)
                    MsgBox("Converted to " & aConvertedDataFilename)
                End If
            End If
        End If
    End Sub

    Private Sub btnImportGPX_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnImportGPX.Click
        Dim lOpenDialog As New Windows.Forms.OpenFileDialog
        With lOpenDialog
            .Title = "Open raw data file"
            .FileName = "*.gpx"
            .Filter = "*.gpx|*.gpx|*.*|*.*"
            If .ShowDialog = Windows.Forms.DialogResult.OK Then
                OpenGPXs(New String() {.FileName})
            End If
        End With
    End Sub

    Private Sub btnImportGPX_DragDrop(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles btnImportGPX.DragDrop
        Me.Activate()
        If e.Data.GetDataPresent(Windows.Forms.DataFormats.FileDrop) Then
            OpenGPXs(e.Data.GetData(Windows.Forms.DataFormats.FileDrop))
        End If
    End Sub

    Private Sub btnImportGPX_DragEnter(ByVal sender As Object, ByVal e As System.Windows.Forms.DragEventArgs) Handles btnImportGPX.DragEnter
        If e.Data.GetDataPresent(Windows.Forms.DataFormats.FileDrop) Then
            e.Effect = Windows.Forms.DragDropEffects.All
        End If
    End Sub

    Private Sub OpenGPXs(ByVal aFilenames() As String)
        Dim lSaveTitle As String = Me.Text
        Dim lNumFiles As Integer = aFilenames.Length
        Dim lCellFilename As String
        Select Case lNumFiles
            Case 1 : lCellFilename = IO.Path.ChangeExtension(aFilenames(0), ".cell")
            Case Is > 1 : lCellFilename = IO.Path.Combine(IO.Path.GetDirectoryName(aFilenames(0)), "Imported.cell")
            Case Else : Return
        End Select
        If IO.File.Exists(lCellFilename) Then IO.File.Delete(lCellFilename)

        Dim lCurFile As Integer = 1

        Dim lCells As New clsCellLayer(pMapForm)
        For Each lFilename As String In aFilenames
            Me.Text = "Loading " & lCurFile & "/" & lNumFiles & " '" & IO.Path.GetFileNameWithoutExtension(lFilename) & "'"
            Dim lGPX As New clsGPX
            lGPX.LoadFile(lFilename)
            lCells.LoadGPX(lGPX, txtMCC.Text.Split(","))
            lCurFile += 1
        Next
        lCells.SaveBinary(lCellFilename)
        Me.Text = lSaveTitle
    End Sub

    Private Sub gunzip(ByVal aGzipFilename As String, ByVal aUnzipToFilename As String)
        Dim lWriteUnzipped As IO.FileStream = IO.File.Create(aUnzipToFilename)
        Dim lBuffer(4096) As Byte

#If UseSharpZip Then
        Dim lReadZipped As New ICSharpCode.SharpZipLib.GZip.GZipInputStream(IO.File.OpenRead(.FileName & ".gz"))
        ICSharpCode.SharpZipLib.Core.StreamUtils.Copy(lReadZipped, lWriteUnzipped, lBuffer)
#Else
        Dim lReadZipped As New System.IO.Compression.GZipStream(IO.File.OpenRead(aGzipFilename), IO.Compression.CompressionMode.Decompress)
        While True
            Dim lBytesRead As Integer = lReadZipped.Read(lBuffer, 0, 4096)
            If lBytesRead = 0 Then
                Exit While
            Else
                lWriteUnzipped.Write(lBuffer, 0, lBytesRead)
            End If
        End While
#End If

        lReadZipped.Close()
        lWriteUnzipped.Close()
    End Sub

    Private pInMCCItemCheck As Boolean = False
    Private Sub lstMCC_ItemCheck(ByVal sender As Object, ByVal e As System.Windows.Forms.ItemCheckEventArgs) Handles lstMCC.ItemCheck
        If Not pInMCCItemCheck Then
            pInMCCItemCheck = True

            lstMCC.SetItemCheckState(e.Index, e.NewValue)

            Dim lMCCs As String = ""
            For Each lCheckedItem As String In lstMCC.CheckedItems
                If lMCCs.Length > 0 Then lMCCs &= ", "
                lMCCs &= lCheckedItem.Substring(0, lCheckedItem.IndexOf(vbTab))
            Next
            txtMCC.Text = lMCCs

            pInMCCItemCheck = False
        End If
    End Sub

    Private Sub btnMapCells_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnMapCells.Click
        Dim lFilename As String = GetAppSetting("OpenCellIDbinary", Nothing)

        If Not IO.File.Exists(lFilename) Then
            Dim lOpenDialog As New Windows.Forms.OpenFileDialog
            With lOpenDialog
                .Title = "Open imported data file"
                .FileName = "cells.cell"
                .Filter = "*.cell|*.cell|*.*|*.*"
                If .ShowDialog = Windows.Forms.DialogResult.OK Then
                    lFilename = .FileName
                    SaveAppSetting("OpenCellIDbinary", lFilename)
                End If
            End With
        End If

        If IO.File.Exists(lFilename) Then
            pMapForm.OpenCell(lFilename)
            Me.Close()
        End If
    End Sub

End Class