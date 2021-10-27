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
        [DynamoDBGlobalSecondaryIndexRangeKey]
        public string PK { get; set; }

        /// <summary>
        /// Note: Catalog table holds albums, artists and genres. Using generic sort key name.
        /// </summary>
        [DynamoDBRangeKey]
        [DynamoDBGlobalSecondaryIndexHashKey]
        public string SK { get; set; }

        public Guid GenreId
        {
            get => _genreId ?? Guid.Parse(PK.Replace("genre#", ""));
            set => _genreId = value;
        }

        [DynamoDBProperty("Genre")]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
