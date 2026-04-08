
using System;
using System.Text;

namespace Contensive.Reporting {
    public class UsersOnlineReportAddon : BaseClasses.AddonBaseClass {
        //
        // constants
        internal const string guidUsersOnlineReport = "{A5439430-ED28-4D72-A9ED-50FB36145955}";

        internal const int AdminFormReports = 12; // Call Reports form (admin only)
        //
        //====================================================================================================
        /// <summary>
        /// addon method 
        /// blank return on OK or cancel button
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(BaseClasses.CPBaseClass cp) {
            try {
                //
                // -- get the button clicked
                string button = cp.Doc.GetText("button", "");
                if (button == Constants.buttonCancel) { return ""; }
                //
                StringBuilder body = new();
                //
                // --- Indented part (Title Area plus page)
                //
                body.Append("<table border=\"0\" cellpadding=\"20\" cellspacing=\"0\" width=\"100%\"><tr><td>" + "<span>");
                //
                // --- set column width
                //
                body.Append("<h2>Visits Today</h2>");
                body.Append("<table border=\"0\" cellpadding=\"3\" cellspacing=\"0\" width=\"100%\" style=\"background-color:white;border-top:1px solid #888;\">");
                //
                // ----- All Visits Today
                //
                using (var csData =  cp.CSNew()) {
                    string sql = "SELECT Count(ccVisits.ID) AS VisitCount, Avg(CAST(ccVisits.PageVisits AS FLOAT)) AS PageCount FROM ccVisits WHERE ((ccVisits.StartTime)>" +  cp.Db.EncodeSQLDate(DateTime.Now.Date) + ");";
                    csData.OpenSQL(sql);
                    if (csData.OK()) {
                        int VisitCount = csData.GetInteger("VisitCount");
                        double PageCount = csData.GetNumber("pageCount");
                        body.Append("<tr>");
                        body.Append("<td style=\"border-bottom:1px solid #888;\" valign=top>All Visits</td>");
                        body.Append($"<td style=\"width:150px;border-bottom:1px solid #888;\" valign=top>{VisitCount}<br>{PageCount:N2} pages/visit.</td>");
                        body.Append("<td style=\"border-bottom:1px solid #888;\" valign=top>This includes all visitors to the website, including guests, bots and administrators. Pages/visit includes page hits and not ajax or remote method hits.</td>");
                        body.Append("</tr>");
                    }
                }
                //
                // ----- Non-Bot Visits Today
                //
                using (var csData = cp.CSNew()) {
                    string sql = "SELECT Count(ccVisits.ID) AS VisitCount, Avg(CAST(ccVisits.PageVisits AS FLOAT)) AS PageCount FROM ccVisits WHERE (ccVisits.CookieSupport=1)and((ccVisits.StartTime)>" + cp.Db.EncodeSQLDate(DateTime.Now.Date) + ");";
                    csData.OpenSQL(sql);
                    if (csData.OK()) {
                        int VisitCount = csData.GetInteger("VisitCount");
                        double PageCount = csData.GetNumber("pageCount");
                        body.Append("<tr>");
                        body.Append("<td style=\"border-bottom:1px solid #888;\" valign=top>Non-bot Visits</td>");
                        body.Append($"<td style=\"border-bottom:1px solid #888;\" valign=top>{VisitCount}<br>{PageCount:N2} pages/visit.</td>");
                        body.Append("<td style=\"border-bottom:1px solid #888;\" valign=top>This excludes hits from visitors identified as bots. Pages/visit includes page hits and not ajax or remote method hits.</td>");
                        body.Append("</tr>");
                    }
                }
                //
                // ----- Visits Today by new visitors
                //
                using (var csData = cp.CSNew()) {
                    string sql = "SELECT Count(ccVisits.ID) AS VisitCount, Avg(CAST(ccVisits.PageVisits AS FLOAT)) AS PageCount FROM ccVisits WHERE (ccVisits.CookieSupport=1)and(ccVisits.StartTime>" + cp.Db.EncodeSQLDate(DateTime.Now.Date) + ")AND(ccVisits.VisitorNew<>0);";
                    csData.OpenSQL(sql);
                    if (csData.OK()) {
                        int VisitCount = csData.GetInteger("VisitCount");
                        double PageCount = csData.GetNumber("pageCount");
                        body.Append("<tr>");
                        body.Append("<td style=\"border-bottom:1px solid #888;\" valign=top>Visits by New Visitors</td>");
                        body.Append($"<td style=\"border-bottom:1px solid #888;\" valign=top>{VisitCount}<br>{PageCount:N2} pages/visit.</td>");
                        body.Append("<td style=\"border-bottom:1px solid #888;\" valign=top>This includes only new visitors not identified as bots. Pages/visit includes page hits and not ajax or remote method hits.</td>");
                        body.Append("</tr>");
                    }
                    csData.Close();
                }
                //
                body.Append("</table>");
                //
                // ----- Visits currently online
                //
                {
                    string Panel = "";
                    body.Append("<h2>Current Visits</h2>");
                    using (var csData = cp.CSNew()) {
                        string sql = "SELECT ccVisits.HTTP_REFERER as referer,ccVisits.remote_addr as Remote_Addr, ccVisits.LastVisitTime as LastVisitTime, ccVisits.PageVisits as PageVisits, ccMembers.Name as MemberName, ccVisits.ID as VisitID, ccMembers.ID as MemberID"
                            + " FROM ccVisits LEFT JOIN ccMembers ON ccVisits.memberId = ccMembers.ID"
                            + " WHERE (((ccVisits.LastVisitTime)>" + cp.Db.EncodeSQLDate(DateTime.Now.AddHours(-1)) + "))"
                            + " ORDER BY ccVisits.LastVisitTime DESC;";
                        csData.OpenSQL(sql);
                        if (csData.OK()) {
                            string cellStyle = "overflow:hidden;text-overflow:ellipsis;white-space:nowrap;max-width:0;";
                            Panel += "<table width=\"100%\" border=\"0\" cellspacing=\"1\" cellpadding=\"2\" style=\"table-layout:fixed;\">";
                            Panel += "<tr bgcolor=\"#B0B0B0\">";
                            Panel += $"<td width=\"20%\" align=\"left\" style=\"{cellStyle}\">User</td>";
                            Panel += $"<td width=\"15%\" align=\"left\" style=\"{cellStyle}\">IP&nbsp;Address</td>";
                            Panel += $"<td width=\"15%\" align=\"left\" style=\"{cellStyle}\">Last&nbsp;Page&nbsp;Hit</td>";
                            Panel += $"<td width=\"10%\" align=\"right\" style=\"{cellStyle}\">Page&nbsp;Hits</td>";
                            Panel += $"<td width=\"10%\" align=\"right\" style=\"{cellStyle}\">Visit</td>";
                            Panel += $"<td width=\"30%\" align=\"left\" style=\"{cellStyle}\">Referer</td>";
                            Panel += "</tr>";
                            string RowColor = "ccPanelRowEven";
                            int peopleCid = cp.Content.GetID("people");
                            while (csData.OK()) {
                                int VisitID = csData.GetInteger("VisitID");
                                Panel += "<tr class=\"" + RowColor + "\">";
                                Panel += $"<td align=\"left\" style=\"{cellStyle}\"><a target=\"_blank\" href=\"/{cp.Utils.EncodeHTML($"{cp.GetAppConfig().adminRoute}?af=4&cid={peopleCid}&id={csData.GetInteger("MemberID")}")}\">{csData.GetText("MemberName")}</a></td>";
                                Panel += $"<td align=\"left\" style=\"{cellStyle}\">{csData.GetText("Remote_Addr")}</td>";
                                Panel += $"<td align=\"left\" style=\"{cellStyle}\">{csData.GetDate("LastVisitTime")}</td>";
                                Panel += $"<td align=\"right\" style=\"{cellStyle}\"><a href=\"?addonguid={{0905279A-6EFB-4A10-96FE-90F243962F75}}&visitid={VisitID}\">{csData.GetText("PageVisits")}</a></td>";
                                Panel += $"<td align=\"right\" style=\"{cellStyle}\">{VisitID}</td>";
                                Panel += $"<td align=\"left\" style=\"{cellStyle}\" title=\"{cp.Utils.EncodeHTML(csData.GetText("referer"))}\">{csData.GetText("referer")}</td>";
                                Panel += "</tr>";
                                if (RowColor == "ccPanelRowEven") {
                                    RowColor = "ccPanelRowOdd";
                                } else {
                                    RowColor = "ccPanelRowEven";
                                }
                                csData.GoNext();
                            }
                            Panel += "</table>";
                        }
                        csData.Close();
                    }
                    body.Append(cp.Html.div(Panel,"", "ccPanel"));
                }
                body.Append("</td></tr></table>");
                //
                //
                var layout = cp.AdminUI.CreateLayoutBuilder();
                layout.title = "Users Online Report";
                layout.description = "";
                layout.body = cp.Html5.Form(body.ToString());
                ////
                //layout.addFormHidden(rnAdminSourceForm, AdminFormQuickStats);
                //
                layout.addFormButton(Constants.buttonCancel);
                layout.addFormButton(Constants.ButtonRefresh);
                //
                return layout.getHtml();
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}