Public Class clsTrip
    Public Segments As New Generic.LinkedList(Of clsSegment)
    Public RollingAverages As New List(Of clsTrip)
    Public NewSegmentMinimumTime As TimeSpan = TimeSpan.FromMinutes(3)
    Public NewSegmentSpeedDifference As Double = 15
    Public TrimSegmentBySpeed As Double = 10

    Public DurationLimit As TimeSpan = TimeSpan.FromDays(200)
    Public DurationLimitMet As Boolean = False

    Public Sub New()
        Dim lDurations As New List(Of TimeSpan)
        lDurations.Add(TimeSpan.FromMinutes(5))
        lDurations.Add(TimeSpan.FromMinutes(10))
        lDurations.Add(TimeSpan.FromMinutes(15))
        lDurations.Add(TimeSpan.FromMinutes(30))
        lDurations.Add(TimeSpan.FromHours(1))
        lDurations.Add(TimeSpan.FromHours(2))
        For Each lDuration As TimeSpan In lDurations
            RollingAverages.Add(New clsTrip(lDuration))
        Next
    End Sub

    Public Sub New(ByVal aDuration As TimeSpan)
        DurationLimit = aDuration
        NewSegmentSpeedDifference = -1
        NewSegmentMinimumTime = TimeSpan.FromHours(0)
    End Sub

    ''' <summary>
    ''' Create a report based on the given GPX
    ''' </summary>
    ''' <param name="aGPX">GPX file to read into clsTrip and base report on</param>
    ''' <returns>Report of the trip in aGPX (as String)</returns>
    ''' <remarks>Original version was built into clsGPX.LoadFile for initial testing</remarks>
    Public Shared Function ReportFromGPX(ByVal aGPX As clsGPX) As String
        Dim lSB As New System.Text.StringBuilder
        Dim lTrackPointOld As clsGPXwaypoint = Nothing
        Dim lSegmentLength As Double = 0.0
        Dim lMetersToMiles As Double = 1 / (0.0254 * 12 * 5280)
        Dim lTrip As New clsTrip
        For Each lTrack As clsGPXtrack In aGPX.trk
            lSB.AppendLine("Track " & lTrack.name & ":" & lTrack.number)
            For Each lTrackSegment As clsGPXtracksegment In lTrack.trkseg
                lSB.AppendLine("TrackSegmentPointCount " & lTrackSegment.trkpt.Count)
                lSB.Append("Time".PadLeft(19) & "Distance".PadLeft(9) & "Speed".PadLeft(6))
                For Each lDuration As clsTrip In lTrip.RollingAverages
                    lSB.Append(lDuration.DurationLimit.TotalMinutes.ToString.PadLeft(6))
                Next
                lSB.AppendLine("")
                For Each lTrackPoint As clsGPXwaypoint In lTrackSegment.trkpt
                    With lTrackPoint
                        lSB.Append(.time) '& " " & .lat & " " & .lon)
                        If lTrackPointOld IsNot Nothing Then
                            Dim lDistanceMiles As Double = lMetersToMiles * MetersBetweenLatLon(lTrackPointOld.lat, lTrackPointOld.lon, .lat, .lon)
                            Dim lTimeElapsed As TimeSpan = .time.Subtract(lTrackPointOld.time)
                            Dim lSpeedMph As Double = lDistanceMiles / lTimeElapsed.TotalHours
                            lSB.Append(Format(lDistanceMiles, "#,##0.000").PadLeft(9) & Format(lSpeedMph, "##.00").PadLeft(6))
                            lTrip.AddSegment(lTrackPointOld.time, lDistanceMiles, lTimeElapsed)
                            For Each lDuration As clsTrip In lTrip.RollingAverages
                                If lDuration.DurationLimitMet AndAlso lDuration.Segments.Count > 0 Then
                                    lSB.Append(Format(lDuration.Total.Speed, "##.00").PadLeft(6))
                                Else
                                    lSB.Append(" ".PadLeft(6))
                                End If
                            Next
                        End If
                        lSB.AppendLine()
                    End With
                    lTrackPointOld = lTrackPoint
                Next
            Next
        Next

        lSB.AppendLine(vbCrLf & lTrip.ToString)
        lTrip.Trim()
        lSB.AppendLine(vbCrLf & "TrimEdges" & vbCrLf & lTrip.ToString)

        Return lSB.ToString

    End Function

    Public Sub AddSegment(ByVal aStartDate As Date, ByVal aDistanceMiles As Double, ByVal aTimeElapsed As TimeSpan)
        If Segments.Count = 0 Then
            Dim lSegment As clsSegment = New clsSegment(aStartDate, aDistanceMiles, aTimeElapsed)
            Segments.AddFirst(lSegment)
        Else
            Dim lSpeedCurrent As Double = aDistanceMiles / aTimeElapsed.TotalHours
            Dim lSegmentCurrent As clsSegment = Segments.Last.Value
            If Math.Abs(lSpeedCurrent - lSegmentCurrent.Speed) > NewSegmentSpeedDifference Then
                'segment by speed
                If Segments.Count > 1 AndAlso lSegmentCurrent.Elapsed < NewSegmentMinimumTime Then
                    'merge this one with prev two, delete last
                    Dim lSegmentBase As clsSegment = Segments.Last.Previous.Value
                    With lSegmentBase
                        .DistanceMiles += lSegmentCurrent.DistanceMiles + aDistanceMiles
                        .EndDate += lSegmentCurrent.Elapsed + aTimeElapsed
                    End With
                    Segments.RemoveLast()
                    While Segments.Count > 1 AndAlso _
                        Math.Abs(Segments.Last.Value.Speed - Segments.Last.Previous.Value.Speed) < NewSegmentSpeedDifference
                        lSegmentBase = Segments.Last.Previous.Value
                        With lSegmentBase
                            .DistanceMiles += Segments.Last.Value.DistanceMiles
                            .EndDate += Segments.Last.Value.Elapsed
                        End With
                        Segments.RemoveLast()
                    End While
                Else
                    Dim lSegment As clsSegment = New clsSegment(aStartDate, aDistanceMiles, aTimeElapsed)
                    Segments.AddLast(lSegment)
                End If
            Else
                With Segments.Last.Value
                    .EndDate += aTimeElapsed
                    .DistanceMiles += aDistanceMiles
                End With
            End If
        End If
        If RollingAverages.Count > 0 Then
            For Each lTrip As clsTrip In RollingAverages
                lTrip.AddSegment(aStartDate, aDistanceMiles, aTimeElapsed)
            Next
        Else
            Dim lTotal As clsSegment = Total()
            If lTotal.Elapsed > DurationLimit Then DurationLimitMet = True
            Dim lCount As Integer = 0
            While Segments.Count > 1 AndAlso lTotal.Elapsed > DurationLimit
                Segments.RemoveFirst()
                lTotal = Total()
            End While
        End If
    End Sub

    Public Function Total() As clsSegment
        Dim lTotal As clsSegment = Nothing
        For Each lSegment As clsSegment In Segments
            With lSegment
                If lTotal Is Nothing Then
                    lTotal = New clsSegment(.StartDate, .DistanceMiles, .Elapsed)
                Else
                    lTotal.DistanceMiles += .DistanceMiles
                    lTotal.EndDate += .Elapsed
                End If
            End With
        Next
        Return lTotal
    End Function

    Public Overrides Function ToString() As String
        Dim lSB As New System.Text.StringBuilder
        lSB.AppendLine(vbCrLf & "Segment Count " & Segments.Count)
        Dim lCount As Integer = 0
        For Each lSegment As clsSegment In Segments
            lCount += 1
            lSB.AppendLine("S" & lCount.ToString.PadLeft(3, "0").ToString & " " & lSegment.ToString)
        Next
        lSB.AppendLine(vbCrLf & "Trip " & Total.ToString)
        Return lSB.ToString
    End Function

    Public Sub Trim()
        While Segments.Last.Value.Elapsed < TimeSpan.FromHours(1) AndAlso Segments.Last.Value.Speed < TrimSegmentBySpeed
            Segments.RemoveLast()
        End While
        While Segments.First.Value.Elapsed < TimeSpan.FromHours(1) AndAlso Segments.First.Value.Speed < TrimSegmentBySpeed
            Segments.RemoveFirst()
        End While
    End Sub
End Class

Public Class clsSegment
    Public StartDate As Date
    Public EndDate As Date
    Public DistanceMiles As Double
    Public Sub New(ByVal aStartDate As Date, ByVal aDistanceMiles As Double, ByVal aTimeElapsed As TimeSpan)
        StartDate = aStartDate
        EndDate = aStartDate + aTimeElapsed
        DistanceMiles = aDistanceMiles
    End Sub
    Public Function Speed() As Double 'mph
        Return DistanceMiles / EndDate.Subtract(StartDate).TotalHours
    End Function
    Public Function Elapsed() As TimeSpan
        Return EndDate.Subtract(StartDate)
    End Function
    Public Overrides Function ToString() As String
        Return StartDate & _
               Format(DistanceMiles, "#,###.00").PadLeft(9) & _
               Format(Speed, "###.00").PadLeft(7) & _
               Format(Elapsed.Hours, "###").PadLeft(4) & ":" & _
               Format(Elapsed.Minutes, "##").PadLeft(2, "0")
    End Function
End Class
