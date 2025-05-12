using Contensive.BaseClasses;
using Contensive.Reporting.Models;
using System;
using System.Collections.Generic;

namespace Contensive.Reporting {
    // 
    // ====================================================================================================
    // 
    public class PageViewsReportAddon : AddonBaseClass {
        // 
        // ====================================================================================================
        // 
        public override object Execute(CPBaseClass CP) {
            try {
                // -- initialize application. If authentication needed and not login page, pass true
                using (ApplicationModel ac = new ApplicationModel(CP, false)) {
                    DateTime StartDate = ac.cp.Doc.GetDate("filterFromDate");
                    DateTime EndDate = ac.cp.Doc.GetDate("filterToDate");
                    // if no date values, then default to last year
                    if ((StartDate <= DateTime.MinValue) | (EndDate <= DateTime.MinValue)) {
                        EndDate = DateTime.Now.Date;
                        StartDate = EndDate.AddDays(-365).Date;
                    }

                    // html for the date select. needs to include hidden addonguid so the old legacy way works
                    string htmlLeftOfTable = ""
                        + ""
                        + "<div class=\"\">"
                        + "<h3>Filters</h3>"
                        + "<div class=\"abFilterRow\">"
                            + "<label>From</label><input type=\"date\" value=\"" + StartDate.ToString("yyyy-MM-dd") + "\" name=\"filterFromDate\" id=\"abFilterFromDate\" class=\"abFilterDate\" required>"
                        + "</div>"
                        + "<div class=\"abFilterRow\">"
                            + "<label>To</label><input type=\"date\" name=\"filterToDate\" id=\"abFilterToDate\" class=\"abFilterDate\" value=\"" + EndDate.ToString("yyyy-MM-dd") + "\" required>"
                        + "</div>"
                        + "<div class=\"abFilterRow\">"
                            + "<label></label><button type=\"submit\">Submit</button>"
                        + "</div>"
                        + "<input type=\"hidden\" name=\"addonguid\" id=\"addonguid\" value=\"{" + Constants.pageViewGuid + "}\">"
                        + "";

                    string Width = ac.cp.Doc.GetText("Width");
                    string Height = ac.cp.Doc.GetText("Height");
                    int durationHours = 24;
                    string DivName = ac.cp.Doc.GetText("TargetDiv");
                    if (DivName == "")
                        DivName = "PageViewChart";
                    double dblDateStart = StartDate.ToOADate();
                    double dblDateEnd = EndDate.ToOADate();
                    // set the visit summary criteria
                    string criteria = "(TimeDuration=" + durationHours + ") AND (DateNumber>=" + dblDateStart + ") AND (DateNumber<" + dblDateEnd + ")";
                    List<Models.VisitSummaryModel> visitSummaryList = Models.VisitSummaryModel.createList<VisitSummaryModel>(ac.cp, criteria, "TimeNumber desc");
                    //
                    string body = "";
                    if ((visitSummaryList.Count == 0)) {
                        body = "<span class=\"ccError\">There is currently no data collected to display this chart. Please check back later.</span>";
                    } else {
                        // legacy code uses isvisitdata=false
                        body = Models.ChartViewModel.getChart(ac, visitSummaryList, DivName, false, Width, Height, (durationHours == 1));
                    }
                    //
                    var layout = CP.AdminUI.CreateLayoutBuilder();
                    layout.title = "Page Views Report";

                    layout.description = "";
                    layout.body = body;
                    layout.isOuterContainer = true;
                    layout.includeBodyPadding = true;
                    layout.includeBodyColor = true;
                    htmlLeftOfTable = "";
                    //
                    layout.addFormButton(Constants.ButtonRefresh);
                    //
                    return layout.getHtml();
                }
            } catch (Exception ex) {
                CP.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}
