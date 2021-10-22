using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Text.Json;

namespace MvcMusicStore.Service
{
    // This class is included to facilitate making HTTP requests to the API.
    internal class HttpRequestHelper
    {
        string baseUrl;
        private static readonly HttpClient httpClient = new HttpClient();


        public HttpRequestHelper(string apiUrl)
        {
            baseUrl = apiUrl;
        }

        static HttpRequestHelper()
        {
            httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        // Simple function that enables the construction of a URI in the form:
        // [baseurl]/action/id?querystring
        private string GetUrl(string action, string id = null, string querystring = null)
        {
            return $"{baseUrl}/{action}" +
                (!string.IsNullOrEmpty(id) ? $"/{id}" : "") +
                (!string.IsNullOrEmpty(querystring) ? $"?{querystring}" : "");            
        }

        // Makes synchonous HTTP call, while preventing deadlocks.
        // Ideally, we would change the entire method chain to async/await.
        public T MakeHttpCall<T>(string action, string id = null, string querystring = null)
        {
            var url = GetUrl(action, id, querystring);
            var getTask = Task.Run(() => httpClient.GetAsync(url));
            getTask.Wait();
            var responseTask = getTask.Result.Content.ReadAsStringAsync();
            responseTask.Wait();
            return JsonSerializer.Deserialize<T>(responseTask.Result);
        }
    }
}