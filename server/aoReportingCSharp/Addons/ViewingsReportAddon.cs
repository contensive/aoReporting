using Contensive.BaseClasses;
using System;

namespace Contensive.Reporting {
    //
    // ====================================================================================================
    //
    public class ViewingsReportAddon : AddonBaseClass {
        // 
        // ====================================================================================================
        // 
        public override object Execute(BaseClasses.CPBaseClass cp) {
            try {
                var layoutBuilder = cp.AdminUI.CreateLayoutBuilderList();
                layoutBuilder.title = "Viewings Report";
                layoutBuilder.isOuterContainer = true;
                layoutBuilder.callbackAddonGuid = "{0905279A-6EFB-4A10-96FE-90F243962F75}";
                //
                // -- add buttons
                layoutBuilder.addFormButton(Constants.ButtonRefresh);
                //
                // -- read filter values
                const string viewName = "ViewingsReport";
                int filterVisitId = layoutBuilder.getFilterInteger("visitid", viewName);
                //
                // -- add filter UI
                layoutBuilder.addFilterTextInput("Visit Id", "visitid", filterVisitId == 0 ? "" : filterVisitId.ToString());
                //
                layoutBuilder.htmlBeforeBody = "<style>table.afwListReportTable td { word-break: break-all; }</style>";
                //
                layoutBuilder.columnCaption = "Row";
                layoutBuilder.columnCaptionClass = "afwWidth20px afwTextAlignCenter";
                layoutBuilder.columnCellClass = "afwTextAlignCenter";
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnCaption = "<input type=\"checkbox\" id=\"abSelectAccountAllNone\">";
                layoutBuilder.columnCaptionClass = "afwWidth20px";
                layoutBuilder.columnCellClass = " afwTextAlignCenter";
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnCaption = "ID";
                layoutBuilder.columnCaptionClass = "afwWidth100px afwTextAlignCenter";
                layoutBuilder.columnCellClass = "";
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnCaption = "User";
                layoutBuilder.columnCaptionClass = "afwWidth100px afwTextAlignLeft";
                layoutBuilder.columnCellClass = "afwTextAlignLeft";
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnCaption = "Host";
                layoutBuilder.columnCaptionClass = "afwTextAlignLeft";
                layoutBuilder.columnCellClass = "afwTextAlignLeft";
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnCaption = "PathPage";
                layoutBuilder.columnCaptionClass = "afwTextAlignLeft";
                layoutBuilder.columnCellClass = "afwTextAlignLeft";
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnCaption = "QueryString";
                layoutBuilder.columnCaptionClass = "afwTextAlignLeft";
                layoutBuilder.columnCellClass = "afwTextAlignLeft";
                //
                layoutBuilder.addColumn();
                layoutBuilder.columnCaption = "Form";
                layoutBuilder.columnCaptionClass = "afwTextAlignLeft";
                layoutBuilder.columnCellClass = "afwTextAlignLeft";
                //
                // -- build where clause
                string criteria = "(1=1)";
                if (filterVisitId > 0) {
                    criteria += $" and (visitid={filterVisitId})";
                }
                //
                // -- get total record count for pagination
                using (CPCSBaseClass csCount = cp.CSNew()) {
                    if (csCount.OpenSQL($"select count(*) as cnt from ccviewings where {criteria}")) {
                        layoutBuilder.recordCount = csCount.GetInteger("cnt");
                    }
                }
                //
                // -- pagination offset
                int pageSize = layoutBuilder.paginationPageSize;
                int pageNumber = layoutBuilder.paginationPageNumber;
                int offset = (pageNumber - 1) * pageSize;
                //
                // -- sort order with default fallback
                string orderBy = string.IsNullOrEmpty(layoutBuilder.sqlOrderBy) ? "id desc" : layoutBuilder.sqlOrderBy;
                //
                // -- paginated query
                string sql = $"select * from ccviewings where {criteria} order by {orderBy} offset {offset} rows fetch next {pageSize} rows only";
                //
                int itemsCid = cp.Content.GetID("viewings");
                string editItemHref = $"?af=4&cid={itemsCid}&id=";
                //
                int rowPtr = 0;
                using (CPCSBaseClass csList = cp.CSNew()) {
                    if (csList.OpenSQL(sql)) {
                        do {
                            int itemId = csList.GetInteger("id");
                            layoutBuilder.addRow();
                            //
                            // -- row number and controls
                            layoutBuilder.addRowEllipseMenuItem("Edit", $"{editItemHref}{itemId}");
                            layoutBuilder.setCell((offset + rowPtr + 1).ToString());
                            //
                            // -- checkbox
                            string rowSelect = cp.Html.CheckBox($"row{rowPtr}", false, "abSelectItemCheckbox");
                            rowSelect += cp.Html5.Hidden($"rowId{rowPtr}", itemId);
                            layoutBuilder.setCell(rowSelect);
                            //
                            // -- id
                            layoutBuilder.setCell($"<a href=\"{editItemHref}{itemId}\">{itemId}</a>");
                            //
                            // -- user
                            layoutBuilder.setCell(csList.GetText("memberid"));
                            //
                            // -- host
                            layoutBuilder.setCell(csList.GetText("host"));
                            //
                            // -- pathpage
                            layoutBuilder.setCell(csList.GetText("path") + csList.GetText("page"));
                            //
                            // -- querystring
                            layoutBuilder.setCell(csList.GetText("querystring"));
                            //
                            // -- form
                            layoutBuilder.setCell(csList.GetText("form"));
                            //
                            rowPtr += 1;
                            csList.GoNext();
                        }
                        while (csList.OK());
                        csList.Close();
                    }
                }
                //
                layoutBuilder.addFormHidden("rowCnt", rowPtr.ToString());
                //
                return layoutBuilder.getHtml();
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}
