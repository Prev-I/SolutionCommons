using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Solution.Core.Extensions
{
    /// <summary>
    /// Static Extensions on DateTime type
    /// </summary>
    public static class DateTimeExt
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ToStringOrDefault(this DateTime? source, string format)
        {
            return ToStringOrDefault(source, format, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="format"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static string ToStringOrDefault(this DateTime? source, string format, string defaultValue)
        {
            if (source.HasValue)
            {
                return source.Value.ToString(format);
            }
            else
            {
                return string.IsNullOrEmpty(defaultValue) ? defaultValue : string.Empty;
            }
        }

        /// <summary>
        /// Check if the "source" date is between "dtFrom" and "dtTo"
        /// </summary>
        public static bool IsBetween(this DateTime source, DateTime dtFrom, DateTime dtTo)
        {
            return source >= dtFrom && source <= dtTo;
        }
    }
}
