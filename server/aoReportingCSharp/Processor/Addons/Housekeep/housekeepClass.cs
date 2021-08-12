using Contensive.BaseClasses;
using System;
//
namespace Contensive.Addons.Reporting.Processor.Addons.Housekeep {
    /// <summary>
    /// support for housekeeping functions for reporting
    /// </summary>
    public class ReportingHouseKeepClass : AddonBaseClass {
        //
        //====================================================================================================
        /// <summary>
        /// addon interface
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public override object Execute(CPBaseClass cp) {
            try {
                //
                cp.Log.Info("Housekeep");
                //
                var env = new HouseKeepEnvironmentModel(cp);
                int TimeoutSave = cp.Db.SQLTimeout;
                cp.Db.SQLTimeout = 1800;

                // -- hourly tasks
                //
                // -- summaries - must be first
                VisitSummaryClass.executeHourlyTasks(cp);
                ViewingSummaryClass.executeHourlyTasks(cp);

                // -- daily tasks
                cp.Log.Info("executeDailyTasks");
                //
                // -- summary (must be first)
                VisitSummaryClass.executeDailyTasks(cp, env);
                ViewingSummaryClass.executeDailyTasks(cp, env);

                cp.Db.SQLTimeout = TimeoutSave;
                return "";
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                cp.Site.LogAlarm("Housekeep, exception, ex [" + ex + "]");
                throw;
            }
        }
    }
}
