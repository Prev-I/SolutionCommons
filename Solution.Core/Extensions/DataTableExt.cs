using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace Solution.Core.Extensions
{
    public static class DataTableExt
    {
        /// <summary>
        /// Check if the datatable is not null and contains rows
        /// </summary>
        public static bool HasRows(this DataTable dt)
        {
            if (dt == null) return false;
            if (dt.Rows.Count <= 0) return false;
            return true;
        }

        /// <summary>
        /// Check if the datatable is not null and contain less than maxRow rows
        /// </summary>
        public static bool HasRows(this DataTable dt, int maxRow)
        {
            if (!dt.HasRows()) return false;
            if (dt.Rows.Count > maxRow) return false;
            return true;
        }

        /// <summary>
        /// Check if the DataSet is not null and contains at least one DataTable with one DataRow
        /// </summary>
        public static bool HasRows(this DataSet ds)
        {
            if (ds == null) return false;
            if (ds.Tables.Count <= 0) return false;
            if (ds.Tables[0].Rows.Count <= 0) return false;
            return true;
        }

        /// <summary>
        /// Check if the DataSet is not null and contains at least one DataTable with less than maxRow rows
        /// </summary>
        public static bool HasRows(this DataSet ds, int maxRow)
        {
            if (!ds.HasRows()) return false;
            if (ds.Tables[0].Rows.Count > maxRow) return false;
            return true;
        }

        /// <summary>
        /// Convert generic DataTable to T object matching columns names with properties names
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static List<T> ConvertToList<T>(this DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }

        /// <summary>
        /// Convert generic List T objects to new DataTable matching properties names with columns names
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DataTable ConvertToDataTable<T>(this IList<T> data)
        {
            PropertyDescriptorCollection properties =
               TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        /// <summary>
        /// Convert single DataRow to a specified Object T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static T GetItem<T>(this DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pInfo in temp.GetProperties())
                {
                    if (pInfo.Name == column.ColumnName)
                    {
                        object value = null;
                        if ((dr[column.ColumnName] is DBNull) == false)
                        {
                            if (Nullable.GetUnderlyingType(pInfo.PropertyType) != null)
                            {
                                value = Convert.ChangeType(dr[column.ColumnName], Nullable.GetUnderlyingType(pInfo.PropertyType));
                            }
                            else
                            {
                                value = Convert.ChangeType(dr[column.ColumnName], pInfo.PropertyType);
                            }
                        }
                        pInfo.SetValue(obj, value, null);
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            return obj;
        }
    }
}
