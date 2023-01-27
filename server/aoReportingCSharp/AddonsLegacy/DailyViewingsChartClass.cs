using Contensive.BaseClasses;
using Contensive.Reporting.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using DbBaseModel = Contensive.Models.Db.DbBaseModel;

namespace Contensive.Reporting {
    public class DailyViewingsChartClass : AddonBaseClass {
        // 
        // =====================================================================================
        public override object Execute(CPBaseClass CP) {
            string result = "";
            Stopwatch sw = new Stopwatch(); sw.Start();
            try {
                using (ApplicationModel ac = new ApplicationModel(CP, false)) {
                    string Width = ac.cp.Doc.GetText("Width");
                    string Height = ac.cp.Doc.GetText("Height");
                    int durationHours = 24;
                    string Rate = ac.cp.Doc.GetText("Rate");
                    if (Rate.ToLower() == "hourly")
                        durationHours = 1;
                    int DurationDays = ac.cp.Doc.GetInteger("Duration", 365);
                    string DivName = ac.cp.Doc.GetText("TargetDiv");
                    if (DivName == "")
                        DivName = "PageViewChart";
                    DateTime DateEnd = DateTime.Now.Date;

                    DateTime DateStart = DateEnd.AddDays(-DurationDays).Date;
                    double dblDateStart = DateStart.ToOADate();
                    double dblDateEnd = DateEnd.ToOADate();
                    string criteria = "(TimeDuration=" + durationHours + ") AND (DateNumber>=" + dblDateStart + ") AND (DateNumber<" + dblDateEnd + ")";


                    List<Models.VisitSummaryModel> visitSummaryList =  DbBaseModel.createList<VisitSummaryModel>(ac.cp, criteria, "TimeNumber desc");
                    if ((visitSummaryList.Count == 0))
                        result = "<span class=\"ccError\">There is currently no data collected to display this chart. Please check back later.</span>";
                    else
                        result = Models.ChartViewModel.getChart(ac, visitSummaryList, DivName, false, Width, Height, (durationHours == 1));
                }
            } catch (Exception ex) {
                CP.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
