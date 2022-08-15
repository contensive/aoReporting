using Contensive.BaseClasses;
using System;

namespace Contensive.Reporting.Controllers {
    public class  HousekeepController {

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
        //
        // ====================================================================================================
        //
        public static int encodeInteger(object expression) {
            if (expression == null) { return 0; }
            string trialString = expression.ToString();
            if (int.TryParse(trialString, out int trialInt)) { return trialInt; }
            if (double.TryParse(trialString, out double trialDbl)) { return (int)trialDbl; }
            if (bool.TryParse(trialString, out bool trialBool)) { return (trialBool) ? 1 : 0; }
            return 0;
        }
        //
        //=============================================================
        /// <summary>
        /// Return a record name given the record id. If not record is found, blank is returned.
        /// </summary>
        public static string getRecordName(CPBaseClass cp, string ContentName, int recordID) {
            try {
                using (CPCSBaseClass cs = cp.CSNew()) {
                    if (cs.Open(ContentName, "id=" + recordID.ToString())) {
                        return cs.GetText("name");
                    }
                }
                return "";
            }
            catch (Exception ex) {
                cp.Site.ErrorReport(ex);
                throw;
            }
        }
    }
}
