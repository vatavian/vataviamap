
Imports System
Imports System.Text
Imports System.Drawing
Imports System.IO
''' <summary>
''' Utility class for working with EXIF data in images. Provides abstraction
''' for most common data and generic utilities for work with all other. 
''' </summary>
''' <remarks>
''' Copyright (c) Michal A. Valášek - Altair Communications, 2003-2005
''' Copmany: http://software.altaircom.net, E-mail: support@altaircom.net
''' Private: http://www.rider.cz, E-mail: rider@rider.cz
''' This is free software licensed under GNU Lesser General Public License
''' </remarks>
''' <history>
''' [altair] 10.09.2003 Created
''' [altair] 12.06.2004 Added capability to write EXIF data
''' [altair] 11.07.2004 Added option to change encoding
''' [altair] 04.09.2005 Changed source of Width and Height properties from EXIF to image
''' [altair] 05.09.2005 Code clean-up and minor changes
''' [marco.ridoni@virgilio.it] 02-11-2006 C# translation
''' [mark-vataviamap@vatavia.net] 2009-11-25: found code at http://www.codeproject.com/KB/vb/exif_reader.aspx
'''                                           converted back to VB.Net
'''                                           changed to use System.BitConverter as suggested by Fabian Tuender, www.fabita.nl
'''                                           started to convert comments to standard format
''' </history>
Public Class ExifWorks
    Implements IDisposable
    Private _Stream As FileStream
    Private _Image As Bitmap
    Private _Encoding As Encoding = Encoding.UTF8

