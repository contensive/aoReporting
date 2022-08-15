
Imports Contensive.BaseClasses
Imports Contensive.Reporting.Controllers
Imports Contensive.Reporting.Models

Namespace Contensive.Reporting
    Public Class SampleAddon
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
            Try
                Using ae As New ApplicationModel(CP, False)
                    '
                    ' -- your code
                    If ae.packageErrorList.Count > 0 Then
                        Return "Hey user, this happened - " & Join(ae.packageErrorList.ToArray, "<br>")
                    End If
                    Return "Hello World"
                End Using
            Catch ex As Exception
                CP.Site.ErrorReport(ex)
                Throw
            End Try
        End Function
    End Class
End Namespace
