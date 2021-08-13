using Contensive.BaseClasses;
using System;
//
namespace Contensive.Addons.Reporting.Processor.Addons.Housekeep {
    //
    //====================================================================================================
    /// <summary>
    /// housekeep environment, to facilitate argument passing
    /// </summary>
    public class HouseKeepEnvironmentModel {
        //
        public CPBaseClass cp;
        //
        //====================================================================================================
        /// <summary>
        /// calls to housekeeping will force both the hourly and daily to run
        /// </summary>
        public bool forceHousekeep {
            get {
                return cp.Doc.GetBoolean("force");
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// the last time housekeep was checked
        /// </summary>
        // public DateTime lastCheckDateTime { get { return cp.Site.GetDate("housekeep, last check", DateTime.Now); } }
        //
        //====================================================================================================
        /// <summary>
        /// the last time housekeep was ran
        /// </summary>
         public DateTime lastRunDateTime { get { return cp.Site.GetDate("housekeep, last run", DateTime.Now); } }
        //
        //====================================================================================================
        /// <summary>
        /// The hour of the day when daily housekeep should run
        /// </summary>
        public int serverHousekeepHour { get { return cp.Site.GetInteger("housekeep, run time hour", 2); } }
        //
        //====================================================================================================
        /// <summary>
        /// day before current mockable date
        /// </summary>
        public DateTime yesterday { get { return DateTime.Now.AddDays(-1).Date; } }
        //
        //====================================================================================================
        /// <summary>
        /// oldest visit we care about (30 days)
        /// </summary>
        public DateTime oldestVisitSummaryWeCareAbout {
            get {
                DateTime oldestVisitSummaryWeCareAbout = DateTime.Now.Date.AddDays(-30);
                if (oldestVisitSummaryWeCareAbout < visitArchiveDate) {
                    oldestVisitSummaryWeCareAbout = visitArchiveDate;
                }
                return oldestVisitSummaryWeCareAbout;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// days that we keep simple archive records like visits, viewings, activitylogs. Not for summary files like visitsummary
        /// minimum 2 days for sites with no archive features
        /// 
        /// </summary>
        public int archiveAgeDays {
            get {
                //
                // -- Get ArchiveAgeDays - use this as the oldest data they care about
                int visitArchiveAgeDays = cp.Site.GetInteger("ArchiveRecordAgeDays", 2);
                if (visitArchiveAgeDays < 2) {
                    visitArchiveAgeDays = 2;
                    cp.Site.SetProperty("ArchiveRecordAgeDays", 2);
                }
                return visitArchiveAgeDays;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// The date before which we delete archives
        /// </summary>
        public DateTime visitArchiveDate {
            get {
                return DateTime.Now.AddDays(-archiveAgeDays).Date;
            }
        }
        public bool runDailyTasks {
            get {
                return ((DateTime.Now > lastRunDateTime.Date) && (serverHousekeepHour < DateTime.Now.Hour));
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="core"></param>
        public HouseKeepEnvironmentModel(CPBaseClass cp) {
            try {
                this.cp = cp;
                cp.Site.SetProperty("housekeep, last check", DateTime.Now);
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
        }
    }
}