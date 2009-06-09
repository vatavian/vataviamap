Option Strict Off
Option Explicit On

Imports System.Xml

''' <summary>
''' Author: Mark Gray mark-gpx@hspf.com
''' Open GPS Exchange Format (GPX) files (version 1.0 or 1.1)
''' GPX files are used for transferring:
''' waypoints (geographic points of interest), 
''' tracks (recordings of where the GPS has been), and
''' routes (sequences of waypoints describing where to go)
''' to and from GPS devices.
''' For more information about GPX see http://www.topografix.com/gpx.asp
''' Free software for communicating with GPS devices and handling GPX and similar data types at http://www.gpsbabel.org/
''' </summary>
''' <remarks></remarks>
Public Class clsGPX
    Inherits clsGPXbase

    Public Filename As String = ""

    Private Shared pTryAppend As Boolean = True 'Change to False to not try appending missing tags
    Private pWaypoints As Generic.List(Of clsGPXwaypoint)
    Private pRoutes As Generic.List(Of clsGPXroute)
    Private pTracks As Generic.List(Of clsGPXtrack)
    Private versionField As String
    Private creatorField As String
    Private nameField As String
    Private descField As String
    Private authorField As XmlElement
    Private copyrightField As clsGPXcopyright
    Private timeField As Date
    Private timeFieldSpecified As Boolean
    Private keywordsField As String
    Private boundsField As clsGPXbounds

    Public Sub New()
        Clear()
    End Sub

    Overrides Sub Clear()
        MyBase.Clear()
        pWaypoints = New Generic.List(Of clsGPXwaypoint)
        pRoutes = New Generic.List(Of clsGPXroute)
        pTracks = New Generic.List(Of clsGPXtrack)
        versionField = "1.1"
        creatorField = ""
        Filename = ""
    End Sub

    Overridable Sub LoadFile(ByVal aFilename As String)
        Dim lTryAppend As Boolean = pTryAppend
        Dim pXMLdoc As New XmlDocument

        'TryAgain:
        Try
            Clear()
            Filename = aFilename
            pXMLdoc.Load(aFilename)
