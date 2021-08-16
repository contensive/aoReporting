
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
            Dim sw As New Stopwatch : sw.Start()
            Try
                '
                ' -- initialize application. If authentication needed and not login page, pass true
                Using ac As New applicationController(CP, False)
                    Dim StartDate As DateTime = ac.cp.Doc.GetDate("StartDate")
                    Dim EndDate As DateTime = ac.cp.Doc.GetDate("EndDate")

                    If (StartDate <= DateTime.MinValue) Or (EndDate <= DateTime.MinValue) Then
                        ' result = "<div><h3 class='abFilterHead'>Filters</h3><div class='abFilterRow'><label for='' abfilterfromdate=''>From</label> "
                        ' result &= "<input type='Text' value='' name='filterFromDate' id='abFilterFromDate' class='abFilterDate hasDatepicker' maxlength='100' size='20'><a href='#' id='abFilterFromDateClear'>X</a></div> "
                        ' result &= "<div class='abFilterRow'><label for='' abfiltertodate=''>To</label><input type='Text' name='filterToDate' id='abFilterToDate' class='abFilterDate hasDatepicker' maxlength='100' size='20'> "
                        ' result &= " <a href='#' id='abFilterToDateClear'>X</a></div></div> "
                        Return result
                    End If



                    Dim Width As String = ac.cp.Doc.GetText("Width")
                    Dim Height As String = ac.cp.Doc.GetText("Height")
                    Dim durationHours As Integer = 24
                    ' Dim Rate As String = ac.cp.Doc.GetText("Rate")
                    ' If Rate.ToLower = "hourly" Then
                    ' durationHours = 1
                    ' End If
                    ' Dim DurationDays As Integer = ac.cp.Doc.GetInteger("Duration", 365)
                    Dim DivName As String = ac.cp.Doc.GetText("TargetDiv")
                    If DivName = "" Then
                        DivName = "PageViewChart"
                    End If
                    ' Dim DateEnd As Date = Now.Date
                    '  Dim DateStart As Date = DateEnd.AddDays(-DurationDays).Date
                    Dim dblDateStart As Double = StartDate.ToOADate()
                    Dim dblDateEnd As Double = EndDate.ToOADate()

                    Dim criteria As String = "(TimeDuration=" & durationHours & ") AND (DateNumber>=" & dblDateStart & ") AND (DateNumber<" & dblDateEnd & ")"
                    Dim visitSummaryList As List(Of Models.visitSummaryModel) = Models.visitSummaryModel.createList(ac.cp, criteria, "TimeNumber desc")
                    If (visitSummaryList.Count = 0) Then
                        result = "<span class=""ccError"">There is currently no data collected to display this chart. Please check back later.</span>"
                    Else
                        result = Models.ChartViewModel.getChart(ac, visitSummaryList, DivName, True, Width, Height, (durationHours = 1))
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
