using Contensive.BaseClasses;
using Contensive.Reporting.Models;
using System;
using System.Diagnostics;

namespace Contensive.Reporting {

    // 
    public class OnInstallClass : AddonBaseClass {
        // 
        // =====================================================================================
        /// <summary>
        ///         ''' 
        ///         ''' </summary>
        ///         ''' <param name="CP"></param>
        ///         ''' <returns></returns>
        public override object Execute(CPBaseClass CP) {
            string result = "";
            Stopwatch sw = new Stopwatch(); sw.Start();
            try {
                // 
                // -- initialize application. If authentication needed and not login page, pass true
                using (ApplicationModel ae = new ApplicationModel(CP, false)) {
                    // 
                    // -- delete the old harcoded email drop report
                    CP.Db.ExecuteNonQuery("delete from ccmenuentries where linkpage='?af=12&rid=28'");
                    // 
                    // -- reset the EmailDropReport
                    using (CPCSBaseClass cs = CP.CSNew()) {
                        int reportId = 0;
                        if ((cs.Open("Admin Framework Reports", "name='Email Drop Report'")))
                            reportId = cs.GetInteger("id");
                        CP.Db.ExecuteNonQuery("delete from AdminFrameworkReports where id=" + reportId);
                        CP.Db.ExecuteNonQuery("delete from AdminFrameworkReportColumns where reportid=" + reportId);
                    }
                }
            } catch (Exception ex) {
                CP.Site.ErrorReport(ex);
            }
            return result;
        }
    }
}