#Region "Type declarations"

    ''' <summary>
    ''' Contains possible values of EXIF tag names (ID)
    ''' </summary>
    ''' <remarks>See GdiPlusImaging.h</remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public Enum TagNames As Integer
        ExifIFD = &H8769
        GpsIFD = &H8825
        NewSubfileType = &HFE
        SubfileType = &HFF
        ImageWidth = &H100
        ImageHeight = &H101
        BitsPerSample = &H102
        Compression = &H103
        PhotometricInterp = &H106
        ThreshHolding = &H107
        CellWidth = &H108
        CellHeight = &H109
        FillOrder = &H10A
        DocumentName = &H10D
        ImageDescription = &H10E
        EquipMake = &H10F
        EquipModel = &H110
        StripOffsets = &H111
        Orientation = &H112
        SamplesPerPixel = &H115
        RowsPerStrip = &H116
        StripBytesCount = &H117
        MinSampleValue = &H118
        MaxSampleValue = &H119
        XResolution = &H11A
        YResolution = &H11B
        PlanarConfig = &H11C
        PageName = &H11D
        XPosition = &H11E
        YPosition = &H11F
        FreeOffset = &H120
        FreeByteCounts = &H121
        GrayResponseUnit = &H122
        GrayResponseCurve = &H123
        T4Option = &H124
        T6Option = &H125
        ResolutionUnit = &H128
        PageNumber = &H129
        TransferFuncition = &H12D
        SoftwareUsed = &H131
        DateTime = &H132
        Artist = &H13B
        HostComputer = &H13C
        Predictor = &H13D
        WhitePoint = &H13E
        PrimaryChromaticities = &H13F
        ColorMap = &H140
        HalftoneHints = &H141
        TileWidth = &H142
        TileLength = &H143
        TileOffset = &H144
        TileByteCounts = &H145
        InkSet = &H14C
        InkNames = &H14D
        NumberOfInks = &H14E
        DotRange = &H150
        TargetPrinter = &H151
        ExtraSamples = &H152
        SampleFormat = &H153
        SMinSampleValue = &H154
        SMaxSampleValue = &H155
        TransferRange = &H156
        JPEGProc = &H200
        JPEGInterFormat = &H201
        JPEGInterLength = &H202
        JPEGRestartInterval = &H203
        JPEGLosslessPredictors = &H205
        JPEGPointTransforms = &H206
        JPEGQTables = &H207
        JPEGDCTables = &H208
        JPEGACTables = &H209
        YCbCrCoefficients = &H211
        YCbCrSubsampling = &H212
        YCbCrPositioning = &H213
        REFBlackWhite = &H214
        ICCProfile = &H8773
        Gamma = &H301
        ICCProfileDescriptor = &H302
        SRGBRenderingIntent = &H303
        ImageTitle = &H320
        Copyright = &H8298
        ResolutionXUnit = &H5001
        ResolutionYUnit = &H5002
        ResolutionXLengthUnit = &H5003
        ResolutionYLengthUnit = &H5004
        PrintFlags = &H5005
        PrintFlagsVersion = &H5006
        PrintFlagsCrop = &H5007
        PrintFlagsBleedWidth = &H5008
        PrintFlagsBleedWidthScale = &H5009
        HalftoneLPI = &H500A
        HalftoneLPIUnit = &H500B
        HalftoneDegree = &H500C
        HalftoneShape = &H500D
        HalftoneMisc = &H500E
        HalftoneScreen = &H500F
        JPEGQuality = &H5010
        GridSize = &H5011
        ThumbnailFormat = &H5012
        ThumbnailWidth = &H5013
        ThumbnailHeight = &H5014
        ThumbnailColorDepth = &H5015
        ThumbnailPlanes = &H5016
        ThumbnailRawBytes = &H5017
        ThumbnailSize = &H5018
        ThumbnailCompressedSize = &H5019
        ColorTransferFunction = &H501A
        ThumbnailData = &H501B
        ThumbnailImageWidth = &H5020
        ThumbnailImageHeight = &H502
        ThumbnailBitsPerSample = &H5022
        ThumbnailCompression = &H5023
        ThumbnailPhotometricInterp = &H5024
        ThumbnailImageDescription = &H5025
        ThumbnailEquipMake = &H5026
        ThumbnailEquipModel = &H5027
        ThumbnailStripOffsets = &H5028
        ThumbnailOrientation = &H5029
        ThumbnailSamplesPerPixel = &H502A
        ThumbnailRowsPerStrip = &H502B
        ThumbnailStripBytesCount = &H502C
        ThumbnailResolutionX = &H502D
        ThumbnailResolutionY = &H502E
        ThumbnailPlanarConfig = &H502F
        ThumbnailResolutionUnit = &H5030
        ThumbnailTransferFunction = &H5031
        ThumbnailSoftwareUsed = &H5032
        ThumbnailDateTime = &H5033
        ThumbnailArtist = &H5034
        ThumbnailWhitePoint = &H5035
        ThumbnailPrimaryChromaticities = &H5036
        ThumbnailYCbCrCoefficients = &H5037
        ThumbnailYCbCrSubsampling = &H5038
        ThumbnailYCbCrPositioning = &H5039
        ThumbnailRefBlackWhite = &H503A
        ThumbnailCopyRight = &H503B
        LuminanceTable = &H5090
        ChrominanceTable = &H5091
        FrameDelay = &H5100
        LoopCount = &H5101
        PixelUnit = &H5110
        PixelPerUnitX = &H5111
        PixelPerUnitY = &H5112
        PaletteHistogram = &H5113
        ExifExposureTime = &H829A
        ExifFNumber = &H829D
        ExifExposureProg = &H8822
        ExifSpectralSense = &H8824
        ExifISOSpeed = &H8827
        ExifOECF = &H8828
        ExifVer = &H9000
        ExifDTOrig = &H9003
        ExifDTDigitized = &H9004
        ExifCompConfig = &H9101
        ExifCompBPP = &H9102
        ExifShutterSpeed = &H9201
        ExifAperture = &H9202
        ExifBrightness = &H9203
        ExifExposureBias = &H9204
        ExifMaxAperture = &H9205
        ExifSubjectDist = &H9206
        ExifMeteringMode = &H9207
        ExifLightSource = &H9208
        ExifFlash = &H9209
        ExifFocalLength = &H920A
        ExifMakerNote = &H927C
        ExifUserComment = &H9286
        ExifDTSubsec = &H9290
        ExifDTOrigSS = &H9291
        ExifDTDigSS = &H9292
        ExifFPXVer = &HA000
        ExifColorSpace = &HA001
        ExifPixXDim = &HA002
        ExifPixYDim = &HA003
        ExifRelatedWav = &HA004
        ExifInterop = &HA005
        ExifFlashEnergy = &HA20B
        ExifSpatialFR = &HA20C
        ExifFocalXRes = &HA20E
        ExifFocalYRes = &HA20F
        ExifFocalResUnit = &HA210
        ExifSubjectLoc = &HA214
        ExifExposureIndex = &HA215
        ExifSensingMethod = &HA217
        ExifFileSource = &HA300
        ExifSceneType = &HA301
        ExifCfaPattern = &HA302
        GpsVer = &H0
        GpsLatitudeRef = &H1
        GpsLatitude = &H2
        GpsLongitudeRef = &H3
        GpsLongitude = &H4
        GpsAltitudeRef = &H5
        GpsAltitude = &H6
        GpsGpsTime = &H7
        GpsGpsSatellites = &H8
        GpsGpsStatus = &H9
        GpsGpsMeasureMode = &HA
        GpsGpsDop = &HB
        GpsSpeedRef = &HC
        GpsSpeed = &HD
        GpsTrackRef = &HE
        GpsTrack = &HF
        GpsImgDirRef = &H10
        GpsImgDir = &H11
        GpsMapDatum = &H12
        GpsDestLatRef = &H13
        GpsDestLat = &H14
        GpsDestLongRef = &H15
        GpsDestLong = &H16
        GpsDestBearRef = &H17
        GpsDestBear = &H18
        GpsDestDistRef = &H19
        GpsDestDist = &H1A
    End Enum

    ''' <summary>
    ''' Real position of 0th row and column of picture
    ''' </summary>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public Enum Orientations
        TopLeft = 1
        TopRight = 2
        BottomRight = 3
        BottomLeft = 4
        LeftTop = 5
        RightTop = 6
        RightBottom = 7
        LftBottom = 8
    End Enum

    ''' <summary>
    ''' Exposure programs
    ''' </summary>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public Enum ExposurePrograms
        Manual = 1
        Normal = 2
        AperturePriority = 3
        ShutterPriority = 4
        Creative = 5
        Action = 6
        Portrait = 7
        Landscape = 8
    End Enum

    ''' <summary>
    ''' Exposure metering modes
    ''' </summary>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public Enum ExposureMeteringModes
        Unknown = 0
        Average = 1
        CenterWeightedAverage = 2
        Spot = 3
        MultiSpot = 4
        MultiSegment = 5
        [Partial] = 6
        Other = 255
    End Enum

    ''' <summary>
    ''' Flash activity modes
    ''' </summary>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public Enum FlashModes
        NotFired = 0
        Fired = 1
        FiredButNoStrobeReturned = 5
        FiredAndStrobeReturned = 7
    End Enum

    ''' <summary>
    ''' Possible light sources (white balance)
    ''' </summary>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public Enum LightSources
        Unknown = 0
        Daylight = 1
        Fluorescent = 2
        Tungsten = 3
        Flash = 10
        StandardLightA = 17
        StandardLightB = 18
        StandardLightC = 19
        D55 = 20
        D65 = 21
        D75 = 22
        Other = 255
    End Enum

    ''' <summary>
    ''' EXIF data types
    ''' </summary>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 12.6.2004 Created
    ''' </history>
    Public Enum ExifDataTypes As Short
        UnsignedByte = 1
        AsciiString = 2
        UnsignedShort = 3
        UnsignedLong = 4
        UnsignedRational = 5
        SignedByte = 6
        Undefined = 7
        SignedShort = 8
        SignedLong = 9
        SignedRational = 10
        SingleFloat = 11
        DoubleFloat = 12
    End Enum

    ''' <summary>
    ''' Represents rational which is type of some Exif properties
    ''' </summary>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public Structure Rational
        Public Numerator As Int32
        Public Denominator As Int32

        ''' <summary>
        ''' Converts rational to string representation
        ''' </summary>
        ''' <param name="Delimiter">Optional, default "/". String to be used as delimiter of components.</param>
        ''' <returns>String representation of the rational.</returns>
        ''' <remarks></remarks>
        ''' <history>
        ''' [altair] 10.09.2003 Created
        ''' </history>

        Shadows Function ToString(Optional ByVal Delimiter As String = "/") As String
            Return Numerator & Delimiter & Denominator
        End Function


        ''' <summary>
        ''' Converts rational to double precision real number
        ''' </summary>
        ''' <returns>The rational as double precision real number.</returns>
        ''' <remarks></remarks>
        ''' <history>
        ''' [altair] 10.09.2003 Created
        ''' </history>

        Public Function ToDouble() As Double
            Return CDbl(Numerator) / Denominator
        End Function
    End Structure

#End Region

    ''' <summary>
    ''' Initializes new instance of this class from a Bitmap
    ''' </summary>
    ''' <param name="SourceBitmap">Bitmap to read exif information from</param>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public Sub New(ByRef SourceBitmap As Bitmap)
        If SourceBitmap Is Nothing Then
            Throw New ArgumentNullException("Bitmap")
        End If
        _Image = SourceBitmap
    End Sub

    ''' <summary>
    ''' Initializes new instance of this class from a file name
    ''' </summary>
    ''' <param name="FileName">Name of file to be loaded</param>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 13.06.2004 Created
    ''' </history>
    Public Sub New(ByVal FileName As String)
        If FileName Is Nothing Then
            Throw New ArgumentNullException("FileName")
        End If
        _Stream = New FileStream(FileName, FileMode.Open, FileAccess.Read)
        'avoid loading here!

        ' _Image = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromFile(FileName);
        _Image = DirectCast(Image.FromStream(_Stream, True, False), Bitmap)
    End Sub

    ''' <summary>
    ''' Get or set encoding used for string metadata
    ''' </summary>
    ''' <value>Encoding used for string metadata</value>
    ''' <remarks>Default encoding is UTF-8</remarks>
    ''' <history>
    ''' [altair] 11.07.2004 Created
    ''' [altair] 05.09.2005 Changed from shared to instance member
    ''' </history>
    Public Property Encoding() As Encoding
        Get
            Return _Encoding
        End Get
        Set(ByVal value As Encoding)
            If value Is Nothing Then
                Throw New ArgumentNullException()
            End If
            _Encoding = value
        End Set
    End Property

    ''' <summary>
    ''' Returns copy of bitmap this instance is working on
    ''' </summary>
    ''' <history>
    ''' [altair] 13.06.2004 Created
    ''' </history>
    Public Function GetBitmap() As Bitmap
        Return DirectCast(_Image.Clone(), Bitmap)
    End Function

    ''' <summary>
    ''' Returns all available data in formatted string form
    ''' </summary>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public Overrides Function ToString() As String
        Dim SB As New StringBuilder()

        SB.Append("Image:")
        SB.Append((vbLf & vbTab & "Dimensions: " & Width & " x ") + Height & " px")
        SB.Append((vbLf & vbTab & "Resolution: " & ResolutionX & " x ") + ResolutionY & " dpi")
        SB.Append(vbLf & vbTab & "Orientation: " & [Enum].GetName(GetType(Orientations), Orientation))
        SB.Append(vbLf & vbTab & "Title: " & Title)
        SB.Append(vbLf & vbTab & "Description: " & Description)
        SB.Append(vbLf & vbTab & "Copyright: " & Copyright)
        SB.Append(vbLf & "Equipment:")
        SB.Append(vbLf & vbTab & "Maker: " & EquipmentMaker)
        SB.Append(vbLf & vbTab & "Model: " & EquipmentModel)
        SB.Append(vbLf & vbTab & "Software: " & Software)
        SB.Append(vbLf & "Date and time:")
        SB.Append(vbLf & vbTab & "General: " & DateTimeLastModified.ToString())
        SB.Append(vbLf & vbTab & "Original: " & DateTimeOriginal.ToString())
        SB.Append(vbLf & vbTab & "Digitized: " & DateTimeDigitized.ToString())
        SB.Append(vbLf & "Shooting conditions:")
        SB.Append(vbLf & vbTab & "Exposure time: " & ExposureTimeRational.ToString() & " s")
        SB.Append(vbLf & vbTab & "Exposure program: " & [Enum].GetName(GetType(ExposurePrograms), ExposureProgram))
        SB.Append(vbLf & vbTab & "Exposure mode: " & [Enum].GetName(GetType(ExposureMeteringModes), ExposureMeteringMode))
        SB.Append(vbLf & vbTab & "Aperture: F" & Aperture.ToString("N2"))
        SB.Append(vbLf & vbTab & "ISO sensitivity: " & ISO)
        SB.Append(vbLf & vbTab & "Subject distance: " & SubjectDistance.ToString("N2") & " m")
        SB.Append(vbLf & vbTab & "Focal length: " & FocalLength)
        SB.Append(vbLf & vbTab & "Flash: " & [Enum].GetName(GetType(FlashModes), FlashMode))
        SB.Append(vbLf & vbTab & "Light source (WB): " & [Enum].GetName(GetType(LightSources), LightSource))
        SB.Append(vbLf & "Location:")
        SB.Append(vbLf & vbTab & "Latitude: " & Latitude)
        SB.Append(vbLf & vbTab & "Longitude: " & Longitude)
        Return SB.ToString()
    End Function

