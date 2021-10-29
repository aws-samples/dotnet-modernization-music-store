
using System.Net.Http;
using System.Threading.Tasks;
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
            // Specify that we always want to receive JSON back from the web service
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

        // Makes the HTTP call to the specified Web API
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