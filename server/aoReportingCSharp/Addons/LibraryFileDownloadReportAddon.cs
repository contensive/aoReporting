using Contensive.BaseClasses;
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
                var layout = cp.AdminUI.CreateLayoutBuilderList();
                layout.title = "Library File Download Report";
                layout.addCsvDownloadCurrentPage = true;
                layout.isOuterContainer = true;

                layout.addFormButton(Constants.buttonCancel);
                layout.addFormButton(Constants.ButtonRefresh);
                layout.addFormHidden(Constants.rnSrcFormId, dstFormId.ToString());
                // 
                // -- read filter values
                const string viewName = "LibraryFileDownloadReport";
                DateTime? filterFromDateNullable = layout.getFilterDate("filterFromDate", viewName);
                DateTime? filterToDateNullable = layout.getFilterDate("filterToDate", viewName);
                //
                // -- default to last 30 days if no filter values
                DateTime filterFromDate = filterFromDateNullable ?? DateTime.Today.AddDays(-30);
                DateTime filterToDate = filterToDateNullable ?? DateTime.Today;
                //
                // -- add filter UI
                layout.addFilterDateInput("From", "filterFromDate", filterFromDate);
                layout.addFilterDateInput("To", "filterToDate", filterToDate);
                //
                // -- create caption with filter text
                string captionWithFilter = "Library File Downloads";
                if (filterFromDateNullable.HasValue && filterToDateNullable.HasValue)
                    captionWithFilter += $", between {filterFromDate.ToShortDateString()} and {filterToDate.ToShortDateString()} inclusive";
                else if (filterFromDateNullable.HasValue)
                    captionWithFilter += $", on or after {filterFromDate.ToShortDateString()}";
                else if (filterToDateNullable.HasValue)
                    captionWithFilter += $", on or before {filterToDate.ToShortDateString()}";
                //
                captionWithFilter += ". This report includes links created with the text editor, or created manually with /downloadLibraryFile?download={guid}. ";
                // 
                layout.columnCaption = "Row";
                layout.columnCaptionClass = "afwWidth20px afwTextAlignCenter";
                layout.columnCellClass = "afwTextAlignCenter";
                // 
                layout.addColumn();
                layout.columnCaption = "Count";
                layout.columnCaptionClass = "afwWidth100px afwTextAlignRight";
                layout.columnCellClass = "afwTextAlignRight";
                // 
                layout.addColumn();
                layout.columnCaption = "Library File";
                layout.columnCaptionClass = "afwTextAlignLeft";
                layout.columnCellClass = "afwTextAlignLeft";
                // 
                string sql = $@"
                    select f.name, count(*) as cnt
                    from
                        cclibrarydownloadlog l
                        left join cclibraryfiles f on f.id=l.fileId
                    where (1=1)
                        and (l.dateadded < {cp.Db.EncodeSQLDate(filterToDate)})
                        and (l.dateadded > {cp.Db.EncodeSQLDate(filterFromDate)})
                    group by f.id, f.name
                    order by cnt desc";
                CPCSBaseClass cs = cp.CSNew();
                cs.OpenSQL(sql);
                qsBase = frameRqs;
                // qsBase = cp.Utils.ModifyQueryString(qsBase, rnDstFeatureGuid, "{FB35BD57-875C-44C6-84D9-541793BF9190}")
                // qsBase = cp.Utils.ModifyQueryString(qsBase, rnDstFormId, formIdAccountDetails.ToString())
                int rowPtr = 1;
                while ((cs.OK())) {
                    layout.addRow();
                    layout.setCell(rowPtr.ToString());
                    layout.setCell(cs.GetInteger("cnt").ToString());
                    layout.setCell(cs.GetText("name"));
                    rowPtr += 1;
                    cs.GoNext();
                }
                layout.description = captionWithFilter;
                result = layout.getHtml();
                result = cp.Html.div(result, "", "abReportFileDownload");
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