#Region "Nicely formatted well-known properties"

    ''' <summary>
    ''' Brand of equipment (EXIF EquipMake)
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public ReadOnly Property EquipmentMaker() As String
        Get
            Return GetPropertyString(TagNames.EquipMake)
        End Get
    End Property

    ''' <summary>
    ''' Model of equipment (EXIF EquipModel)
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public ReadOnly Property EquipmentModel() As String
        Get
            Return GetPropertyString(TagNames.EquipModel)
        End Get
    End Property

    ''' <summary>
    ''' Software used for processing (EXIF Software)
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public ReadOnly Property Software() As String
        Get
            Return GetPropertyString(TagNames.SoftwareUsed)
        End Get
    End Property

    ''' <summary>
    ''' Orientation of image (position of row 0, column 0) (EXIF Orientation)
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public ReadOnly Property Orientation() As Orientations
        Get
            Dim X As Int32 = GetPropertyInt16(TagNames.Orientation)

            If Not [Enum].IsDefined(GetType(Orientations), X) Then
                Return Orientations.TopLeft
            Else
                Return CType([Enum].Parse(GetType(Orientations), [Enum].GetName(GetType(Orientations), X)), Orientations)
            End If
        End Get
    End Property

    ''' <summary>
    ''' Time when image was last modified (EXIF DateTime).
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public Property DateTimeLastModified() As DateTime
        Get
            Try
                Return DateTime.ParseExact(GetPropertyString(TagNames.DateTime), "yyyy\:MM\:dd HH\:mm\:ss", Nothing)
            Catch
                Return DateTime.MinValue
            End Try
        End Get
        Set(ByVal value As DateTime)
            Try
                SetPropertyString(TagNames.DateTime, value.ToString("yyyy\:MM\:dd HH\:mm\:ss"))
            Catch
            End Try
        End Set
    End Property

    ''' <summary>
    ''' Time when image was taken (EXIF DateTimeOriginal).
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public Property DateTimeOriginal() As DateTime
        Get
            Try
                Return DateTime.ParseExact(GetPropertyString(TagNames.ExifDTOrig), "yyyy\:MM\:dd HH\:mm\:ss", Nothing)
            Catch
                Return DateTime.MinValue
            End Try
        End Get
        Set(ByVal value As DateTime)
            Try
                SetPropertyString(TagNames.ExifDTOrig, value.ToString("yyyy\:MM\:dd HH\:mm\:ss"))
            Catch
            End Try
        End Set
    End Property

    ''' <summary>
    ''' Time when image was digitized (EXIF DateTimeDigitized).
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public Property DateTimeDigitized() As DateTime
        Get
            Try
                Return DateTime.ParseExact(GetPropertyString(TagNames.ExifDTDigitized), "yyyy\:MM\:dd HH\:mm\:ss", Nothing)
            Catch
                Return DateTime.MinValue
            End Try
        End Get
        Set(ByVal value As DateTime)
            Try
                SetPropertyString(TagNames.ExifDTDigitized, value.ToString("yyyy\:MM\:dd HH\:mm\:ss"))
            Catch
            End Try
        End Set
    End Property

    ''' <summary>
    ''' Image width
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' [altair] 04.09.2005 Changed output to Int32, load from image instead of EXIF
    ''' </history>
    Public ReadOnly Property Width() As Int32
        Get
            Return _Image.Width
        End Get
    End Property

    ''' <summary>
    ''' Image height
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' [altair] 04.09.2005 Changed output to Int32, load from image instead of EXIF
    ''' </history>
    Public ReadOnly Property Height() As Int32
        Get
            Return _Image.Height
        End Get
    End Property

    ''' <summary>
    ''' X resolution in dpi (EXIF XResolution/ResolutionUnit)
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public ReadOnly Property ResolutionX() As Double
        Get
            Dim R As Double = GetPropertyRational(TagNames.XResolution).ToDouble()

            If GetPropertyInt16(TagNames.ResolutionUnit) = 3 Then
                ' -- resolution is in points/cm
                Return R * 2.54
            Else
                ' -- resolution is in points/inch
                Return R
            End If
        End Get
    End Property

    ''' <summary>
    ''' Y resolution in dpi (EXIF YResolution/ResolutionUnit)
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public ReadOnly Property ResolutionY() As Double
        Get
            Dim R As Double = GetPropertyRational(TagNames.YResolution).ToDouble()

            If GetPropertyInt16(TagNames.ResolutionUnit) = 3 Then
                ' -- resolution is in points/cm
                Return R * 2.54
            Else
                ' -- resolution is in points/inch
                Return R
            End If
        End Get
    End Property

    ''' <summary>
    ''' Image title (EXIF ImageTitle)
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public Property Title() As String
        Get
            Return GetPropertyString(TagNames.ImageTitle)
        End Get
        Set(ByVal value As String)
            Try
                SetPropertyString(TagNames.ImageTitle, value)
            Catch
            End Try
        End Set
    End Property

    ''' <summary>
    ''' User comment (EXIF UserComment)
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 13.06.2004 Created
    ''' </history>
    Public Property UserComment() As String
        Get
            Return GetPropertyString(TagNames.ExifUserComment)
        End Get
        Set(ByVal value As String)
            Try
                SetPropertyString(TagNames.ExifUserComment, value)
            Catch
            End Try
        End Set
    End Property

    ''' <summary>
    ''' Artist name (EXIF Artist)
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 13.06.2004 Created
    ''' </history>
    Public Property Artist() As String
        Get
            Return GetPropertyString(TagNames.Artist)
        End Get
        Set(ByVal value As String)
            Try
                SetPropertyString(TagNames.Artist, value)
            Catch
            End Try
        End Set
    End Property

    ''' <summary>
    ''' Image description (EXIF ImageDescription)
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public Property Description() As String
        Get
            Return GetPropertyString(TagNames.ImageDescription)
        End Get
        Set(ByVal value As String)
            Try
                SetPropertyString(TagNames.ImageDescription, value)
            Catch
            End Try
        End Set
    End Property

    ''' <summary>
    ''' Image copyright (EXIF Copyright)
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public Property Copyright() As String
        Get
            Return GetPropertyString(TagNames.Copyright)
        End Get
        Set(ByVal value As String)
            Try
                SetPropertyString(TagNames.Copyright, value)
            Catch
            End Try
        End Set
    End Property

    ''' <summary>
    ''' Exposure time in seconds (EXIF ExifExposureTime/ExifShutterSpeed)
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public ReadOnly Property ExposureTimeDouble() As Double
        Get
            If IsPropertyDefined(TagNames.ExifExposureTime) Then
                ' -- Exposure time is explicitly specified
                Return GetPropertyRational(TagNames.ExifExposureTime).ToDouble()
            ElseIf IsPropertyDefined(TagNames.ExifShutterSpeed) Then
                '-- Compute exposure time from shutter speed
                Return 1 / (2 ^ GetPropertyRational(TagNames.ExifShutterSpeed).ToDouble)
            Else
                ' -- Can't figure out
                Return 0
            End If
        End Get
    End Property

    Public ReadOnly Property ExposureTimeRational() As Rational
        Get
            If IsPropertyDefined(TagNames.ExifExposureTime) Then
                ' -- Exposure time is explicitly specified
                Return GetPropertyRational(TagNames.ExifExposureTime)
            Else
                Return New Rational()
            End If
        End Get
    End Property

    ''' <summary>
    ''' Aperture value as F number (EXIF ExifFNumber/ExifApertureValue)
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public ReadOnly Property Aperture() As Double
        Get
            If IsPropertyDefined(TagNames.ExifFNumber) Then
                Return GetPropertyRational(TagNames.ExifFNumber).ToDouble()
            ElseIf IsPropertyDefined(TagNames.ExifAperture) Then
                Return System.Math.Sqrt(2) ^ GetPropertyRational(TagNames.ExifAperture).ToDouble()
            Else
                Return 0
            End If
        End Get
    End Property

    ''' <summary>
    ''' Exposure program used (EXIF ExifExposureProg)
    ''' </summary>
    ''' <value></value>
    ''' <remarks>If not specified, returns Normal (2)</remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public ReadOnly Property ExposureProgram() As ExposurePrograms
        Get
            Dim X As Int32 = GetPropertyInt16(TagNames.ExifExposureProg)

            If [Enum].IsDefined(GetType(ExposurePrograms), X) Then
                Return CType([Enum].Parse(GetType(ExposurePrograms), [Enum].GetName(GetType(ExposurePrograms), X)), ExposurePrograms)
            Else
                Return ExposurePrograms.Normal
            End If
        End Get
    End Property

    ''' <summary>
    ''' ISO sensitivity
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public ReadOnly Property ISO() As Int16
        Get
            Return GetPropertyInt16(TagNames.ExifISOSpeed)
        End Get
    End Property

    ''' <summary>
    ''' Subject distance in meters (EXIF SubjectDistance)
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public ReadOnly Property SubjectDistance() As Double
        Get
            Return GetPropertyRational(TagNames.ExifSubjectDist).ToDouble()
        End Get
    End Property

    ''' <summary>
    ''' Exposure method metering mode used (EXIF MeteringMode)
    ''' </summary>
    ''' <value></value>
    ''' <remarks>If not specified, returns Unknown (0)</remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public ReadOnly Property ExposureMeteringMode() As ExposureMeteringModes
        Get
            Dim X As Int32 = GetPropertyInt16(TagNames.ExifMeteringMode)

            If [Enum].IsDefined(GetType(ExposureMeteringModes), X) Then
                Return CType([Enum].Parse(GetType(ExposureMeteringModes), [Enum].GetName(GetType(ExposureMeteringModes), X)), ExposureMeteringModes)
            Else
                Return ExposureMeteringModes.Unknown
            End If
        End Get
    End Property

    ''' <summary>
    ''' Focal length of lenses in mm (EXIF FocalLength)
    ''' </summary>
    ''' <value></value>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public ReadOnly Property FocalLength() As Double
        Get
            Return GetPropertyRational(TagNames.ExifFocalLength).ToDouble()
        End Get
    End Property

    ''' <summary>
    ''' Flash mode (EXIF Flash)
    ''' </summary>
    ''' <value></value>
    ''' <remarks>If not present, value NotFired (0) is returned</remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public ReadOnly Property FlashMode() As FlashModes
        Get
            Dim X As Int32 = GetPropertyInt16(TagNames.ExifFlash)

            If [Enum].IsDefined(GetType(FlashModes), X) Then
                Return CType([Enum].Parse(GetType(FlashModes), [Enum].GetName(GetType(FlashModes), X)), FlashModes)
            Else
                Return FlashModes.NotFired
            End If
        End Get
    End Property

    ''' <summary>
    ''' Light source / white balance (EXIF LightSource)
    ''' </summary>
    ''' <value></value>
    ''' <remarks>If not specified, returns Unknown (0).</remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public ReadOnly Property LightSource() As LightSources
        Get
            Dim X As Int32 = GetPropertyInt16(TagNames.ExifLightSource)

            If [Enum].IsDefined(GetType(LightSources), X) Then
                Return CType([Enum].Parse(GetType(LightSources), [Enum].GetName(GetType(LightSources), X)), LightSources)
            Else
                Return LightSources.Unknown
            End If
        End Get
    End Property

