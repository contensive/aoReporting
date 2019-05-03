
Option Strict On
Option Explicit On

Imports Contensive.Addons.Reporting.Controllers
Imports Contensive.BaseClasses

Namespace Views
    '
    Public Class VisitsClass
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
                Using ae As New applicationController(CP, False)
                    '
                    ' -- your code
                    result = "Hello World"
                    If ae.packageErrorList.Count > 0 Then
                        result = "Hey user, this happened - " & Join(ae.packageErrorList.ToArray, "<br>")
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
        '        CS = Main.OpenCSContent("Visit Summary", "(TimeDuration=24) AND (DateNumber>=" & dblDateStart & ") AND (DateNumber<=" & dblDateEnd & ")", "TimeNumber desc", , , , "DateNumber,TimeNumber,Visits")
        '        If Main.CSOK(CS) Then
        '            Stream = cmn.GetChart(Main, CS, "visits", True, "100%", "400px")
        '        Else
        '            Stream = "<span class=""ccError"">There is currently no data collected to display this chart. Please check back later.</span>"
        '        End If
        '        Call Main.CloseCS(CS)
        '    Else
        '        Stream = Main.GetAdminHintWrapper("Please enter a valid Start and End Date to view the Visit Chart.")
        '    End If
        '    GetContent = Stream
        'End Function
    End Class
End Namespace
