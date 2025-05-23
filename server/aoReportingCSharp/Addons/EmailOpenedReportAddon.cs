﻿using Contensive.BaseClasses;
using System;

namespace Contensive.Reporting {
    public class EmailOpenedReportAddon : AddonBaseClass {
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
                int emaildropid = CP.Doc.GetInteger("emaildropid");
                if (srcFormId > 0)
                    dstFormId = processForm(CP, srcFormId, frameRqs, rightNow);

                dstFormId = processForm(CP, srcFormId, frameRqs, rightNow);
                // 
                // -- workaround for a formset that only has one form.
                if ((dstFormId == 0))
                    dstFormId = Constants.formIdDefault;
                // 
                // -- get the next form
                return getForm(CP, dstFormId, frameRqs, rightNow, emaildropid);
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
                            cp.Response.Redirect("?addonguid=%7BA10B5F49-3147-4E32-9DCF-76D65CCFF9F1%7D");
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
        internal string getForm(CPBaseClass cp, int dstFormId, string frameRqs, DateTime rightNow, int emaildropid) {
            string result = "";
            try {
                var layout = cp.AdminUI.CreateLayoutBuilderList();
                layout.title = "Email Opened Report";
                layout.addCsvDownloadCurrentPage = true;
                layout.isOuterContainer = true;
                //
                layout.addFormButton(Constants.buttonCancel);
                // 
                layout.columnCaption = "Row";
                layout.columnCaptionClass = "afwWidth20px afwTextAlignCenter";
                layout.columnCellClass = "afwTextAlignCenter";
                // 
                layout.addColumn();
                layout.columnCaption = "Email";
                layout.columnCaptionClass = "afwWidth100px afwTextAlignLeft";
                layout.columnCellClass = "afwTextAlignLeft";
                // 
                layout.addColumn();
                layout.columnCaption = "Opened By";
                layout.columnCaptionClass = "afwTextAlignLeft";
                layout.columnCellClass = "afwTextAlignLeft";
                // 
                string sqlcriteria = emaildropid == 0 ? "" : " and l.EmailDropID=" + emaildropid + "";
                string sql = $"select distinct m.name, m.email, d.name as dropName from ccEmailLog l left join ccMembers m on m.id=l.memberid left join ccemaildrops d on d.id=l.emailDropId where (logtype=2) and (m.name is not null) {sqlcriteria} order by m.name asc";
                CPCSBaseClass cs = cp.CSNew();
                cs.OpenSQL(sql);

                int rowPtr = 1;
                while ((cs.OK())) {
                    layout.addRow();
                    layout.setCell(rowPtr.ToString());
                    layout.setCell(cs.GetText("dropName"));
                    layout.setCell("Name: " + cs.GetText("name") + "<br>" + "Email: " + cs.GetText("email"));
                    rowPtr += 1;
                    cs.GoNext();
                }
                cs.Close();
                // 
                // -- filters
                layout.htmlLeftOfBody = ""
                     + "<h3 class=\"abFilterHead\">Filters</h3>"
                     + "<div class=\"abFilterRow\"><div class=\"form-group\"><label for\"abFilterEmailDropId\">Email Drop</label>" + cp.Html5.SelectContent("emailDropId", emaildropid, "Email Drops", "", "All Email Drops", "form-control", "abFilterEmailDripId") + "</div></div>"
                    + "";
                layout.description = "Emails opened from an email drop.";
                result = layout.getHtml();
                result = cp.Html.div(result, "", "abReportEmailDrop");
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
