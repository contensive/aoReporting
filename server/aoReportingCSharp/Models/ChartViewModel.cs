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
using Contensive.Reporting.Models;
using Microsoft.VisualBasic;

namespace Contensive.Reporting.Models {
    public sealed class ChartViewModel {
        // 
        // ====================================================================================================
        // 
        public static string getChart(ApplicationModel ae, List<VisitSummaryModel> visitSummaryList, string Div, bool isVisitData, string Width = "100%", string Height = "400px", bool AllowHourly = false) {
            StringBuilder result = new StringBuilder();
            try {
                if (Width == "")
                    Width = "99%";
                if (Height == "")
                    Height = "400px";
                string Caption;
                if (isVisitData)
                    Caption = "Visits";
                else
                    Caption = "Page Views";
                result.Append("<script type='text/javascript'>" + Environment.NewLine);
                result.Append("google.load('visualization', '1', {'packages':['annotatedtimeline']});" + Environment.NewLine);
                result.Append("google.setOnLoadCallback(drawChart);" + Environment.NewLine);
                result.Append("function drawChart() {" + Environment.NewLine);
                result.Append("var data = new google.visualization.DataTable();" + Environment.NewLine);
                result.Append("data.addColumn('date', 'Date');" + Environment.NewLine);
                result.Append("data.addColumn('number', '" + Caption + "');" + Environment.NewLine);
                result.Append("data.addRows(" + visitSummaryList.Count + ");" + Environment.NewLine);
                int visits = 0;
                int pageCount = 0;
                int newVisitsTotal = 0;
                int onePageVisitsTotal = 0;
                int authenticatedvisitTotal = 0;
                // 
                int Pointer = 0;
                foreach (VisitSummaryModel viewSummary in visitSummaryList) {
                    string Value = string.Empty;
                    DateTime nrmDate;
                    if ((viewSummary.timeNumber != 0) & (AllowHourly))
                        nrmDate = DateTime.FromOADate((viewSummary.dateNumber + (viewSummary.timeNumber / (double)24.0!)));
                    else
                        nrmDate = DateTime.FromOADate(viewSummary.dateNumber);

                    if (isVisitData) {
                        Value = viewSummary.visits.ToString();
                        visits += viewSummary.visits;
                        pageCount += viewSummary.pagesViewed;
                        newVisitsTotal += viewSummary.newVisitorVisits;
                        onePageVisitsTotal += viewSummary.singlePageVisits;
                        authenticatedvisitTotal += viewSummary.authenticatedVisits;
                    } else
                        Value = viewSummary.pagesViewed.ToString();

                    result.Append("data.setValue(" + Pointer + ", 0, new Date(" + nrmDate.Year + "," + (nrmDate.Month  - 1) + "," + nrmDate.Day + "," + nrmDate.Hour.ToString("00") + ",00,00));" + "");
                    result.Append("data.setValue(" + Pointer + ", 1, " + Value + ");" + "");
                    Pointer = Pointer + 1;
                }
                result.Append("var chart = new google.visualization.AnnotatedTimeLine(document.getElementById('" + Div + "'));" + "");
                result.Append("chart.draw(data, {displayAnnotations: false});" + "");
                result.Append("}" + "");
                result.Append("</script>" + "" + "");
                result.Append("<div id='" + Div + "' style='width: " + Width + "; height: " + Height + "; float:left; padding:0;'></div>");

                if (isVisitData) {
                    // add the summary table
                    // new visits/total visits
                    double newVisitors = 0;
                    // authenticated vists / ttal visits
                    double loginPercent = 0;
                    // bounce rate is single page visits/total visits
                    double bounceRate = 0;
                    // pages/vistis
                    double pagesPerVisit = 0;

                    if (visits > 0) {
                        bounceRate = onePageVisitsTotal / (double)visits;
                        bounceRate = bounceRate * 100;
                        bounceRate = Math.Round(bounceRate);
                        // pages per visits
                        pagesPerVisit = pageCount / (double)visits;
                        pagesPerVisit = Math.Round(pagesPerVisit);
                        // new visitors
                        newVisitors = newVisitsTotal / (double)visits;
                        newVisitors = newVisitors * 100;
                        newVisitors = Math.Round(newVisitors);
                        // login percent
                        loginPercent = authenticatedvisitTotal / (double)visits;
                        loginPercent = loginPercent * 100;
                        loginPercent = Math.Round(loginPercent);
                    }
                    // add html for summary table
                    string summaryTable = "<div class='summaryContainer'><table border='0' width='100%' cellpadding='3' cellspacing='0'><tbody><tr>";
                    summaryTable += "<td class='summaryHeader' colspan='2' width='100%'>Summary</td></tr><tr><td class='summaryCell' width='50%'>";
                    summaryTable += "<span Class='summaryValue'>" + visits.ToString() + "</span> <span Class='summaryCaption'>Visits</span></a></td>";
                    summaryTable += "<td class='summaryCell' width='50%'>";
                    summaryTable += "<span Class='summaryValue'>" + bounceRate.ToString() + "%" + "</span> <span Class='summaryCaption'>Bounce Rate</span></a></td></tr><tr><td Class='summaryCell' width='50%'>";
                    summaryTable += "<span class='summaryValue'>" + pageCount.ToString() + "</span>";
                    summaryTable += " <span Class='summaryCaption'>Pages</span></a></td><td Class='summaryCell' width='50%'>";
                    summaryTable += "<span class='summaryValue'>" + pagesPerVisit.ToString() + "</span>";
                    summaryTable += " <span Class='summaryCaption'>Pages/Visit</span></a></td></tr><tr><td Class='summaryCell' width='50%'>";
                    summaryTable += "<span Class='summaryValue'>" + newVisitors.ToString() + "%" + "</span> <span Class='summaryCaption'>New Visitors</span></a></td><td Class='summaryCell' width='50%'><span Class='summaryValue'>" + loginPercent.ToString() + "%" + "</span> ";
                    summaryTable += "<span Class='summaryCaption'>Log In</span></a></td></tr></tbody></table></div>";
                    result.Append(summaryTable);
                }
            } catch (Exception ex) {
                ae.cp.Site.ErrorReport(ex);
            }
            return result.ToString();
        }
        // 
        // ====================================================================================================
        // 
        // RSData is a 3xlength array of the data
        // a(0,n) = DateNumber (int of Dbl Date)
        // a(1,n) = TimeNumber (0-23)
        // a(2,n) = value to plot
        // AllowHourly - if true, there must be all 24 time numbers for each date number
        // 
        internal static string GetChart2(ApplicationModel ae, List<Models.VisitSummaryModel> visitSummaryList, string Div, bool isVisitData, string Width = "100%", string Height = "400px", bool AllowHourly = false) {
            StringBuilder result = new StringBuilder();
            try {
                if (Width == "")
                    Width = "100%";
                if (Height == "")
                    Height = "400px";
                string Caption = "Page Views";
                if (isVisitData)
                    Caption = "Visits";
                result.Append("<script type='text/javascript'>" + Environment.NewLine);
                result.Append("google.load('visualization', '1', {'packages':['annotatedtimeline']});" + Environment.NewLine);
                result.Append("google.setOnLoadCallback(drawChart);" + Environment.NewLine);
                result.Append("function drawChart() {" + Environment.NewLine);
                result.Append("var data = new google.visualization.DataTable();" + Environment.NewLine);
                result.Append("data.addColumn('date', 'Date');" + Environment.NewLine);
                result.Append("data.addColumn('number', '" + Caption + "');" + Environment.NewLine);
                result.Append("data.addRows(" + (visitSummaryList.Count + 1) + ");" + Environment.NewLine);
                // 
                // Plot what we got
                // 
                int Ptr = 0;
                foreach (var visitSummary in visitSummaryList) {
                    double DateNumber = visitSummary.dateNumber;
                    double TimeNumber = visitSummary.timeNumber;
                    double PlotValue = visitSummary.pagesViewed;
                    if (isVisitData)
                        PlotValue = visitSummary.visits;
                    DateTime PlotDate;
                    if ((TimeNumber != 0) & (AllowHourly))
                        PlotDate = DateTime.FromOADate(DateNumber + (TimeNumber / 24.0!));
                    else
                        PlotDate = DateTime.FromOADate(DateNumber);
                    result.Append("data.setValue(" + Ptr + ", 0, new Date(" + PlotDate.Year + "," + (PlotDate.Month- 1) + "," + PlotDate.Day + "," + PlotDate.Hour.ToString("00") + ",00,00));" + Environment.NewLine);
                    result.Append("data.setValue(" + Ptr + ", 1, " + PlotValue + ");" + Environment.NewLine);
                    Ptr += 1;
                }
                result.Append("var chart = new google.visualization.AnnotatedTimeLine(document.getElementById('" + Div + "'));" + Environment.NewLine);
                result.Append("chart.draw(data, {displayAnnotations: false});" + Environment.NewLine);
                result.Append("}" + Environment.NewLine);
                result.Append("</script>" + Environment.NewLine + Environment.NewLine);
                result.Append("<div id='" + Div + "' style='width: " + Width + "; height: " + Height + ";  float:left; padding:0;'></div>");
            } catch (Exception ex) {
                ae.cp.Site.ErrorReport(ex);
            }
            return result.ToString();
        }
    }
}
