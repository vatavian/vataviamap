Public Class frmLayers
    Public Event CheckedItemsChanged(ByVal aCheckedItems As ArrayList)
    Public Event ColorChanged()

    Private pPopulating As Boolean = False
    Private pLayers As New Generic.List(Of clsLayer)
    Private pRandom As New Random

    Private Sub RaiseChanged()
        If Not pPopulating Then
            Dim lCheckedItems As New ArrayList
            For Each lItem As String In lstLayers.CheckedItems
                lCheckedItems.Add(lItem)
            Next
            RaiseEvent CheckedItemsChanged(lCheckedItems)
        End If
    End Sub

    Private Sub lstLayers_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lstLayers.SelectedIndexChanged
        Dim lLayer As clsLayer = pLayers.Item(lstLayers.SelectedIndex)
        If lLayer.GetType.Name = "clsLayerGPX" Then
            Dim lGPX As clsLayerGPX = lLayer
            Dim lColor As Color = lGPX.PenTrack.Color
            txtOpacity.Text = lColor.ToArgb
        End If

        RaiseChanged()
    End Sub

    Public Sub PopulateList(ByVal aLayers As Generic.List(Of clsLayer))
        Dim lSelectedIndex As Integer = -1
        pPopulating = True
        lstLayers.Items.Clear()
        Dim lHaveLayers As Boolean = aLayers IsNot Nothing AndAlso aLayers.Count > 0
        If lHaveLayers Then
            pLayers = aLayers
            For Each lLayer As clsLayer In aLayers                
                lstLayers.Items.Add(lLayer.Filename)
                lstLayers.SetItemChecked(lstLayers.Items.Count - 1, lLayer.Visible)                
            Next
        ElseIf pLayers.Count > 0 Then
            pLayers = New Generic.List(Of clsLayer)
        End If
        '        If IO.Directory.Exists(txtGPXFolder.Text) Then
        '            For Each lFilename As String In IO.Directory.GetFiles(txtGPXFolder.Text, "*.gpx")
        '                If lHaveGPX Then
        '                    For Each lGPX As clsGPX In GPXfilesLoaded
        '                        If lGPX.Filename = lFilename Then GoTo NextFile
        '                    Next
        '                End If
        '                lItem = New ListViewItem(lFilename)
        '                lstGPX.Items.Add(lItem)
        'NextFile:
        '            Next
        '        End If
        If lstLayers.Items.Count > 0 Then
            lstLayers.SelectedIndex = 0
        End If
        pPopulating = False
    End Sub

    Private Sub chkDifferentColors_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkDifferentColors.CheckedChanged
        Dim lColor As Color
        Dim lGPX As clsLayerGPX

        Dim lAlpha As Integer = 255
        If IsNumeric(txtOpacity.Text) Then
            lAlpha = CInt(txtOpacity.Text) * 2.55
        End If

        Dim lWidth As Integer = 1
        If IsNumeric(txtWidth.Text) Then
            lWidth = CInt(txtWidth.Text) * 2.55
        End If

        For Each lLayer As clsLayer In pLayers
            If lLayer.GetType.Name = "clsLayerGPX" Then
                lGPX = lLayer
                If chkDifferentColors.Checked Then
                    lColor = RandomRGBColor(lAlpha)
                Else
                    If lColor = Color.White Then
                        lColor = lGPX.PenTrack.Color
                    End If
                End If
                lGPX.PenTrack = New Pen(lColor, lWidth)
            End If
        Next
        RaiseChanged()
    End Sub

    ' Return a random RGB color.
    Public Function RandomRGBColor(Optional ByVal aAlpha As Integer = 255) As Color
        Return Color.FromArgb(aAlpha, _
                 pRandom.Next(0, 255), _
                 pRandom.Next(0, 255), _
                 pRandom.Next(0, 255))
    End Function

End Class