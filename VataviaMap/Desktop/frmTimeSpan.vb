Public Class frmTimeSpan

    Public Event Changed(ByVal aUTCoffset As TimeSpan)

    Private pPopulating As Boolean = False
    Private pLayers As New Generic.List(Of clsLayer)

    Public Sub PopulateList(ByVal aLayers As Generic.List(Of clsLayer))
        Dim lSelectedIndex As Integer = -1
        pPopulating = True
        'lstLayers.Items.Clear()
        Dim lHaveLayers As Boolean = aLayers IsNot Nothing AndAlso aLayers.Count > 0
        If lHaveLayers Then
            Dim lLabelFieldsSet As Boolean = False
            pLayers = aLayers
            For Each lLayer As clsLayer In aLayers
                'Dim lItem As ListViewItem = lstLayers.Items.Add(lLayer.Filename)
                'lItem.BackColor = lLayer.LegendColor
                'If lItem.BackColor.GetBrightness < 0.5 Then
                '    lItem.ForeColor = Color.White
                'End If
                'lItem.Checked = lLayer.Visible
                If lLayer.GetType.Name = "clsLayerGPX" Then
                    Dim lGPX As clsLayerGPX = lLayer
                    Dim lFirstPoint As clsGPXwaypoint = lGPX.GPX.FirstPoint
                    Dim lLastPoint As clsGPXwaypoint = Nothing
                    If lFirstPoint Is Nothing OrElse lFirstPoint.time.Year < 2 Then
                        'lItem.SubItems.Add("")
                    Else
                        'lItem.SubItems.Add(lFirstPoint.time.ToString)
                    End If
                    If lLastPoint Is Nothing OrElse lLastPoint.time.Subtract(lFirstPoint.time).TotalSeconds < 1 Then
                        'lItem.SubItems.Add("")
                    Else
                        'lItem.SubItems.Add(lLastPoint.time.Subtract(lFirstPoint.time).ToString)
                    End If
                Else
                    'lItem.SubItems.Add("") 'TODO: get file date from lLayer.Filename
                    'lItem.SubItems.Add("")
                End If
                If Not lLabelFieldsSet Then
                    'cboLabelField.Items.Clear()
                    'cboLabelField.Items.AddRange(lLayer.Fields)
                    'cboGroupField.Items.Clear()
                    'cboGroupField.Items.AddRange(lLayer.Fields)
                    'lLabelFieldsSet = (cboLabelField.Items.Count > 0)
                End If
            Next
        ElseIf pLayers.Count > 0 Then
            pLayers = New Generic.List(Of clsLayer)
        End If
        'cboLayerZoom.Items.Clear()
        'For lZoom As Integer = g_TileServer.ZoomMin To g_TileServer.ZoomMax
        'cboLayerZoom.Items.Add(lZoom)
        'Next
        pPopulating = False
    End Sub

End Class