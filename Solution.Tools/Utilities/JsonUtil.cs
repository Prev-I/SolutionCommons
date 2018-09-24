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
    public static class JsonUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public static Stream Serialize(object value, Formatting jsonFormat = Formatting.None)
        {
            try
            {
                MemoryStream resultStream;

                using (var jsonStream = new MemoryStream())
                using (var writer = new StreamWriter(jsonStream))
                using (var jsonWriter = new JsonTextWriter(writer))
                {
                    JsonSerializer jsonSerializer = new JsonSerializer();
                    jsonSerializer.Formatting = jsonFormat;
                    jsonSerializer.Serialize(jsonWriter, value);
                    jsonWriter.Flush();
                    resultStream = new MemoryStream(jsonStream.ToArray());
                }
                return resultStream;
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                throw new Exception("Escalated exception", e);
            }
        }

        public static T Deserialize<T>(Stream jsonStream, Formatting jsonFormat = Formatting.None)
        {
            try
            {
                if (jsonStream == null || jsonStream.CanRead == false)
                    return default(T);

                using (var reader = new StreamReader(jsonStream))
                using (var jsonReader = new JsonTextReader(reader))
                {
                    JsonSerializer jsonSerializer = new JsonSerializer();
                    jsonSerializer.Formatting = jsonFormat;
                    return jsonSerializer.Deserialize<T>(jsonReader);
                }
            }
            catch (Exception e)
            {
                log.Error(e.ToString());
                throw new Exception("Escalated exception", e);
            }
        }


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
