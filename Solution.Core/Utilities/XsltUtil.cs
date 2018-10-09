using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.IO;

using log4net;


namespace Solution.Core.Utilities
{
    /// <summary>
    /// Utility to execute XSLT 1.0 transformations 
    /// </summary>
    public static class XlstUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Load XML and transformation from specified path and return a stream with resulting HTML
        /// </summary>
        /// <param name="xslFile">Path where to find XSL transformation</param>
        /// <param name="xmlFile">Path where to find XML data file</param>
        /// <returns>MemoryStream containing an HTML</returns>
        public static MemoryStream Transform(string xslFile, string xmlFile)
        {
            try
            {
                XslCompiledTransform xslt = new XslCompiledTransform();
                XsltArgumentList xsltArguments = new XsltArgumentList();
                MemoryStream resStream = new MemoryStream();
                XPathDocument xmlDoc = new XPathDocument(xmlFile);

                //XmlReaderSettings settings;
                //XmlUrlResolver resolver;
                //Implement resolver to load additional transformations
                //resolver = new XmlUrlResolver();
                //settings = new XmlReaderSettings();
                //settings.XmlResolver = resolver;

                xslt.Load(xslFile);
                //xsltArguments.AddParam("dataClass", "", crfName);
                xslt.Transform(xmlDoc, xsltArguments, resStream);
                resStream.Position = 0;

                return resStream;
                //Example on how to read the memory stream
                //XmlDocument xmldoc = new XmlDocument();
                //var sr = new StreamReader(resultStream);
                //string xmlResult =  sr.ReadToEnd();
                //xmldoc.LoadXml(xmlResult);
                //return xmldoc.DocumentElement;
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
        }

        /// <summary>
        /// Load XML and transformation from specified path and write the resulting HTML to disk
        /// </summary>
        /// <param name="xslFile">Path where to find XSL transformation</param>
        /// <param name="xmlFile">Path where to find XML data file</param>
        /// <param name="resultFile">Path where to write the resulting HTML</param>
        public static void Transform(string xslFile, string xmlFile, string resultFile)
        {
            try
            {
                XslCompiledTransform xslt = new XslCompiledTransform();
                XsltArgumentList xsltArguments = new XsltArgumentList();
                XPathDocument xPathDoc = new XPathDocument(xmlFile);
                using (XmlTextWriter resWriter = new XmlTextWriter(resultFile, null))
                {
                    xslt.Load(xslFile);
                    xslt.Transform(xPathDoc, xsltArguments, resWriter);
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
        }

    }
}
