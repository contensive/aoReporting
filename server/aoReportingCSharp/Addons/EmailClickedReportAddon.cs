using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Contensive.BaseClasses;
using Contensive.Addons.PortalFramework;

namespace Contensive.Reporting {

    // 
    public class EmailClickedReportAddon : AddonBaseClass {
        // 
        // =====================================================================================
        // 
        public override object Execute(BaseClasses.CPBaseClass CP) {
            string returnHtml = "";
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
                returnHtml = getForm(CP, dstFormId, frameRqs, rightNow, emaildropid);
            } catch (Exception ex) {
                CP.Site.ErrorReport(ex, "Exception in adminAccountsClass.execute()");
            }
            return returnHtml;
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
                    case  Constants.buttonCancel: {
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
                // 
                string rqs = cp.Utils.ModifyQueryString(frameRqs, Constants.rnDstFormId, dstFormId.ToString());
                string qsBase = "";
                // 
                var report = new ReportListClass() {
                    title = "Email Clicked Report",
                    refreshQueryString = rqs,
                    addCsvDownloadCurrentPage = true,
                    isOuterContainer = true
                };
                report.addFormButton(Constants.buttonCancel);
                // 
                report.columnCaption = "Row";
                report.columnCaptionClass = "afwWidth20px afwTextAlignCenter";
                report.columnCellClass = "afwTextAlignCenter";
                // 
                report.addColumn();
                report.columnCaption = "Email";
                report.columnCaptionClass = "afwWidth100px afwTextAlignLeft";
                report.columnCellClass = "afwTextAlignLeft";
                // 
                report.addColumn();
                report.columnCaption = "Clicked By";
                report.columnCaptionClass = "afwTextAlignLeft";
                report.columnCellClass = "afwTextAlignLeft";
                // 
                string sqlcriteria = emaildropid == 0 ? "" : " and l.EmailDropID=" + emaildropid + "";
                string sql = $"select distinct m.name, m.email, d.name as dropName from ccEmailLog l left join ccMembers m on m.id=l.memberid left join ccemaildrops d on d.id=l.emailDropId where (logtype=3) and (m.name is not null) {sqlcriteria} order by m.name asc";
                CPCSBaseClass cs = cp.CSNew();
                cs.OpenSQL(sql);

                qsBase = frameRqs;
                int rowPtr = 1;
                while ((cs.OK())) {
                    report.addRow();
                    report.setCell(rowPtr.ToString());
                    report.setCell(cs.GetText("dropName"));
                    report.setCell("Name: " + cs.GetText("name") + "<br>" + "Email: " + cs.GetText("email"));
                    rowPtr += 1;
                    cs.GoNext();
                }
                cs.Close();
                // 
                // -- filters
                report.htmlLeftOfTable = ""
                     + "<h3 class=\"abFilterHead\">Filters</h3>"
                     + "<div class=\"abFilterRow\"><div class=\"form-group\"><label for\"abFilterEmailDropId\">Email Drop</label>" + cp.Html5.SelectContent("emailDropId", emaildropid, "Email Drops", "", "All Email Drops", "form-control", "abFilterEmailDripId") + "</div></div>"
                    + "";
                report.description = "Emails clicked from an email drop.";

                result = report.getHtml(cp);
                result = cp.Html.div(result, "", "abReportEmailDrop");
                cp.Doc.AddHeadStyle(report.styleSheet);
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