LoadedXML:
            For Each lChild As XmlNode In pXMLdoc.ChildNodes(1).ChildNodes
                Select Case lChild.Name
                    Case "extensions"
                        SetExtensions(lChild)
                    Case "link"
                        AddLink(lChild)
                    Case "rte"
                        pRoutes.Add(New clsGPXroute(lChild))
                    Case "trk"
                        pTracks.Add(New clsGPXtrack(lChild))
                    Case "wpt", "waypoint"
                        pWaypoints.Add(New clsGPXwaypoint(lChild))
                    Case "bounds"
                        boundsField = New clsGPXbounds(lChild)
                    Case Else
                        SetSomething(Me, lChild.Name, lChild.InnerXml)
                        'Logger.Dbg("Skipped unknown node type: " & lChild.Name)
                End Select
            Next
            If Me.boundsField Is Nothing Then
                Me.boundsField = New clsGPXbounds
                For Each lWaypoint As clsGPXwaypoint In wpt
                    ExpandBounds(lWaypoint)
                Next

                For Each lTrack As clsGPXtrack In trk
                    For Each lTrackSegment As clsGPXtracksegment In lTrack.trkseg
                        For Each lTrackPoint As clsGPXwaypoint In lTrackSegment.trkpt
                            ExpandBounds(lTrackPoint)
                        Next
                    Next
                Next
            End If
        Catch e As Exception
            If lTryAppend AndAlso e.Message.StartsWith("Unexpected end of file has occurred. The following elements are not closed: trkseg, trk, gpx") Then
                lTryAppend = False 'don't try again with the same file
                Try
                    Dim lReader As IO.StreamReader = IO.File.OpenText(aFilename)
                    pXMLdoc.LoadXml(lReader.ReadToEnd() & "</trkseg></trk></gpx>")
                    lReader.Close()
                    'Adding close tags seems to have made it load successfully
                    Try ' Append tags to file to make it correct
                        Dim lFile As IO.StreamWriter = IO.File.AppendText(aFilename)
                        lFile.Write("</trkseg></trk></gpx>")
                        lFile.Close()
                    Catch e3 As Exception
                        'Ignore inability to append to file
                    End Try
                    GoTo LoadedXML
                Catch e2 As Exception
                End Try
            End If
            Throw New ApplicationException("Error opening '" & aFilename & "': " & e.Message, e)
        End Try
    End Sub

    Public Overrides Function ToString() As String
        Dim lXML As String = "<gpx xmlns=""http://www.topografix.com/GPX/1/1"" version=""1.1"" creator=""" _
            & creator & ">"

        If pWaypoints.Count > 0 Then
            lXML &= "<wpt>"
            For Each lWpt As clsGPXwaypoint In pWaypoints
                lXML &= lWpt.ToString
            Next
            lXML &= "</wpt>"
        End If
        lXML &= extensionsString()
        lXML &= linkString()
        Return lXML & "</gpx>"
    End Function

    Public Sub ExpandBounds(ByVal aWaypoint As clsGPXwaypoint)
        If Me.boundsField Is Nothing Then Me.boundsField = New clsGPXbounds
        With aWaypoint
            If .lat < boundsField.minlat Then
                boundsField.minlat = .lat
            End If
            If .lat > boundsField.maxlat Then
                boundsField.maxlat = .lat
            End If
            If .lon < boundsField.minlon Then
                boundsField.minlon = .lon
            End If
            If .lon > boundsField.maxlon Then
                boundsField.maxlon = .lon
            End If
        End With
    End Sub

    <System.Xml.Serialization.XmlElementAttribute("wpt")> _
    Public Property wpt() As Generic.List(Of clsGPXwaypoint)
        Get
            Return Me.pWaypoints
        End Get
        Set(ByVal value As Generic.List(Of clsGPXwaypoint))
            Me.pWaypoints = value
        End Set
    End Property


    <System.Xml.Serialization.XmlElementAttribute("rte")> _
    Public Property rte() As Generic.List(Of clsGPXroute)
        Get
            Return Me.pRoutes
        End Get
        Set(ByVal value As Generic.List(Of clsGPXroute))
            Me.pRoutes = value
        End Set
    End Property


    <System.Xml.Serialization.XmlElementAttribute("trk")> _
    Public Property trk() As Generic.List(Of clsGPXtrack)
        Get
            Return Me.pTracks
        End Get
        Set(ByVal value As Generic.List(Of clsGPXtrack))
            Me.pTracks = value
        End Set
    End Property


    <System.Xml.Serialization.XmlAttributeAttribute()> _
    Public Property version() As String
        Get
            Return Me.versionField
        End Get
        Set(ByVal value As String)
            Me.versionField = value
        End Set
    End Property


    <System.Xml.Serialization.XmlAttributeAttribute()> _
    Public Property creator() As String
        Get
            Return Me.creatorField
        End Get
        Set(ByVal value As String)
            Me.creatorField = value
        End Set
    End Property

    Public Property name() As String
        Get
            Return Me.nameField
        End Get
        Set(ByVal value As String)
            Me.nameField = value
        End Set
    End Property

    Public Property desc() As String
        Get
            Return Me.descField
        End Get
        Set(ByVal value As String)
            Me.descField = value
        End Set
    End Property

    Public Property author() As XmlElement
        Get
            Return Me.authorField
        End Get
        Set(ByVal value As XmlElement)
            Me.authorField = value
        End Set
    End Property

    Public Property copyright() As clsGPXcopyright
        Get
            Return Me.copyrightField
        End Get
        Set(ByVal value As clsGPXcopyright)
            Me.copyrightField = value
        End Set
    End Property

    Public Property time() As Date
        Get
            Return Me.timeField
        End Get
        Set(ByVal value As Date)
            Me.timeField = value
            timeSpecified = True
        End Set
    End Property

    Public Property timeSpecified() As Boolean
        Get
            Return Me.timeFieldSpecified
        End Get
        Set(ByVal value As Boolean)
            Me.timeFieldSpecified = value
        End Set
    End Property

    Public Property keywords() As String
        Get
            Return Me.keywordsField
        End Get
        Set(ByVal value As String)
            Me.keywordsField = value
        End Set
    End Property

    Public Property bounds() As clsGPXbounds
        Get
            Return Me.boundsField
        End Get
        Set(ByVal value As clsGPXbounds)
            Me.boundsField = value
        End Set
    End Property

End Class

Public Class clsGPXtracksegment
    Inherits clsGPXbase

    Private trkptField As Generic.List(Of clsGPXwaypoint)

    Public Sub New()
        trkptField = New Generic.List(Of clsGPXwaypoint)
    End Sub

    Public Sub New(ByVal aXML As XmlNode)
        trkptField = New Generic.List(Of clsGPXwaypoint)
        For Each lChild As XmlNode In aXML.ChildNodes
            Try
                Select Case lChild.Name
                    Case "extensions"
                        SetExtensions(lChild)
                    Case "link"
                        AddLink(lChild)
                    Case "trkpt"
                        trkptField.Add(New clsGPXwaypoint(lChild))
                    Case Else
                        Dim lPropertyInfo As System.Reflection.PropertyInfo = Me.GetType.GetProperty(lChild.Name)
                        If lPropertyInfo IsNot Nothing Then
                            lPropertyInfo.SetValue(Me, lChild.InnerText, Nothing)
                        Else
                            'Logger.Dbg("Skipped unknown node type: " & lChild.Name)
                        End If
                End Select
            Catch e As Exception
                'Logger.Dbg("Unable to set " & lChild.Name & " = " & lChild.InnerXml)
            End Try
        Next
    End Sub

    Public Property trkpt() As Generic.List(Of clsGPXwaypoint)
        Get
            Return Me.trkptField
        End Get
        Set(ByVal value As Generic.List(Of clsGPXwaypoint))
            Me.trkptField = value
        End Set
    End Property

