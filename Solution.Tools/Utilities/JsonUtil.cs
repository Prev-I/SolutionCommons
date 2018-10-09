using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using log4net;
using Newtonsoft.Json;


namespace Solution.Tools.Utilities
{
    /// <summary>
    /// Utility to handle serializzation and deserializzation of JSON from Stream
    /// REQUIRE: Newtonsoft.Json
    /// </summary>
    public static class JsonUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Serialize an object to JSON stream using Json.Net
        /// </summary>
        /// <param name="value">Any DotNet object</param>
        /// <param name="jsonFormat">Specify if indent when formatting</param>
        /// <returns>Stream pointing to a valid JSON</returns>
        public static Stream Serialize(object value, Formatting jsonFormat = Formatting.None)
        {
            try
            {
                MemoryStream resultStream;

                using (var jsonStream = new MemoryStream())
                using (var writer = new StreamWriter(jsonStream))
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    JsonSerializer jsonSerializer = new JsonSerializer
                    {
                        Formatting = jsonFormat
                    };
                    jsonSerializer.Serialize(jsonWriter, value);
                    jsonWriter.Flush();
                    resultStream = new MemoryStream(jsonStream.ToArray());
                }
                return resultStream;
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
        }

        /// <summary>
        /// Deserialize a JSON stream to a type T object using Json.Net
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonStream">Stream pointing to a valid JSON</param>
        /// <param name="jsonFormat">Specify if provided JSON is indented</param>
        /// <returns>DotNet object casted to type T</returns>
        public static T Deserialize<T>(Stream jsonStream, Formatting jsonFormat = Formatting.None)
        {
            try
            {
                if (jsonStream == null || jsonStream.CanRead == false)
                    return default(T);

                using (var reader = new StreamReader(jsonStream))
                using (var jsonReader = new JsonTextReader(reader))
                {
                    JsonSerializer jsonSerializer = new JsonSerializer
                    {
                        Formatting = jsonFormat
                    };
                    return jsonSerializer.Deserialize<T>(jsonReader);
                }
            }
            catch (Exception e)
            {
                log.Error(e.Message, e);
                throw new Exception("Escalated exception", e);
            }
        }

        /// <summary>
        /// Support function to serialize json asynchronously
        /// </summary>
        /// <param name="stream">JSON stream to be serialized</param>
        /// <returns>Task with serialized JSON string</returns>
        public static async Task<string> StreamToStringAsync(Stream stream)
        {
            string content = null;

            if (stream != null)
                using (var sr = new StreamReader(stream))
                    content = await sr.ReadToEndAsync();
            return content;
        }

    }
}
