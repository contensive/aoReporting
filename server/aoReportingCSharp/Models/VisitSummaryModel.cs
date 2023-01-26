﻿using Contensive.BaseClasses;
using Contensive.Reporting.Controllers;
using System;

namespace Contensive.Reporting.Models {
    /// <summary>
    /// Housekeep this content
    /// </summary>
    public class VisitSummaryModel : Contensive.Models.Db.VisitSummaryModel {
        //
        //====================================================================================================
        /// <summary>
        /// execute hourly tasks
        /// </summary>
        /// <param name="core"></param>
        public static void executeHourlyTasks(CPBaseClass cp) {
            try {
                //
                cp.Log.Info("Housekeep, executeHourlyTasks, VisitSummary");
                //
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                cp.Site.LogAlarm("Housekeep, exception, ex [" + ex + "]");
                throw;
            }
        }
        //
        //=========================================================================================
        /// <summary>
        /// summarized visits hourly
        /// </summary>
        /// <param name="core"></param>
        /// <param name="env"></param>
        public static void executeDailyTasks(CPBaseClass cp, HouseKeepEnvironmentModel env) {
            try {
                //
                cp.Log.Info("Housekeep, visitsummary");
                //
                //bool newHour = (DateTime.Now.Hour != env.lastRunDateTime.Hour);
                //if (env.forceHousekeep || newHour) {
                //
                // Set NextSummaryStartDate based on the last time we ran hourly summarization
                //
                DateTime LastTimeSummaryWasRun = env.visitArchiveDate;
                cp.Db.SQLTimeout = 180;
                using (CPCSBaseClass csData = cp.CSNew()) {
                    if (csData.OpenSQL(HousekeepController.getSQLSelect("ccVisitSummary", "DateAdded", "(timeduration=1)and(Dateadded>" + cp.Db.EncodeSQLDate(env.visitArchiveDate) + ")", "id Desc", "", 1))) {
                        LastTimeSummaryWasRun = csData.GetDate("DateAdded");
                        cp.Log.Info("Update hourly visit summary, last time summary was run was [" + LastTimeSummaryWasRun + "]");
                    }
                    else {
                        cp.Log.Info("Update hourly visit summary, no hourly summaries were found, set start to [" + LastTimeSummaryWasRun + "]");
                    }
                }
                DateTime NextSummaryStartDate = LastTimeSummaryWasRun;
                //
                // Each hourly entry includes visits that started during that hour, but we do not know when they finished (maybe during last hour)
                //   Find the oldest starttime of all the visits with endtimes after the LastTimeSummaryWasRun. Resummarize all periods
                //   from then to now
                //
                //   For the past 24 hours, find the oldest visit with the last viewing during the last hour
                //
                DateTime StartOfHour = (new DateTime(LastTimeSummaryWasRun.Year, LastTimeSummaryWasRun.Month, LastTimeSummaryWasRun.Day, LastTimeSummaryWasRun.Hour, 1, 1)).AddHours(-1); // (Int(24 * LastTimeSummaryWasRun) / 24) - PeriodStep
                DateTime OldestDateAdded = StartOfHour;
                cp.Db.SQLTimeout = 180;
                using (CPCSBaseClass csData = cp.CSNew()) {
                    if (csData.OpenSQL(HousekeepController.getSQLSelect("ccVisits", "DateAdded", "LastVisitTime>" + cp.Db.EncodeSQLDate(StartOfHour), "dateadded", "", 1))) {
                        OldestDateAdded = csData.GetDate("DateAdded");
                        if (OldestDateAdded < NextSummaryStartDate) {
                            NextSummaryStartDate = OldestDateAdded;
                            cp.Log.Info("Update hourly visit summary, found a visit with the last viewing during the past hour. It started [" + OldestDateAdded + "], before the last summary was run.");
                        }
                    }
                }
                DateTime PeriodStartDate = DateTime.Now.AddDays(-90);
                double PeriodStep = 1;
                int HoursPerDay = 0;
                cp.Db.SQLTimeout = 180;
                //
                // -- search for day with missing visit summaries in the 90 days before yesterday
                DateTime DateofMissingSummary = DateTime.MinValue;
                for (double PeriodDatePtr = PeriodStartDate.ToOADate(); PeriodDatePtr <= OldestDateAdded.ToOADate(); PeriodDatePtr += PeriodStep) {
                    //
                    // Verify there are 24 hour records for every day back the past 90 days
                    //
                    using (CPCSBaseClass csData = cp.CSNew()) {
                        if (csData.OpenSQL("select count(id) as HoursPerDay from ccVisitSummary where TimeDuration=1 and DateNumber=" + HousekeepController.encodeInteger(PeriodDatePtr) + " group by DateNumber")) {
                            HoursPerDay = csData.GetInteger("HoursPerDay");
                        }
                    }
                    if (HoursPerDay < 24) {
                        DateofMissingSummary = DateTime.FromOADate(PeriodDatePtr);
                        break;
                    }
                }
                if ((DateofMissingSummary != DateTime.MinValue) && (DateofMissingSummary < NextSummaryStartDate)) {
                    cp.Log.Info("Found a missing hourly period in the visit summary table [" + DateofMissingSummary + "], it only has [" + HoursPerDay + "] hourly summaries.");
                    NextSummaryStartDate = DateofMissingSummary;
                }
                {
                    //
                    // Now summarize all visits during all hourly periods between OldestDateAdded and the previous Hour
                    //
                    cp.Log.Info("Summaryize visits hourly, starting [" + NextSummaryStartDate + "]");
                    PeriodStep = (double)1 / (double)24;
                    string BuildVersion = cp.Site.GetText("BuildVersion");
                    //make sure it is only checking up to the last finished hour (this prevents the record for 5pm being added at 5pm when it should be added after 6pm)
                    DateTime endOfHour = (new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, 1, 1)).AddHours(-1);
                    summarizePeriod(cp, env, NextSummaryStartDate, endOfHour, 1, BuildVersion, env.oldestVisitSummaryWeCareAbout);
                }
                {
                    string BuildVersion = cp.Site.GetText("BuildVersion");
                    //
                    // Find missing daily summaries, summarize that date
                    //
                    string SQL = HousekeepController.getSQLSelect("ccVisitSummary", "DateNumber", "TimeDuration=24 and DateNumber>=" + env.oldestVisitSummaryWeCareAbout.Date.ToOADate(), "DateNumber,TimeNumber", "", 100000);

                    using (CPCSBaseClass csData = cp.CSNew()) {
                        csData.OpenSQL(SQL);
                        DateTime datePtr = env.oldestVisitSummaryWeCareAbout;
                        while (datePtr <= env.yesterday) {
                            if (!csData.OK()) {
                                //
                                // Out of data, start with this DatePtr
                                //
                                VisitSummaryModel.summarizePeriod(cp, env, datePtr, datePtr, 24, BuildVersion, env.oldestVisitSummaryWeCareAbout);
                            }
                            else {
                                DateTime workingDate = DateTime.MinValue.AddDays(csData.GetInteger("DateNumber"));
                                if (datePtr < workingDate) {
                                    //
                                    // There are missing dates, update them
                                    //
                                    VisitSummaryModel.summarizePeriod(cp, env, datePtr, workingDate.AddDays(-1), 24, BuildVersion, env.oldestVisitSummaryWeCareAbout);
                                }
                            }
                            if (csData.OK()) {
                                //
                                // if there is more data, go to the next record
                                //
                                csData.GoNext();
                            }
                            datePtr = datePtr.AddDays(1).Date;
                        }
                        csData.Close();
                    }
                }

                //}       //end if eith force or newhour
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                cp.Site.LogAlarm("Housekeep, exception, ex [" + ex + "]");
                throw;
            }
        }
        //
        //=========================================================================================
        // Summarize the visits
        //   excludes non-cookie visits
        //   excludes administrator and developer visits
        //   excludes authenticated users with ExcludeFromReporting
        //
        // Average time on site
        //
        //   Example data
        //   Pages       TimeToLastHit
        //   1           0           - hit 1 page, start time = last time
        //   10          3510        - hit 10 pages, first hit time - last hit time = 3510
        //   2           30          - hit 2 pages, first hit time - last hit time = 30
        //
        // AveReadTime is the average time spent reading pages
        //   this is calculated from the multi-page visits only
        //   = MultiPageTimeToLastHitSum / ( MultiPageHitCnt - MultiPageVisitCnt )
        //   = ( 3510 + 30 ) / ((10+2) - 2 )
        //   = 354
        //
        // TotalTimeOnSite is the total time people spent reading pages
        //   There are two parts:
        //     1) the TimeToLastHit, which covers all but the last hit of each visit
        //     2) assume the last hit of each visit is the AveReadTime
        //   = MultiPageTimeToLastHitSum + ( AveReadTime * VisitCnt )
        //   = ( 3510 + 30 ) + ( 354 * 3 )
        //   = 4602
        //
        // AveTimeOnSite
        //   = TotalTimeOnSite / TotalHits
        //   = 4602 / 3
        //   = 1534
        //
        //=========================================================================================
        //
        private static void summarizePeriod(CPBaseClass cp, HouseKeepEnvironmentModel env, DateTime StartTimeDate, DateTime EndTimeDate, int HourDuration, string BuildVersion, DateTime OldestVisitSummaryWeCareAbout) {
            try {
                //
                //if (string.CompareOrdinal(BuildVersion, cp.SiteodeVersion()) >= 0) {
                DateTime PeriodStart = default;
                PeriodStart = StartTimeDate;
                if (PeriodStart < OldestVisitSummaryWeCareAbout) {
                    PeriodStart = OldestVisitSummaryWeCareAbout;
                }
                double StartTimeHoursSinceMidnight = PeriodStart.TimeOfDay.TotalHours;
                PeriodStart = PeriodStart.Date.AddHours(StartTimeHoursSinceMidnight);
                DateTime PeriodDatePtr = default;
                PeriodDatePtr = PeriodStart;
                while (PeriodDatePtr <= EndTimeDate) {
                    //
                    int DateNumber = HousekeepController.encodeInteger(PeriodDatePtr.AddHours(HourDuration / 2.0).ToOADate());
                    int TimeNumber = HousekeepController.encodeInteger(PeriodDatePtr.TimeOfDay.TotalHours);
                    DateTime DateStart = default;
                    DateStart = PeriodDatePtr.Date;
                    DateTime DateEnd = default;
                    DateEnd = PeriodDatePtr.AddHours(HourDuration).Date;
                    //
                    // No Cookie Visits
                    //
                    string SQL = "select count(v.id) as NoCookieVisits"
                        + " from ccvisits v"
                        + " where (v.CookieSupport<>1)"
                        + " and(v.dateadded>=" + cp.Db.EncodeSQLDate(DateStart) + ")"
                        + " and (v.dateadded<" + cp.Db.EncodeSQLDate(DateEnd) + ")"
                        + " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))"
                        + "";
                    int NoCookieVisits = 0;
                    using (CPCSBaseClass csData = cp.CSNew()) {
                        cp.Db.SQLTimeout = 180;
                        csData.OpenSQL(SQL);
                        if (csData.OK()) {
                            NoCookieVisits = csData.GetInteger("NoCookieVisits");
                        }
                    }
                    //
                    // Total Visits
                    //
                    SQL = "select count(v.id) as VisitCnt ,Sum(v.PageVisits) as HitCnt ,sum(v.TimetoLastHit) as TimeOnSite"
                        + " from ccvisits v"
                        + " where (v.CookieSupport<>0)"
                        + " and(v.dateadded>=" + cp.Db.EncodeSQLDate(DateStart) + ")"
                        + " and (v.dateadded<" + cp.Db.EncodeSQLDate(DateEnd) + ")"
                        + " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))"
                        + "";
                    //
                    int VisitCnt = 0;
                    int HitCnt = 0;
                    using (CPCSBaseClass csData = cp.CSNew()) {
                        cp.Db.SQLTimeout = 180;
                        csData.OpenSQL(SQL);
                        if (csData.OK()) {
                            VisitCnt = csData.GetInteger("VisitCnt");
                            HitCnt = csData.GetInteger("HitCnt");
                            double TimeOnSite = csData.GetNumber("TimeOnSite");
                        }
                    }
                    //
                    // -- Visits by new visitors
                    int NewVisitorVisits = 0;
                    int SinglePageVisits = 0;
                    int AuthenticatedVisits = 0;
                    int MobileVisits = 0;
                    int BotVisits = 0;
                    double AveTimeOnSite = 0;
                    if (VisitCnt > 0) {
                        SQL = "select count(v.id) as NewVisitorVisits"
                            + " from ccvisits v"
                            + " where (v.CookieSupport<>0)"
                            + " and(v.dateadded>=" + cp.Db.EncodeSQLDate(DateStart) + ")"
                            + " and (v.dateadded<" + cp.Db.EncodeSQLDate(DateEnd) + ")"
                            + " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))"
                            + " and(v.VisitorNew<>0)"
                            + "";
                        using (CPCSBaseClass csData = cp.CSNew()) {
                            cp.Db.SQLTimeout = 180;
                            csData.OpenSQL(SQL);
                            if (csData.OK()) {
                                NewVisitorVisits = csData.GetInteger("NewVisitorVisits");
                            }
                        }
                        //
                        // Single Page Visits
                        //
                        SQL = "select count(v.id) as SinglePageVisits"
                            + " from ccvisits v"
                            + " where (v.CookieSupport<>0)"
                            + " and(v.dateadded>=" + cp.Db.EncodeSQLDate(DateStart) + ")"
                            + " and (v.dateadded<" + cp.Db.EncodeSQLDate(DateEnd) + ")"
                            + " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))"
                            + " and(v.PageVisits=1)"
                            + "";
                        using (CPCSBaseClass csData = cp.CSNew()) {
                            cp.Db.SQLTimeout = 180;
                            csData.OpenSQL(SQL);
                            if (csData.OK()) {
                                SinglePageVisits = csData.GetInteger("SinglePageVisits");
                            }
                        }
                        //
                        // Multipage Visits
                        //
                        SQL = "select count(v.id) as VisitCnt ,sum(v.PageVisits) as HitCnt ,sum(v.TimetoLastHit) as TimetoLastHitSum "
                            + " from ccvisits v"
                            + " where (v.CookieSupport<>0)"
                            + " and(v.dateadded>=" + cp.Db.EncodeSQLDate(DateStart) + ")"
                            + " and (v.dateadded<" + cp.Db.EncodeSQLDate(DateEnd) + ")"
                            + " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))"
                            + " and(PageVisits>1)"
                            + "";
                        int MultiPageHitCnt = 0;
                        int MultiPageVisitCnt = 0;
                        double MultiPageTimetoLastHitSum = 0;
                        using (CPCSBaseClass csData = cp.CSNew()) {
                            cp.Db.SQLTimeout = 180;
                            csData.OpenSQL(SQL);
                            if (csData.OK()) {
                                MultiPageVisitCnt = csData.GetInteger("VisitCnt");
                                MultiPageHitCnt = csData.GetInteger("HitCnt");
                                MultiPageTimetoLastHitSum = csData.GetNumber("TimetoLastHitSum");
                            }
                        }
                        //
                        // Authenticated Visits
                        //
                        SQL = "select count(v.id) as AuthenticatedVisits "
                            + " from ccvisits v"
                            + " where (v.CookieSupport<>0)"
                            + " and(v.dateadded>=" + cp.Db.EncodeSQLDate(DateStart) + ")"
                            + " and (v.dateadded<" + cp.Db.EncodeSQLDate(DateEnd) + ")"
                            + " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))"
                            + " and(VisitAuthenticated<>0)"
                            + "";
                        using (CPCSBaseClass csData = cp.CSNew()) {
                            cp.Db.SQLTimeout = 180;
                            csData.OpenSQL(SQL);
                            if (csData.OK()) {
                                AuthenticatedVisits = csData.GetInteger("AuthenticatedVisits");
                            }
                        }
                        // 
                        //
                        // Mobile Visits
                        //
                        SQL = "select count(v.id) as cnt "
                            + " from ccvisits v"
                            + " where (v.CookieSupport<>0)"
                            + " and(v.dateadded>=" + cp.Db.EncodeSQLDate(DateStart) + ")"
                            + " and (v.dateadded<" + cp.Db.EncodeSQLDate(DateEnd) + ")"
                            + " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))"
                            + " and(Mobile<>0)"
                            + "";
                        using (CPCSBaseClass csData = cp.CSNew()) {
                            cp.Db.SQLTimeout = 180;
                            csData.OpenSQL(SQL);
                            if (csData.OK()) {
                                MobileVisits = csData.GetInteger("cnt");
                            }
                        }
                        //
                        // Bot Visits
                        //
                        SQL = "select count(v.id) as cnt "
                            + " from ccvisits v"
                            + " where (v.CookieSupport<>0)"
                            + " and(v.dateadded>=" + cp.Db.EncodeSQLDate(DateStart) + ")"
                            + " and (v.dateadded<" + cp.Db.EncodeSQLDate(DateEnd) + ")"
                            + " and((v.ExcludeFromAnalytics is null)or(v.ExcludeFromAnalytics=0))"
                            + " and(Bot<>0)"
                            + "";
                        using (CPCSBaseClass csData = cp.CSNew()) {
                            cp.Db.SQLTimeout = 180;
                            csData.OpenSQL(SQL);
                            if (csData.OK()) {
                                BotVisits = csData.GetInteger("cnt");
                            }
                        }
                        //
                        if ((MultiPageHitCnt > MultiPageVisitCnt) && (HitCnt > 0)) {
                            int AveReadTime = HousekeepController.encodeInteger(MultiPageTimetoLastHitSum / (MultiPageHitCnt - MultiPageVisitCnt));
                            double TotalTimeOnSite = MultiPageTimetoLastHitSum + (AveReadTime * VisitCnt);
                            AveTimeOnSite = TotalTimeOnSite / VisitCnt;
                        }
                    }
                    //
                    // Add or update the Visit Summary Record
                    //
                    using (CPCSBaseClass csData = cp.CSNew()) {
                        cp.Db.SQLTimeout = 180;
                        csData.Open("Visit Summary", "(timeduration=" + HourDuration + ")and(DateNumber=" + DateNumber + ")and(TimeNumber=" + TimeNumber + ")");
                        if (!csData.OK()) {
                            csData.Close();
                            csData.Insert("Visit Summary");
                        }
                        //
                        if (csData.OK()) {
                            csData.SetField("name", HourDuration + " hr summary for " + DateTime.FromOADate(DateNumber).ToShortDateString() + " " + TimeNumber + ":00");
                            csData.SetField("DateNumber", DateNumber);
                            csData.SetField("TimeNumber", TimeNumber);
                            csData.SetField("Visits", VisitCnt);
                            csData.SetField("PagesViewed", HitCnt);
                            csData.SetField("TimeDuration", HourDuration);
                            csData.SetField("NewVisitorVisits", NewVisitorVisits);
                            csData.SetField("SinglePageVisits", SinglePageVisits);
                            csData.SetField("AuthenticatedVisits", AuthenticatedVisits);
                            csData.SetField("NoCookieVisits", NoCookieVisits);
                            csData.SetField("AveTimeOnSite", AveTimeOnSite);
                            {
                                csData.SetField("MobileVisits", MobileVisits);
                                csData.SetField("BotVisits", BotVisits);
                            }
                        }
                    }
                    PeriodDatePtr = PeriodDatePtr.AddHours(HourDuration);
                }
                {
                    //
                    // Delete any daily visit summary duplicates during this period(keep the first)
                    //
                    string SQL = "delete from ccvisitsummary"
                        + " where id in ("
                        + " select d.id from ccvisitsummary d,ccvisitsummary f"
                        + " where f.datenumber=d.datenumber"
                        + " and f.datenumber>" + env.oldestVisitSummaryWeCareAbout.ToOADate() + " and f.datenumber<" + env.yesterday.ToOADate() + " and f.TimeDuration=24"
                        + " and d.TimeDuration=24"
                        + " and f.id<d.id"
                        + ")";
                    cp.Db.SQLTimeout = 180;
                    cp.Db.ExecuteNonQuery(SQL);
                    ////
                    //// Find missing daily summaries, summarize that date
                    ////
                    //SQL = core.db.getSQLSelect("ccVisitSummary", "DateNumber", "TimeDuration=24 and DateNumber>=" + env.oldestVisitSummaryWeCareAbout.Date.ToOADate(), "DateNumber,TimeNumber");
                    //  using (CPCSBaseClass csData = cp.CSNew()) {
                    //    csData.OpenSQL(SQL);
                    //    DateTime datePtr = env.oldestVisitSummaryWeCareAbout;
                    //    while (datePtr <= env.yesterday) {
                    //        if (!csData.ok()) {
                    //            //
                    //            // Out of data, start with this DatePtr
                    //            //
                    //            VisitSummaryModel.summarizePeriod(core, env, datePtr, datePtr, 24, core.siteProperties.dataBuildVersion, env.oldestVisitSummaryWeCareAbout);
                    //        } else {
                    //            DateTime workingDate = DateTime.MinValue.AddDays(csData.GetInteger("DateNumber"));
                    //            if (datePtr < workingDate) {
                    //                //
                    //                // There are missing dates, update them
                    //                //
                    //                VisitSummaryModel.summarizePeriod(core, env, datePtr, workingDate.AddDays(-1), 24, core.siteProperties.dataBuildVersion, env.oldestVisitSummaryWeCareAbout);
                    //            }
                    //        }
                    //        if (csData.ok()) {
                    //            //
                    //            // if there is more data, go to the next record
                    //            //
                    //            csData.goNext();
                    //        }
                    //        datePtr = datePtr.AddDays(1).Date;
                    //    }
                    //    csData.close();
                    //}
                }
                //end code version if}
                //
                return;
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
            }
        }


    }
}