End Class

Public Class clsGroundspeakLog
    Public logid As String
    Public logdate As String
    Public logtype As String
    Public logfinder As String
    Public logfinderid As String
    Public logtext As String

    Public Sub New(ByVal aXML As XmlNode)
        For Each lAttribute As XmlAttribute In aXML.Attributes
            SetSomething(Me, "log" & lAttribute.Name, lAttribute.InnerText)
        Next

        For Each lChild As XmlNode In aXML.ChildNodes
            Dim lChildName As String = lChild.Name.Replace("groundspeak:", "")
            If lChildName = "finder" Then logfinderid = lChild.Attributes(0).InnerText
            SetSomething(Me, "log" & lChildName, lChild.InnerText)
        Next
    End Sub

    Public Overrides Function ToString() As String
        Return "<groundspeak:log id=""" & logid & """>" & ControlChars.Lf _
             & "<groundspeak:date>" & logdate & "</groundspeak:date>" & ControlChars.Lf _
             & "<groundspeak:type>" & logtype & "</groundspeak:type>" & ControlChars.Lf _
             & "<groundspeak:finder id=""" & logfinderid & """>" & logfinder & "</groundspeak:finder>" & ControlChars.Lf _
             & "<groundspeak:text>" & logtext & "</groundspeak:text>" & ControlChars.Lf
    End Function
End Class

Public Class clsGroundspeakCache

    Public id As String
    Public available As Boolean
    Public archived As Boolean
    Public name As String
    Public placed_by As String
    Public owner As String
    Public cachetype As String
    Public container As String
    Public difficulty As Single
    Public terrain As Single
    Public country As String
    Public state As String
    Public short_description As String
    Public long_description As String
    Public encoded_hints As String
    Public logs As New Generic.List(Of clsGroundspeakLog)
    Public travelbugs As String

    Public Sub New(ByVal aXML As XmlNode)
        For Each lAttribute As XmlAttribute In aXML.Attributes
            SetSomething(Me, lAttribute.Name, lAttribute.InnerText)
        Next

        For Each lChild As XmlNode In aXML.ChildNodes
            Dim lChildName As String = lChild.Name.Replace("groundspeak:", "")
            Select Case lChildName
                Case "type" : lChildName = "cachetype"
                Case "logs"
                    For Each lLog As XmlNode In lChild.ChildNodes
                        logs.Add(New clsGroundspeakLog(lLog))
                    Next
                    lChildName = Nothing
            End Select
            If lChildName IsNot Nothing AndAlso lChildName.Length > 0 Then
                SetSomething(Me, lChildName, lChild.InnerText)
            End If
        Next
    End Sub

    Public Overrides Function ToString() As String
        Return "<groundspeak:cache id =""" & id & """ archived = """ & archived & ">" & ControlChars.Lf _
             & "<name>" & name & "</name>" & ControlChars.Lf _
             & "<placed_by>" & placed_by & "</placed_by>" & ControlChars.Lf _
             & "<owner>" & owner & "</owner>" & ControlChars.Lf _
             & "<type>" & cachetype & "</type>" & ControlChars.Lf _
             & "<container>" & container & "</container>" & ControlChars.Lf _
             & "<difficulty>" & difficulty & "</difficulty>" & ControlChars.Lf _
             & "<terrain>" & terrain & "</terrain>" & ControlChars.Lf _
             & "<short_description>" & short_description & "</short_description>" & ControlChars.Lf _
             & "<long_description>" & long_description & "</long_description>" & ControlChars.Lf _
             & "<encoded_hints>" & encoded_hints & "</encoded_hints>" & ControlChars.Lf _
             & "<logs>" & logs.ToString & "</logs>" & ControlChars.Lf _
             & "<travelbugs>" & travelbugs & "</travelbugs>" & ControlChars.Lf
    End Function
End Class

