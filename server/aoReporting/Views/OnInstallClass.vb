
Option Strict On
Option Explicit On

Imports Contensive.Addons.Reporting.Controllers
Imports Contensive.BaseClasses

Namespace Views

    '
    Public Class OnInstallClass
        Inherits AddonBaseClass
        '
        '=====================================================================================
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="CP"></param>
        ''' <returns></returns>
        Public Overrides Function Execute(ByVal CP As CPBaseClass) As Object
            Dim result As String = ""
            Dim sw As New Stopwatch : sw.Start()
            Try
                '
                ' -- initialize application. If authentication needed and not login page, pass true
                Using ae As New applicationController(CP, False)
                    '
                    ' -- delete the old harcoded email drop report
                    CP.Db.ExecuteNonQuery("delete from ccmenuentries where linkpage='?af=12&rid=28'")
                    '
                    ' -- reset the EmailDropReport
                    Using cs As CPCSBaseClass = CP.CSNew()
                        Dim reportId As Integer = 0
                        If (cs.Open("Admin Framework Reports", "name='Email Drop Report'")) Then
                            reportId = cs.GetInteger("id")
                        End If
                        CP.Db.ExecuteNonQuery("delete from AdminFrameworkReports where id=" & reportId)
                        CP.Db.ExecuteNonQuery("delete from AdminFrameworkReportColumns where reportid=" & reportId)
                    End Using
                End Using
            Catch ex As Exception
                CP.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace
