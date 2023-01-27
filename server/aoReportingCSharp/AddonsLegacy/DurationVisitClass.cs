using Contensive.BaseClasses;
using Contensive.Reporting.Models;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Contensive.Reporting {
    // 
    // ====================================================================================================
    // 
    public class DurationVisitClass : AddonBaseClass {
        // 
        // =====================================================================================
        public override object Execute(CPBaseClass CP) {
            StringBuilder result = new StringBuilder();
            Stopwatch sw = new Stopwatch(); sw.Start();
            try {
                using (ApplicationModel ac = new ApplicationModel(CP, false)) {
                    string Width = ac.cp.Doc.GetText("Chart Width", "100%");
                    string Height = ac.cp.Doc.GetText("Chart Height", "400px");
                    string Rate = ac.cp.Doc.GetText("Rate", "hourly");
                    int intRate = 24;
                    bool AllowHourly = false;
                    if (Rate.ToLower() == "hourly") {
                        intRate = 1;
                        AllowHourly = true;
                    }
                    int Duration = ac.cp.Doc.GetInteger("Duration", 365);
                    string DivName = ac.cp.Doc.GetText("Target Div", "");
                    if (DivName == "")
                        DivName = ac.cp.Doc.GetText("TargetDiv", "durationChart");
                    DateTime DateEnd = DateTime.Now.Date;
                    DateTime DateStart = DateEnd.AddDays(-Duration).Date;
                    string cacheName = "DurationVisit-" + Width + "-" + Height + "-" + DivName + "-" + System.Convert.ToString(AllowHourly) + "-" + intRate + "-" + Duration;
                    string cacheValue = ac.cp.Cache.GetText(cacheName);
                    if ((string.IsNullOrEmpty(cacheValue))) {
                        if (Information.IsDate(DateStart) & Information.IsDate(DateEnd)) {
                            int intDateStart = CP.Utils.EncodeInteger(DateStart.ToOADate());
                            int intDateEnd = CP.Utils.EncodeInteger(DateEnd.ToOADate());
                            string criteria = "(TimeDuration=" + intRate + ") AND (DateNumber>=" + intDateStart + ") AND (DateNumber<=" + intDateEnd + ")";
                            List<VisitSummaryModel> visitSummaryList = Contensive.Models.Db.DbBaseModel.createList<VisitSummaryModel>(ac.cp, criteria, "DateNumber, TimeNumber");
                            if ((visitSummaryList.Count == 0))
                                result.Append("<span class=\"ccError\">There is currently no data collected to display this chart. Please check back later.</span>");
                            else {
                                result.Append(Models.ChartViewModel.GetChart2(ac, visitSummaryList, DivName, true, Width, Height, AllowHourly));
                                result.Append(getSummary2(ac, visitSummaryList, AllowHourly));
                            }
                            ac.cp.Cache.Store(cacheName, cacheValue);
                        } else
                            result.Append("<span class=\"ccError\">Please enter a valid Start and End Date to view the Visit Chart.</span>");
                    } else
                        result.Append(cacheValue);
                }
            } catch (Exception ex) {
                CP.Site.ErrorReport(ex);
            }
            return result.ToString();
        }
        // 
        // ====================================================================================================
        // 
        public string getSummary2(ApplicationModel ac, List<Models.VisitSummaryModel> visitSummaryList, bool IsHourlyChart) {
            StringBuilder result = new StringBuilder();
            try {
                double Visits = 0.0;
                double Viewings = 0.0;
                double PagePerVisit = 0.0;
                double NewVisits = 0.0;
                double TimeOnSite = 0.0;
                double BounceRate = 0.0;
                double Authenticated = 0.0;
                double DayCnt = 0.0;
                // 
                DayCnt = visitSummaryList.Count + 1;
                if (IsHourlyChart)
                    DayCnt = DayCnt / 24;
                foreach (var visitSummary in visitSummaryList) {
                    Visits = Visits + visitSummary.visits;
                    Viewings = Viewings + visitSummary.pagesViewed;
                    NewVisits = NewVisits + visitSummary.newVisitorVisits;
                    TimeOnSite = TimeOnSite + visitSummary.aveTimeOnSite;
                    BounceRate = BounceRate + visitSummary.singlePageVisits;
                    Authenticated = Authenticated + visitSummary.authenticatedVisits;
                }
                if ((Viewings != 0) & (Visits != 0)) {
                    PagePerVisit = (Viewings / Visits);
                    TimeOnSite = (TimeOnSite / Visits);
                    BounceRate = ((BounceRate / Visits) * 100);
                    NewVisits = ((NewVisits / Visits) * 100);
                    Authenticated = ((Authenticated / Visits) * 100);
                }

                result.Append("<div class=\"summaryContainer\">");
                result.Append("<table border=\"0\" width=\"100%\" cellpadding=\"3\" cellspacing=\"0\">");
                result.Append("<tr>");
                result.Append("<td class=\"summaryHeader\" colspan=\"2\" width=\"100%\">Summary: Last " + DayCnt + " Days</td>");
                result.Append("</tr>");
                result.Append("<tr>");
                result.Append("<td class=\"summaryCell\" width=\"50%\"><a style=\"text-decoration:none;\" href=\"#\" title=\"" + Constants.VisitDesc + "\" onClick=\"return false;\"><span class=\"summaryValue\">" + Visits + "</span> <span class=\"summaryCaption\">Visits</span></a></td>");
                result.Append("<td class=\"summaryCell\" width=\"50%\"><a style=\"text-decoration:none;\" href=\"#\" title=\"" + Constants.BounceDesc + "\" onClick=\"return false;\"><span class=\"summaryValue\">" + BounceRate + "%</span> <span class=\"summaryCaption\">Bounce Rate</span></a></td>");
                result.Append("</tr>");
                result.Append("<tr>");
                result.Append("<td class=\"summaryCell\" width=\"50%\"><a style=\"text-decoration:none;\" href=\"#\" title=\"" + Constants.PageDesc + "\" onClick=\"return false;\"><span class=\"summaryValue\">" + Viewings + "</span> <span class=\"summaryCaption\">Pages</span></a></td>");
                result.Append("<td class=\"summaryCell\" width=\"50%\"><a style=\"text-decoration:none;\" href=\"#\" title=\"" + Constants.PVDesc + "\" onClick=\"return false;\"><span class=\"summaryValue\">" + PagePerVisit + "</span> <span class=\"summaryCaption\">Pages/Visit</span></a></td>");
                result.Append("</tr>");
                result.Append("<tr>");
                result.Append("<td class=\"summaryCell\" width=\"50%\"><a style=\"text-decoration:none;\" href=\"#\" title=\"" + Constants.VisitorDesc + "\" onClick=\"return false;\"><span class=\"summaryValue\">" + NewVisits + "%</span> <span class=\"summaryCaption\">New Visitors</span></a></td>");
                result.Append("<td class=\"summaryCell\" width=\"50%\"><a style=\"text-decoration:none;\" href=\"#\" title=\"" + Constants.LogInDesc + "\" onClick=\"return false;\"><span class=\"summaryValue\">" + Authenticated + "%</span> <span class=\"summaryCaption\">Log In</span></a></td>");
                result.Append("</tr>");
                result.Append("</table>");
                result.Append("</div>");
            } catch (Exception ex) {
                ac.cp.Site.ErrorReport(ex);
            }
            return result.ToString();
        }
    }
}
