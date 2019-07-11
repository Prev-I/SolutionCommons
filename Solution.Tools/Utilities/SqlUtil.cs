using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.IO;
using System.Globalization;

using log4net;
using System.ComponentModel;

namespace Solution.Tools.Utilities
{
    /// <summary>
    /// Utility to execute SQL command on SQLServer DBMS
    /// </summary>
    public static class SqlUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int MaxRetry = 3;
        private const int SleepRetry = 20000; //20 sec


        #region single connection per query

        public static void BulkInsert<T>(string connectionString, string destinationTable, IList<T> list, bool retryQuery = false)
        {
            using (SqlConnection sqlConn = new SqlConnection(connectionString))
            {
                sqlConn.Open();
                BulkInsert<T>(sqlConn, destinationTable, list, null, retryQuery);
            }
        }

        public static object ExecuteScalar(string connectionString, string sqlCommand, SqlParameter[] parameters, bool retryQuery = false)
        {
            using (SqlConnection sqlConn = new SqlConnection(connectionString))
            {
                sqlConn.Open();
                return ExecuteScalar(sqlConn, sqlCommand, parameters, null, retryQuery);
            }
        }

        public static int ExecuteNonQuery(string connectionString, string sqlCommand, SqlParameter[] parameters, bool retryQuery = false)
        {
            using (SqlConnection sqlConn = new SqlConnection(connectionString))
            {
                sqlConn.Open();
                return ExecuteNonQuery(sqlConn, sqlCommand, parameters, null, retryQuery);
            }
        }

        public static DataTable ExecuteAdapter(string connectionString, string sqlCommand, SqlParameter[] parameters, bool retryQuery = false)
        {
            using (SqlConnection sqlConn = new SqlConnection(connectionString))
            {
                sqlConn.Open();
                return ExecuteAdapter(sqlConn, sqlCommand, parameters, null, retryQuery);
            }
        }

        public static void ExecuteSqlFile(string connectionString, FileInfo sqlFile, SqlParameter[] parameters, bool retryQuery = false)
        {
            using (SqlConnection sqlConn = new SqlConnection(connectionString))
            {
                sqlConn.Open();
                ExecuteSqlFile(sqlConn, sqlFile, parameters, null, retryQuery);
            }
        }

        #endregion


        #region Shared connection for queries

