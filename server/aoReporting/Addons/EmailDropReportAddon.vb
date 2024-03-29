
Imports Contensive.Addons.PortalFramework
Imports Contensive.BaseClasses
Imports Contensive.ReportingVb.Controllers

Namespace Contensive.ReportingVb

    '
    Public Class EmailDropReportAddon
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
                If srcFormId > 0 Then
                    dstFormId = processForm(CP, srcFormId, frameRqs, rightNow)
                End If
                '
                ' -- workaround for a formset that only has one form.
                If (dstFormId = 0) Then dstFormId = formIdDefault
                '
                ' -- get the next form
                returnHtml = getForm(CP, dstFormId, frameRqs, rightNow)
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
                        cp.Response.Redirect("?")
                    Case Else
                        '
                        ' -- the default action
                        nextFormId = formIdDefault
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
        Friend Function getForm(ByVal cp As CPBaseClass, ByVal dstFormId As Integer, ByVal frameRqs As String, ByVal rightNow As Date) As String
            Dim result As String = ""
            Dim hint As String = "1"
            Try
                Dim rqs As String = cp.Utils.ModifyQueryString(frameRqs, rnDstFormId, dstFormId.ToString())
                '
                ' -- initialize report
                Dim report = New ReportListClass() With {
                    .title = "Email Drop Report",
                    .refreshQueryString = rqs,
                    .addCsvDownloadCurrentPage = True,
                    .isOuterContainer = True
                }
                cp.Doc.AddHeadStyle(report.styleSheet)
                '
                ' -- add buttons
                report.addFormButton(buttonCancel)
                report.addFormButton(ButtonRefresh)
                '
                ' -- add src view
                report.addFormHidden(rnSrcFormId, dstFormId.ToString())
                '
                ' -- filterDateFrom
                Const userPropertyFromDate As String = "ReportEmailDrop-filterFromDate"
                Dim filterFromDate As Date
                Dim filterFromDateString As String
                Dim filterFromDateStringTest As String = cp.Doc.GetText("filterFromDate")
                Dim userFilterFromDateString As String = cp.User.GetText(userPropertyFromDate)
                If (Not String.IsNullOrEmpty(filterFromDateStringTest)) Then
                    filterFromDate = genericController.encodeMinDate(cp.Doc.GetDate("filterFromDate"))
                ElseIf (Not String.IsNullOrEmpty(userFilterFromDateString)) Then
                    filterFromDate = genericController.encodeMinDate(cp.Utils.EncodeDate(userFilterFromDateString))
                Else
                    filterFromDate = Today.AddDays(-30)
                End If
                cp.User.SetProperty(userPropertyFromDate, filterFromDate.ToString())
                If filterFromDate = Date.MinValue Then
                    filterFromDateString = ""
                Else
                    filterFromDateString = filterFromDate.ToShortDateString
                End If
                '
                ' -- filterDateTo
                Const userPropertyToDate As String = "ReportEmailDrop-filterToDate"
                Dim filterToDate As Date
                Dim filterToDateString As String
                Dim filterToDateStringTest As String = cp.Doc.GetText("filterToDate")
                Dim userFilterToDateString As String = cp.User.GetText(userPropertyToDate)
                If (Not String.IsNullOrEmpty(filterToDateStringTest)) Then
                    filterToDate = genericController.encodeMinDate(cp.Doc.GetDate("filterToDate"))
                ElseIf (Not String.IsNullOrEmpty(userFilterToDateString)) Then
                    filterFromDate = genericController.encodeMinDate(cp.Utils.EncodeDate(userFilterFromDateString))
                Else
                    filterToDate = Today
                End If
                cp.User.SetProperty(userPropertyToDate, filterToDate.ToString())
                If filterToDate = Date.MinValue Then
                    filterToDateString = ""
                Else
                    filterToDateString = filterToDate.ToShortDateString
                End If
                '
                ' -- create caption with filter text
                Dim captionWithFilter As String = "This report summarizes data from the Email Log and Email Drops and includes emails sent from all Group Emails"
                If filterFromDateString <> "" And filterToDateString <> "" Then
                    captionWithFilter &= ", between " & filterFromDateString & " and " & filterToDateString & " inclusive"
                ElseIf filterFromDateString <> "" Then
                    captionWithFilter &= ", on or after " & filterFromDateString
                ElseIf filterToDateString <> "" Then
                    captionWithFilter &= ", on or before " & filterToDateString
                End If
                captionWithFilter &= "."
                report.description = captionWithFilter
                '
                hint = "setup columns"
                report.columnCaption = "Row"
                report.columnCaptionClass = "afwWidth20px afwTextAlignCenter"
                report.columnCellClass = "afwTextAlignCenter"
                '
                report.addColumn()
                report.columnCaption = "Drop Date"
                report.columnCaptionClass = "afwWidth200px afwTextAlignLeft"
                report.columnCellClass = "afwTextAlignLeft"
                '
                report.addColumn()
                report.columnCaption = "Email"
                report.columnCaptionClass = "afwTextAlignLeft"
                report.columnCellClass = "afwTextAlignLeft"
                '
                report.addColumn()
                report.columnCaption = "Sent"
                report.columnCaptionClass = "afwWidth50px afwTextAlignRight"
                report.columnCellClass = "afwTextAlignRight"
                '
                report.addColumn()
                report.columnCaption = "Opened"
                report.columnCaptionClass = "afwWidth50px afwTextAlignRight"
                report.columnCellClass = "afwTextAlignRight"
                '
                report.addColumn()
                report.columnCaption = "Clicked"
                report.columnCaptionClass = "afwWidth50px afwTextAlignRight"
                report.columnCellClass = "afwTextAlignRight"
                '
                ' -- run query and fill rows
                Dim sql As String = My.Resources.sqlReportEmailDrop
                If (filterFromDate = Date.MinValue) Then
                    sql = sql.Replace("{dateFrom}", cp.Db.EncodeSQLDate(New Date(1990, 1, 1)))
                Else
                    sql = sql.Replace("{dateFrom}", cp.Db.EncodeSQLDate(filterFromDate))
                End If
                If (filterToDate = Date.MinValue) Then
                    sql = sql.Replace("{dateTo}", cp.Db.EncodeSQLDate(Now.AddDays(1)))
                Else
                    sql = sql.Replace("{dateTo}", cp.Db.EncodeSQLDate(filterToDate))
                End If
                Using cs As CPCSBaseClass = cp.CSNew
                    If (cs.OpenSQL(sql)) Then
                        Dim rowPtr As Integer = 1
                        Do
                            Dim emaildropid As Integer = cs.GetInteger("dropid")
                            Dim openedCount As String = If(cs.GetInteger("opened").Equals(0), "0", "<a href=""?addonguid=%7BF4EE3D38-E0A9-4C93-9906-809F524B9690%7D&emaildropid=" & emaildropid.ToString() & """>" & cs.GetInteger("opened").ToString() & "</a>")
                            Dim clickedCount As String = If(cs.GetInteger("clicked").Equals(0), "0", "<a href=""?addonguid=%7B29271653-BDE3-4DC1-8058-D54E53F1D06B%7D&emaildropid=" & emaildropid.ToString() & """>" & cs.GetInteger("clicked").ToString() & "</a>")
                            report.addRow()
                            report.setCell(rowPtr.ToString())
                            report.setCell(cs.GetDate("dropDate").ToString())
                            report.setCell(cs.GetText("emailName"))
                            report.setCell(cs.GetInteger("sent").ToString())
                            report.setCell(openedCount)
                            report.setCell(clickedCount)
                            rowPtr += 1
                            cs.GoNext()
                        Loop While (cs.OK)
                    End If
                End Using
                '
                ' -- filters
                report.htmlLeftOfTable = "" _
                    & cr & "<h3 class=""abFilterHead"">Filters</h3>" _
                    & cr & "<div class=""abFilterRow""><label for""abFilterFromDate"">From</label>" & cp.Html.InputText("filterFromDate", filterFromDateString, 100, "abFilterDate", "abFilterFromDate") & "<a href=""#"" id=""abFilterFromDateClear"">X</a></div>" _
                    & cr & "<div class=""abFilterRow""><label for""abFilterToDate"">To</label>" & cp.Html.InputText("filterToDate", filterToDateString, 100, "abFilterDate", "abFilterToDate") & "<a href=""#"" id=""abFilterToDateClear"">X</a></div>" _
                    & ""
                result = cp.Html.div(report.getHtml(cp), "", "abReportEmailDrop")
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace