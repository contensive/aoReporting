
using System;
using System.Text;
using Contensive.Addons.PortalFramework;
using Contensive.BaseClasses;
using Contensive.Reporting;

namespace Contensive.Reporting {
    public class CurrentActivityReportAddon : AddonBaseClass {
        //
        internal const string SpanClassAdminNormal = "<span class=\"ccAdminNormal\">";
        internal const string rnAdminForm = "af";
        internal const string rnAdminSourceForm = "asf";
        //
        //====================================================================================================
        //
        public override object Execute(CPBaseClass cp) {
            try {
                string sql = null;
                string RowColor = null;
                string Panel = null;
                int VisitCount = 0;
                double PageCount = 0;
                StringBuilder Stream = new();
                //
                // --- set column width
                //
                Stream.Append("<table border=\"0\" cellpadding=\"3\" cellspacing=\"0\" width=\"100%\" style=\"background-color:white;border-top:1px solid #888;\">");
                //
                // ----- All Visits Today
                //
                using (var csData = cp.CSNew()) {
                    sql = "SELECT Count(ccVisits.ID) AS VisitCount, Avg(ccVisits.PageVisits) AS PageCount FROM ccVisits WHERE ((ccVisits.StartTime)>" + cp.Db.EncodeSQLDate(DateTime.Now.Date) + ");";
                    csData.OpenSQL(sql);
                    if (csData.OK()) {
                        VisitCount = csData.GetInteger("VisitCount");
                        PageCount = csData.GetInteger("pageCount");
                        Stream.Append("<tr>");
                        Stream.Append("<td style=\"border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "All Visits</span></td>");
                        Stream.Append("<td style=\"width:150px;border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "<a target=\"_blank\" href=\"/" + cp.Utils.EncodeHTML("?" + rnAdminForm + "=" + 0 + "&rid=3&DateFrom=" + DateTime.Now.Date.ToShortDateString() + "&DateTo=" + DateTime.Now.Date.ToShortDateString()) + "\">" + VisitCount + "</A>, " + string.Format("{0:N2}", PageCount) + " pages/visit.</span></td>");
                        Stream.Append("<td style=\"border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "This includes all visitors to the website, including guests, bots and administrators. Pages/visit includes page hits and not ajax or remote method hits.</span></td>");
                        Stream.Append("</tr>");
                    }
                }
                //
                // ----- Non-Bot Visits Today
                //
                using (var csData = cp.CSNew()) {
                    sql = "SELECT Count(ccVisits.ID) AS VisitCount, Avg(ccVisits.PageVisits) AS PageCount FROM ccVisits WHERE (ccVisits.CookieSupport=1)and((ccVisits.StartTime)>" + cp.Db.EncodeSQLDate(DateTime.Now.Date) + ");";
                    csData.OpenSQL(sql);
                    if (csData.OK()) {
                        VisitCount = csData.GetInteger("VisitCount");
                        PageCount = csData.GetInteger("pageCount");
                        Stream.Append("<tr>");
                        Stream.Append("<td style=\"border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "Non-bot Visits</span></td>");
                        Stream.Append("<td style=\"border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "<a target=\"_blank\" href=\"/" + cp.Utils.EncodeHTML("?" + rnAdminForm + "=" + 0 + "&rid=3&DateFrom=" + DateTime.Now.ToShortDateString() + "&DateTo=" + DateTime.Now.ToShortDateString()) + "\">" + VisitCount + "</A>, " + string.Format("{0:N2}", PageCount) + " pages/visit.</span></td>");
                        Stream.Append("<td style=\"border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "This excludes hits from visitors identified as bots. Pages/visit includes page hits and not ajax or remote method hits.</span></td>");
                        Stream.Append("</tr>");
                    }
                }
                //
                // ----- Visits Today by new visitors
                //
                using (var csData = cp.CSNew()) {
                    sql = "SELECT Count(ccVisits.ID) AS VisitCount, Avg(ccVisits.PageVisits) AS PageCount FROM ccVisits WHERE (ccVisits.CookieSupport=1)and(ccVisits.StartTime>" + cp.Db.EncodeSQLDate(DateTime.Now.Date) + ")AND(ccVisits.VisitorNew<>0);";
                    csData.OpenSQL(sql);
                    if (csData.OK()) {
                        VisitCount = csData.GetInteger("VisitCount");
                        PageCount = csData.GetNumber("pageCount");
                        Stream.Append("<tr>");
                        Stream.Append("<td style=\"border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "Visits by New Visitors</span></td>");
                        Stream.Append("<td style=\"border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "<a target=\"_blank\" href=\"/" + cp.Utils.EncodeHTML("?" + rnAdminForm + "=" + 0 + "&rid=3&ExcludeOldVisitors=1&DateFrom=" + DateTime.Now.ToShortDateString() + "&DateTo=" + DateTime.Now.ToShortDateString()) + "\">" + VisitCount + "</A>, " + string.Format("{0:N2}", PageCount) + " pages/visit.</span></td>");
                        Stream.Append("<td style=\"border-bottom:1px solid #888;\" valign=top>" + SpanClassAdminNormal + "This includes only new visitors not identified as bots. Pages/visit includes page hits and not ajax or remote method hits.</span></td>");
                        Stream.Append("</tr>");
                    }
                    csData.Close();
                }
                //
                Stream.Append("</table>");
                //
                // ----- Visits currently online
                //
                {
                    int personContentId = cp.Content.GetID("people");
                    int visitContentId = cp.Content.GetID("visits");
                    Panel = "";
                    Stream.Append("<h2>Current Visits</h2>");
                    using (var csData = cp.CSNew()) {
                        sql = "SELECT ccVisits.HTTP_REFERER as referer,ccVisits.remote_addr as Remote_Addr, ccVisits.LastVisitTime as LastVisitTime, ccVisits.PageVisits as PageVisits, ccMembers.Name as MemberName, ccVisits.ID as visitID, ccMembers.ID as memberId"
                            + " FROM ccVisits LEFT JOIN ccMembers ON ccVisits.memberId = ccMembers.ID"
                            + " WHERE (((ccVisits.LastVisitTime)>" + cp.Db.EncodeSQLDate(DateTime.Now.AddHours(-1)) + "))"
                            + " ORDER BY ccVisits.LastVisitTime DESC;";
                        csData.OpenSQL(sql);
                        if (csData.OK()) {
                            Panel += "<table width=\"100%\" border=\"0\" cellspacing=\"1\" cellpadding=\"2\">";
                            Panel += "<tr bgcolor=\"#B0B0B0\">";
                            Panel += "<td width=\"20%\" align=\"left\">" + SpanClassAdminNormal + "User</td>";
                            Panel += "<td width=\"20%\" align=\"left\">" + SpanClassAdminNormal + "IP&nbsp;Address</td>";
                            Panel += "<td width=\"20%\" align=\"left\">" + SpanClassAdminNormal + "Last&nbsp;Page&nbsp;Hit</td>";
                            Panel += "<td width=\"10%\" align=\"right\">" + SpanClassAdminNormal + "Page&nbsp;Hits</td>";
                            Panel += "<td width=\"10%\" align=\"right\">" + SpanClassAdminNormal + "Visit</td>";
                            Panel += "<td width=\"30%\" align=\"left\">" + SpanClassAdminNormal + "Referer</td>";
                            Panel += "</tr>";
                            RowColor = "ccPanelRowEven";
                            while (csData.OK()) {
                                int visitId = csData.GetInteger("VisitID");
                                int personId = csData.GetInteger("memberId");
                                Panel += "<tr class=\"" + RowColor + "\">";
                                Panel += "<td align=\"left\">" + SpanClassAdminNormal + $"<a target=\"_blank\" href=\"?cid={personContentId}&af=4&id={personId}\">" + csData.GetText("MemberName") + "</A></span></td>";
                                Panel += "<td align=\"left\">" + SpanClassAdminNormal + csData.GetText("Remote_Addr") + "</span></td>";
                                Panel += "<td align=\"left\">" + SpanClassAdminNormal + csData.GetDate("LastVisitTime").ToString("") + "</span></td>";
                                Panel += "<td align=\"right\">" + SpanClassAdminNormal + $"<a target=\"_blank\" href=\"?addonguid=%7B0905279A-6EFB-4A10-96FE-90F243962F75%7D&visitid={visitId}\">{csData.GetText("PageVisits")}</A></span></td>";
                                Panel += "<td align=\"right\">" + SpanClassAdminNormal + $"<a target=\"_blank\" href=\"?cid={visitContentId}&af=4&id={visitId}\">{visitId}</A></span></td>";
                                Panel += "<td align=\"left\">" + SpanClassAdminNormal + "&nbsp;" + csData.GetText("referer") + "</span></td>";
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
                    Stream.Append(getPanel(Panel, "ccPanel",  "100%", 0));
                }
                Stream.Append("</td></tr></table>");
                //
                // --- Start a form to make a refresh button
                LayoutBuilderSimple layout = new() {
                    title = "Daily Visits Report",
                    description = "",
                    body = cp.Html.Form(Stream.ToString()),
                    isOuterContainer = true,
                    includeBodyPadding = true,
                    includeBodyColor = true
                };
                layout.addFormButton(Constants.buttonCancel);
                layout.addFormButton(Constants.ButtonRefresh);
                layout.addFormHidden("asf", Constants.AdminFormQuickStats);
                //
                // --- Indented part (Title Area plus page)
                //
                layout.title = "Current Activity Report";
                return layout.getHtml(cp);
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
        //
        public string getPanel(string content, string stylePanel, string width, int padding) {
            string ContentPanelWidth = "100%";
            string contentPanelWidthStyle = ContentPanelWidth;
            //
            string s0 = ""
                + "<td style=\"padding:" + padding + "px;vertical-align:top\" class=\"" + stylePanel + "\">"
                + content
                + "</td>"
                + "";
            //
            string s1 = ""
                + "<tr>"
                + s0
                + "</tr>"
                + "";
            string s2 = ""
                + "<table style=\"width:" + contentPanelWidthStyle + ";border:0px;\" class=\"" + stylePanel + "\" cellspacing=\"0\">"
                + s1
                + "</table>"
                + "";
            string s3 = ""
                + "<td colspan=\"3\" width=\"" + ContentPanelWidth + "\" valign=\"top\" align=\"left\" class=\"" + stylePanel + "\">"
                + s2
                + "</td>"
                + "";
            string s4 = ""
                + "<tr>"
                + s3
                + "</tr>"
                + "";
            string result = ""
                + "<table border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"" + width + "\" class=\"" + stylePanel + "\">"
                + s4
                + "</table>"
                + "";
            return result;
        }

    }
}
