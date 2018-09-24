using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Threading;
using System.IO;
using System.Globalization;

using log4net;

using Npgsql;


namespace Solution.Tools.Utilities
{
    public class NpgsqlUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int MaxRetry = 3;
        private const int SleepRetry = 20000; //20 sec


        #region single connection per query

        public static Object ExecuteScalar(string connectionString, string sqlCommand, NpgsqlParameter[] parameters, bool retryQuery = false)
        {
            using (NpgsqlConnection sqlConn = new NpgsqlConnection(connectionString))
            {
                sqlConn.Open();
                return ExecuteScalar(sqlConn, sqlCommand, parameters, null, retryQuery);
            }
        }

        public static int ExecuteNonQuery(string connectionString, string sqlCommand, NpgsqlParameter[] parameters, bool retryQuery = false)
        {
            using (NpgsqlConnection sqlConn = new NpgsqlConnection(connectionString))
            {
                sqlConn.Open();
                return ExecuteNonQuery(sqlConn, sqlCommand, parameters, null, retryQuery);
            }
        }

        public static DataTable ExecuteAdapter(string connectionString, string sqlCommand, NpgsqlParameter[] parameters, bool retryQuery = false)
        {
            using (NpgsqlConnection sqlConn = new NpgsqlConnection(connectionString))
            {
                sqlConn.Open();
                return ExecuteAdapter(sqlConn, sqlCommand, parameters, null, retryQuery);
            }
        }

        public static void ExecuteSqlFile(string connectionString, FileInfo sqlFile, NpgsqlParameter[] parameters, bool retryQuery = false)
        {
            using (NpgsqlConnection sqlConn = new NpgsqlConnection(connectionString))
            {
                sqlConn.Open();
                ExecuteSqlFile(sqlConn, sqlFile, parameters, null, retryQuery);
            }
        }

        #endregion


        #region Shared connection for queries

        public static Object ExecuteScalar(NpgsqlConnection connection, string sqlCommand, NpgsqlParameter[] parameters, NpgsqlTransaction transaction, bool retryQuery = false)
        {
            Object res = null;
            int count = 0;

            while (!retryQuery || (retryQuery && count < MaxRetry))
            {
                try
                {
                    using (NpgsqlCommand cmd = connection.CreateCommand())
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

                            // Coommit della transazione da fare esternamente se passata
                            //if (transaction != null)
                            //    transaction.Commit();
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
                    log.Error(e);

                    if (retryQuery && count < MaxRetry)
                    {
                        Thread.Sleep(SleepRetry);
                        count++;
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
            return res;
        }

        public static int ExecuteNonQuery(NpgsqlConnection connection, string sqlCommand, NpgsqlParameter[] parameters, NpgsqlTransaction transaction, bool retryQuery = false)
        {
            int res = -1;
            int count = 0;

            while (!retryQuery || (retryQuery && count < MaxRetry))
            {
                try
                {
                    using (NpgsqlCommand cmd = connection.CreateCommand())
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

                            // Coommit della transazione da fare esternamente se passata
                            //if (transaction != null)
                            //    transaction.Commit();
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
                    log.Error(e);

                    if (retryQuery && count < MaxRetry)
                    {
                        Thread.Sleep(SleepRetry);
                        count++;
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
            return res;
        }

        public static DataTable ExecuteAdapter(NpgsqlConnection connection, string sqlCommand, NpgsqlParameter[] parameters, NpgsqlTransaction transaction, bool retryQuery = false)
        {
            DataTable res = new DataTable();
            int count = 0;

            while (!retryQuery || (retryQuery && count < MaxRetry))
            {
                try
                {
                    using (NpgsqlCommand cmd = connection.CreateCommand())
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
                            using (NpgsqlDataAdapter npgsqlAdapter = new NpgsqlDataAdapter(cmd))
                            {
                                npgsqlAdapter.Fill(res);
                            }

                            // Coommit della transazione da fare esternamente se passata
                            //if (transaction != null)
                            //    transaction.Commit();
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
                    log.Error(e);

                    if (retryQuery && count < MaxRetry)
                    {
                        Thread.Sleep(SleepRetry);
                        count++;
                    }
                    else
                    {
                        throw e;
                    }
                }
            }
            return res;
        }

        public static void ExecuteSqlFile(NpgsqlConnection connection, FileInfo sqlFile, NpgsqlParameter[] parameters, NpgsqlTransaction transaction, bool retryQuery = false)
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
                log.Error(e);
                throw e;
            }
        }

        #endregion


        #region Support

        public static string QuerySqlToString(string sqlCommand, NpgsqlParameter[] parameters)
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
                    string value = "'" + parameter.NpgsqlValue.ToString().Replace("'", "''") + "'";
                    sqlCommand = sqlCommand.Replace(parameter.ParameterName, (value == "''") ? "NULL" : value);
                }
            }
            Thread.CurrentThread.CurrentCulture = defaultCulture;
            return sqlCommand;
        }

        #endregion

    }
}
