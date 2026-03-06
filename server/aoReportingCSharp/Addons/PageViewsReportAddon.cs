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
                    //
                    var layout = CP.AdminUI.CreateLayoutBuilder();
                    layout.title = "Page Views Report";
                    layout.isOuterContainer = true;
                    layout.includeBodyPadding = true;
                    layout.includeBodyColor = true;
                    //
                    // -- read filter values
                    const string viewName = "PageViewsReport";
                    DateTime? filterFromDate = layout.getFilterDate("filterFromDate", viewName);
                    DateTime? filterToDate = layout.getFilterDate("filterToDate", viewName);
                    //
                    // -- default to last year if no filter values
                    DateTime StartDate = filterFromDate ?? DateTime.Now.Date.AddDays(-365);
                    DateTime EndDate = filterToDate ?? DateTime.Now.Date;
                    //
                    // -- add filter UI
                    layout.addFilterDateInput("From", "filterFromDate", StartDate);
                    layout.addFilterDateInput("To", "filterToDate", EndDate);
                    //
                    string Width = ac.cp.Doc.GetText("Width");
                    string Height = ac.cp.Doc.GetText("Height");
                    int durationHours = 24;
                    string DivName = ac.cp.Doc.GetText("TargetDiv");
                    if (DivName == "")
                        DivName = "PageViewChart";
                    double dblDateStart = StartDate.ToOADate();
                    double dblDateEnd = EndDate.ToOADate();
                    // set the visit summary criteria
                    string criteria = $"(TimeDuration={durationHours}) AND (DateNumber>={dblDateStart}) AND (DateNumber<{dblDateEnd})";
                    List<Models.VisitSummaryModel> visitSummaryList = Models.VisitSummaryModel.createList<VisitSummaryModel>(ac.cp, criteria, "TimeNumber desc");
                    //
                    string body;
                    if ((visitSummaryList.Count == 0)) {
                        body = "<span class=\"ccError\">There is currently no data collected to display this chart. Please check back later.</span>";
                    } else {
                        body = Models.ChartViewModel.getChart(ac, visitSummaryList, DivName, false, Width, Height, (durationHours == 1));
                    }
                    //
                    layout.description = "";
                    layout.body = $"<div style=\"min-height:400px\">{body}</div>";
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
