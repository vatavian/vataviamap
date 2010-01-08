Public Class frmAbout

    Private Sub mnuClose_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles mnuClose.Click
        Me.Close()
    End Sub

    Private Sub frmAbout_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load
        comboPage.SelectedIndex = 0
    End Sub

    Private Sub frmAbout_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Resize
        Dim lBarcodeHeight As Integer = Me.ClientSize.Height - picDownloadBarcode.Top - picDownloadBarcode.Left
        Dim lBarcodeWidth As Integer = Me.ClientSize.Width - 2 * picDownloadBarcode.Left
        Dim lBarcodeSize = Math.Min(lBarcodeWidth, lBarcodeHeight)
        picDownloadBarcode.Height = lBarcodeSize
        picDownloadBarcode.Width = lBarcodeSize
        picProjectBarcode.Height = lBarcodeSize
        picProjectBarcode.Width = lBarcodeSize
    End Sub

    Private Sub comboPage_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles comboPage.SelectedIndexChanged
        Select Case comboPage.SelectedIndex
            Case 0
                lblProjectURL.Visible = False
                lblDownloadURL.Visible = True
                picProjectBarcode.Visible = False
                picDownloadBarcode.Visible = True
            Case 1
                lblProjectURL.Visible = True
                lblDownloadURL.Visible = False
                picProjectBarcode.Visible = True
                picDownloadBarcode.Visible = False
        End Select
    End Sub
End Class