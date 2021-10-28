using Amazon.DynamoDBv2.DataModel;
using System;

namespace MvcMusicStore.CatalogApi.Models
{
    // TODO: Map this to the appropriate DynamoDB table (single table design)
    [DynamoDBTable("Catalog")]
    public class GenreModel
    {
        private Guid? _genreId;

        /// <summary>
        /// Note: Catalog table holds albums, artists and genres. Using generic partition key name.
        /// </summary>
        [DynamoDBHashKey]
        public string PartitionKey { get; set; }

        /// <summary>
        /// Note: Catalog table holds albums, artists and genres. Using generic sort key name.
        /// </summary>
        [DynamoDBRangeKey]
        public string SortKey { get; set; }

        public Guid GenreId
        {
            get => _genreId ?? Guid.Parse(SortKey.Replace("genre#", ""));
            set => _genreId = value;
        }

        [DynamoDBProperty("Title")]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