#End Region

#Region "Support methods for working with EXIF properties"

    ''' <summary>
    ''' Checks if current image has specified certain property
    ''' </summary>
    ''' <param name="PID">Property ID</param>
    ''' <returns>True if image has specified property, False otherwise.</returns>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public Function IsPropertyDefined(ByVal PID As TagNames) As Boolean
        Return (Array.IndexOf(_Image.PropertyIdList, CInt(PID)) > -1)
    End Function

    ''' <summary>
    ''' Gets specified Int32 property
    ''' </summary>
    ''' <param name="PID">Property ID</param>
    ''' <param name="DefaultValue">Optional, default 0. Default value returned if property is not present.</param>
    ''' <remarks>Value of property or DefaultValue if property is not present.</remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public Function GetPropertyInt32(ByVal PID As TagNames, Optional ByVal DefaultValue As Int32 = 0) As Int32
        If IsPropertyDefined(PID) Then
            Return GetInt32(_Image.GetPropertyItem(PID).Value)
        Else
            Return DefaultValue
        End If
    End Function

    ''' <summary>
    ''' Gets specified Int16 property
    ''' </summary>
    ''' <param name="PID">Property ID</param>
    ''' <param name="DefaultValue">Optional, default 0. Default value returned if property is not present.</param>
    ''' <remarks>Value of property or DefaultValue if property is not present.</remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public Function GetPropertyInt16(ByVal PID As TagNames, Optional ByVal DefaultValue As Int16 = 0) As Int16
        If IsPropertyDefined(PID) Then
            Return GetInt16(_Image.GetPropertyItem(PID).Value)
        Else
            Return DefaultValue
        End If
    End Function

    ''' <summary>
    ''' Gets specified string property
    ''' </summary>
    ''' <param name="PID">Property ID</param>
    ''' <param name="DefaultValue">Optional, default String.Empty. Default value returned if property is not present.</param>
    ''' <returns></returns>
    ''' <remarks>Value of property or DefaultValue if property is not present.</remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public Function GetPropertyString(ByVal PID As TagNames, Optional ByVal DefaultValue As String = "") As String
        If IsPropertyDefined(PID) Then
            Return GetString(_Image.GetPropertyItem(PID).Value)
        Else
            Return DefaultValue
        End If
    End Function

    ''' <summary>
    ''' Gets specified property in raw form
    ''' </summary>
    ''' <param name="PID">Property ID</param>
    ''' <param name="DefaultValue">Optional, default Nothing. Default value returned if property is not present.</param>
    ''' <returns></returns>
    ''' <remarks>Is recommended to use typed methods (like <see cref="GetPropertyString" /> etc.) instead, when possible.</remarks>
    ''' <history>
    ''' [altair] 05.09.2005 Created
    ''' </history>
    Public Function GetProperty(ByVal PID As TagNames, Optional ByVal DefaultValue As Byte() = Nothing) As Byte()
        If IsPropertyDefined(PID) Then
            Return _Image.GetPropertyItem(PID).Value
        Else
            Return DefaultValue
        End If
    End Function

    ''' <summary>
    ''' Gets specified rational property
    ''' </summary>
    ''' <param name="PID">Property ID</param>
    ''' <returns></returns>
    ''' <remarks>Value of property or 0/1 if not present.</remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public Function GetPropertyRational(ByVal PID As TagNames) As Rational
        If IsPropertyDefined(PID) Then
            Return GetRational(_Image.GetPropertyItem(PID).Value)
        Else
            Dim R As Rational
            R.Numerator = 0
            R.Denominator = 1
            Return R
        End If
    End Function

    ''' <summary>
    ''' Sets specified string property
    ''' </summary>
    ''' <param name="PID">Property ID</param>
    ''' <param name="Value">Value to be set</param>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 12.6.2004 Created
    ''' </history>
    Public Sub SetPropertyString(ByVal PID As Int32, ByVal Value As String)
        Dim Data As Byte() = _Encoding.GetBytes(Value & ControlChars.NullChar)
        SetProperty(PID, Data, ExifDataTypes.AsciiString)
    End Sub

    ''' <summary>
    ''' Sets specified Int16 property
    ''' </summary>
    ''' <param name="PID">Property ID</param>
    ''' <param name="Value">Value to be set</param>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 12.6.2004 Created
    ''' </history>
    Public Sub SetPropertyInt16(ByVal PID As Int32, ByVal Value As Int16)
        Dim Data(1) As Byte
        Data(0) = CType(Value And &HFF, Byte)
        Data(1) = CType((Value And &HFF00) >> 8, Byte)
        SetProperty(PID, Data, ExifDataTypes.SignedShort)
    End Sub

    ''' <summary>
    ''' Set specified Int32 property
    ''' </summary>
    ''' <param name="PID">Property ID</param>
    ''' <param name="Value">Value to be set</param>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 13.06.2004 Created
    ''' </history>
    Public Sub SetPropertyInt32(ByVal PID As Int32, ByVal Value As Int32)
        Dim Data(3) As Byte
        For I As Int32 = 0 To 3
            Data(I) = CType(Value And &HFF, Byte)
            Value >>= 8
        Next
        SetProperty(PID, Data, ExifDataTypes.SignedLong)
    End Sub

    ''' <summary>
    ''' Set specified propery in raw form
    ''' </summary>
    ''' <param name="PID">Property ID</param>
    ''' <param name="Data">Raw data</param>
    ''' <param name="Type">EXIF data type</param>
    ''' <remarks>Is recommended to use typed methods (like <see cref="SetPropertyString" /> etc.) instead, when possible.</remarks>
    ''' <history>
    ''' [altair] 12.6.2004 Created
    ''' </history>
    Public Sub SetProperty(ByVal PID As Int32, ByVal Data As Byte(), ByVal Type As ExifDataTypes)
        Dim P As System.Drawing.Imaging.PropertyItem = _Image.PropertyItems(0)
        P.Id = PID
        P.Value = Data
        P.Type = Type
        P.Len = Data.Length
        _Image.SetPropertyItem(P)
    End Sub

    ''' <summary>
    ''' Read Int32 from EXIF bytearray.
    ''' </summary>
    ''' <param name="B">EXIF bytearray to process</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' [altair] 05.09.2005 Changed from public shared to private instance method
    ''' </history>
    Private Shared Function GetInt32(ByVal B As Byte()) As Int32
        If B.Length < 4 Then
            Throw New ArgumentException("Data too short (4 bytes expected)", "B")
        End If
        Return System.BitConverter.ToInt32(B, 0)
    End Function

    ''' <summary>
    ''' Read Int16 from EXIF bytearray.
    ''' </summary>
    ''' <param name="B">EXIF bytearray to process</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' [altair] 05.09.2005 Changed from public shared to private instance method
    ''' </history>
    Private Shared Function GetInt16(ByVal B As Byte()) As Int16
        If B.Length < 2 Then
            Throw New ArgumentException("Data too short (2 bytes expected)", "B")
        End If

        Return System.BitConverter.ToInt16(B, 0)
    End Function

    ''' <summary>
    ''' Read string from EXIF bytearray.
    ''' </summary>
    ''' <param name="B">EXIF bytearray to process</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' [altair] 05.09.2005 Changed from public shared to private instance method
    ''' </history>
    Private Function GetString(ByVal B As Byte()) As String
        Dim R As String = _Encoding.GetString(B)
        If R.EndsWith(vbNullChar) Then
            R = R.Substring(0, R.Length - 1)
        End If
        Return R
    End Function

    ''' <summary>
    ''' Read rational from EXIF bytearray.
    ''' </summary>
    ''' <param name="B">EXIF bytearray to process</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' [altair] 05.09.2005 Changed from public shared to private instance method
    ''' </history>
    Private Shared Function GetRational(ByVal B As Byte()) As Rational
        Dim R As New Rational
        R.Denominator = System.BitConverter.ToInt32(B, 4)
        R.Numerator = System.BitConverter.ToInt32(B, 0)
        Return R
    End Function

    ''' <summary>
    ''' Get specified rational property
    ''' </summary>
    ''' <param name="PID">Property ID</param>
    ''' <param name="rank"></param>
    ''' <returns>Value of property or 0/1 if not present</returns>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public Function GetPropertyRational(ByVal PID As TagNames, ByVal rank As Integer) As Rational
        If IsPropertyDefined(PID) Then
            Dim N As Byte() = New Byte(7) {}
            Array.Copy(_Image.GetPropertyItem(PID).Value, rank * 8, N, 0, 8)
            Return GetRational(N)
        Else
            Dim R As New Rational()
            Return R
        End If
    End Function

    ''' <summary>
    ''' Degrees, Minutes, Seconds to Decimal Degrees
    ''' </summary>
    ''' <param name="Deg">Degrees</param>
    ''' <param name="Min">Minutes</param>
    ''' <param name="Sec">Seconds</param>
    ''' <returns>Decimal Degrees</returns>
    ''' <remarks>in EXIF, Deg is always positive, but added case for negative degrees in case this routine is reused</remarks>
    Private Shared Function DMStoDD(ByVal Deg As Double, ByVal Min As Double, ByVal Sec As Double) As Double
        Dim lFraction As Double = (Min / 60) + (Sec / 3600)
        If Deg > 0 Then
            Return Deg + lFraction
        Else
            Return Deg - lFraction
        End If
    End Function

    ''' <summary>
    ''' GPS Latitude
    ''' </summary>
    ''' <returns>Decimal degrees of latitude</returns>
    Public ReadOnly Property Latitude() As Double
        Get
            If IsPropertyDefined(TagNames.GpsLatitude) Then
                Dim deg As Rational = GetPropertyRational(TagNames.GpsLatitude, 0)
                Dim min As Rational = GetPropertyRational(TagNames.GpsLatitude, 1)
                Dim sec As Rational = GetPropertyRational(TagNames.GpsLatitude, 2)
                Dim res As [Double] = DMStoDD(deg.ToDouble(), min.ToDouble(), sec.ToDouble())
                Dim Ref As [String] = GetPropertyString(TagNames.GpsLatitudeRef)
                If Ref <> "N" Then
                    res = -res
                End If
                Return res
            Else
                Return [Double].NaN
            End If
        End Get
    End Property

    ''' <summary>
    ''' GPS Longitude
    ''' </summary>
    ''' <returns>Decimal degrees of longitude</returns>
    Public ReadOnly Property Longitude() As Double
        Get
            If IsPropertyDefined(TagNames.GpsLongitude) Then
                Dim deg As Rational = GetPropertyRational(TagNames.GpsLongitude, 0)
                Dim min As Rational = GetPropertyRational(TagNames.GpsLongitude, 1)
                Dim sec As Rational = GetPropertyRational(TagNames.GpsLongitude, 2)
                Dim res As [Double] = DMStoDD(deg.ToDouble(), min.ToDouble(), sec.ToDouble())
                Dim Ref As [String] = GetPropertyString(TagNames.GpsLongitudeRef)
                If Ref <> "E" Then
                    res = -res
                End If
                Return res
            Else
                Return [Double].NaN
            End If
        End Get
    End Property

#End Region

#Region " IDisposable implementation "

    ''' <summary>
    ''' Disposes unmanaged resources of this class
    ''' </summary>
    ''' <remarks></remarks>
    ''' <history>
    ''' [altair] 10.09.2003 Created
    ''' </history>
    Public Sub Dispose() Implements IDisposable.Dispose
        if _Image IsNot Nothing Then _Image.Dispose()
        if _Stream IsNot Nothing Then _Stream.Dispose()
    End Sub

#End Region
End Class
