using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace MvcMusicStore.DataAccess
{
    public class HitEntry
    {
        public string Index { get; set; }
        public Dictionary<string, object> Source;
    }

    public class OpenSearchHelper
    {
        Lazy<ElasticClient> elasticClient = new Lazy<ElasticClient>(() => {
            string endpoint = ConfigurationManager.AppSettings["OpensearchEndpoint"];
            string username = ConfigurationManager.AppSettings["OpensearchUsername"];
            string pwd = ConfigurationManager.AppSettings["OpensearchPassword"];

            var node = new Uri(endpoint);
            var settings = new ConnectionSettings(node).BasicAuthentication(username, pwd);
            var esClient = new ElasticClient(settings);

            return esClient;
        });

        private readonly Dictionary<string, Type> indexModelMap;

        public OpenSearchHelper(Dictionary<string, Type> indexModelMap)
        {
            this.indexModelMap = indexModelMap;
        }

        public async Task<Dictionary<Type, List<object>>> Search(string searchTerm)
        {
            var response = await elasticClient.Value.SearchAsync<Dictionary<string, object>>(s => s
                .AllIndices()
                .Query(q => q.SimpleQueryString(sq => sq.AnalyzeWildcard(true).Query($"*{searchTerm}*")))
            );

            var hits = response.Hits
                .Select(hit => new HitEntry { Index = hit.Index, Source = hit.Source });

            Dictionary<Type, List<object>> groupedSearchResults = this.GroupHitsByModel(hits);
            return groupedSearchResults;
        }

        public async Task<Dictionary<Type, List<object>>> MultiGetSameType(string index, IEnumerable<string> ids)
        {
            var response = await elasticClient.Value.MultiGetAsync(m => m.GetMany<object>(ids, (op, id) => op.Index(index)));
            var hits = response.Hits
                .Select(hit => new HitEntry { Index = hit.Index, Source = (Dictionary<string, object>)hit.Source });

            Dictionary<Type, List<object>> groupedSearchResults = this.GroupHitsByModel(hits);
            return groupedSearchResults;
        }

        public static IEnumerable<T> ResultsOfType<T>(Dictionary<Type, List<object>> allResults)
        {
            List<object> results;
            return allResults.TryGetValue(typeof(T), out results) ? results.Cast<T>() : Enumerable.Empty<T>();
        }

        /// <summary>
        /// Groups search hits by type and converts Dictionary objects to POCOs.
        /// TODO: Update with using NEST native mapping, if possible.
        /// </summary>
        /// <param name="hits">Search hits</param>
        /// <param name="indexModelMap"></param>
        private Dictionary<Type, List<object>> GroupHitsByModel(IEnumerable<HitEntry> hits)
        {
            // TODO: re-write as a group-by ToDictionary() linq/lambda.

            var map = new Dictionary<Type, List<object>>();

            foreach (var hit in hits)
            {
                Type objectType = indexModelMap[hit.Index];
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

        private static object ToObject(Type pocoType, IDictionary<string, object> source)
        {
            // TODO; Figure out ES native mapping for better performance
            var json = JsonConvert.SerializeObject(source, Formatting.Indented);
            var poco = JsonConvert.DeserializeObject(json, pocoType);

            return poco;
        }
    }
}