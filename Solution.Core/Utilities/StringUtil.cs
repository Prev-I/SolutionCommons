using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using log4net;


namespace Solution.Core.Utilities
{
    public static class StringUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        #region String Parsing

        public static T Parse<T>(this object obj)
        {
            if (obj == null) return default(T);

            return obj.ToString().Parse<T>();
        }

        public static T Parse<T>(this string obj)
        {
            return Parse<T>(obj, default(T));
        }

        public static T Parse<T>(this string obj, object defaultValue)
        {
            if (String.IsNullOrEmpty(obj))
            {
                if (defaultValue == null || String.IsNullOrEmpty(defaultValue.ToString()))
                {
                    return (T)default(T);
                }
                else
                {
                    return (T)defaultValue;
                }
            }

            Type t = typeof(T);

            if (t.IsGenericType && (t.GetGenericTypeDefinition() == typeof(Nullable<>)))
            {
                return (T)Convert.ChangeType(obj, Nullable.GetUnderlyingType(typeof(T)));
            }
            else
            {
                return (T)Convert.ChangeType(obj, typeof(T));
            }
        }

        #endregion


        #region ToString Overrides

        public static string ToStringOrDefault(this decimal? source, string format)
        {
            return ToStringOrDefault(source, format, null);
        }

        public static string ToStringOrDefault(this decimal? source, string format, string defaultValue)
        {
            if (source.HasValue)
            {
                return source.Value.ToString(format);
            }
            else
            {
                return String.IsNullOrEmpty(defaultValue) ? defaultValue : String.Empty;
            }
        }

        public static string ToStringOrDefault(this DateTime? source, string format)
        {
            return ToStringOrDefault(source, format, null);
        }

        public static string ToStringOrDefault(this DateTime? source, string format, string defaultValue)
        {
            if (source.HasValue)
            {
                return source.Value.ToString(format);
            }
            else
            {
                return String.IsNullOrEmpty(defaultValue) ? defaultValue : String.Empty;
            }
        }

        #endregion


        #region version Parsing and comparing

        public class VersionComparer : IComparer<string>
        {
            public int Compare(string a, string b)
            {
                return Enumerable.Zip(a.Split('.'), b.Split('.'),
                                     (x, y) => int.Parse(x).CompareTo(int.Parse(y)))
                                 .FirstOrDefault(i => i != 0);
            }
        }

        public class VerComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                string[] first = x.Split('.');
                string[] second = y.Split('.');
                // a.CompareTo(b)
                return first.Zip(second, (a, b) => String.CompareOrdinal(a, b))
                            .FirstOrDefault(c => c != 0);
            }
        }

        #endregion

    }
}
