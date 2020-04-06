

Option Strict On
    Option Explicit On

    Imports adminFramework
    Imports Contensive.Addons.Reporting.Controllers
    Imports Contensive.BaseClasses

Namespace Views

    '
    Public Class EmailClickedReportClass
        Inherits AddonBaseClass
        '
        '=====================================================================================
        '
        Public Overrides Function Execute(CP As BaseClasses.CPBaseClass) As Object
            Dim returnHtml As String = ""
            Try
                Dim rightNow As Date = Now
                ' -- if report method includes multiple forms, this the form submitted
                Dim thisFormid As Integer = formIdDefault
                ' -- the Refresh Query String orinally sent to the page
                ' -- this query string will refresh the entire page
                Dim frameRqs As String = CP.Doc.RefreshQueryString()
                ' Dim frameRqs As String = CP.Doc.GetText(rnFrameRqs)
                ' -- dst form is the one to be presented next, can be forced if src form is missing
                Dim dstFormId As Integer = CP.Doc.GetInteger("dstFormId")
                ' -- src form is the form being submitted
                Dim srcFormId As Integer = CP.Doc.GetInteger("srcFormId")
                Dim emaildropid As Integer = CP.Doc.GetInteger("emaildropid")
                If srcFormId > 0 Then
                    dstFormId = processForm(CP, srcFormId, frameRqs, rightNow)
                End If

                dstFormId = processForm(CP, srcFormId, frameRqs, rightNow)
                '
                ' -- workaround for a formset that only has one form.
                If (dstFormId = 0) Then dstFormId = formIdDefault
                '
                ' -- get the next form
                returnHtml = getForm(CP, dstFormId, frameRqs, rightNow, emaildropid)
            Catch ex As Exception
                CP.Site.ErrorReport(ex, "Exception in adminAccountsClass.execute()")
            End Try
            Return returnHtml
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' process the src form (form being submitted) and return the dst form (form to display)
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="srcFormId"></param>
        ''' <param name="frameRqs"></param>
        ''' <param name="rightNow"></param>
        ''' <returns></returns>
        Friend Function processForm(ByVal cp As CPBaseClass, ByVal srcFormId As Integer, ByVal frameRqs As String, ByVal rightNow As Date) As Integer
            Dim nextFormId As Integer = srcFormId
            Try
                Dim button As String = cp.Doc.GetText(rnButton)
                Select Case button
                    Case buttonCancel
                        cp.Response.Redirect("?addonguid=%7BA10B5F49-3147-4E32-9DCF-76D65CCFF9F1%7D")
                End Select
                Return nextFormId
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
                Return srcFormId
            End Try
        End Function
        '
        '====================================================================================================
        ''' <summary>
        ''' return the form html
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <param name="dstFormId"></param>
        ''' <param name="frameRqs"></param>
        ''' <param name="rightNow"></param>
        ''' <returns></returns>
        Friend Function getForm(ByVal cp As CPBaseClass, ByVal dstFormId As Integer, ByVal frameRqs As String, ByVal rightNow As Date, emaildropid As Integer) As String
            Dim result As String = ""
            Dim hint As String = "1"
            Try
                '
                Dim orderBy As String = ""
                Dim rqs As String = cp.Utils.ModifyQueryString(frameRqs, rnDstFormId, dstFormId.ToString())
                Dim qs As String = ""
                Dim qsBase As String = ""
                '
                Dim report = New ReportListClass(cp) With {
                    .title = "Email Clicked Report",
                    .name = "Email Clicked Report",
                    .guid = "{29271653-BDE3-4DC1-8058-D54E53F1D06B}",
                    .refreshQueryString = rqs,
                    .addCsvDownloadCurrentPage = True,
                    .isOuterContainer = True
                }
                report.addFormButton(buttonCancel)
                '
                hint = "setup columns"
                report.columnCaption = "Row"
                report.columnCaptionClass = "afwWidth20px afwTextAlignCenter"
                report.columnCellClass = "afwTextAlignCenter"
                '
                report.addColumn()
                report.columnCaption = "Email"
                report.columnCaptionClass = "afwWidth100px afwTextAlignLeft"
                report.columnCellClass = "afwTextAlignLeft"
                '
                report.addColumn()
                report.columnCaption = "Clicked By"
                report.columnCaptionClass = "afwTextAlignLeft"
                report.columnCellClass = "afwTextAlignLeft"


                Dim emailName As String = ""
                Dim emailNameSql As String = "select e.name from ccEmailDrops left join ccemail e on e.id = ccEmaildrops.EmailID where ccemaildrops.id=" & emaildropid
                Dim csName As CPCSBaseClass = cp.CSNew()
                If csName.OpenSQL(emailNameSql) Then
                    emailName = csName.GetText("name")
                End If

                hint = "run query"
                Dim sql As String = "select distinct m.name, m.email from ccEmailLog left join ccMembers m on m.id = ccemaillog.memberid where (logtype=3) and (m.name is not null) and ccemaillog.EmailDropID=" & emaildropid & " order by m.name asc"
                Dim cs As CPCSBaseClass = cp.CSNew
                cs.OpenSQL(sql)

                qsBase = frameRqs
                Dim rowPtr As Integer = 1
                Do While (cs.OK)
                    report.addRow()
                    report.setCell(rowPtr.ToString())
                    report.setCell(emailName)
                    report.setCell("Name: " & cs.GetText("name") & "<br>" & "Email: " & cs.GetText("email"))
                    rowPtr += 1
                    cs.GoNext()
                Loop
                cs.Close()
                report.description = ""
                result = report.getHtml(cp)
                result = cp.Html.div(result, "", "abReportEmailDrop")
                cp.Doc.AddHeadStyle(report.styleSheet)
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace