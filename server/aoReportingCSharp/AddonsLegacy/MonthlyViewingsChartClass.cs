using Contensive.BaseClasses;
using Contensive.Reporting.Models;
using System;
using System.Diagnostics;

namespace Contensive.Reporting {
    // 
    // ====================================================================================================
    // 
    public class MonthlyViewingsChartClass : AddonBaseClass {
        // 
        // =====================================================================================
        public override object Execute(CPBaseClass CP) {
            string result = "";
            Stopwatch sw = new Stopwatch(); sw.Start();
            try {
                // 
                // -- initialize application. If authentication needed and not login page, pass true
                using (ApplicationModel ae = new ApplicationModel(CP, false)) {
                    // 
                    // -- your code
                    result = "Hello World";
                    if (ae.packageErrorList.Count > 0)
                        result = "Hey user, this happened - " + string.Join("<br>", ae.packageErrorList);
                }
            } catch (Exception ex) {
                CP.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
