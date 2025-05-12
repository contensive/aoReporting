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
                layoutBuilder.addCsvDownloadCurrentPage = true;
                layoutBuilder.isOuterContainer = true;
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
                layoutBuilder.columnCaptionClass = "afwWidth20px afwTextAlignCenter";
                layoutBuilder.columnCellClass = "";
                // 
                layoutBuilder.addColumn();
                layoutBuilder.columnCaption = "User";
                layoutBuilder.columnCaptionClass = "afwTextAlignLeft";
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
                string criteria = "(1=1)";
                int visitId = cp.Request.GetInteger("visitid");
                if (visitId > 0) {
                    criteria += $"and(visitid={visitId})";
                }
                //
                string sql = $"select * from ccviewings where {criteria} order by id";
                // 
                int itemsCid = cp.Content.GetID("items");
                string rqs = cp.Doc.RefreshQueryString;
                string editItemHref = "?af=4&cid=" + itemsCid + "&id=";
                // 
                int rowPtr = 0;
                using (CPCSBaseClass csList = cp.CSNew()) {
                    if (csList.OpenSQL(sql)) {
                        do {
                            int itemId = csList.GetInteger("id");
                            layoutBuilder.addRow();
                            // 
                            // -- row number and controls
                            layoutBuilder.addRowEllipseMenuItem("Edit", editItemHref + itemId.ToString());
                            layoutBuilder.setCell((rowPtr + 1).ToString());
                            // 
                            // -- checkbox
                            string rowSelect = cp.Html.CheckBox("row" + rowPtr, false, "abSelectItemCheckbox");
                            rowSelect += cp.Html5.Hidden("rowId" + rowPtr, itemId);
                            layoutBuilder.setCell(rowSelect);
                            // 
                            // -- id
                            layoutBuilder.setCell("<a href=\"" + editItemHref + itemId + "\">" + itemId.ToString() + "</a>");
                            // layoutBuilder.setCell(itemId.ToString)
                            // 
                            // -- user
                            layoutBuilder.setCell(csList.GetText("memberid"));
                            // 
                            // -- 
                            layoutBuilder.setCell(csList.GetText("host"));
                            // 
                            // -- 
                            layoutBuilder.setCell(csList.GetText("path") + csList.GetText("page"));
                            // 
                            // -- 
                            layoutBuilder.setCell(csList.GetText("querystring"));
                            // 
                            // -- 
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
                layoutBuilder.htmlLeftOfBody = ""
                    + "<h3 class=\"abFilterHead\">Filters</h3>"
                    + "<h4 class=\"abFilterCaption\">VisitId</h3>"
                    + "<div class=\"abFilterRow\">" + cp.Html.InputText("visitid", visitId.ToString(), 8, "abFilterText", "abFilterInvoice") + "<a href=\"#\" id=\"abFilterInvoiceClear\">X</a></div>"
                    + "";
                layoutBuilder.addFormButton(Constants.ButtonRefresh);
                layoutBuilder.addFormHidden("rowCnt", rowPtr.ToString());
                string jqueryAddButtonEvent = ""
                    + "jQuery('.abItemAddButton').click(function(){"
                    + "window.location='" + cp.Site.GetText("adminUrl") + "?af=4&id=0&cid=" + cp.Content.GetID("items") + "';"
                    + "return false;"
                    + "});";
                cp.Doc.AddHeadJavascript("document.addEventListener(\"DOMContentLoaded\", function(event) {" + jqueryAddButtonEvent + "});");
                cp.Doc.AddHeadStyle(" .abAccountListAlertCustom { background-color: " + cp.Site.GetText("abActivatedAlertsBackgroundColor", "#ffff00") + " !important; color: " + cp.Site.GetText("abActivatedAlertsFontColor", "#000000") + " !important; } ");
                // 
                return layoutBuilder.getHtml();
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}
