using Contensive.BaseClasses;
using Contensive.Reporting.Models;
using System;
using System.Collections.Generic;

namespace Contensive.Reporting {
    // 
    public class DailyVisitsChartAddon : AddonBaseClass {
        // 
        // =====================================================================================
        public override object Execute(CPBaseClass CP) {
            try {
                using ApplicationModel ac = new(CP, true);
                var layout = CP.AdminUI.CreateLayoutBuilder();
                layout.title = "Daily Visits Report";
                layout.description = "";
                layout.body = getChart(ac);
                layout.isOuterContainer = true;
                return layout.getHtml();
            } catch (Exception ex) {
                CP.Site.ErrorReport(ex);
                throw;
            }
        }
        //
        public string getChart(ApplicationModel ac) {
            try {
                string Width = ac.cp.Doc.GetText("Width");
                string Height = ac.cp.Doc.GetText("Height");
                int durationHours = 24;
                int DurationDays = ac.cp.Doc.GetInteger("Duration", 365);
                string DivName = ac.cp.Doc.GetText("TargetDiv");
                if (DivName == "")
                    DivName = "PageViewChart";
                DateTime DateEnd = DateTime.Now.Date;
                DateTime DateStart = DateEnd.AddDays(-DurationDays).Date;
                double dblDateStart = DateStart.ToOADate();
                double dblDateEnd = DateEnd.ToOADate();
                string criteria = "(TimeDuration=" + durationHours + ") AND (DateNumber>=" + dblDateStart + ") AND (DateNumber<" + dblDateEnd + ")";
                List<Models.VisitSummaryModel> visitSummaryList = Models.VisitSummaryModel.createList<VisitSummaryModel>(ac.cp, criteria, "TimeNumber desc");
                if ((visitSummaryList.Count == 0))
                    return "<span class=\"ccError\">There is currently no data collected to display this chart. Please check back later.</span>";
                else
                    return Models.ChartViewModel.getChart(ac, visitSummaryList, DivName, true, Width, Height, (durationHours == 1));
            } catch (Exception ex) {
                ac.cp.Site.ErrorReport(ex);
                throw;
            }

        }
    }
}
