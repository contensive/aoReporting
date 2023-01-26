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
    public class WeeklyVisitClass : AddonBaseClass {
        // 
        // ====================================================================================================
        // 
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
