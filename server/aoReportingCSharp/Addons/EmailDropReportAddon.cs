using Contensive.BaseClasses;
using Contensive.Reporting.Controllers;
using System;

namespace Contensive.Reporting {

    // 
    public class EmailDropReportAddon : AddonBaseClass {
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
                    case Constants.buttonCancel: {
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
                string rqs = cp.Utils.ModifyQueryString(frameRqs, Constants.rnDstFormId, dstFormId.ToString());
                // 
                // -- initialize report
                var layout = cp.AdminUI.CreateLayoutBuilderList();
                layout.title = "Email Drop Report";
                layout.addCsvDownloadCurrentPage = true;
                layout.isOuterContainer = true;
                // 
                // -- add buttons
                layout.addFormButton(Constants.buttonCancel);
                layout.addFormButton(Constants.ButtonRefresh);
                // 
                // -- add src view
                layout.addFormHidden(Constants.rnSrcFormId, dstFormId.ToString());
                // 
                // -- filterDateFrom
                const string userPropertyFromDate = "ReportEmailDrop-filterFromDate";
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
                const string userPropertyToDate = "ReportEmailDrop-filterToDate";
                DateTime filterToDate = new();
                string filterToDateString;
                string filterToDateStringTest = cp.Doc.GetText("filterToDate");
                string userFilterToDateString = cp.User.GetText(userPropertyToDate);
                if ((!string.IsNullOrEmpty(filterToDateStringTest)))
                    filterToDate = GenericController.encodeMinDate(cp.Doc.GetDate("filterToDate"));
                else if ((!string.IsNullOrEmpty(userFilterToDateString)))
                    filterFromDate = GenericController.encodeMinDate(cp.Utils.EncodeDate(userFilterFromDateString));
                else
                    filterToDate = DateTime.Today;
                cp.User.SetProperty(userPropertyToDate, filterToDate.ToString());
                if (filterToDate == DateTime.MinValue)
                    filterToDateString = "";
                else
                    filterToDateString = filterToDate.ToShortDateString();
                // 
                // -- create caption with filter text
                string captionWithFilter = "This report summarizes data from the Email Log and Email Drops and includes emails sent from all Group Emails";
                if (filterFromDateString != "" & filterToDateString != "")
                    captionWithFilter += ", between " + filterFromDateString + " and " + filterToDateString + " inclusive";
                else if (filterFromDateString != "")
                    captionWithFilter += ", on or after " + filterFromDateString;
                else if (filterToDateString != "")
                    captionWithFilter += ", on or before " + filterToDateString;
                captionWithFilter += ".";
                layout.description = captionWithFilter;
                // 
                layout.columnCaption = "Row";
                layout.columnCaptionClass = "afwWidth20px afwTextAlignCenter";
                layout.columnCellClass = "afwTextAlignCenter";
                // 
                layout.addColumn();
                layout.columnCaption = "Drop Date";
                layout.columnCaptionClass = "afwWidth200px afwTextAlignLeft";
                layout.columnCellClass = "afwTextAlignLeft";
                // 
                layout.addColumn();
                layout.columnCaption = "Email";
                layout.columnCaptionClass = "afwTextAlignLeft";
                layout.columnCellClass = "afwTextAlignLeft";
                // 
                layout.addColumn();
                layout.columnCaption = "Sent";
                layout.columnCaptionClass = "afwWidth50px afwTextAlignRight";
                layout.columnCellClass = "afwTextAlignRight";
                // 
                layout.addColumn();
                layout.columnCaption = "Opened";
                layout.columnCaptionClass = "afwWidth50px afwTextAlignRight";
                layout.columnCellClass = "afwTextAlignRight";
                // 
                layout.addColumn();
                layout.columnCaption = "Clicked";
                layout.columnCaptionClass = "afwWidth50px afwTextAlignRight";
                layout.columnCellClass = "afwTextAlignRight";
                // 
                // -- run query and fill rows
                string sql = @$"
                    select d.id as dropId, d.name as dropName, e.id as emailId, e.name as emailName, d.DateAdded as dropDate
                    ,(select count(distinct memberid) from ccemaillog where ((logtype=1)or(logtype=6))and(EmailDropID=d.id)) as Sent
                    ,(select count(distinct memberid) from ccemaillog where (logtype=2)and(EmailDropID=d.id)) as Opened
                    ,(select count(distinct memberid) from ccemaillog where (logtype=3)and(EmailDropID=d.id)) as Clicked
                     from ccemaildrops d 
                     left join ccemail e on e.id=d.EmailID
                     where (1=1)
                        {(filterToDate == DateTime.MinValue ? "" : $"and (d.dateadded < {cp.Db.EncodeSQLDate(filterToDate)})")}  
                        {(filterFromDate == DateTime.MinValue ? "" : $"and (d.dateadded > {cp.Db.EncodeSQLDate(filterFromDate)})")}  
                        and (e.id is not null)
                     order by 
                        d.id desc";
                using (CPCSBaseClass cs = cp.CSNew()) {
                    if ((cs.OpenSQL(sql))) {
                        int rowPtr = 1;
                        do {
                            int emaildropid = cs.GetInteger("dropid");
                            string openedCount = cs.GetInteger("opened").Equals(0) ? "0" : "<a href=\"?addonguid=%7BF4EE3D38-E0A9-4C93-9906-809F524B9690%7D&emaildropid=" + emaildropid.ToString() + "\">" + cs.GetInteger("opened").ToString() + "</a>";
                            string clickedCount = cs.GetInteger("clicked").Equals(0) ? "0" : "<a href=\"?addonguid=%7B29271653-BDE3-4DC1-8058-D54E53F1D06B%7D&emaildropid=" + emaildropid.ToString() + "\">" + cs.GetInteger("clicked").ToString() + "</a>";
                            layout.addRow();
                            layout.setCell(rowPtr.ToString());
                            layout.setCell(cs.GetDate("dropDate").ToString());
                            layout.setCell(cs.GetText("emailName"));
                            layout.setCell(cs.GetInteger("sent").ToString());
                            layout.setCell(openedCount);
                            layout.setCell(clickedCount);
                            rowPtr += 1;
                            cs.GoNext();
                        }
                        while ((cs.OK()));
                    }
                }
                // 
                // -- filters
                layout.htmlLeftOfBody = ""
                     + "<h3 class=\"abFilterHead\">Filters</h3>"
                     + "<div class=\"abFilterRow\"><label for\"abFilterFromDate\">From</label>" + cp.Html.InputText("filterFromDate", filterFromDateString, 100, "abFilterDate", "abFilterFromDate") + "<a href=\"#\" id=\"abFilterFromDateClear\">X</a></div>"
                     + "<div class=\"abFilterRow\"><label for\"abFilterToDate\">To</label>" + cp.Html.InputText("filterToDate", filterToDateString, 100, "abFilterDate", "abFilterToDate") + "<a href=\"#\" id=\"abFilterToDateClear\">X</a></div>"
                    + "";
                result = cp.Html.div(layout.getHtml(), "", "abReportEmailDrop");
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
