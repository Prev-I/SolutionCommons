using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;

using log4net;


namespace Solution.Core.Utilities
{
    /// <summary>
    /// Utility to load and save CSV files between Disk and DataTable
    /// </summary>
    public static class CsvUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Serialize a datatable to a string as a CSV with specified options
        /// </summary>
        /// <param name="exportTable">Datatable to be serialized</param>
        /// <param name="separator">Custom separator, default ;</param>
        /// <param name="wrapper">Custom field wrapper, default "</param>
        /// <param name="addHeadersFromTable">Specify if the first row serialized has to be the header row</param>
        /// <returns>String containing the CSV file</returns>
        public static string SerializeDataTable(DataTable exportTable, string separator = ";", string wrapper = "\"", bool addHeadersFromTable = true)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                //serializza anche l'header nella stringa
                if (addHeadersFromTable)
                {
                    foreach (DataColumn column in exportTable.Columns)
                    {
                        sb.Append(wrapper + column.Caption + wrapper + separator);
                    }
                    sb.Append(Environment.NewLine);
                }

                //Itera la datatable e costruisce il csv
                foreach (DataRow row in exportTable.Rows)
                {
                    IEnumerable<string> fields = row.ItemArray.Select(field => wrapper + field.ToString() + wrapper);
                    sb.AppendLine(string.Join(separator, fields));
                }
                return sb.ToString();
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw;
            }
        }

        /// <summary>
        /// Load a CSV file inside a datatable using different format for parsing
        /// </summary>
        /// <param name="csvFile">FileInfo object that point to CSV file</param>
        /// <param name="separator">Custom separator, default ;</param>
        /// <param name="wrapper">Custom field wrapper, default "</param>
        /// <param name="csvContainHeaders">Specify if the first row contains header or actual table data</param>
        /// <returns>DataTable of the loaded CSV</returns>
        public static DataTable LoadDataTable(FileInfo csvFile, string separator = ";", string wrapper = "\"", bool csvContainHeaders = true)
        {
            try
            {
                DataTable dt = new DataTable();

                if (csvFile.Length > 0)
                {
                    using (StreamReader sr = new StreamReader(csvFile.FullName))
                    {
                        //string[] headers = sr.ReadLine().Split(separator);
                        string[] headers = sr.ReadLine().Split(new string[] { separator }, StringSplitOptions.None);
                        for (int i = 0; i < headers.Length; i++)
                        {
                            if (csvContainHeaders)
                                dt.Columns.Add(headers[i]);
                            else
                                dt.Columns.Add(i.ToString());
                        }
                        if (!csvContainHeaders)
                        {
                            DataRow dr = dt.NewRow();
                            for (int i = 0; i < headers.Length; i++)
                            {
                                if (string.IsNullOrEmpty(wrapper))
                                {
                                    dr[i] = headers[i];
                                }
                                else
                                {
                                    int wrapperLength = wrapper.Length;
                                    dr[i] = headers[i].Substring(wrapperLength, headers[i].Length - wrapperLength * 2);
                                }
                            }
                            dt.Rows.Add(dr);
                        }
                        while (!sr.EndOfStream)
                        {
                            //string[] rows = sr.ReadLine().Split(separator);
                            string[] rows = sr.ReadLine().Split(new string[] { separator }, StringSplitOptions.None);
                            DataRow dr = dt.NewRow();
                            for (int i = 0; i < headers.Length; i++)
                            {
                                if (string.IsNullOrEmpty(wrapper))
                                {
                                    dr[i] = rows[i];
                                }
                                else
                                {
                                    int wrapperLength = wrapper.Length;
                                    dr[i] = rows[i].Substring(wrapperLength, rows[i].Length - wrapperLength * 2);
                                }
                            }
                            dt.Rows.Add(dr);
                        }
                    }
                }
                return dt;
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                throw;
            }
        }

    }
}
