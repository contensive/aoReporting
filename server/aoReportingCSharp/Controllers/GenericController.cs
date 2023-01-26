using System;
using Contensive.BaseClasses;
using Contensive.Reporting.Models;


namespace Contensive.Reporting.Controllers {
    public sealed class GenericController {
        private GenericController() {
        }
        // 
        // ====================================================================================================
        /// <summary>
        ///         ''' if date is invalid, set to minValue
        ///         ''' </summary>
        ///         ''' <param name="srcDate"></param>
        ///         ''' <returns></returns>
        public static DateTime encodeMinDate(DateTime srcDate) {
            DateTime returnDate = srcDate;
            if (srcDate < new DateTime(1900, 1, 1))
                returnDate = DateTime.MinValue;
            return returnDate;
        }
        // 
        // ====================================================================================================
        /// <summary>
        ///         ''' if valid date, return the short date, else return blank string 
        ///         ''' </summary>
        ///         ''' <param name="srcDate"></param>
        ///         ''' <returns></returns>
        public static string getShortDateString(DateTime srcDate) {
            string returnString = "";
            DateTime workingDate = encodeMinDate(srcDate);
            if (!isDateEmpty(srcDate))
                returnString = workingDate.ToShortDateString();
            return returnString;
        }
        // 
        // ====================================================================================================
        public static bool isDateEmpty(DateTime srcDate) {
            return (srcDate < new DateTime(1900, 1, 1));
        }
        // 
        // ====================================================================================================
        public static string getSortOrderFromInteger(int id) {
            return id.ToString().PadLeft(7, '0');
        }
        // 
        // ====================================================================================================
        public static string getDateForHtmlInput(DateTime source) {
            if (isDateEmpty(source))
                return "";
            else
                return source.Year.ToString() + "-" + source.Month.ToString().PadLeft(2, '0') + "-" + source.Day.ToString().PadLeft(2, '0');
        }
    }
}
