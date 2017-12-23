
Option Strict On
Option Explicit On

Imports Contensive.Addons.VisitCharts.Controllers
Imports Contensive.BaseClasses

Namespace Contensive.Addons.VisitCharts.Views
    '
    '====================================================================================================
    '
    Public Class WeeklyViewingsClass
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
        '    Dim dblDate As Double
        '    Dim cmn As New CommonClass
        '    dblDate = CDbl(Date)
        '    CS = Main.OpenCSContent("Visit Summary", "(TimeDuration=1) AND (DateNumber>=" & dblDate - 7 & ") AND (DateNumber<=" & dblDate & ")", "TimeNumber desc", , , , "DateNumber,TimeNumber,PagesViewed")
        '    If Main.CSOK(CS) Then
        '        Stream = cmn.GetChart(Main, CS, "weekly-visits", False, "100%", "400px")
        '    Else
        '        Stream = "<span class=""ccError"">There is currently no data collected to display this chart. Please check back later.</span>"
        '    End If
        '    Call Main.CloseCS(CS)
        '    GetContent = Stream
        'End Function
    End Class
End Namespace
