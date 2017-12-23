Option Strict On
Option Explicit On

Imports System.Text
Imports Contensive.Addons.VisitCharts.Controllers
Imports Contensive.BaseClasses

Namespace Contensive.Addons.VisitCharts.Views
    '
    '====================================================================================================
    '
    Public Class DurationVisitClass
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
                    result = getContent(ac)
                End Using
            Catch ex As Exception
                CP.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        '
        '====================================================================================================
        '
        Public Function getContent(ac As applicationController) As String
            Dim result As New StringBuilder()

            Try
                Dim Stream As String
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
                Dim cacheName As String
                '
                Width = ac.cp.Doc.GetText("Chart Width")
                If Width = "" Then
                    Width = "100%"
                End If
                '
                Height = ac.cp.Doc.GetText("Chart Height")
                If Height = "" Then
                    Height = "400px"
                ElseIf InStr(1, Height, "%") <> 0 Then
                    Height = "400px"
                End If
                '
                Rate = ac.cp.Doc.GetText("Daily or Hourly")
                If Rate = "Hourly" Then
                    intRate = 1
                    AllowHourly = True
                Else
                    intRate = 24
                End If
                '
                Duration = ac.cp.Doc.GetInteger(("Days to Display"))
                If Duration = 0 Then
                    Duration = 365
                End If
                '
                DivName = ac.cp.Doc.GetText("Target Div")
                If DivName = "" Then
                    DivName = ac.cp.Doc.GetText("TargetDiv")
                    If DivName = "" Then
                        DivName = "durationChart"
                    End If
                End If

                DateEnd = Now()
                DateStart = DateEnd.AddDays(-Duration)
                '
                cacheName = "DurationVisit-" & Width & "-" & Height & "-" & DivName & "-" & CStr(AllowHourly) & "-" & intRate & "-" & Duration
                Stream = ac.cp.Cache.Read(cacheName)
                If (String.IsNullOrEmpty(Stream)) Then
                    If IsDate(DateStart) And IsDate(DateEnd) Then
                        dblDateStart = DateStart.ToOADate()
                        dblDateEnd = DateEnd.ToOADate()
                        Dim criteria As String = "(TimeDuration=" & intRate & ") AND (DateNumber>=" & dblDateStart & ") AND (DateNumber<=" & dblDateEnd & ")"
                        Dim visitSummaryList As List(Of Models.visitSummaryModel) = Models.visitSummaryModel.createList(ac.cp, criteria, "DateNumber, TimeNumber")
                        If (visitSummaryList.Count = 0) Then
                            result.Append("<span class=""ccError"">There is currently no data collected to display this chart. Please check back later.</span>")
                        Else
                            result.Append(Models.chartViewModel.GetChart2(ac, visitSummaryList, DivName, True, Width, Height, AllowHourly))
                            result.Append(GetSummary2(ac, visitSummaryList, AllowHourly))
                        End If
                    Else
                        result.Append("<span class=""ccError"">Please enter a valid Start and End Date to view the Visit Chart.</span>")
                    End If
                End If
            Catch ex As Exception
                ac.cp.Site.ErrorReport(ex)
            End Try
            Return result.ToString()
        End Function
        '
        '====================================================================================================
        '
        Public Function GetSummary2(ac As applicationController, visitSummaryList As List(Of Models.visitSummaryModel), IsHourlyChart As Boolean) As String
            Dim result As New StringBuilder
            Try
                Dim Visits As Double
                Dim Viewings As Double
                Dim PagePerVisit As Double
                Dim NewVisits As Double
                Dim TimeOnSite As Double
                Dim BounceRate As Double
                Dim Authenticated As Double
                Dim DayCnt As Double
                '
                DayCnt = visitSummaryList.Count + 1
                If IsHourlyChart Then
                    DayCnt = DayCnt / 24
                End If
                For Each visitSummary In visitSummaryList
                    Visits = Visits + visitSummary.Visits
                    Viewings = Viewings + visitSummary.PagesViewed
                    NewVisits = NewVisits + visitSummary.NewVisitorVisits
                    TimeOnSite = TimeOnSite + visitSummary.AveTimeOnSite
                    BounceRate = BounceRate + visitSummary.SinglePageVisits
                    Authenticated = Authenticated + visitSummary.AuthenticatedVisits
                Next
                If (Viewings <> 0) And (Visits <> 0) Then
                    PagePerVisit = (Viewings / Visits)
                    TimeOnSite = (TimeOnSite / Visits)
                    BounceRate = ((BounceRate / Visits) * 100)
                    NewVisits = ((NewVisits / Visits) * 100)
                    Authenticated = ((Authenticated / Visits) * 100)
                End If

                result.Append("<div class=""summaryContainer"">")
                result.Append("<table border=""0"" width=""100%"" cellpadding=""3"" cellspacing=""0"">")
                result.Append("<tr>")
                result.Append("<td class=""summaryHeader"" colspan=""2"" width=""100%"">Summary: Last " & DayCnt & " Days</td>")
                result.Append("</tr>")
                result.Append("<tr>")
                result.Append("<td class=""summaryCell"" width=""50%""><a style=""text-decoration:none;"" href=""#"" title=""" & VisitDesc & """ onClick=""return false;""><span class=""summaryValue"">" & Visits & "</span> <span class=""summaryCaption"">Visits</span></a></td>")
                result.Append("<td class=""summaryCell"" width=""50%""><a style=""text-decoration:none;"" href=""#"" title=""" & BounceDesc & """ onClick=""return false;""><span class=""summaryValue"">" & BounceRate & "%</span> <span class=""summaryCaption"">Bounce Rate</span></a></td>")
                result.Append("</tr>")
                result.Append("<tr>")
                result.Append("<td class=""summaryCell"" width=""50%""><a style=""text-decoration:none;"" href=""#"" title=""" & PageDesc & """ onClick=""return false;""><span class=""summaryValue"">" & Viewings & "</span> <span class=""summaryCaption"">Pages</span></a></td>")
                result.Append("<td class=""summaryCell"" width=""50%""><a style=""text-decoration:none;"" href=""#"" title=""" & PVDesc & """ onClick=""return false;""><span class=""summaryValue"">" & PagePerVisit & "</span> <span class=""summaryCaption"">Pages/Visit</span></a></td>")
                result.Append("</tr>")
                result.Append("<tr>")
                result.Append("<td class=""summaryCell"" width=""50%""><a style=""text-decoration:none;"" href=""#"" title=""" & VisitorDesc & """ onClick=""return false;""><span class=""summaryValue"">" & NewVisits & "%</span> <span class=""summaryCaption"">New Visitors</span></a></td>")
                result.Append("<td class=""summaryCell"" width=""50%""><a style=""text-decoration:none;"" href=""#"" title=""" & LogInDesc & """ onClick=""return false;""><span class=""summaryValue"">" & Authenticated & "%</span> <span class=""summaryCaption"">Log In</span></a></td>")
                result.Append("</tr>")
                result.Append("</table>")
                result.Append("</div>")
            Catch ex As Exception
                ac.cp.Site.ErrorReport(ex)
            End Try
            Return result.ToString()
        End Function
        '
        '====================================================================================================
        '
        'Public Function GetSummary(Pointer as integer ) As String
        '    On Error GoTo ErrorTrap
        '
        '    Dim Stream As String
        '
        '    Dim Visits As Double
        '    Dim Viewings As Double
        '    Dim PagePerVisit As Double
        '    Dim NewVisits As Double
        '    Dim TimeOnSite As Double
        '    Dim BounceRate As Double
        '    Dim Authenticated As Double
        '
        '    Dim RecordCount As Double
        '
        '    Call Main.FirstCSRecord(Pointer)
        '
        '    If Main.CSOK(Pointer) Then
        '        RecordCount = Main.GetCSRowCount(Pointer)
        '        Do While Main.CSOK(Pointer)
        '
        '            Visits = Main.GetCSInteger(Pointer, "Visits") + Visits
        '            Viewings = Main.GetCSInteger(Pointer, "PagesViewed") + Viewings
        '            NewVisits = Main.GetCSInteger(Pointer, "NewVisitorVisits") + NewVisits
        '            TimeOnSite = Main.GetCSInteger(Pointer, "AveTimeOnSite") + TimeOnSite
        '            BounceRate = Main.GetCSInteger(Pointer, "SinglePageVisits") + BounceRate
        '            Authenticated = Main.GetCSInteger(Pointer, "AuthenticatedVisits") + Authenticated
        '
        '            Call Main.NextCSRecord(Pointer)
        '        Loop
        '
        '        If (Viewings <> 0) And (Visits <> 0) Then
        '            PagePerVisit = FormatNumber((Viewings / Visits), 1)
        '            TimeOnSite = (TimeOnSite / Visits)
        '            BounceRate = FormatNumber(((BounceRate / Visits) * 100), 1)
        '            NewVisits = FormatNumber(((NewVisits / Visits) * 100), 1)
        '            Authenticated = FormatNumber(((Authenticated / Visits) * 100), 1)
        '        End If
        '
        '        result.Append(  "<div class=""summaryContainer"">"
        '        result.Append(  "<table border=""0"" width=""100%"" cellpadding=""3"" cellspacing=""0"">"
        '        result.Append(  "<tr>"
        '        result.Append(  "<td class=""summaryHeader"" colspan=""2"" width=""100%"">Summary*</td>"
        '        result.Append(  "</tr>"
        '        result.Append(  "<tr>"
        '        result.Append(  "<td class=""summaryCell"" width=""50%""><a style=""text-decoration:none;"" href=""#"" title=""" & VisitDesc & """ onClick=""return false;""><span class=""summaryValue"">" & Visits & "</span> <span class=""summaryCaption"">Visits</span></a></td>"
        '        result.Append(  "<td class=""summaryCell"" width=""50%""><a style=""text-decoration:none;"" href=""#"" title=""" & BounceDesc & """ onClick=""return false;""><span class=""summaryValue"">" & BounceRate & "%</span> <span class=""summaryCaption"">Bounce Rate</span></a></td>"
        '        result.Append(  "</tr>"
        '        result.Append(  "<tr>"
        '        result.Append(  "<td class=""summaryCell"" width=""50%""><a style=""text-decoration:none;"" href=""#"" title=""" & PageDesc & """ onClick=""return false;""><span class=""summaryValue"">" & Viewings & "</span> <span class=""summaryCaption"">Pages</span></a></td>"
        '        result.Append(  "<td class=""summaryCell"" width=""50%""><a style=""text-decoration:none;"" href=""#"" title=""" & PVDesc & """ onClick=""return false;""><span class=""summaryValue"">" & PagePerVisit & "</span> <span class=""summaryCaption"">Pages/Visit</span></a></td>"
        '        result.Append(  "</tr>"
        '        result.Append(  "<tr>"
        '        result.Append(  "<td class=""summaryCell"" width=""50%""><a style=""text-decoration:none;"" href=""#"" title=""" & VisitorDesc & """ onClick=""return false;""><span class=""summaryValue"">" & NewVisits & "%</span> <span class=""summaryCaption"">New Visitors</span></a></td>"
        '        result.Append(  "<td class=""summaryCell"" width=""50%""><a style=""text-decoration:none;"" href=""#"" title=""" & LogInDesc & """ onClick=""return false;""><span class=""summaryValue"">" & Authenticated & "%</span> <span class=""summaryCaption"">Log In</span></a></td>"
        '        result.Append(  "</tr>"
        '        result.Append(  "</table>"
        '        result.Append(  "</div>"
        '    End If
        '    Call Main.CloseCS(Pointer)
        '
        '    GetSummary = Stream
        '
        '    Exit Function
        'ErrorTrap:
        '    Call HandleError("DurationVisitClass", "GetSummary", Err.Number, Err.Source, Err.Description, True, False)
        '    End Function

    End Class
End Namespace
