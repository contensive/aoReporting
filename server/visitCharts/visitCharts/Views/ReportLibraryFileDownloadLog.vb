
Option Strict On
Option Explicit On

Imports adminFramework
Imports Contensive.Addons.Reporting.Controllers
Imports Contensive.BaseClasses

Namespace Views

    '
    Public Class ReportLibraryFileDownloadLogClass
        Inherits AddonBaseClass
        '
        ' - use NuGet to add Contentive.clib reference
        ' - Verify project root name space is empty
        ' - Change the namespace to the collection name
        ' - Change this class name to the addon name
        ' - Create a Contensive Addon record with the namespace apCollectionName.ad
        '
        '=====================================================================================


        '====================================================================================================
        Public Overrides Function Execute(CP As BaseClasses.CPBaseClass) As Object
            Dim returnHtml As String = ""
            Try
                Dim dstFormId As Integer = CP.Doc.GetInteger("dstFormId")
                Dim srcFormId As Integer = CP.Doc.GetInteger("srcFormId")
                Dim frameRqs As String = CP.Doc.GetText(rnFrameRqs)
                Dim rightNow As Date = Now
                Dim thisFormid As Integer = formIdReportTopBuyers
                '
                If srcFormId > 0 Then
                    dstFormId = processForm(CP, thisFormid, frameRqs, rightNow)
                    If dstFormId = thisFormid Then
                        returnHtml = getForm(CP, thisFormid, frameRqs, rightNow)
                    End If
                Else
                    returnHtml = getForm(CP, thisFormid, frameRqs, rightNow)
                End If
            Catch ex As Exception
                CP.Site.ErrorReport(ex, "Exception in adminAccountsClass.execute()")
            End Try
            Return returnHtml
        End Function
        '====================================================================================================
        Friend Function processForm(ByVal cp As CPBaseClass, ByVal srcFormId As Integer, ByVal frameRqs As String, ByVal rightNow As Date) As Integer
            Dim nextFormId As Integer = srcFormId
            Try
                Dim button As String = cp.Doc.GetText(rnButton)
                '
                Select Case button
                    Case buttonCancel
                        nextFormId = formIdReportList
                End Select
            Catch ex As Exception
                '
                '
                '
                errorReport(ex, cp, "processForm")
            End Try
            Return nextFormId
        End Function
        '
        '
        '
        Friend Function getForm(ByVal cp As CPBaseClass, ByVal dstFormId As Integer, ByVal frameRqs As String, ByVal rightNow As Date) As String
            Dim body As String = ""
            Dim rowPtr As Integer = 1
            Dim hint As String = ""
            Try
                'Dim body As CPBlockBaseClass = cp.BlockNew()
                Dim report As reportListClass
                Dim cs As CPCSBaseClass = cp.CSNew
                'Dim accountId As Integer = cp.Utils.EncodeInteger(cp.Doc.GetProperty(rnAccountId))
                Dim adminUrl As String = ""
                Dim js As String = ""
                Dim accountName As String = ""
                Dim accountId As Integer = 0
                Dim sql As String = ""
                Dim columnSort As String = cp.Doc.GetText("columnSort")
                Dim orderBy As String = ""
                Dim rqs As String = frameRqs
                Dim reportDescription As String = ""
                Dim qs As String = ""
                Dim qsBase As String = ""
                Dim filterCategoryId As Integer = cp.Doc.GetInteger("filterCategoryId")
                '
                report = New reportListClass(cp)
                report.title = "Top Buyers"
                report.name = "Account Report Top Buyers"
                report.guid = "{F03B9917-657C-4FC6-BD7D-FEF81EBAB70A}"
                rqs = cp.Utils.ModifyQueryString(rqs, rnDstFormId, dstFormId.ToString())
                report.refreshQueryString = rqs
                reportDescription = "Top buyers"
                report.addCsvDownloadCurrentPage = True
                report.addFormButton(buttonCancel)
                report.addFormButton(ButtonRefresh)
                report.addFormHidden(rnSrcFormId, dstFormId.ToString())
                '
                ' dateCompleted
                '
                Dim filterFromDate As Date '= genericController.encodeDateMinValue(cp.Doc.GetDate("filterFromDate"))
                Dim filterFromDateString As String
                Dim filterFromDateStringTest As String = cp.Doc.GetText("filterFromDate")
                Dim userFilterFromDateString As String = cp.User.GetProperty("abFilterTopBuyerFromDate")
                If (Not String.IsNullOrEmpty(filterFromDateStringTest)) Then
                    filterFromDate = genericController.encodeMinDate(cp.Doc.GetDate("filterFromDate"))
                ElseIf (Not String.IsNullOrEmpty(userFilterFromDateString)) Then
                    filterFromDate = genericController.encodeMinDate(cp.User.GetDate("abFilterTopBuyerFromDate"))
                Else
                    filterFromDate = Today.AddDays(-30)
                End If
                cp.User.SetProperty("abFilterTopBuyerFromDate", filterFromDate.ToString())
                '
                If filterFromDate = Date.MinValue Then
                    filterFromDateString = ""
                Else
                    filterFromDateString = filterFromDate.ToShortDateString
                End If
                '
                Dim filterToDate As Date '= genericController.encodeDateMinValue(cp.Doc.GetDate("filterToDate"))
                Dim filterToDateString As String
                Dim filterToDateStringTest As String = cp.Doc.GetText("filterToDate")
                Dim userFilterToDateString As String = cp.User.GetProperty("abFilterTopBuyerToDate")
                If (Not String.IsNullOrEmpty(filterToDateStringTest)) Then
                    filterToDate = genericController.encodeMinDate(cp.Doc.GetDate("filterToDate"))
                ElseIf (Not String.IsNullOrEmpty(userFilterToDateString)) Then
                    filterToDate = genericController.encodeMinDate(cp.User.GetDate("abFilterTopBuyerToDate"))
                Else
                    filterToDate = Date.MinValue
                End If
                cp.User.SetProperty("abFilterTopBuyerToDate", filterToDate.ToString())
                '
                If filterToDate = Date.MinValue Then
                    filterToDateString = ""
                Else
                    filterToDateString = filterToDate.ToShortDateString
                End If
                '
                If filterFromDateString <> "" And filterToDateString <> "" Then
                    reportDescription &= ", purchases between " & filterFromDateString & " and " & filterToDateString & " inclusive"
                ElseIf filterFromDateString <> "" Then
                    reportDescription &= ", purchases on or after " & filterFromDateString
                ElseIf filterToDateString <> "" Then
                    reportDescription &= ", purchases on or before " & filterToDateString
                Else
                    'reportDescription &= ", purchases on record"
                End If
                If filterCategoryId <> 0 Then
                    reportDescription &= ", items in category " & cp.Content.GetRecordName("item categories", filterCategoryId)
                End If
                '
                reportDescription &= ". This amounts in this report represent item totals so they do not include shipping or tax. "
                '
                hint = "setup columns"
                report.columnCaption = "Row"
                report.columnCaptionClass = "afwWidth20px afwTextAlignCenter"
                report.columnCellClass = "afwTextAlignCenter"
                '
                report.addColumn()
                report.columnCaption = "Amount"
                report.columnCaptionClass = "afwWidth100px afwTextAlignRight"
                report.columnCellClass = "afwTextAlignRight"
                '
                report.addColumn()
                report.columnCaption = "Account"
                report.columnCaptionClass = "afwTextAlignLeft"
                report.columnCellClass = "afwTextAlignLeft"
                '
                hint = "run query"
                sql = "select sum(d.quantity*d.unitPrice) as amount,o.accountid,a.name as accountName" _
                    & " from (((abaccounts a" _
                    & " left join orders o on o.accountid=a.id)" _
                    & " left join orderdetails d on d.orderid=o.id)" _
                    & " left join items i on i.id=d.itemid)" _
                    & " where" _
                    & " (o.paidByTransactionId Is Not null)" _
                    & " and(o.dateCanceled is null)" _
                    & " and(o.paydate Is Not null)" _
                    & ""
                If filterFromDate > Date.MinValue Then
                    sql &= "and(o.payDate>=" & cp.Db.EncodeSQLDate(filterFromDate) & ")"
                End If
                If filterToDate > Date.MinValue Then
                    sql &= "and(o.payDate<" & cp.Db.EncodeSQLDate(filterToDate.AddDays(1)) & ")"
                End If
                If filterCategoryId <> 0 Then
                    sql &= "and(i.categoryid=" & filterCategoryId & ")"
                End If
                sql &= "" _
                    & " group by o.accountid,a.name" _
                    & " order by sum(d.quantity*d.unitPrice) desc" _
                    & ""
                cs.OpenSQL(sql)
                qsBase = frameRqs
                qsBase = cp.Utils.ModifyQueryString(qsBase, rnDstFeatureGuid, "{FB35BD57-875C-44C6-84D9-541793BF9190}")
                qsBase = cp.Utils.ModifyQueryString(qsBase, rnDstFormId, formIdAccountDetails.ToString())
                Do While (cs.OK)
                    hint = "get accountid, row " & rowPtr
                    accountId = cs.GetInteger("accountId")
                    If accountId <> 0 Then
                        hint = "adding data row " & rowPtr
                        qs = cp.Utils.ModifyQueryString(qsBase, rnAccountId, accountId.ToString())
                        report.addRow()
                        report.setCell(rowPtr.ToString())
                        report.setCell(FormatCurrency(cs.GetNumber("amount"), 2), cs.GetNumber("amount").ToString())
                        report.setCell("<a href=""?" & qs & """>" & cs.GetText("accountName") & "</a>", cs.GetText("accountName"))
                        rowPtr += 1
                    End If
                    cs.GoNext()
                Loop
                hint = "set filter text"
                report.htmlLeftOfTable = "" _
                    & cr & "<h3 class=""abFilterHead"">Filters</h3>" _
                    & cr & "<h4 class=""abFilterCaption"">Category</h4>" _
                    & cr & "<div class=""abFilterRow"">" & cp.Html.SelectContent("filterCategoryId", filterCategoryId.ToString(), "item Categories", , "Any Category", "abFilterCategory", "abFilterCategory") & "</div>" _
                    & cr & "<h4 class=""abFilterCaption"">Date</h4>" _
                    & cr & "<div class=""abFilterRow""><label for""abFilterFromDate"">From</label>" & cp.Html.InputText("filterFromDate", filterFromDateString, , , , "abFilterDate", "abFilterFromDate") & "<a href=""#"" id=""abFilterFromDateClear"">X</a></div>" _
                    & cr & "<div class=""abFilterRow""><label for""abFilterToDate"">To</label>" & cp.Html.InputText("filterToDate", filterToDateString, , , , "abFilterDate", "abFilterToDate") & "<a href=""#"" id=""abFilterToDateClear"">X</a></div>" _
                    & ""
                hint = "output body"
                report.description = reportDescription
                body = report.getHtml(cp)
                body = cp.Html.div(body, , , "abReportTopBuyers")
            Catch ex As Exception
                '
                '
                '
                errorReport(ex, cp, "getForm, hint=" & hint)
            End Try
            Return body
        End Function
        '
        '
        '
        Private Sub errorReport(ByVal ex As Exception, ByVal cp As CPBaseClass, ByVal method As String)
            cp.Site.ErrorReport(ex, "error in Contensive.Addons.aoAccountBilling.adminReportOrdersClass." & method)
        End Sub




    End Class
End Namespace