Public Class clsGPXwaypoint
    Inherits clsGPXbase

    Private tagField As String 'wpt or trkpt
    Private latField As Double
    Private lonField As Double
    Private eleField As Double
    Private eleFieldSpecified As Boolean
    Private timeField As Date
    Private timeFieldSpecified As Boolean
    Private magvarField As Double
    Private magvarFieldSpecified As Boolean
    Private geoidheightField As Double
    Private geoidheightFieldSpecified As Boolean
    Private nameField As String
    Private cmtField As String
    Private descField As String
    Private srcField As String
    Private symField As String
    Private typeField As String
    Private fixField As String
    Private fixFieldSpecified As Boolean
    Private satField As String
    Private hdopField As Double
    Private hdopFieldSpecified As Boolean
    Private vdopField As Double
    Private vdopFieldSpecified As Boolean
    Private pdopField As Double
    Private pdopFieldSpecified As Boolean
    Private ageofdgpsdataField As Double
    Private ageofdgpsdataFieldSpecified As Boolean
    Private dgpsidField As String

    Private urlField As String
    Private urlnameField As String
    Private cacheField As clsGroundspeakCache
    Private speedField As Double
    Private speedFieldSpecified As Boolean
    Private courseField As Double
    Private courseFieldSpecified As Boolean

    Public Sub New(ByVal aTag As String, ByVal aLatitude As Double, ByVal aLongitude As Double)
        tagField = aTag
        latField = aLatitude
        lonField = aLongitude
    End Sub

    Public Sub New(ByVal aXML As XmlNode)
        tagField = aXML.Name
        For Each lAttribute As XmlAttribute In aXML.Attributes
            SetSomething(Me, lAttribute.Name, lAttribute.InnerText)
        Next

        For Each lChild As XmlNode In aXML.ChildNodes
            Select Case lChild.Name
                Case "extensions"
                    For Each lExtension As Xml.XmlElement In lChild.ChildNodes
                        Select Case lExtension.Name.ToLower
                            Case "speed" : speed = lExtension.InnerText
                            Case "course" : course = lExtension.InnerText
                            Case Else : SetExtension(lExtension.Name, lExtension.InnerXml)
                        End Select
                    Next
                Case "link"
                    AddLink(lChild)
                Case "groundspeak:cache"
                    cacheField = New clsGroundspeakCache(lChild)
                Case Else
                    SetSomething(Me, lChild.Name, lChild.InnerText)
            End Select
        Next

        If tagField = "waypoint" Then ' .loc format
            sym = typeField
            If typeField = "Geocache" Then typeField = "Geocache|Traditional Cache"
            For Each lChild As XmlNode In aXML.ChildNodes
                Select Case lChild.Name
                    Case "name"
                        name = lChild.Attributes("id").InnerText
                        urlname = lChild.ChildNodes(0).InnerText
                    Case "coord"
                        lat = lChild.Attributes("lat").InnerText
                        lon = lChild.Attributes("lon").InnerText
                    Case "link"
                        url = lChild.InnerText
                End Select
            Next
        End If
    End Sub

    Public Overrides Function ToString() As String
        Dim lXML As String = "<" & tagField _
            & " lat=""" & latField.ToString("#.########") & """" _
            & " lon=""" & lonField.ToString("#.########") & """>" & ControlChars.Lf
        If eleFieldSpecified Then lXML &= "<ele>" & Format(eleField, "0.###") & "</ele>" & ControlChars.Lf
        If timeFieldSpecified Then lXML &= "<time>" & timeField.ToString("yyyy-MM-ddTHH:mm:ss.fff") & "Z</time>" & ControlChars.Lf
        If nameField IsNot Nothing AndAlso nameField.Length > 0 Then lXML &= "<name>" & nameField & "</name>" & ControlChars.Lf
        If cmtField IsNot Nothing AndAlso cmtField.Length > 0 Then lXML &= "<cmt>" & cmtField & "</cmt>" & ControlChars.Lf
        If descField IsNot Nothing AndAlso descField.Length > 0 Then lXML &= "<desc>" & descField & "</desc>" & ControlChars.Lf
        If srcField IsNot Nothing AndAlso srcField.Length > 0 Then lXML &= "<src>" & srcField & "</src>" & ControlChars.Lf
        If symField IsNot Nothing AndAlso symField.Length > 0 Then lXML &= "<sym>" & symField & "</sym>" & ControlChars.Lf
        If typeField IsNot Nothing AndAlso typeField.Length > 0 Then lXML &= "<type>" & typeField & "</type>" & ControlChars.Lf
        If fixFieldSpecified Then lXML &= "<fix>" & fixField & "</fix>" & ControlChars.Lf
        If satField IsNot Nothing AndAlso satField.Length > 0 Then lXML &= "<sat>" & satField & "</sat>" & ControlChars.Lf
        If hdopFieldSpecified Then lXML &= "<hdop>" & hdopField & "</hdop>" & ControlChars.Lf
        If vdopFieldSpecified Then lXML &= "<vdop>" & vdopField & "</vdop>" & ControlChars.Lf
        If pdopFieldSpecified Then lXML &= "<pdop>" & pdopField & "</pdop>" & ControlChars.Lf
        If ageofdgpsdataFieldSpecified Then lXML &= "<ageofdgpsdata>" & ageofdgpsdataField & "</ageofdgpsdata>" & ControlChars.Lf
        If dgpsidField IsNot Nothing AndAlso dgpsidField.Length > 0 Then lXML &= "<dgpsid>" & dgpsidField & "</dgpsid>" & ControlChars.Lf
        'If speedFieldSpecified OrElse courseFieldSpecified OrElse extensionsField IsNot Nothing Then
        '    lXML &= "<extensions>" & ControlChars.Lf
        '    If speedFieldSpecified Then lXML &= "<speed>" & Format(speedField, "0.###") & "</speed>" & ControlChars.Lf
        '    If courseFieldSpecified Then lXML &= "<course>" & Format(courseField, "0.###") & "</course>" & ControlChars.Lf
        lXML &= extensionsString()
        lXML &= linkString()
        'lXML &= "</extensions>" & ControlChars.Lf
        'End If
        If urlField IsNot Nothing AndAlso urlField.Length > 0 Then lXML &= "<url>" & urlField & "</url>" & ControlChars.Lf
        If urlnameField IsNot Nothing AndAlso urlnameField.Length > 0 Then lXML &= "<urlname>" & urlnameField & "</urlname>" & ControlChars.Lf
        If cacheField IsNot Nothing Then lXML &= cacheField.ToString
        Return lXML & "</" & tagField & ">" & ControlChars.Lf
    End Function

    Public Property tag() As String
        Get
            Return Me.tagField
        End Get
        Set(ByVal value As String)
            Me.tagField = value
        End Set
    End Property

    Public Property ele() As Double
        Get
            Return Me.eleField
        End Get
        Set(ByVal value As Double)
            Me.eleField = value
            eleSpecified = True
        End Set
    End Property

    <System.Xml.Serialization.XmlIgnoreAttribute()> _
    Public Property eleSpecified() As Boolean
        Get
            Return Me.eleFieldSpecified
        End Get
        Set(ByVal value As Boolean)
            Me.eleFieldSpecified = value
        End Set
    End Property

    Public Property time() As Date
        Get
            Return Me.timeField
        End Get
        Set(ByVal value As Date)
            Me.timeField = value
            timeSpecified = True
        End Set
    End Property

    <System.Xml.Serialization.XmlIgnoreAttribute()> _
    Public Property timeSpecified() As Boolean
        Get
            Return Me.timeFieldSpecified
        End Get
        Set(ByVal value As Boolean)
            Me.timeFieldSpecified = value
        End Set
    End Property

    Public Property magvar() As Double
        Get
            Return Me.magvarField
        End Get
        Set(ByVal value As Double)
            Me.magvarField = value
            magvarSpecified = True
        End Set
    End Property

    <System.Xml.Serialization.XmlIgnoreAttribute()> _
    Public Property magvarSpecified() As Boolean
        Get
            Return Me.magvarFieldSpecified
        End Get
        Set(ByVal value As Boolean)
            Me.magvarFieldSpecified = value
        End Set
    End Property

    Public Property geoidheight() As Double
        Get
            Return Me.geoidheightField
        End Get
        Set(ByVal value As Double)
            Me.geoidheightField = value
            geoidheightSpecified = True
        End Set
    End Property

    <System.Xml.Serialization.XmlIgnoreAttribute()> _
    Public Property geoidheightSpecified() As Boolean
        Get
            Return Me.geoidheightFieldSpecified
        End Get
        Set(ByVal value As Boolean)
            Me.geoidheightFieldSpecified = value
        End Set
    End Property

    Public Property name() As String
        Get
            Return Me.nameField
        End Get
        Set(ByVal value As String)
            Me.nameField = value
        End Set
    End Property

    Public Property cmt() As String
        Get
            Return Me.cmtField
        End Get
        Set(ByVal value As String)
            Me.cmtField = value
        End Set
    End Property

    Public Property desc() As String
        Get
            Return Me.descField
        End Get
        Set(ByVal value As String)
            Me.descField = value
        End Set
    End Property

    Public Property src() As String
        Get
            Return Me.srcField
        End Get
        Set(ByVal value As String)
            Me.srcField = value
        End Set
    End Property

    Public Property sym() As String
        Get
            Return Me.symField
        End Get
        Set(ByVal value As String)
            Me.symField = value
        End Set
    End Property

    Public Property type() As String
        Get
            Return Me.typeField
        End Get
        Set(ByVal value As String)
            Me.typeField = value
        End Set
    End Property

    Public Property fix() As String
        Get
            Return Me.fixField
        End Get
        Set(ByVal value As String)
            Me.fixField = value
            fixSpecified = True
        End Set
    End Property

    <System.Xml.Serialization.XmlIgnoreAttribute()> _
    Public Property fixSpecified() As Boolean
        Get
            Return Me.fixFieldSpecified
        End Get
        Set(ByVal value As Boolean)
            Me.fixFieldSpecified = value
        End Set
    End Property

    Public Property sat() As String
        Get
            Return Me.satField
        End Get
        Set(ByVal value As String)
            Me.satField = value
        End Set
    End Property

    Public Property hdop() As Double
        Get
            Return Me.hdopField
        End Get
        Set(ByVal value As Double)
            Me.hdopField = value
            hdopSpecified = True
        End Set
    End Property

    <System.Xml.Serialization.XmlIgnoreAttribute()> _
    Public Property hdopSpecified() As Boolean
        Get
            Return Me.hdopFieldSpecified
        End Get
        Set(ByVal value As Boolean)
            Me.hdopFieldSpecified = value
        End Set
    End Property

    Public Property vdop() As Double
        Get
            Return Me.vdopField
        End Get
        Set(ByVal value As Double)
            Me.vdopField = value
            vdopSpecified = True
        End Set
    End Property

    <System.Xml.Serialization.XmlIgnoreAttribute()> _
    Public Property vdopSpecified() As Boolean
        Get
            Return Me.vdopFieldSpecified
        End Get
        Set(ByVal value As Boolean)
            Me.vdopFieldSpecified = value
        End Set
    End Property

    Public Property pdop() As Double
        Get
            Return Me.pdopField
        End Get
        Set(ByVal value As Double)
            Me.pdopField = value
            pdopSpecified = True
        End Set
    End Property

    <System.Xml.Serialization.XmlIgnoreAttribute()> _
    Public Property pdopSpecified() As Boolean
        Get
            Return Me.pdopFieldSpecified
        End Get
        Set(ByVal value As Boolean)
            Me.pdopFieldSpecified = value
        End Set
    End Property

    Public Property ageofdgpsdata() As Double
        Get
            Return Me.ageofdgpsdataField
        End Get
        Set(ByVal value As Double)
            Me.ageofdgpsdataField = value
            ageofdgpsdataSpecified = True
        End Set
    End Property

    <System.Xml.Serialization.XmlIgnoreAttribute()> _
    Public Property ageofdgpsdataSpecified() As Boolean
        Get
            Return Me.ageofdgpsdataFieldSpecified
        End Get
        Set(ByVal value As Boolean)
            Me.ageofdgpsdataFieldSpecified = value
        End Set
    End Property

    <System.Xml.Serialization.XmlElementAttribute(DataType:="integer")> _
    Public Property dgpsid() As String
        Get
            Return Me.dgpsidField
        End Get
        Set(ByVal value As String)
            Me.dgpsidField = value
        End Set
    End Property

    'Public Property extensions() As XmlElement
    '    Get
    '        Return Me.extensionsField
    '    End Get
    '    Set(ByVal value As XmlElement)
    '        Me.extensionsField = value
    '    End Set
    'End Property

    <System.Xml.Serialization.XmlAttributeAttribute()> _
    Public Property lat() As Double
        Get
            Return Me.latField
        End Get
        Set(ByVal value As Double)
            Me.latField = value
        End Set
    End Property

    <System.Xml.Serialization.XmlAttributeAttribute()> _
    Public Property lon() As Double
        Get
            Return Me.lonField
        End Get
        Set(ByVal value As Double)
            Me.lonField = value
        End Set
    End Property

    Public Property url() As String
        Get
            Return Me.urlField
        End Get
        Set(ByVal value As String)
            Me.urlField = value
        End Set
    End Property

    Public Property urlname() As String
        Get
            Return Me.urlnameField
        End Get
        Set(ByVal value As String)
            Me.urlnameField = value
        End Set
    End Property

    Public Property cache() As clsGroundspeakCache
        Get
            Return Me.cacheField
        End Get
        Set(ByVal value As clsGroundspeakCache)
            Me.cacheField = value
        End Set
    End Property

    Public Property speed() As Double
        Get
            Return Me.speedField
        End Get
        Set(ByVal value As Double)
            Me.speedField = value
            speedSpecified = True
            SetExtension("speed", value)
        End Set
    End Property

    <System.Xml.Serialization.XmlIgnoreAttribute()> _
    Public Property speedSpecified() As Boolean
        Get
            Return Me.speedFieldSpecified
        End Get
        Set(ByVal value As Boolean)
            Me.speedFieldSpecified = value
        End Set
    End Property

    Public Property course() As Double
        Get
            Return Me.courseField
        End Get
        Set(ByVal value As Double)
            Me.courseField = value
            courseSpecified = True
            SetExtension("course", value)
        End Set
    End Property

    <System.Xml.Serialization.XmlIgnoreAttribute()> _
    Public Property courseSpecified() As Boolean
        Get
            Return Me.courseFieldSpecified
        End Get
        Set(ByVal value As Boolean)
            Me.courseFieldSpecified = value
        End Set
    End Property
End Class


Public Class clsGPXlink

    Private textField As String
    Private typeField As String
    Private hrefField As String

    Public Sub New(ByVal aXML As XmlNode)
        For Each lChild As XmlNode In aXML.ChildNodes
            Try
                Dim lPropertyInfo As System.Reflection.PropertyInfo = Me.GetType.GetProperty(lChild.Name)
                If lPropertyInfo IsNot Nothing Then
                    lPropertyInfo.SetValue(Me, lChild.InnerText, Nothing)
                Else
                    'Logger.Dbg("Skipped unknown node type: " & lChild.Name)
                End If
            Catch e As Exception
                'Logger.Dbg("Unable to set " & lChild.Name & " = " & lChild.InnerXml)
            End Try
        Next
    End Sub

    Public Property text() As String
        Get
            Return Me.textField
        End Get
        Set(ByVal value As String)
            Me.textField = value
        End Set
    End Property

    Public Property type() As String
        Get
            Return Me.typeField
        End Get
        Set(ByVal value As String)
            Me.typeField = value
        End Set
    End Property

    <System.Xml.Serialization.XmlAttributeAttribute(DataType:="anyURI")> _
    Public Property href() As String
        Get
            Return Me.hrefField
        End Get
        Set(ByVal value As String)
            Me.hrefField = value
        End Set
    End Property

    Public Overrides Function ToString() As String
        Dim lString As String = "<link href=""" & hrefField & ">"
        If textField IsNot Nothing AndAlso textField.Length > 0 Then
            lString &= "<text>" & textField & "</text>"
        End If
        If textField IsNot Nothing AndAlso textField.Length > 0 Then
            lString &= "<type>" & typeField & "</type>"
        End If
        Return lString & "</link>"
    End Function
End Class

Partial Public Class clsGPXtrack
    Inherits clsGPXbase

    Private nameField As String
    Private cmtField As String
    Private descField As String
    Private srcField As String
    Private numberField As String
    Private typeField As String
    Private trksegField As Generic.List(Of clsGPXtracksegment)

    Public Sub New(ByVal aName As String)
        nameField = aName
        trksegField = New Generic.List(Of clsGPXtracksegment)
    End Sub

    Public Sub New(ByVal aXML As XmlNode)
        trksegField = New Generic.List(Of clsGPXtracksegment)
        For Each lChild As XmlNode In aXML.ChildNodes
            Try
                Select Case lChild.Name
                    Case "extensions"
                        SetExtensions(lChild)
                    Case "trkseg"
                        trksegField.Add(New clsGPXtracksegment(lChild))
                    Case "link"
                        AddLink(lChild)
                    Case Else
                        Dim lPropertyInfo As System.Reflection.PropertyInfo = Me.GetType.GetProperty(lChild.Name)
                        If lPropertyInfo IsNot Nothing Then
                            lPropertyInfo.SetValue(Me, lChild.InnerText, Nothing)
                        Else
                            'Logger.Dbg("Skipped unknown node type: " & lChild.Name)
                        End If
                End Select
            Catch e As Exception
                'Logger.Dbg("Unable to set " & lChild.Name & " = " & lChild.InnerXml)
            End Try
        Next
    End Sub

    Public Property name() As String
        Get
            Return Me.nameField
        End Get
        Set(ByVal value As String)
            Me.nameField = value
        End Set
    End Property

    Public Property cmt() As String
        Get
            Return Me.cmtField
        End Get
        Set(ByVal value As String)
            Me.cmtField = value
        End Set
    End Property

    Public Property desc() As String
        Get
            Return Me.descField
        End Get
        Set(ByVal value As String)
            Me.descField = value
        End Set
    End Property

    Public Property src() As String
        Get
            Return Me.srcField
        End Get
        Set(ByVal value As String)
            Me.srcField = value
        End Set
    End Property

    Public Property number() As String
        Get
            Return Me.numberField
        End Get
        Set(ByVal value As String)
            Me.numberField = value
        End Set
    End Property

    Public Property type() As String
        Get
            Return Me.typeField
        End Get
        Set(ByVal value As String)
            Me.typeField = value
        End Set
    End Property

    Public Property trkseg() As Generic.List(Of clsGPXtracksegment)
        Get
            Return Me.trksegField
        End Get
        Set(ByVal value As Generic.List(Of clsGPXtracksegment))
            Me.trksegField = value
        End Set
    End Property

End Class

Partial Public Class clsGPXroute
    Inherits clsGPXbase

    Private nameField As String
    Private cmtField As String
    Private descField As String
    Private srcField As String
    Private numberField As String
    Private typeField As String
    Private rteptField As Generic.List(Of clsGPXwaypoint)

    Public Sub New(ByVal aXML As XmlNode)
        rteptField = New Generic.List(Of clsGPXwaypoint)
        For Each lChild As XmlNode In aXML.ChildNodes
            Try
                Select Case lChild.Name
                    Case "extensions"
                        SetExtensions(lChild)
                    Case "rteseg"
                        rteptField.Add(New clsGPXwaypoint(lChild))
                    Case "link"
                        AddLink(lChild)
                    Case Else
                        Dim lPropertyInfo As System.Reflection.PropertyInfo = Me.GetType.GetProperty(lChild.Name)
                        If lPropertyInfo IsNot Nothing Then
                            lPropertyInfo.SetValue(Me, lChild.InnerText, Nothing)
                        Else
                            'Logger.Dbg("Skipped unknown node type: " & lChild.Name)
                        End If
                End Select
            Catch e As Exception
                'Logger.Dbg("Unable to set " & lChild.Name & " = " & lChild.InnerXml)
            End Try
        Next
    End Sub

    Public Property name() As String
        Get
            Return Me.nameField
        End Get
        Set(ByVal value As String)
            Me.nameField = value
        End Set
    End Property

    Public Property cmt() As String
        Get
            Return Me.cmtField
        End Get
        Set(ByVal value As String)
            Me.cmtField = value
        End Set
    End Property

    Public Property desc() As String
        Get
            Return Me.descField
        End Get
        Set(ByVal value As String)
            Me.descField = value
        End Set
    End Property

    Public Property src() As String
        Get
            Return Me.srcField
        End Get
        Set(ByVal value As String)
            Me.srcField = value
        End Set
    End Property

    Public Property number() As String
        Get
            Return Me.numberField
        End Get
        Set(ByVal value As String)
            Me.numberField = value
        End Set
    End Property

    Public Property type() As String
        Get
            Return Me.typeField
        End Get
        Set(ByVal value As String)
            Me.typeField = value
        End Set
    End Property

    Public Property rtept() As Generic.List(Of clsGPXwaypoint)
        Get
            Return Me.rteptField
        End Get
        Set(ByVal value As Generic.List(Of clsGPXwaypoint))
            Me.rteptField = value
        End Set
    End Property

