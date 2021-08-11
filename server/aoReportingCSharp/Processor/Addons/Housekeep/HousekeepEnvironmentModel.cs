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
        /*
        //
        //====================================================================================================
        /// <summary>
        /// returns true if daily has not run today, and it is after the house-keep-hour
        /// </summary>
        public bool runDailyTasks {
            get {
                return ((cp.dateTimeNowMockable.Date > lastCheckDateTime.Date) && (serverHousekeepHour < core.dateTimeNowMockable.Hour));
            }
        }
        */
        //
        //====================================================================================================
        /// <summary>
        /// the last time housekeep was run
        /// </summary>
        public DateTime lastCheckDateTime { get { return cp.Site.GetDate("housekeep, last check", DateTime.Now); } }
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
        /// 90 days ago
        /// </summary>
        public DateTime aLittleWhileAgo { get { return DateTime.Now.AddDays(-90).Date; } }
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
        //
        //====================================================================================================
        /// <summary>
        /// how long we keep guest records
        /// </summary>
        public int guestArchiveAgeDays {
            get {
                //
                // -- Get GuestArchiveAgeDays
                int guestArchiveAgeDays = cp.Site.GetInteger("ArchivePeopleAgeDays", 2);
                if (guestArchiveAgeDays < 2) {
                    guestArchiveAgeDays = 2;
                    cp.Site.SetProperty("ArchivePeopleAgeDays", guestArchiveAgeDays);
                }
                return guestArchiveAgeDays;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// how many days the email drop and email log data are kept
        /// </summary>
        public int emailDropArchiveAgeDays {
            get {
                //
                // -- Get EmailDropArchiveAgeDays
                int emailDropArchiveAgeDays = cp.Site.GetInteger("ArchiveEmailDropAgeDays", 90);
                if (emailDropArchiveAgeDays < 2) {
                    emailDropArchiveAgeDays = 2;
                    cp.Site.SetProperty("ArchiveEmailDropAgeDays", emailDropArchiveAgeDays);
                }
                if (emailDropArchiveAgeDays > 365) {
                    emailDropArchiveAgeDays = 365;
                    cp.Site.SetProperty("ArchiveEmailDropAgeDays", emailDropArchiveAgeDays);
                }
                return emailDropArchiveAgeDays;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// how long to keep no-cookie visits
        /// </summary>
        public bool archiveDeleteNoCookie { get { return cp.Site.GetBoolean("ArchiveDeleteNoCookie", true); } }
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