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
    /// <summary>
    /// Utility class to use Dot.Net HttpClient in memory efficient way
    /// Based on: https://johnthiriet.com/efficient-api-calls , https://github.com/johnthiriet/EfficientHttpClient
    /// REQUIRE: Newtonsoft.Json
    /// </summary>
    public static class HttpUtil
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        #region Use stream serialization for content

        /// <summary>
        /// Execute a GET request as async operation storing the result as a stream before parsing
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="uri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<T> GetAsync<T>(HttpClient client, Uri uri, CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            using (var response = await client
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false))
            {
                //response.EnsureSuccessStatusCode();
                return await GetResult<T>(response);
            }
        }

        /// <summary>
        /// Execute a POST request as async operation storing the content inside a stream before sending
        /// </summary>
        /// <param name="client"></param>
        /// <param name="uri"></param>
        /// <param name="content"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<T> PostAsync<T>(HttpClient client, Uri uri, object content, CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
            using (var httpContent = CreateHttpContent(content))
            {
                request.Content = httpContent;

                using (var response = await client
                    .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                    .ConfigureAwait(false))
                {
                    //response.EnsureSuccessStatusCode();
                    return await GetResult<T>(response);
                }
            }
        }

        #endregion


        #region Use string serialization for content

        /// <summary>
        /// Execute a GET request as async operation storing the result in a string before parsing
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="client"></param>
        /// <param name="uri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static async Task<T> GetAsyncBasic<T>(HttpClient client, Uri uri, CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, uri))
            using (var response = await client
                .SendAsync(request, cancellationToken)
                .ConfigureAwait(false))
            {
                //response.EnsureSuccessStatusCode();
                return await GetResultBasic<T>(response);
            }
        }

        /// <summary>
        /// Execute a POST request as async operation storing the content inside a string before sending
        /// </summary>
        /// <param name="client"></param>
        /// <param name="uri"></param>
        /// <param name="content"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static async Task<T> PostAsyncBasic<T>(HttpClient client, Uri uri, object content, CancellationToken cancellationToken)
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
                        //response.EnsureSuccessStatusCode();
                        return await GetResultBasic<T>(response);
                    }
                }
            }
        }

        #endregion


        #region support

        /// <summary>
        /// Parse the querystring, add a new key-value pair and return the result
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string AddToQueryString(string queryString, string key, string value)
        {
            var parsedQueryString = HttpUtility.ParseQueryString(queryString);
            parsedQueryString.Set(key, value);
            return parsedQueryString.ToString();
        }

        /// <summary>
        /// Serialize a generic object to JSON inside an HttpContent using a support stream
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Extract a generic object T from JSON inside an HttpResponseMessage unsing stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        public static async Task<T> GetResult<T>(HttpResponseMessage response)
        {
            var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                return JsonUtil.Deserialize<T>(stream);

            var content = await JsonUtil.StreamToStringAsync(stream).ConfigureAwait(false);
            throw new ApiException { StatusCode = (int)response.StatusCode, ErrorMessage = response.ReasonPhrase, Content = content };
        }

        /// <summary>
        /// Extract a generic object T from JSON inside an HttpResponseMessage using string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="response"></param>
        /// <returns></returns>
        public static async Task<T> GetResultBasic<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<T>(content);

            throw new ApiException { StatusCode = (int)response.StatusCode, ErrorMessage = response.ReasonPhrase, Content = content };
        }

        #endregion

    }

    /// <summary>
    /// Exception on remote endpoint failed operations
    /// </summary>
    public class ApiException : Exception
    {
        /// <summary>
        /// HTTP status code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// HTTP error message
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Content of the failed method
        /// </summary>
        public string Content { get; set; }
    }
}
