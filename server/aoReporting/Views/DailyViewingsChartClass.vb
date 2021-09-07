
Option Strict On
Option Explicit On

Imports Contensive.Addons.Reporting.Controllers
Imports Contensive.BaseClasses

Namespace Views
    Public Class DailyViewingsChartClass
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
    End Class
End Namespace
