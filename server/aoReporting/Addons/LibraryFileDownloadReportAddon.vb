
Imports Contensive.Addons.PortalFramework
Imports Contensive.BaseClasses
Imports Contensive.ReportingVb.Controllers

Namespace Contensive.ReportingVb

    '
    Public Class LibraryFileDownloadReportAddon
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
                '
                Dim orderBy As String = ""
                Dim rqs As String = cp.Utils.ModifyQueryString(frameRqs, rnDstFormId, dstFormId.ToString())
                Dim qs As String = ""
                Dim qsBase As String = ""
                '
                Dim report = New ReportListClass() With {
                    .title = "Library File Download Report",
                    .refreshQueryString = rqs,
                    .addCsvDownloadCurrentPage = True,
                    .isOuterContainer = True
                }
                report.addFormButton(buttonCancel)
                report.addFormButton(ButtonRefresh)
                report.addFormHidden(rnSrcFormId, dstFormId.ToString())
                '
                ' -- filterDateFrom
                Const userPropertyFromDate As String = "ReportLibraryFileDownloadLog-filterFromDate"
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
                Const userPropertyToDate As String = "ReportLibraryFileDownloadLog-filterToDate"
                Dim filterToDate As Date
                Dim filterToDateString As String
                Dim filterToDateStringTest As String = cp.Doc.GetText("filterToDate")
                Dim userFilterToDateString As String = cp.User.GetText(userPropertyToDate)
                If (Not String.IsNullOrEmpty(filterToDateStringTest)) Then
                    filterToDate = genericController.encodeMinDate(cp.Doc.GetDate("filterToDate"))
                ElseIf (Not String.IsNullOrEmpty(userFilterToDateString)) Then
                    filterToDate = genericController.encodeMinDate(cp.User.GetDate("abFilterTopBuyerToDate"))
                Else
                    filterToDate = Today.AddDays(-30)
                End If
                cp.User.SetProperty(userPropertyToDate, filterToDate.ToString())
                If filterToDate = Date.MinValue Then
                    filterToDateString = ""
                Else
                    filterToDateString = filterToDate.ToShortDateString
                End If
                '
                ' -- create caption with filter text
                Dim captionWithFilter As String = "Library File Downloads"
                If filterFromDateString <> "" And filterToDateString <> "" Then
                    captionWithFilter &= ", between " & filterFromDateString & " and " & filterToDateString & " inclusive"
                ElseIf filterFromDateString <> "" Then
                    captionWithFilter &= ", on or after " & filterFromDateString
                ElseIf filterToDateString <> "" Then
                    captionWithFilter &= ", on or before " & filterToDateString
                End If
                '
                captionWithFilter &= ". This report includes links created with the text editor, or created manually with /downloadLibraryFile?download={guid}. "
                '
                hint = "setup columns"
                report.columnCaption = "Row"
                report.columnCaptionClass = "afwWidth20px afwTextAlignCenter"
                report.columnCellClass = "afwTextAlignCenter"
                '
                report.addColumn()
                report.columnCaption = "Count"
                report.columnCaptionClass = "afwWidth100px afwTextAlignRight"
                report.columnCellClass = "afwTextAlignRight"
                '
                report.addColumn()
                report.columnCaption = "Library File"
                report.columnCaptionClass = "afwTextAlignLeft"
                report.columnCellClass = "afwTextAlignLeft"
                '
                hint = "run query"
                Dim sql As String = My.Resources.sqlReportLibraryFileDownload
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
                Dim cs As CPCSBaseClass = cp.CSNew
                cs.OpenSQL(sql)
                qsBase = frameRqs
                'qsBase = cp.Utils.ModifyQueryString(qsBase, rnDstFeatureGuid, "{FB35BD57-875C-44C6-84D9-541793BF9190}")
                'qsBase = cp.Utils.ModifyQueryString(qsBase, rnDstFormId, formIdAccountDetails.ToString())
                Dim rowPtr As Integer = 1
                Do While (cs.OK)
                    report.addRow()
                    report.setCell(rowPtr.ToString())
                    report.setCell(cs.GetInteger("cnt").ToString())
                    report.setCell(cs.GetText("name"))
                    rowPtr += 1
                    cs.GoNext()
                Loop
                hint = "set filter text"
                report.htmlLeftOfTable = "" _
                    & cr & "<h3 class=""abFilterHead"">Filters</h3>" _
                    & cr & "<div class=""abFilterRow""><label for""abFilterFromDate"">From</label>" & cp.Html.InputText("filterFromDate", filterFromDateString, 100, "abFilterDate", "abFilterFromDate") & "<a href=""#"" id=""abFilterFromDateClear"">X</a></div>" _
                    & cr & "<div class=""abFilterRow""><label for""abFilterToDate"">To</label>" & cp.Html.InputText("filterToDate", filterToDateString, 100, "abFilterDate", "abFilterToDate") & "<a href=""#"" id=""abFilterToDateClear"">X</a></div>" _
                    & ""
                hint = "output body"
                report.description = captionWithFilter
                result = report.getHtml(cp)
                result = cp.Html.div(result, "", "abReportFileDownload")
                cp.Doc.AddHeadStyle(report.styleSheet)
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace