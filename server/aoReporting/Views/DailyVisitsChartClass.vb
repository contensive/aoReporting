
Option Strict On
Option Explicit On

Imports Contensive.Addons.Reporting.Controllers
Imports Contensive.BaseClasses

Namespace Views
    '
    Public Class DailyVisitsChartClass
        Inherits AddonBaseClass
        '
        '=====================================================================================
        Public Overrides Function Execute(ByVal CP As CPBaseClass) As Object
            Dim result As String = ""
            Dim sw As New Stopwatch : sw.Start()
            Try
                Using ac As New applicationController(CP, False)
                    Dim Width As String = ac.cp.Doc.GetText("Width")
                    Dim Height As String = ac.cp.Doc.GetText("Height")
                    Dim durationHours As Integer = 24
                    Dim Rate As String = ac.cp.Doc.GetText("Rate")
                    If Rate.ToLower = "hourly" Then
                        durationHours = 1
                    End If
                    Dim DurationDays As Integer = ac.cp.Doc.GetInteger("Duration", 365)
                    Dim DivName As String = ac.cp.Doc.GetText("TargetDiv")
                    If DivName = "" Then
                        DivName = "PageViewChart"
                    End If
                    Dim DateEnd As Date = Now.Date
                    Dim DateStart As Date = DateEnd.AddDays(-DurationDays).Date
                    Dim dblDateStart As Double = DateStart.ToOADate()
                    Dim dblDateEnd As Double = DateEnd.ToOADate()
                    ' Dim DateNumber As Integer = encodeInteger(DateStart.AddHours(24 / 2.0).ToOADate())
                    '  Dim DateEndNumber As Integer = encodeInteger(DateEnd.AddHours(24 / 2.0).ToOADate())
                    '  Dim dblDateStart As Double = DateNumber
                    ' Dim dblDateEnd As Double = DateEndNumber

                    Dim criteria As String = "(TimeDuration=" & durationHours & ") AND (DateNumber>=" & dblDateStart & ") AND (DateNumber<" & dblDateEnd & ")"


                    Dim visitSummaryList As List(Of Models.visitSummaryModel) = Models.visitSummaryModel.createList(ac.cp, criteria, "TimeNumber desc")
                    If (visitSummaryList.Count = 0) Then
                        result = "<span class=""ccError"">There is currently no data collected to display this chart. Please check back later.</span>"
                    Else
                        result = Models.ChartViewModel.getChart(ac, visitSummaryList, DivName, False, Width, Height, (durationHours = 1))
                    End If
                End Using
            Catch ex As Exception
                CP.Site.ErrorReport(ex)
            End Try
            Return result
        End Function


        Public Function encodeInteger(expression As Object) As Integer
            If (expression Is Nothing) Then
                Return 0
            End If
            Dim trialString As String = expression.ToString()
            Dim trialInt As Integer = 0
            If (Integer.TryParse(trialString, trialInt)) Then
                Return trialInt
            End If
            Dim trialDbl As Double = 0
            If (Double.TryParse(trialString, trialDbl)) Then
                Return Convert.ToInt32(trialDbl)
            End If
            Dim trialBool As Boolean
            Return 0
        End Function


        ''
        ''====================================================================================================
        ''
        'Public Function GetContent(OptionString As String) As String
        '    Dim Stream As String
        '    Dim CS As Integer
        '    Dim dblDate As Double
        '    Dim cmn As New CommonClass
        '    dblDate = CDbl(Date)
        '    CS = Main.OpenCSContent("Visit Summary", "(TimeDuration=1) AND (DateNumber=" & dblDate & ")", "TimeNumber desc", , , , "DateNumber,TimeNumber,Visits")
        '    If Main.CSOK(CS) Then
        '        Stream = cmn.GetChart(Main, CS, "daily-visits", True)
        '    Else
        '        Stream = "<span class=""ccError"">There is currently no data collected to display this chart. Please check back later.</span>"
        '    End If
        '    Call Main.CloseCS(CS)
        '    GetContent = Stream
        'End Function

    End Class
End Namespace
