using MvcMusicStore.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Configuration;
using Nest;
using Newtonsoft.Json;

namespace MvcMusicStore.Models
{
    public class SearchCriteriaModel
    {
        [Required]
        [Display(Name = "Search")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 3)]
        public string SearchTerm { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string ErrorMessage { get; set; }

        Lazy<ElasticClient> elasticClient = new Lazy<ElasticClient>(() => {
            string endpoint = ConfigurationManager.AppSettings["OpensearchEndpoint"];
            string username = ConfigurationManager.AppSettings["OpensearchUsername"];
            string pwd = ConfigurationManager.AppSettings["OpensearchPassword"];

            var node = new Uri(endpoint);
            var settings = new ConnectionSettings(node).BasicAuthentication(username, pwd);
            var esClient = new ElasticClient(settings);

            return esClient;
        });

        public async Task<IReadOnlyCollection<IHit<Dictionary<string, object>>>> Search(string searchTerm) =>
            (await elasticClient.Value.SearchAsync<Dictionary<string, object>>(s => s
                .AllIndices()
                .Query(q => q.SimpleQueryString(sq => sq.AnalyzeWildcard(true).Query($"*{searchTerm}*")))
            )).Hits;

        internal async Task<SearchResultsViewModel> Search()
        {
            var hits = await Search(this.SearchTerm);

            Dictionary<Type, List<object>> modelMapping = GroupHitsByModel(hits, new Dictionary<string, Type>()
            {
                ["genres"] = typeof(Genre),
                ["artists"] = typeof(Artist),
                ["albums"] = typeof(Album)
            });

            return new SearchResultsViewModel
            {
                Albums = ResultsOfType<Album>(modelMapping),
                Artists = ResultsOfType<Artist>(modelMapping),
                Genres = ResultsOfType<Genre>(modelMapping)
            };
        }

        public static List<T> ResultsOfType<T>(Dictionary<Type, List<object>> allResults)
        {
            List<object> results;
            return allResults.TryGetValue(typeof(T), out results) ? results.Cast<T>().ToList() : new List<T>();
        }
            

        /// <summary>
        /// Groups search hits by type and converts Dictionary objects to POCOs.
        /// TODO: Update with using NEST native mapping, if possible.
        /// </summary>
        /// <param name="hits">Search hits</param>
        /// <param name="indexModelMapping">Index to </param>
        /// <returns></returns>
        public static Dictionary<Type, List<object>> GroupHitsByModel(
                IReadOnlyCollection<IHit<Dictionary<string, object>>> hits,
                Dictionary<string, Type> indexTypeMap)
        {
            // TODO: re-write as a group-by ToDictionary() linq/lambda.

            var map = new Dictionary<Type, List<object>>();

            foreach (var hit in hits)
            {
                Type objectType = indexTypeMap[hit.Index];
                List<object> entries;

                if (!map.TryGetValue(objectType, out entries))
                {
                    entries = new List<object>();
                    map[objectType] = entries;
                }

                var result = ToObject(objectType, hit.Source);
                entries.Add(result);
            }

            return map;
        }

        public static object ToObject(Type pocoType, IDictionary<string, object> source)
        {
            // TODO; Figure out ES native mapping for better performance
            var json = JsonConvert.SerializeObject(source, Formatting.Indented);
            var poco = JsonConvert.DeserializeObject(json, pocoType);

            return poco;
        }
    }
}