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
using Contensive.Reporting.Models;

namespace Contensive.Reporting {
    // 
    // ====================================================================================================
    // 
    public class PageViewsAddon : AddonBaseClass {
        // 
        // ====================================================================================================
        // 
        public override object Execute(CPBaseClass CP) {
            string result = "";
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
                    result = "<form><div class='afwBodyColor'><h3>Filters</h3><div class='abFilterRow'><input type='hidden' name='addonguid' id='addonguid' value='{" + Constants.pageViewGuid + "}'>";
                    result += "<label>From</label><input type='date' value='" + StartDate.ToString("yyyy-MM-dd") + "' name='filterFromDate' id='abFilterFromDate' class='abFilterDate' required></div> ";
                    result += "<div class='abFilterRow'><label>To</label><input type='date' name='filterToDate' id='abFilterToDate' class='abFilterDate' value='" + EndDate.ToString("yyyy-MM-dd") + "' required></div>";
                    result += "<div class='abFilterRow'><label></label><button type='submit'>Submit</button></div><br></form>";

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
                    if ((visitSummaryList.Count == 0))
                        result += "<span class=\"ccError\">There is currently no data collected to display this chart. Please check back later.</span>";
                    else
                        // legacy code uses isvisitdata=false
                        result += Models.ChartViewModel.getChart(ac, visitSummaryList, DivName, false, Width, Height, (durationHours == 1));
                }
            } catch (Exception ex) {
                CP.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