End Class

Partial Public Class clsGPXbounds

    Private minlatField As Double = 90
    Private minlonField As Double = 180
    Private maxlatField As Double = -90
    Private maxlonField As Double = -180

    Public Sub New()
    End Sub

    Public Sub New(ByVal aXML As XmlNode)
        For Each lAttribute As XmlAttribute In aXML.Attributes
            SetSomething(Me, lAttribute.Name, lAttribute.InnerXml)
        Next
    End Sub

    Public Property minlat() As Double
        Get
            Return Me.minlatField
        End Get
        Set(ByVal value As Double)
            Me.minlatField = value
        End Set
    End Property

    Public Property minlon() As Double
        Get
            Return Me.minlonField
        End Get
        Set(ByVal value As Double)
            Me.minlonField = value
        End Set
    End Property

    Public Property maxlat() As Double
        Get
            Return Me.maxlatField
        End Get
        Set(ByVal value As Double)
            Me.maxlatField = value
        End Set
    End Property

    Public Property maxlon() As Double
        Get
            Return Me.maxlonField
        End Get
        Set(ByVal value As Double)
            Me.maxlonField = value
        End Set
    End Property
End Class

Partial Public Class clsGPXcopyright

    Private yearField As String
    Private licenseField As String
    Private authorField As String

    <System.Xml.Serialization.XmlElementAttribute(DataType:="gYear")> _
    Public Property year() As String
        Get
            Return Me.yearField
        End Get
        Set(ByVal value As String)
            Me.yearField = value
        End Set
    End Property

    <System.Xml.Serialization.XmlElementAttribute(DataType:="anyURI")> _
    Public Property license() As String
        Get
            Return Me.licenseField
        End Get
        Set(ByVal value As String)
            Me.licenseField = value
        End Set
    End Property

    Public Property author() As String
        Get
            Return Me.authorField
        End Get
        Set(ByVal value As String)
            Me.authorField = value
        End Set
    End Property
