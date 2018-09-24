using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

using log4net;
using Newtonsoft.Json;


namespace Solution.Tools.Utilities
{
    public static class HttpUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        #region Serialize object as Stream

        public static async Task<T> GetAsync<T>(HttpClient client, Uri uri, CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            using (var response = await client
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false))
            {
                var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                    return JsonUtil.Deserialize<T>(stream);

                var content = await JsonUtil.StreamToStringAsync(stream).ConfigureAwait(false);
                throw new ApiException { StatusCode = (int)response.StatusCode, Content = content };
            }
        }

        public static async Task PostAsync(HttpClient client, Uri uri, object content, CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
            using (var httpContent = CreateHttpContent(content))
            {
                request.Content = httpContent;

                using (var response = await client
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                    .ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        #endregion


        #region Serialize object as string

        private static async Task<T> GetBasicAsync<T>(HttpClient client, Uri uri, CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            using (var response = await client
                .SendAsync(request, cancellationToken)
                .ConfigureAwait(false))
            {
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<T>(content);
            }
        }

        private static async Task PostBasicAsync(HttpClient client, Uri uri, object content, CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
            {
                var json = JsonConvert.SerializeObject(content);
                using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    request.Content = stringContent;

                    using (var response = await client
                        .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                        .ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();
                    }
                }
            }
        }

        #endregion


        #region support

        public static string AddToQueryString(string queryString, string key, string value)
        {
            var parsedQueryString = HttpUtility.ParseQueryString(queryString);
            parsedQueryString.Set(key, value);
            return parsedQueryString.ToString();
        }

        public static HttpContent CreateHttpContent(object content)
        {
            HttpContent httpContent = null;

            if (content != null)
            {
                var jsonStream = JsonUtil.Serialize(content);
                jsonStream.Position = 0;
                httpContent = new StreamContent(jsonStream);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }
            return httpContent;
        }

        #endregion

    }

    public class ApiException : Exception
    {
        public int StatusCode { get; set; }

        public string Content { get; set; }
    }
}
