using Contensive.Addons.PortalFramework;
using Contensive.BaseClasses;
using Contensive.Reporting.Controllers;
using System;

namespace Contensive.Reporting {

    // 
    public class LibraryFileDownloadReportAddon : AddonBaseClass {
        // 
        // =====================================================================================
        // 
        public override object Execute(BaseClasses.CPBaseClass CP) {
            try {
                DateTime rightNow = DateTime.Now;
                // -- if report method includes multiple forms, this the form submitted
                //int thisFormid = Constants.formIdDefault;
                // -- the Refresh Query String orinally sent to the page
                // -- this query string will refresh the entire page
                string frameRqs = CP.Doc.RefreshQueryString;
                // Dim frameRqs As String = CP.Doc.GetText(rnFrameRqs)
                // -- dst form is the one to be presented next, can be forced if src form is missing
                int dstFormId = CP.Doc.GetInteger("dstFormId");
                // -- src form is the form being submitted
                int srcFormId = CP.Doc.GetInteger("srcFormId");
                if (srcFormId > 0)
                    dstFormId = processForm(CP, srcFormId, frameRqs, rightNow);
                // 
                // -- workaround for a formset that only has one form.
                if ((dstFormId == 0))
                    dstFormId = Constants.formIdDefault;
                // 
                // -- get the next form
                return getForm(CP, dstFormId, frameRqs, rightNow);
            } catch (Exception ex) {
                CP.Site.ErrorReport(ex, "Exception in adminAccountsClass.execute()");
                throw;
            }
        }
        // 
        // ====================================================================================================
        /// <summary>
        ///         ''' process the src form (form being submitted) and return the dst form (form to display)
        ///         ''' </summary>
        ///         ''' <param name="cp"></param>
        ///         ''' <param name="srcFormId"></param>
        ///         ''' <param name="frameRqs"></param>
        ///         ''' <param name="rightNow"></param>
        ///         ''' <returns></returns>
        internal int processForm(CPBaseClass cp, int srcFormId, string frameRqs, DateTime rightNow) {
            int nextFormId = srcFormId;
            try {
                string button = cp.Doc.GetText(Constants.rnButton);
                switch (button) {
                    case   Constants.buttonCancel: {
                            cp.Response.Redirect("?");
                            break;
                        }

                    default: {
                            // 
                            // -- the default action
                            nextFormId = Constants.formIdDefault;
                            break;
                        }
                }
                return nextFormId;
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                return srcFormId;
            }
        }
        // 
        // ====================================================================================================
        /// <summary>
        ///         ''' return the form html
        ///         ''' </summary>
        ///         ''' <param name="cp"></param>
        ///         ''' <param name="dstFormId"></param>
        ///         ''' <param name="frameRqs"></param>
        ///         ''' <param name="rightNow"></param>
        ///         ''' <returns></returns>
        internal string getForm(CPBaseClass cp, int dstFormId, string frameRqs, DateTime rightNow) {
            string result = "";
            try {
                // 
                string rqs = cp.Utils.ModifyQueryString(frameRqs, Constants.rnDstFormId, dstFormId.ToString());
                string qsBase = "";
                // 
                var report = new ReportListClass() {
                    title = "Library File Download Report",
                    refreshQueryString = rqs,
                    addCsvDownloadCurrentPage = true,
                    isOuterContainer = true
                };
                report.addFormButton(Constants.buttonCancel);
                report.addFormButton(Constants.ButtonRefresh);
                report.addFormHidden(Constants.rnSrcFormId, dstFormId.ToString());
                // 
                // -- filterDateFrom
                const string userPropertyFromDate = "ReportLibraryFileDownloadLog-filterFromDate";
                DateTime filterFromDate;
                string filterFromDateString;
                string filterFromDateStringTest = cp.Doc.GetText("filterFromDate");
                string userFilterFromDateString = cp.User.GetText(userPropertyFromDate);
                if ((!string.IsNullOrEmpty(filterFromDateStringTest)))
                    filterFromDate = GenericController.encodeMinDate(cp.Doc.GetDate("filterFromDate"));
                else if ((!string.IsNullOrEmpty(userFilterFromDateString)))
                    filterFromDate = GenericController.encodeMinDate(cp.Utils.EncodeDate(userFilterFromDateString));
                else
                    filterFromDate = DateTime.Today.AddDays(-30);
                cp.User.SetProperty(userPropertyFromDate, filterFromDate.ToString());
                if (filterFromDate == DateTime.MinValue)
                    filterFromDateString = "";
                else
                    filterFromDateString = filterFromDate.ToShortDateString();
                // 
                // -- filterDateTo
                const string userPropertyToDate = "ReportLibraryFileDownloadLog-filterToDate";
                DateTime filterToDate;
                string filterToDateString;
                string filterToDateStringTest = cp.Doc.GetText("filterToDate");
                string userFilterToDateString = cp.User.GetText(userPropertyToDate);
                if ((!string.IsNullOrEmpty(filterToDateStringTest)))
                    filterToDate = GenericController.encodeMinDate(cp.Doc.GetDate("filterToDate"));
                else if ((!string.IsNullOrEmpty(userFilterToDateString)))
                    filterToDate = GenericController.encodeMinDate(cp.User.GetDate("abFilterTopBuyerToDate"));
                else
                    filterToDate = DateTime.Today.AddDays(-30);
                cp.User.SetProperty(userPropertyToDate, filterToDate.ToString());
                if (filterToDate == DateTime.MinValue)
                    filterToDateString = "";
                else
                    filterToDateString = filterToDate.ToShortDateString();
                // 
                // -- create caption with filter text
                string captionWithFilter = "Library File Downloads";
                if (filterFromDateString != "" & filterToDateString != "")
                    captionWithFilter += ", between " + filterFromDateString + " and " + filterToDateString + " inclusive";
                else if (filterFromDateString != "")
                    captionWithFilter += ", on or after " + filterFromDateString;
                else if (filterToDateString != "")
                    captionWithFilter += ", on or before " + filterToDateString;
                // 
                captionWithFilter += ". This report includes links created with the text editor, or created manually with /downloadLibraryFile?download={guid}. ";
                // 
                report.columnCaption = "Row";
                report.columnCaptionClass = "afwWidth20px afwTextAlignCenter";
                report.columnCellClass = "afwTextAlignCenter";
                // 
                report.addColumn();
                report.columnCaption = "Count";
                report.columnCaptionClass = "afwWidth100px afwTextAlignRight";
                report.columnCellClass = "afwTextAlignRight";
                // 
                report.addColumn();
                report.columnCaption = "Library File";
                report.columnCaptionClass = "afwTextAlignLeft";
                report.columnCellClass = "afwTextAlignLeft";
                // 
                string sql = Properties.Resources.sqlReportLibraryFileDownload;
                if ((filterFromDate == DateTime.MinValue))
                    sql = sql.Replace("{dateFrom}", cp.Db.EncodeSQLDate(new DateTime(1990, 1, 1)));
                else
                    sql = sql.Replace("{dateFrom}", cp.Db.EncodeSQLDate(filterFromDate));
                if ((filterToDate == DateTime.MinValue))
                    sql = sql.Replace("{dateTo}", cp.Db.EncodeSQLDate(DateTime.Now.AddDays(1)));
                else
                    sql = sql.Replace("{dateTo}", cp.Db.EncodeSQLDate(filterToDate));
                CPCSBaseClass cs = cp.CSNew();
                cs.OpenSQL(sql);
                qsBase = frameRqs;
                // qsBase = cp.Utils.ModifyQueryString(qsBase, rnDstFeatureGuid, "{FB35BD57-875C-44C6-84D9-541793BF9190}")
                // qsBase = cp.Utils.ModifyQueryString(qsBase, rnDstFormId, formIdAccountDetails.ToString())
                int rowPtr = 1;
                while ((cs.OK())) {
                    report.addRow();
                    report.setCell(rowPtr.ToString());
                    report.setCell(cs.GetInteger("cnt").ToString());
                    report.setCell(cs.GetText("name"));
                    rowPtr += 1;
                    cs.GoNext();
                }
                report.htmlLeftOfTable = ""
                     + "<h3 class=\"abFilterHead\">Filters</h3>"
                     + "<div class=\"abFilterRow\"><label for\"abFilterFromDate\">From</label>" + cp.Html.InputText("filterFromDate", filterFromDateString, 100, "abFilterDate", "abFilterFromDate") + "<a href=\"#\" id=\"abFilterFromDateClear\">X</a></div>"
                     + "<div class=\"abFilterRow\"><label for\"abFilterToDate\">To</label>" + cp.Html.InputText("filterToDate", filterToDateString, 100, "abFilterDate", "abFilterToDate") + "<a href=\"#\" id=\"abFilterToDateClear\">X</a></div>"
                    + "";
                report.description = captionWithFilter;
                result = report.getHtml(cp);
                result = cp.Html.div(result, "", "abReportFileDownload");
                cp.Doc.AddHeadStyle(report.styleSheet);
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
