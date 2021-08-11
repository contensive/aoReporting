
using Contensive.BaseClasses;
using Contensive.Processor.Controllers;
using System;
//
namespace Contensive.Processor.Addons.Housekeeping {
    /// <summary>
    /// support for housekeeping functions for reporting
    /// </summary>
    public class HouseKeepClass : AddonBaseClass {
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
                var env = new HouseKeepEnvironmentModel(core);
                int TimeoutSave = core.db.sqlCommandTimeout;
                core.db.sqlCommandTimeout = 1800;



                // -- hourly tasks
                //
                // -- summaries - must be first
                // VisitSummaryClass.executeHourlyTasks(cp);
                ViewingSummaryClass.executeHourlyTasks(cp);
                // -- daily tasks
                cp.Log.Info("executeDailyTasks");
                //
                // -- summary (must be first)
                //  VisitSummaryClass.executeDailyTasks(cp, env);
                ViewingSummaryClass.executeDailyTasks(cp, env);




                //
                // -- daily tasks
                HousekeepDailyTasksClass.executeDailyTasks(core, env);
                core.db.sqlCommandTimeout = TimeoutSave;
                return "";
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                cp.Site.LogAlarm("Housekeep, exception, ex [" + ex + "]");
                throw;
            }
        }


        //========================================================================
        /// <summary>
        /// Return an sql select based on the arguments
        /// </summary>
        /// <param name="from"></param>
        /// <param name="fieldList"></param>
        /// <param name="where"></param>
        /// <param name="orderBy"></param>
        /// <param name="groupBy"></param>
        /// <param name="recordLimit"></param>
        /// <returns></returns>
        public static string getSQLSelect(string from, string fieldList, string where, string orderBy, string groupBy, int recordLimit) {
            string sql = "select";
            if (recordLimit != 0) { sql += " top " + recordLimit; }
            sql += (string.IsNullOrWhiteSpace(fieldList)) ? " *" : " " + fieldList;
            sql += " from " + from;
            if (!string.IsNullOrWhiteSpace(where)) { sql += " where " + where; }
            if (!string.IsNullOrWhiteSpace(orderBy)) { sql += " order by " + orderBy; }
            if (!string.IsNullOrWhiteSpace(groupBy)) { sql += " group by " + groupBy; }
            return sql;
        }
    }
}
