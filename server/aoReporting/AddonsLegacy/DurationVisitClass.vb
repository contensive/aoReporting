
Imports System.Text
Imports Contensive.BaseClasses
Imports Contensive.Reporting.Models

Namespace Contensive.Reporting
    '
    '====================================================================================================
    '
    Public Class DurationVisitClass
        Inherits AddonBaseClass
        '
        '=====================================================================================
        Public Overrides Function Execute(ByVal CP As CPBaseClass) As Object
            Dim result As New StringBuilder()
            Dim sw As New Stopwatch : sw.Start()
            Try
                Using ac As New ApplicationModel(CP, False)
                    Dim Width As String = ac.cp.Doc.GetText("Chart Width", "100%")
                    Dim Height As String = ac.cp.Doc.GetText("Chart Height", "400px")
                    Dim Rate As String = ac.cp.Doc.GetText("Rate", "hourly")
                    Dim intRate As Integer = 24
                    Dim AllowHourly As Boolean = False
                    If Rate.ToLower() = "hourly" Then
                        intRate = 1
                        AllowHourly = True
                    End If
                    Dim Duration As Integer = ac.cp.Doc.GetInteger("Duration", 365)
                    Dim DivName As String = ac.cp.Doc.GetText("Target Div", "")
                    If DivName = "" Then
                        DivName = ac.cp.Doc.GetText("TargetDiv", "durationChart")
                    End If
                    Dim DateEnd As Date = Now().Date
                    Dim DateStart As Date = DateEnd.AddDays(-Duration).Date
                    Dim cacheName As String = "DurationVisit-" & Width & "-" & Height & "-" & DivName & "-" & CStr(AllowHourly) & "-" & intRate & "-" & Duration
                    Dim cacheValue As String = ac.cp.Cache.GetText(cacheName)
                    If (String.IsNullOrEmpty(cacheValue)) Then
                        If IsDate(DateStart) And IsDate(DateEnd) Then
                            Dim intDateStart As Integer = CP.Utils.EncodeInteger(DateStart.ToOADate())
                            Dim intDateEnd As Integer = CP.Utils.EncodeInteger(DateEnd.ToOADate())
                            Dim criteria As String = "(TimeDuration=" & intRate & ") AND (DateNumber>=" & intDateStart & ") AND (DateNumber<=" & intDateEnd & ")"
                            Dim visitSummaryList As List(Of Models.visitSummaryModel) = Models.visitSummaryModel.createList(ac.cp, criteria, "DateNumber, TimeNumber")
                            If (visitSummaryList.Count = 0) Then
                                result.Append("<span class=""ccError"">There is currently no data collected to display this chart. Please check back later.</span>")
                            Else
                                result.Append(Models.ChartViewModel.GetChart2(ac, visitSummaryList, DivName, True, Width, Height, AllowHourly))
                                result.Append(getSummary2(ac, visitSummaryList, AllowHourly))
                            End If
                            ac.cp.Cache.Store(cacheName, cacheValue)
                        Else
                            result.Append("<span class=""ccError"">Please enter a valid Start and End Date to view the Visit Chart.</span>")
                        End If
                    Else
                        result.Append(cacheValue)
                    End If
                End Using
            Catch ex As Exception
                CP.Site.ErrorReport(ex)
            End Try
            Return result.ToString()
        End Function
        '
        '====================================================================================================
        '
        Public Function getSummary2(ac As ApplicationModel, visitSummaryList As List(Of Models.visitSummaryModel), IsHourlyChart As Boolean) As String
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
