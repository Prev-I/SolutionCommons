using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

using log4net;
using Newtonsoft.Json;


namespace Solution.Tools.Extensions
{
    /// <summary>
    /// Utility to handle serializzation and deserializzation of POCO
    /// REQUIRE: Newtonsoft.Json
    /// </summary>
    public static class ObjectExt
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Serialize a generic object to an XML string
        /// </summary>
        /// <param name="value">Any DotNet object</param>
        /// <param name="indent"></param>
        /// <returns></returns>
        public static string ToStringXml(this object value, bool indent = false)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                XmlSerializer xs = new XmlSerializer(value.GetType());
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

                ns.Add(string.Empty, string.Empty);

                using (XmlWriter xw = XmlWriter.Create(sb,
                                                      new XmlWriterSettings
                                                      {
                                                          OmitXmlDeclaration = true,
                                                          Indent = indent
                                                      }))
                {
                    xs.Serialize(xw, value, ns);
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
        /// Serialize a generic object to a JSON string
        /// </summary>
        /// <param name="value">Any DotNet object</param>
        /// <param name="jsonFormat"></param>
        /// <returns></returns>
        public static string ToStringJson(this object value, Newtonsoft.Json.Formatting jsonFormat = Newtonsoft.Json.Formatting.None)
        {
            try
            {
                return JsonConvert.SerializeObject(value, jsonFormat);
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw;
            }
        }
    }
}