        public static void BulkInsert<T>(SqlConnection connection, string destinationTable, IList<T> list, SqlTransaction transaction, bool retryQuery = false)
        {
            int count = 0;

            while (!retryQuery || (retryQuery && count < MaxRetry))
            {
                try
                {
                    using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.KeepIdentity, transaction))
                    {
                        try
                        {
                            bulkCopy.BatchSize = list.Count;
                            bulkCopy.DestinationTableName = destinationTable;

                            //Dirty hack to make sure we only have system data types  
                            //i.e. filter out the relationships/collections 
                            var props = TypeDescriptor.GetProperties(typeof(T))
                                                        .Cast<PropertyDescriptor>()
                                                        .Where(propertyInfo => propertyInfo.PropertyType.Namespace.Equals("System"))
                                                        .ToArray();
                            var values = new object[props.Length];
                            var table = new DataTable();

                            foreach (var propertyInfo in props)
                            {
                                bulkCopy.ColumnMappings.Add(propertyInfo.Name, propertyInfo.Name);
                                table.Columns.Add(propertyInfo.Name, Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType);
                            }
                            foreach (var item in list)
                            {
                                for (var i = 0; i < values.Length; i++)
                                {
                                    values[i] = props[i].GetValue(item);
                                }
                                table.Rows.Add(values);
                            }
                            bulkCopy.WriteToServer(table);
                        }
                        catch (Exception e)
                        {
                            if (transaction != null)
                                transaction.Rollback();
                            throw e;
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Error(e.Message, e);

                    if (retryQuery && count < MaxRetry)
                    {
                        Thread.Sleep(SleepRetry);
                        count++;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }

        public static object ExecuteScalar(SqlConnection connection, string sqlCommand, SqlParameter[] parameters, SqlTransaction transaction, bool retryQuery = false)
        {
            object res = null;
            int count = 0;

            while (!retryQuery || (retryQuery && count < MaxRetry))
            {
                try
                {
                    using (SqlCommand cmd = connection.CreateCommand())
                    {
                        try
                        {
                            if (parameters != null)
                            {
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddRange(parameters);
                            }
                            if (transaction != null)
                            {
                                cmd.Transaction = transaction;
                            }
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = sqlCommand;
                            log.Debug(QuerySqlToString(sqlCommand, parameters));
                            res = cmd.ExecuteScalar();
                            break;
                        }
                        catch (Exception e)
                        {
                            if (transaction != null)
                                transaction.Rollback();
                            throw e;
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Error(e.Message, e);
                    
                    if (retryQuery && count < MaxRetry)
                    {
                        Thread.Sleep(SleepRetry);
                        count++;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return res;
        }

        public static int ExecuteNonQuery(SqlConnection connection, string sqlCommand, SqlParameter[] parameters, SqlTransaction transaction, bool retryQuery = false)
        {
            int res = -1;
            int count = 0;

            while (!retryQuery || (retryQuery && count < MaxRetry))
            {
                try
                {
                    using (SqlCommand cmd = connection.CreateCommand())
                    {
                        try
                        {
                            if (parameters != null)
                            {
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddRange(parameters);
                            }
                            if (transaction != null)
                            {
                                cmd.Transaction = transaction;
                            }
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = sqlCommand;
                            log.Debug(QuerySqlToString(sqlCommand, parameters));
                            res = cmd.ExecuteNonQuery();
                            break;
                        }
                        catch (Exception e)
                        {
                            if (transaction != null)
                                transaction.Rollback();
                            throw e;
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Error(e.Message, e);

                    if (retryQuery && count < MaxRetry)
                    {
                        Thread.Sleep(SleepRetry);
                        count++;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return res;
        }

        public static DataTable ExecuteAdapter(SqlConnection connection, string sqlCommand, SqlParameter[] parameters, SqlTransaction transaction, bool retryQuery = false)
        {
            DataTable res = new DataTable();
            int count = 0;

            while (!retryQuery || (retryQuery && count < MaxRetry))
            {
                try
                {
                    using (SqlCommand cmd = connection.CreateCommand())
                    {
                        try
                        {
                            if (parameters != null)
                            {
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddRange(parameters);
                            }
                            if (transaction != null)
                            {
                                cmd.Transaction = transaction;
                            }
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = sqlCommand;
                            log.Debug(QuerySqlToString(sqlCommand, parameters));
                            using (SqlDataAdapter npgsqlAdapter = new SqlDataAdapter(cmd))
                            {
                                npgsqlAdapter.Fill(res);
                            }
                            break;
                        }
                        catch (Exception e)
                        {
                            if (transaction != null)
                                transaction.Rollback();
                            throw e;
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Error(e.Message, e);

                    if (retryQuery && count < MaxRetry)
                    {
                        Thread.Sleep(SleepRetry);
                        count++;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return res;
        }

        public static void ExecuteSqlFile(SqlConnection connection, FileInfo sqlFile, SqlParameter[] parameters, SqlTransaction transaction, bool retryQuery = false)
        {
            try
            {
                using (var fileReader = sqlFile.OpenText())
                {
                    string line = string.Empty;
                    string query = string.Empty;

                    while ((line = fileReader.ReadLine()) != null)
                    {
                        query += line + " ";
                        if (line.EndsWith(";"))
                        {
                            ExecuteNonQuery(connection, query, parameters, transaction, retryQuery);
                            query = string.Empty;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw;
            }
        }

        #endregion


        #region Support

        public static string QuerySqlToString(string sqlCommand, SqlParameter[] parameters)
        {
            //NOTE: fix the number separator to use dot (depend on the collation of the database)
            CultureInfo defaultCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            CultureInfo customCulture = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";
            Thread.CurrentThread.CurrentCulture = customCulture;

            if (parameters != null)
            {
                foreach (var parameter in parameters.OrderByDescending(q => q.ParameterName.Length))
                {
                    string value = "'" + parameter.SqlValue.ToString().Replace("'", "''") + "'";
                    sqlCommand = sqlCommand.Replace(parameter.ParameterName, (value == "''") ? "NULL" : value);
                }
            }
            Thread.CurrentThread.CurrentCulture = defaultCulture;
            return sqlCommand;
        }

        public static string PaginateSelect(string mainQuery, string overOrderByColumn, int pageNumber, int pageSize, string orderBy)
        {
            int pageIndex = pageNumber - 1;
            int rowStart = pageSize * pageIndex + 1;
            int rowEnd = pageSize * (pageIndex + 1);
            string sql = string.Format(@"
                SELECT  * 
                FROM    (
                            SELECT  ROW_NUMBER()
                            OVER    (ORDER BY MAINQUERY.{0} {1}) 
                            AS      [rowIndex], *
                            FROM    ( {2} ) as MAINQUERY
                        ) AS T
                WHERE   T.rowIndex BETWEEN {3} AND {4}",
                overOrderByColumn.Trim(),
                orderBy.ToUpper(),
                mainQuery,
                rowStart.ToString(),
                rowEnd.ToString()
            );
            return sql;
        }

        #endregion

    }
}
