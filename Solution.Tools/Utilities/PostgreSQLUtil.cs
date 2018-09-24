using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;

using log4net;
using Npgsql;


namespace Solution.Tools.Utilities
{
    public static class PostgreSQLUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        #region Query Creation

        /* PARAMETERS CONVENTIONS
            existingRows = null ->  don't add where condition to generated query
            existingRows = list<string>() -> filter the first value in the where condition
            existingRows = {"col1", "col2"} -> add where condition with specified columns, even if those are non present in values
         */

        public static string CreateSqlPrimaryKey(string tableName, string columnName)
        {
            string keyCreate = "ALTER TABLE \"" + tableName + "\" ADD PRIMARY KEY (\"" + columnName + "\");";
            return keyCreate;
        }

        public static string CreateSqlIndex(string tableName, string columnName, string indexType = "btree")
        {
            string indexCreate = "CREATE INDEX \"" + tableName + "_" + columnName + "_IDX\" ON \"" + tableName + "\" USING " + indexType + " (\"" + columnName + "\" ASC NULLS LAST) TABLESPACE pg_default;";
            return indexCreate;
        }

        public static string CreateSqlAddColumn(string tableName, string columnName, string columnType = "text")
        {
            string alterTable = "ALTER TABLE \"" + tableName + "\" ADD COLUMN \"" + columnName + "\"" + columnType + ";";
            return alterTable;
        }

        public static string CreateSqlTruncate(string tableName)
        {
            string truncateTable = "TRUNCATE (\"" + tableName + "\");";
            return truncateTable;
        }


        public static string CreateSqlInsert(string tableName, DataRow values, List<string> existingRows, List<string> excludeRows)
        {
            string queryInsert = "INSERT INTO \"" + tableName + "\" (";
            string querySelect = " SELECT ";
            string queryCondition = " WHERE NOT EXISTS (SELECT 1 FROM  \"" + tableName + "\" WHERE";

            for (int i = 0; i < values.Table.Columns.Count; i++)
            {
                if (excludeRows == null || (excludeRows != null && !excludeRows.Contains(values.Table.Columns[i].ColumnName)))
                {
                    queryInsert += " \"" + values.Table.Columns[i].ColumnName + "\",";
                    querySelect += " @" + values.Table.Columns[i].ColumnName + ",";

                    if (existingRows != null && existingRows.Count == 0 && i == 0)
                    {
                        queryCondition += " \"" + values.Table.Columns[i].ColumnName + "\" = @" + values.Table.Columns[i].ColumnName + ");";
                    }
                }
            }
            if (existingRows == null)
            {
                queryCondition = ";";
            }
            else if (existingRows.Count > 0)
            {
                foreach (string row in existingRows)
                {
                    queryCondition += " \"" + row + "\" = @" + row + " AND";
                }
                queryCondition = queryCondition.Remove(queryCondition.Length - 4) + ");";
            }
            queryInsert = queryInsert.Remove(queryInsert.Length - 1) + ")";
            querySelect = querySelect.Remove(querySelect.Length - 1);
            return queryInsert + querySelect + queryCondition;
        }

        public static string CreateSqlInsert(string tableName, Dictionary<string, string> values, List<string> existingRows, List<string> excludeRows)
        {
            string queryInsert = "INSERT INTO \"" + tableName + "\" (";
            string querySelect = " SELECT ";
            string queryCondition = " WHERE NOT EXISTS (SELECT 1 FROM  \"" + tableName + "\" WHERE";

            foreach (var elem in values)
            {
                if (excludeRows == null || (excludeRows != null && !excludeRows.Contains(elem.Key)))
                {
                    queryInsert += " \"" + elem.Key + "\",";
                    querySelect += " @" + elem.Key + ",";

                    if (existingRows != null && existingRows.Count == 0 && queryCondition.EndsWith(" WHERE"))
                    {
                        queryCondition += " \"" + elem.Key + "\" = @" + elem.Key + ");";
                    }
                }
            }
            if (existingRows == null)
            {
                queryCondition = ";";
            }
            else if (existingRows.Count > 0)
            {
                foreach (string row in existingRows)
                {
                    queryCondition += " \"" + row + "\" = @" + row + " AND";
                }
                queryCondition = queryCondition.Remove(queryCondition.Length - 4) + ");";
            }
            queryInsert = queryInsert.Remove(queryInsert.Length - 1) + ")";
            querySelect = querySelect.Remove(querySelect.Length - 1);
            return queryInsert + querySelect + queryCondition;
        }


