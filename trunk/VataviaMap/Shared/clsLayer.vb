Public Class clsLayer
    Public Filename As String
    Public Visible As Boolean = True
    Public Map As ctlMap 'frmMap 'used for clipping to current view

    Public LabelField As String
    Public LabelMinZoom As Integer = 13
    Public BrushLabel As SolidBrush
    Public FontLabel As Font

    'Filters for what to render
    Public OmitBefore As Date = Date.MinValue
    Public OmitAfter As Date = Date.MaxValue

    Protected pGroupField As String

    Protected pBounds As clsGPXbounds
    Protected pLegendColor As Color = Drawing.Color.HotPink

    Public Sub New(ByVal aFilename As String, ByVal aMap As ctlMap)
        Filename = aFilename
        Map = aMap
    End Sub

    Public Overridable Property Bounds() As clsGPXbounds
        Get
            Return pBounds
        End Get
        Set(ByVal value As clsGPXbounds)
            pBounds = value
        End Set
    End Property

    Public Overridable Sub Clear()
        Filename = ""
    End Sub

    Public Overridable Property GroupField() As String
        Get
            Return pGroupField
        End Get
        Set(ByVal value As String)
            pGroupField = value
        End Set
    End Property

    Public Overridable Property LegendColor() As Color
        Get
            Return pLegendColor
        End Get
        Set(ByVal value As Color)
            pLegendColor = value
        End Set
    End Property

    Public Overridable Sub Render(ByVal aTileServer As clsServer, ByVal g As Graphics, ByVal aTopLeftTile As Point, ByVal aOffsetToCenter As Point)
    End Sub

    Public Overridable Function Fields() As String()
        Dim lFields() As String = {}
        Return lFields
    End Function
End Class