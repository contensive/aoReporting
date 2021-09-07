using Contensive.Addons.Reporting.Processor.Addons.Housekeep;
using Contensive.BaseClasses;
using System;
using System.Xml;

namespace Contensive.Addons.Reporting.Processor.Addons.Housekeep {
    /// <summary>
    /// Housekeep this content
    /// </summary>
    public static class ViewingSummaryClass {
        //
        //====================================================================================================
        /// <summary>
        /// execute hourly tasks
        /// </summary>
        /// <param name="core"></param>
        public static void executeHourlyTasks(CPBaseClass cp) {
            try {
                //
                cp.Log.Info("Housekeep, executeHourlyTasks, ViewingSummary");
                //
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                cp.Site.LogAlarm("Housekeep, exception, ex [" + ex + "]");
                throw;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// execute Daily Tasks
        /// </summary>
        /// <param name="core"></param>
        /// <param name="env"></param>
        public static void executeDailyTasks(CPBaseClass cp, HouseKeepEnvironmentModel env) {
            try {
                //
                cp.Log.Info("Housekeep, viewingsummary");
                DateTime datePtr = default;
                using (CPCSBaseClass csData = cp.CSNew()) {
                    if (!csData.OpenSQL(HousekeepController.getSQLSelect("ccviewingsummary", "DateNumber", "TimeDuration=24 and DateNumber>=" + env.oldestVisitSummaryWeCareAbout.Date.ToOADate(), "DateNumber Desc", "", 1))) {
                        datePtr = env.oldestVisitSummaryWeCareAbout;
                    }
                    else {
                        datePtr = DateTime.MinValue.AddDays(csData.GetNumber("DateNumber"));
                    }
                }
                if (datePtr < env.oldestVisitSummaryWeCareAbout) { datePtr = env.oldestVisitSummaryWeCareAbout; }
                pageViewSummary(cp, datePtr, env.yesterday, 24, env.oldestVisitSummaryWeCareAbout);
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                cp.Site.LogAlarm("Housekeep, exception, ex [" + ex + "]");
                throw;
            }
        }






        //====================================================================================================
        /// <summary>
        /// Summarize the page views, excludes non-cookie visits, excludes administrator and developer visits, excludes authenticated users with ExcludeFromReporting
        /// </summary>
        /// <param name="core"></param>
        /// <param name="StartTimeDate"></param>
        /// <param name="EndTimeDate"></param>
        /// <param name="HourDuration"></param>
        /// <param name="OldestVisitSummaryWeCareAbout"></param>
        //
        public static void pageViewSummary(CPBaseClass cp, DateTime StartTimeDate, DateTime EndTimeDate, int HourDuration, DateTime OldestVisitSummaryWeCareAbout) {
            int hint = 0;
            string hinttxt = "";
            try {
                XmlDocument LibraryCollections = new XmlDocument();
                XmlDocument LocalCollections = new XmlDocument();
                XmlDocument Doc = new XmlDocument();
                {
                    hint = 1;
                    DateTime PeriodStart = default;
                    PeriodStart = StartTimeDate;
                    if (PeriodStart < OldestVisitSummaryWeCareAbout) {
                        PeriodStart = OldestVisitSummaryWeCareAbout;
                    }
                    DateTime PeriodDatePtr = default;
                    PeriodDatePtr = PeriodStart.Date;
                    while (PeriodDatePtr < EndTimeDate) {
                        hint = 2;
                        //
                        hinttxt = ", HourDuration [" + HourDuration + "], PeriodDatePtr [" + PeriodDatePtr + "], PeriodDatePtr.AddHours(HourDuration / 2.0) [" + PeriodDatePtr.AddHours(HourDuration / 2.0) + "]";
                        int DateNumber = HousekeepController.encodeInteger((PeriodDatePtr - default(DateTime)).TotalDays);
                        // encodeInteger(PeriodDatePtr.AddHours(HourDuration / 2.0).ToOADate());
                        int TimeNumber = HousekeepController.encodeInteger(PeriodDatePtr.TimeOfDay.TotalHours);
                        DateTime DateStart = default;
                        DateStart = PeriodDatePtr.Date;
                        DateTime DateEnd = default;
                        DateEnd = PeriodDatePtr.AddHours(HourDuration).Date;
                        string PageTitle = "";
                        int PageId = 0;
                        int PageViews = 0;
                        int AuthenticatedPageViews = 0;
                        int MobilePageViews = 0;
                        int BotPageViews = 0;
                        int NoCookiePageViews = 0;
                        //
                        // Loop through all the pages hit during this period
                        //
                        //
                        // for now ignore the problem caused by addons like Blogs creating multiple 'pages' within on pageid
                        // One way to handle this is to expect the addon to set something unquie in he page title
                        // then use the title to distinguish a page. The problem with this is the current system puts the
                        // visit number and page number in the name. if we select on district name, they will all be.
                        //
                        using (CPCSBaseClass csPages = cp.CSNew()) {
                            string sql = "select distinct recordid,pagetitle from ccviewings h"
                                + " where (h.recordid<>0)"
                                + " and(h.dateadded>=" + cp.Db.EncodeSQLDate(DateStart) + ")"
                                + " and (h.dateadded<" + cp.Db.EncodeSQLDate(DateEnd) + ")"
                                + " and((h.ExcludeFromAnalytics is null)or(h.ExcludeFromAnalytics=0))"
                                + "order by recordid";
                            hint = 3;
                            if (!csPages.OpenSQL(sql)) {
                                //
                                // no hits found - add or update a single record for this day so we know it has been calculated
                                csPages.Open("Page View Summary", "(timeduration=" + HourDuration + ")and(DateNumber=" + DateNumber + ")and(TimeNumber=" + TimeNumber + ")and(pageid=" + PageId + ")and(pagetitle=" + cp.Db.EncodeSQLText(PageTitle) + ")");
                                if (!csPages.OK()) {
                                    csPages.Close();
                                    csPages.Insert("Page View Summary");
                                }
                                //
                                if (csPages.OK()) {
                                    csPages.SetField("name", HourDuration + " hr summary for " + DateTime.MinValue.AddDays(DateNumber) + " " + TimeNumber + ":00, " + PageTitle);
                                    csPages.SetField("DateNumber", DateNumber);
                                    csPages.SetField("TimeNumber", TimeNumber);
                                    csPages.SetField("TimeDuration", HourDuration);
                                    csPages.SetField("PageViews", PageViews);
                                    csPages.SetField("PageID", PageId);
                                    csPages.SetField("PageTitle", PageTitle);
                                    csPages.SetField("AuthenticatedPageViews", AuthenticatedPageViews);
                                    csPages.SetField("NoCookiePageViews", NoCookiePageViews);
                                    {
                                        csPages.SetField("MobilePageViews", MobilePageViews);
                                        csPages.SetField("BotPageViews", BotPageViews);
                                    }
                                }
                                csPages.Close();
                                hint = 4;
                            }
                            else {
                                hint = 5;
                                //
                                // add an entry for each page hit on this day
                                //
                                while (csPages.OK()) {
                                    PageId = csPages.GetInteger("recordid");
                                    PageTitle = csPages.GetText("pagetitle");
                                    string baseCriteria = ""
                                        + " (h.recordid=" + PageId + ")"
                                        + " "
                                        + " and(h.dateadded>=" + cp.Db.EncodeSQLDate(DateStart) + ")"
                                        + " and(h.dateadded<" + cp.Db.EncodeSQLDate(DateEnd) + ")"
                                        + " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))"
                                        + " and((h.ExcludeFromAnalytics is null)or(h.ExcludeFromAnalytics=0))"
                                        + "";
                                    if (!string.IsNullOrEmpty(PageTitle)) {
                                        baseCriteria = baseCriteria + "and(h.pagetitle=" + cp.Db.EncodeSQLText(PageTitle) + ")";
                                    }
                                    hint = 6;
                                    //
                                    // Total Page Views
                                    using (CPCSBaseClass csPageViews = cp.CSNew()) {
                                        sql = "select count(h.id) as cnt"
                                            + " from ccviewings h left join ccvisits v on h.visitid=v.id"
                                            + " where " + baseCriteria + " and (v.CookieSupport<>0)"
                                            + "";
                                        csPageViews.OpenSQL(sql);
                                        if (csPageViews.OK()) {
                                            PageViews = csPageViews.GetInteger("cnt");
                                        }
                                    }
                                    //
                                    // Authenticated Visits
                                    //
                                    using (CPCSBaseClass csAuthPages = cp.CSNew()) {
                                        sql = "select count(h.id) as cnt"
                                            + " from ccviewings h left join ccvisits v on h.visitid=v.id"
                                            + " where " + baseCriteria + " and(v.CookieSupport<>0)"
                                            + " and(v.visitAuthenticated<>0)"
                                            + "";
                                        csAuthPages.OpenSQL(sql);
                                        if (csAuthPages.OK()) {
                                            AuthenticatedPageViews = csAuthPages.GetInteger("cnt");
                                        }
                                    }
                                    //
                                    // No Cookie Page Views
                                    //
                                    using (CPCSBaseClass csNoCookie = cp.CSNew()) {
                                        sql = "select count(h.id) as NoCookiePageViews"
                                            + " from ccviewings h left join ccvisits v on h.visitid=v.id"
                                            + " where " + baseCriteria + " and((v.CookieSupport=0)or(v.CookieSupport is null))"
                                            + "";
                                        csNoCookie.OpenSQL(sql);
                                        if (csNoCookie.OK()) {
                                            NoCookiePageViews = csNoCookie.GetInteger("NoCookiePageViews");
                                        }
                                    }
                                    //
                                    //
                                    // Mobile Visits
                                    using (CPCSBaseClass csMobileVisits = cp.CSNew()) {
                                        sql = "select count(h.id) as cnt"
                                            + " from ccviewings h left join ccvisits v on h.visitid=v.id"
                                            + " where " + baseCriteria + " and(v.CookieSupport<>0)"
                                            + " and(v.mobile<>0)"
                                            + "";
                                        csMobileVisits.OpenSQL(sql);
                                        if (csMobileVisits.OK()) {
                                            MobilePageViews = csMobileVisits.GetInteger("cnt");
                                        }
                                    }
                                    //
                                    // Bot Visits
                                    using (CPCSBaseClass csBotVisits =  cp.CSNew()) {
                                        sql = "select count(h.id) as cnt"
                                            + " from ccviewings h left join ccvisits v on h.visitid=v.id"
                                            + " where " + baseCriteria + " and(v.CookieSupport<>0)"
                                            + " and(v.bot<>0)"
                                            + "";
                                        csBotVisits.OpenSQL(sql);
                                        if (csBotVisits.OK()) {
                                            BotPageViews = csBotVisits.GetInteger("cnt");
                                        }
                                    }
                                    //
                                    // Add or update the Visit Summary Record
                                    //
                                    using (CPCSBaseClass csPVS = cp.CSNew()) {
                                        if (!csPVS.Open("Page View Summary", "(timeduration=" + HourDuration + ")and(DateNumber=" + DateNumber + ")and(TimeNumber=" + TimeNumber + ")and(pageid=" + PageId + ")and(pagetitle=" + cp.Db.EncodeSQLText(PageTitle) + ")")) {
                                            csPVS.Insert("Page View Summary");
                                        }
                                        //
                                        if (csPVS.OK()) {
                                            hint = 11;
                                            string PageName = "";
                                            if (string.IsNullOrEmpty(PageTitle)) {
                                                PageName = HousekeepController.getRecordName(cp, "page content", PageId);
                                                csPVS.SetField("name", HourDuration + " hr summary for " + DateTime.MinValue.AddDays(DateNumber) + " " + TimeNumber + ":00, " + PageName);
                                                csPVS.SetField("PageTitle", PageName);
                                            }
                                            else {
                                                csPVS.SetField("name", HourDuration + " hr summary for " + DateTime.MinValue.AddDays(DateNumber) + " " + TimeNumber + ":00, " + PageTitle);
                                                csPVS.SetField("PageTitle", PageTitle);
                                            }
                                            csPVS.SetField("DateNumber", DateNumber);
                                            csPVS.SetField("TimeNumber", TimeNumber);
                                            csPVS.SetField("TimeDuration", HourDuration);
                                            csPVS.SetField("PageViews", PageViews);
                                            csPVS.SetField("PageID", PageId);
                                            csPVS.SetField("AuthenticatedPageViews", AuthenticatedPageViews);
                                            csPVS.SetField("NoCookiePageViews", NoCookiePageViews);
                                            hint = 12;
                                            {
                                                csPVS.SetField("MobilePageViews", MobilePageViews);
                                                csPVS.SetField("BotPageViews", BotPageViews);
                                            }
                                        }
                                    }
                                    csPages.GoNext();
                                }
                            }
                        }
                        PeriodDatePtr = PeriodDatePtr.AddHours(HourDuration);
                    }
                }
                return;
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex, "hint [" + hint + "]");
            }
        }
        

    }
}