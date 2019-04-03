using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using log4net;


namespace Solution.Core.Utilities
{
    /// <summary>
    /// Additional operation on string values used in compare and parsing
    /// </summary>
    public static class StringUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        #region String Parsing

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T Parse<T>(this object obj)
        {
            if (obj == null)
                return default(T);
            return obj.ToString().Parse<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static T Parse<T>(this string obj)
        {
            return Parse<T>(obj, default(T));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T Parse<T>(this string obj, object defaultValue)
        {
            if (string.IsNullOrEmpty(obj))
            {
                if (defaultValue == null || string.IsNullOrEmpty(defaultValue.ToString()))
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

    }

    /// <summary>
    /// Comparer for dot separated versions with integer values
    /// </summary>
    public class VersionComparerNumber : IComparer<string>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public int Compare(string a, string b)
        {
            return Enumerable.Zip(a.Split('.'), b.Split('.'),
                                 (x, y) => int.Parse(x).CompareTo(int.Parse(y)))
                             .FirstOrDefault(i => i != 0);
        }
    }

    /// <summary>
    /// Comparer for dot separated versions with string values
    /// </summary>
    public class VersionComparerString : IComparer<string>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(string x, string y)
        {
            string[] first = x.Split('.');
            string[] second = y.Split('.');
            // a.CompareTo(b)
            return first.Zip(second, (a, b) => string.CompareOrdinal(a, b))
                        .FirstOrDefault(c => c != 0);
        }
    }

}
