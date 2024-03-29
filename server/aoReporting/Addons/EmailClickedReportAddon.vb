﻿
Imports Contensive.Addons.PortalFramework
Imports Contensive.BaseClasses

Namespace Contensive.ReportingVb

    '
    Public Class EmailClickedReportAddon
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
                Dim frameRqs As String = CP.Doc.RefreshQueryString
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
                Dim report = New ReportListClass() With {
                    .title = "Email Clicked Report",
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
                '
                hint = "run query"
                Dim sqlcriteria As String = If(emaildropid = 0, "", " and l.EmailDropID=" & emaildropid & "")
                Dim sql As String = $"select distinct m.name, m.email, d.name as dropName from ccEmailLog l left join ccMembers m on m.id=l.memberid left join ccemaildrops d on d.id=l.emailDropId where (logtype=3) and (m.name is not null) {sqlcriteria} order by m.name asc"
                Dim cs As CPCSBaseClass = cp.CSNew
                cs.OpenSQL(sql)

                qsBase = frameRqs
                Dim rowPtr As Integer = 1
                Do While (cs.OK)
                    report.addRow()
                    report.setCell(rowPtr.ToString())
                    report.setCell(cs.GetText("dropName"))
                    report.setCell("Name: " & cs.GetText("name") & "<br>" & "Email: " & cs.GetText("email"))
                    rowPtr += 1
                    cs.GoNext()
                Loop
                cs.Close()
                '
                ' -- filters
                report.htmlLeftOfTable = "" _
                    & cr & "<h3 class=""abFilterHead"">Filters</h3>" _
                    & cr & "<div class=""abFilterRow""><div class=""form-group""><label for""abFilterEmailDropId"">Email Drop</label>" & cp.Html5.SelectContent("emailDropId", emaildropid, "Email Drops", "", "All Email Drops", "form-control", "abFilterEmailDripId") & "</div></div>" _
                    & ""
                report.description = "Emails clicked from an email drop."

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