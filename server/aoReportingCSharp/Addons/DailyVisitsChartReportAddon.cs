using Contensive.BaseClasses;
using Contensive.Reporting.Models;
using System;

namespace Contensive.Reporting {
    // 
    public class DailyVisitsChartReportAddon : AddonBaseClass {
        // 
        // =====================================================================================
        public override object Execute(CPBaseClass CP) {
            try {
                using (ApplicationModel ac = new(CP, false)) {
                    var layout = CP.AdminUI.CreateLayoutBuilder();
                    layout.title = "Daily Visits Report";
                    layout.description = "";
                    layout.body = (new DailyVisitsChartAddon()).getChart(ac);
                    layout.isOuterContainer = true;
                    layout.includeBodyPadding = true;
                    layout.includeBodyColor = true;
                    layout.addFormButton(Constants.ButtonRefresh);
                    return layout.getHtml();
                }
            } catch (Exception ex) {
                CP.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}
