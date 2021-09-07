Option Strict On
Option Explicit On

Imports Contensive.Addons.Reporting.Controllers
Imports Contensive.BaseClasses

Namespace Views
    '
    '====================================================================================================
    '
    Public Class PageViewsClass
        Inherits AddonBaseClass
        '
        '====================================================================================================
        '
        Public Overrides Function Execute(ByVal CP As CPBaseClass) As Object
            Dim result As String = ""
            Try
                ' -- initialize application. If authentication needed and not login page, pass true
                Using ac As New applicationController(CP, False)
                    Dim StartDate As DateTime = ac.cp.Doc.GetDate("filterFromDate")
                    Dim EndDate As DateTime = ac.cp.Doc.GetDate("filterToDate")
                    'if no date values, then default to last year
                    If (StartDate <= DateTime.MinValue) Or (EndDate <= DateTime.MinValue) Then
                        EndDate = Now.Date
                        StartDate = EndDate.AddDays(-365).Date
                    End If

                    'html for the date select. needs to include hidden addonguid so the old legacy way works
                    result = "<form><div class='afwBodyColor'><h3>Filters</h3><div class='abFilterRow'><input type='hidden' name='addonguid' id='addonguid' value='{" & constants.pageViewGuid & "}'>"
                    result &= "<label>From</label><input type='date' value='" & StartDate.ToString("yyyy-MM-dd") & "' name='filterFromDate' id='abFilterFromDate' class='abFilterDate' required></div> "
                    result &= "<div class='abFilterRow'><label>To</label><input type='date' name='filterToDate' id='abFilterToDate' class='abFilterDate' value='" & EndDate.ToString("yyyy-MM-dd") & "' required></div>"
                    result &= "<div class='abFilterRow'><label></label><button type='submit'>Submit</button></div><br></form>"

                    Dim Width As String = ac.cp.Doc.GetText("Width")
                    Dim Height As String = ac.cp.Doc.GetText("Height")
                    Dim durationHours As Integer = 24
                    Dim DivName As String = ac.cp.Doc.GetText("TargetDiv")
                    If DivName = "" Then
                        DivName = "PageViewChart"
                    End If
                    Dim dblDateStart As Double = StartDate.ToOADate()
                    Dim dblDateEnd As Double = EndDate.ToOADate()
                    'set the visit summary criteria
                    Dim criteria As String = "(TimeDuration=" & durationHours & ") AND (DateNumber>=" & dblDateStart & ") AND (DateNumber<" & dblDateEnd & ")"
                    Dim visitSummaryList As List(Of Models.visitSummaryModel) = Models.visitSummaryModel.createList(ac.cp, criteria, "TimeNumber desc")
                    If (visitSummaryList.Count = 0) Then
                        result &= "<span class=""ccError"">There is currently no data collected to display this chart. Please check back later.</span>"
                    Else
                        'legacy code uses isvisitdata=false
                        result &= Models.ChartViewModel.getChart(ac, visitSummaryList, DivName, False, Width, Height, (durationHours = 1))
                    End If
                End Using
            Catch ex As Exception
                CP.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
        ''
        ''====================================================================================================
        ''
        'Public Function GetContent(OptionString As String) As String
        '    Dim Stream As String
        '    Dim CS As Integer
        '    Dim cmn As New CommonClass
        '    Dim dblDateStart As Double
        '    Dim dblDateEnd As Double
        '    Dim DateStart As Date
        '    Dim DateEnd As Date
        '    DateStart = KmaEncodeDate(ae.cp.doc.gettext("StartDate"))
        '    DateEnd = KmaEncodeDate(ae.cp.doc.gettext("EndDate"))
        '    If IsDate(DateStart) And IsDate(DateEnd) Then
        '        dblDateStart = CDbl(DateStart)
        '        dblDateEnd = CDbl(DateEnd)
        '        CS = Main.OpenCSContent("Visit Summary", "(TimeDuration=24) AND (DateNumber>=" & dblDateStart & ") AND (DateNumber<=" & dblDateEnd & ")", "TimeNumber desc", , , , "DateNumber,TimeNumber,PagesViewed")
        '        If Main.CSOK(CS) Then
        '            Stream = cmn.GetChart(Main, CS, "page-views", False, "100%", "400px")
        '        Else
        '            Stream = "<span class=""ccError"">There is currently no data collected to display this chart. Please check back later.</span>"
        '        End If
        '        Call Main.CloseCS(CS)
        '    Else
        '        Stream = Main.GetAdminHintWrapper("Please enter a valid Start and End Date to view the Page Views Chart.")
        '    End If
        'End Function
    End Class
End Namespace