End Class

Public Class clsGPXbase
    Private extensions As Generic.List(Of String)
    Private linkField As Generic.List(Of clsGPXlink)

    Public Sub New()
    End Sub

    Public Overridable Sub Clear()
        extensions = Nothing
        linkField = Nothing
    End Sub

    Public Sub SetExtensions(ByVal aExtensionsNode As XmlElement)
        For Each lExtension As Xml.XmlElement In aExtensionsNode.ChildNodes
            SetExtension(lExtension.Name, lExtension.InnerXml)
        Next
    End Sub

    Public Sub SetExtension(ByVal aTag As String, ByVal aValue As String)
        Dim lOpenTag As String = "<" & aTag & ">"
        If extensions Is Nothing Then
            extensions = New Generic.List(Of String)
        Else
            For lIndex As Integer = 0 To extensions.Count - 1
                If extensions(lIndex).StartsWith(lOpenTag) Then
                    extensions.RemoveAt(lIndex)
                    Exit For
                End If
            Next
        End If
        extensions.Add(lOpenTag & aValue & "</" & aTag & ">")
    End Sub

    Public Function extensionsString() As String
        Dim lString As String = ""
        If extensions IsNot Nothing AndAlso extensions.Count > 0 Then
            lString = "<extensions>" & ControlChars.Lf
            For Each lExtension As String In extensions
                lString &= lExtension & ControlChars.Lf
            Next
            lString &= "</extensions>" & ControlChars.Lf
        End If
        Return lString
    End Function

    <System.Xml.Serialization.XmlElementAttribute("link")> _
    Public Property link() As Generic.List(Of clsGPXlink)
        Get
            Return Me.linkField
        End Get
        Set(ByVal value As Generic.List(Of clsGPXlink))
            Me.linkField = value
        End Set
    End Property

    Protected Sub AddLink(ByVal aLink As XmlElement)
        If linkField Is Nothing Then linkField = New Generic.List(Of clsGPXlink)
        linkField.Add(New clsGPXlink(aLink))
    End Sub

    Public Function linkString() As String
        Dim lString As String = ""
        If link IsNot Nothing AndAlso link.Count > 0 Then
            For Each lLink As clsGPXlink In link
                lString &= lLink.ToString & ControlChars.Lf
            Next
        End If
        Return lString
    End Function

End Class