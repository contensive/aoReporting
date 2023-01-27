using Contensive.Addons.PortalFramework;
using Contensive.BaseClasses;
using Contensive.Reporting.Models;
using System;

namespace Contensive.Reporting {
    // 
    public class DailyVisitsReportAddon : AddonBaseClass {
        // 
        // =====================================================================================
        public override object Execute(CPBaseClass CP) {
            try {
                using (ApplicationModel ac = new(CP, false)) {
                    LayoutBuilderSimple layout = new() {
                        title = "Daily Visits Report",
                        description = "",
                        body = (new DailyVisitsChartAddon()).getChart(ac),
                        isOuterContainer = true,
                        includeBodyPadding = true,
                        includeBodyColor = true
                    };
                    layout.addFormButton(Constants.ButtonRefresh);
                    return layout.getHtml(ac.cp);
                }
            } catch (Exception ex) {
                CP.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}
