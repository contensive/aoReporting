
Option Strict On
Option Explicit On

Imports Contensive.Addons.VisitCharts.Controllers
Imports Contensive.BaseClasses

Namespace Contensive.Addons.VisitCharts.Views

    '
    Public Class addonClass
        Inherits AddonBaseClass
        '
        ' - use NuGet to add Contentive.clib reference
        ' - Verify project root name space is empty
        ' - Change the namespace to the collection name
        ' - Change this class name to the addon name
        ' - Create a Contensive Addon record with the namespace apCollectionName.ad
        '
        '=====================================================================================
        ''' <summary>
        ''' AddonDescription
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
    End Class
End Namespace
