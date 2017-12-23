
Option Strict On
Option Explicit On

Imports Contensive.Addons.VisitCharts.Controllers
Imports Contensive.BaseClasses

Namespace Contensive.Addons.VisitCharts.Views
    Public Class DurationViewClass
        Inherits AddonBaseClass
        '
        '=====================================================================================
        Public Overrides Function Execute(ByVal CP As CPBaseClass) As Object
            Dim result As String = ""
            Dim sw As New Stopwatch : sw.Start()
            Try
                '
                ' -- initialize application. If authentication needed and not login page, pass true
                Using ac As New applicationController(CP, False)
                    '
                    ' -- your code
                    result = GetContent(ac)
                End Using
            Catch ex As Exception
                CP.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        '
        Public Function GetContent(ac As applicationController) As String
            Dim result As String = String.Empty
            Try
                Dim dblDateStart As Double
                Dim dblDateEnd As Double
                Dim DateStart As Date
                Dim DateEnd As Date
                Dim Duration As Integer
                Dim DivName As String
                Dim Rate As String
                Dim intRate As Integer
                Dim AllowHourly As Boolean
                Dim Width As String
                Dim Height As String

                Width = ac.cp.Doc.GetText("Width")
                Height = ac.cp.Doc.GetText("Height")

                Rate = ac.cp.Doc.GetText("Rate")
                If Rate = "Hourly" Then
                    intRate = 1
                    AllowHourly = True
                Else
                    intRate = 24
                End If
                Duration = ac.cp.Doc.GetInteger(("Duration"))
                DivName = ac.cp.Doc.GetText("TargetDiv")
                If DivName = "" Then
                    DivName = "PageViewChart"
                End If

                DateStart = Now.AddDays(-Duration).Date
                DateEnd = Now.Date

                If IsDate(DateStart) And IsDate(DateEnd) Then

                    dblDateStart = DateStart.ToOADate()
                    dblDateEnd = DateEnd.ToOADate()
                    Dim criteria As String = "(TimeDuration=" & intRate & ") AND (DateNumber>=" & dblDateStart & ") AND (DateNumber<=" & dblDateEnd & ")"
                    Dim visitSummaryList As List(Of Models.visitSummaryModel) = Models.visitSummaryModel.createList(ac.cp, criteria, "TimeNumber desc")
                    If (visitSummaryList.Count = 0) Then
                        result = "<span class=""ccError"">There is currently no data collected to display this chart. Please check back later.</span>"
                    Else
                        result = Models.chartViewModel.GetChart(ac, visitSummaryList, DivName, False, Width, Height, AllowHourly)
                    End If
                Else
                    result = "<span class=""ccError"">Please enter a valid Start and End Date to view the Page Views Chart.</span>"
                End If
                GetContent = result
            Catch ex As Exception
                ac.cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace
