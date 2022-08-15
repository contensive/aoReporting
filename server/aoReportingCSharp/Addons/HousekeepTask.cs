using Contensive.BaseClasses;
using Contensive.Reporting.Models;
using System;
//
namespace Contensive.Reporting {
    /// <summary>
    /// support for housekeeping functions for reporting
    /// </summary>
    public class HousekeepTask : AddonBaseClass {
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
                VisitSummaryModel.executeHourlyTasks(cp);
                ViewingSummaryModel.executeHourlyTasks(cp);

                // -- daily tasks
                cp.Log.Info("executeDailyTasks");
                //
                // -- summary (must be first)
                VisitSummaryModel.executeDailyTasks(cp, env);
                ViewingSummaryModel.executeDailyTasks(cp, env);
                cp.Site.SetProperty("housekeep, last run", DateTime.Now);


                cp.Db.SQLTimeout = TimeoutSave;
                return "";
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}