
using Contensive.BaseClasses;
using System;

namespace Contensive.Reporting {
    public class CurrentActivityReportAddon : AddonBaseClass {
        //
        internal const string addonGuidUsersOnlineReport = "{a5439430-ed28-4d72-a9ed-50fb36145955}";
        
        //
        //====================================================================================================
        //
        public override object Execute(CPBaseClass cp) {
            try {
                cp.Site.ErrorReport("Call to incorrect duplicate UsersOnlineReport. See Reporting collection. Find click origin and fix.");
                return cp.Addon.Execute(addonGuidUsersOnlineReport);
            } catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}
