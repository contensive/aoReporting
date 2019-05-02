
Option Explicit On
Option Strict On

Imports System.Collections.Generic
Imports System.Text
Imports Contensive.BaseClasses
Imports Contensive.Addons.Reporting.Models
Imports Contensive.Addons.Reporting.Views
Imports Contensive.Addons.Reporting.Controllers

Namespace Models
    Public NotInheritable Class ChartViewModel
        '
        '====================================================================================================
        '
        Public Shared Function getChart(ae As Controllers.applicationController, visitSummaryList As List(Of Models.visitSummaryModel), Div As String, isVisitData As Boolean, Optional Width As String = "100%", Optional Height As String = "400px", Optional AllowHourly As Boolean = False) As String
            Dim result As New StringBuilder
            Try
                If Width = "" Then
                    Width = "100%"
                End If
                If Height = "" Then
                    Height = "400px"
                End If
                Dim Caption As String
                If isVisitData Then
                    Caption = "Visits"
                Else
                    Caption = "Page Views"
                End If
                result.Append("<script type='text/javascript'>" & vbCrLf)
                result.Append("google.load('visualization', '1', {'packages':['annotatedtimeline']});" & vbCrLf)
                result.Append("google.setOnLoadCallback(drawChart);" & vbCrLf)
                result.Append("function drawChart() {" & vbCrLf)
                result.Append("var data = new google.visualization.DataTable();" & vbCrLf)
                result.Append("data.addColumn('date', 'Date');" & vbCrLf)
                result.Append("data.addColumn('number', '" & Caption & "');" & vbCrLf)
                result.Append("data.addRows(" & visitSummaryList.Count & ");" & vbCrLf)
                '
                Dim Pointer As Integer = 0
                For Each viewSummary As Contensive.Addons.Reporting.Models.visitSummaryModel In visitSummaryList
                    Dim Value As String = String.Empty
                    Dim nrmDate As Date
                    If (viewSummary.TimeNumber <> 0) And (AllowHourly) Then
                        nrmDate = Date.FromOADate((viewSummary.DateNumber + (viewSummary.TimeNumber / 24.0!)))
                    Else
                        nrmDate = Date.FromOADate(viewSummary.DateNumber)
                    End If

                    If isVisitData Then
                        Value = viewSummary.Visits.ToString()
                    Else
                        Value = viewSummary.PagesViewed.ToString()
                    End If

                    result.Append("data.setValue(" & Pointer & ", 0, new Date(" & Year(nrmDate) & "," & Month(nrmDate) - 1 & "," & Day(nrmDate) & "," & DatePart("H", nrmDate) & ",00,00));" & vbCrLf)
                    result.Append("data.setValue(" & Pointer & ", 1, " & Value & ");" & vbCrLf)
                    Pointer = Pointer + 1
                Next
                result.Append("var chart = new google.visualization.AnnotatedTimeLine(document.getElementById('" & Div & "'));" & vbCrLf)
                result.Append("chart.draw(data, {displayAnnotations: false});" & vbCrLf)
                result.Append("}" & vbCrLf)
                result.Append("</script>" & vbCrLf & vbCrLf)
                result.Append("<div id='" & Div & "' style='width: " & Width & "; height: " & Height & ";'></div>")
            Catch ex As Exception
                ae.cp.Site.ErrorReport(ex)
            End Try
            Return result.ToString()
        End Function
        '
        '====================================================================================================
        '
        '   RSData is a 3xlength array of the data
        '       a(0,n) = DateNumber (int of Dbl Date)
        '       a(1,n) = TimeNumber (0-23)
        '       a(2,n) = value to plot
        ' AllowHourly - if true, there must be all 24 time numbers for each date number
        '
        Friend Shared Function GetChart2(ae As Controllers.applicationController, visitSummaryList As List(Of Models.visitSummaryModel), Div As String, isVisitData As Boolean, Optional Width As String = "100%", Optional Height As String = "400px", Optional AllowHourly As Boolean = False) As String
            Dim result As New StringBuilder
            Try
                If Width = "" Then
                    Width = "100%"
                End If
                If Height = "" Then
                    Height = "400px"
                End If
                Dim Caption As String = "Page Views"
                If isVisitData Then
                    Caption = "Visits"
                End If
                result.Append("<script type='text/javascript'>" & vbCrLf)
                result.Append("google.load('visualization', '1', {'packages':['annotatedtimeline']});" & vbCrLf)
                result.Append("google.setOnLoadCallback(drawChart);" & vbCrLf)
                result.Append("function drawChart() {" & vbCrLf)
                result.Append("var data = new google.visualization.DataTable();" & vbCrLf)
                result.Append("data.addColumn('date', 'Date');" & vbCrLf)
                result.Append("data.addColumn('number', '" & Caption & "');" & vbCrLf)
                result.Append("data.addRows(" & (visitSummaryList.Count + 1) & ");" & vbCrLf)
                '
                ' Plot what we got
                '
                Dim Ptr As Integer = 0
                For Each visitSummary In visitSummaryList
                    Dim DateNumber As Double = Int(visitSummary.DateNumber + 0.5)
                    Dim TimeNumber As Double = Int(visitSummary.TimeNumber + 0.5)
                    Dim PlotValue As Double = visitSummary.PagesViewed
                    If isVisitData Then
                        PlotValue = visitSummary.Visits
                    End If
                    Dim PlotDate As Date
                    If (TimeNumber <> 0) And (AllowHourly) Then
                        PlotDate = Date.FromOADate(DateNumber + (TimeNumber / 24.0!))
                    Else
                        PlotDate = Date.FromOADate(DateNumber)
                    End If
                    result.Append("data.setValue(" & Ptr & ", 0, new Date(" & Year(PlotDate) & "," & Month(PlotDate) - 1 & "," & Day(PlotDate) & "," & DatePart("H", PlotDate) & ",00,00));" & vbCrLf)
                    result.Append("data.setValue(" & Ptr & ", 1, " & PlotValue & ");" & vbCrLf)
                    Ptr += 1
                Next
                result.Append("var chart = new google.visualization.AnnotatedTimeLine(document.getElementById('" & Div & "'));" & vbCrLf)
                result.Append("chart.draw(data, {displayAnnotations: false});" & vbCrLf)
                result.Append("}" & vbCrLf)
                result.Append("</script>" & vbCrLf & vbCrLf)
                result.Append("<div id='" & Div & "' style='width: " & Width & "; height: " & Height & ";'></div>")
            Catch ex As Exception
                ae.cp.Site.ErrorReport(ex)
            End Try
            Return result.ToString()
        End Function
    End Class
End Namespace

