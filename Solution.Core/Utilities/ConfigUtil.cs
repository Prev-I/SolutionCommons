using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Configuration;

using log4net;


namespace Solution.Core.Utilities
{
    /// <summary>
    /// Utility to help manage the update of appsetting value in .config files
    /// </summary>
    public static class ConfigUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Update the currrent application settings with given confing
        /// </summary>
        /// <param name="newConfig">XML containing the modified app.config settings</param>
        /// <returns></returns>
        public static void UpdateAllSettings(XmlDocument newConfig)
        {
            foreach (XmlNode sections in newConfig.DocumentElement.ChildNodes)
            {
                if (sections.Name == "appSettings")
                {
                    foreach (XmlNode element in sections)
                    {
                        if (element.Name == "add")
                        {
                            UpdateSetting(element.Attributes["key"].Value, element.Attributes["value"].Value);
                        }
                        else if (element.Name == "remove")
                        {
                            RemoveSetting(element.Attributes["key"].Value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Update a single key in current application settings
        /// </summary>
        /// <param name="key">Key to be updated</param>
        /// <param name="value">Value to be inserted in new key</param>
        /// <returns></returns>
        public static void UpdateSetting(string key, string value)
        {
            //Inside visualstudio the file will not be changed, it works when launched in single exe
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (configuration.AppSettings.Settings[key] != null)
            {
                configuration.AppSettings.Settings[key].Value = value;
            }
            else
            {
                configuration.AppSettings.Settings.Add(key, value);
            }
            configuration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        /// <summary>
        /// Remove a single key in current application settings
        /// </summary>
        /// <param name="key">Key to be removed</param>
        /// <returns></returns>
        public static void RemoveSetting(string key)
        {
            //Inside visualstudio the file will not be changed, it works when launched in single exe
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (configuration.AppSettings.Settings[key] != null)
            {
                configuration.AppSettings.Settings.Remove(key);
            }
            configuration.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        /// <summary>
        /// Return the current application app.config version
        /// </summary>
        /// <returns>String containing ConfigVersion</returns>
        public static string GetConfigVersion()
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string res = "";

            if (configuration.AppSettings.Settings["ConfigVersion"] != null)
            {
                res = configuration.AppSettings.Settings["ConfigVersion"].Value;
            }
            return res;
        }

    }
}