        public static string CreateSqlUpdate(string tableName, DataRow values, List<string> existingRows, List<string> excludeRows)
        {
            string queryUpdate = "UPDATE \"" + tableName + "\" SET ";
            string queryCondition = " WHERE";

            for (int i = 0; i < values.Table.Columns.Count; i++)
            {
                if (excludeRows == null || (excludeRows != null && !excludeRows.Contains(values.Table.Columns[i].ColumnName)))
                {
                    queryUpdate += " \"" + values.Table.Columns[i].ColumnName + "\" = " + "@" + values.Table.Columns[i].ColumnName + ",";

                    if (existingRows != null && existingRows.Count == 0 && i == 0)
                    {
                        queryCondition += " \"" + values.Table.Columns[i].ColumnName + "\" = " + "@" + values.Table.Columns[i].ColumnName + ";";
                    }
                }
            }
            if (existingRows == null)
            {
                queryCondition = ";";
            }
            else if (existingRows.Count > 0)
            {
                foreach (string row in existingRows)
                {
                    queryCondition += " \"" + row + "\" = @" + row + " AND";
                }
                queryCondition = queryCondition.Remove(queryCondition.Length - 4) + ";";
            }
            queryUpdate = queryUpdate.Remove(queryUpdate.Length - 1);
            return queryUpdate + queryCondition;
        }

        public static string CreateSqlUpdate(string tableName, Dictionary<string, string> values, List<string> existingRows, List<string> excludeRows)
        {
            string queryUpdate = "UPDATE \"" + tableName + "\" SET ";
            string queryCondition = " WHERE";

            foreach (var elem in values)
            {
                if (excludeRows == null || (excludeRows != null && !excludeRows.Contains(elem.Key)))
                {
                    queryUpdate += " \"" + elem.Key + "\" = " + "@" + elem.Key + ",";

                    if (existingRows != null && existingRows.Count == 0 && queryCondition == " WHERE")
                    {
                        queryCondition += " \"" + elem.Key + "\" = " + "@" + elem.Key + ";";
                    }
                }
            }
            if (existingRows == null)
            {
                queryCondition = ";";
            }
            else if (existingRows.Count > 0)
            {
                foreach (string row in existingRows)
                {
                    queryCondition += " \"" + row + "\" = @" + row + " AND";
                }
                queryCondition = queryCondition.Remove(queryCondition.Length - 4) + ";";
            }
            queryUpdate = queryUpdate.Remove(queryUpdate.Length - 1);
            return queryUpdate + queryCondition;
        }

        #endregion


        #region parameters 

        public static List<NpgsqlParameter> GetParameters(DataRow values, List<string> excludeRows)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            for (int i = 0; i < values.Table.Columns.Count; i++)
            {
                if (excludeRows == null || (excludeRows != null && !excludeRows.Contains(values.Table.Columns[i].ColumnName)))
                {
                    parameters.Add(new NpgsqlParameter() { ParameterName = "@" + values.Table.Columns[i].ColumnName, NpgsqlValue = values[i] });
                }
            }
            return parameters;
        }

        public static List<NpgsqlParameter> GetParameters(Dictionary<string, string> values, List<string> excludeRows)
        {
            List<NpgsqlParameter> parameters = new List<NpgsqlParameter>();

            foreach (KeyValuePair<string, string> elem in values)
            {
                if (excludeRows == null || (excludeRows != null && !excludeRows.Contains(elem.Key)))
                {
                    parameters.Add(new NpgsqlParameter() { ParameterName = "@" + elem.Key, NpgsqlValue = elem.Value });
                }
            }
            return parameters;
        }

        #endregion     

    }
}
