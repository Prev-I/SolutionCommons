using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Solution.Core.Extensions
{
    /// <summary>
    /// Static Extensions on Decimal type
    /// </summary>
    public static class DecimalExt
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string ToStringOrDefault(this decimal? source, string format)
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
        public static string ToStringOrDefault(this decimal? source, string format, string defaultValue)
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
    }
}
