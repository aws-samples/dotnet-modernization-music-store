using Amazon.DynamoDBv2.DataModel;
using System;

namespace MvcMusicStore.Catalog.Models
{
    // TODO: Map this to the appropriate DynamoDB table (single table design)
    [DynamoDBTable("Albums")]
    public class GenreModel
    {
        private Guid? _genreId;

        /// <summary>
        /// DynamoDB partition Key.
        /// Note: Album dynamodb table holds albums, artists and genres. Using generic partition key name.
        /// </summary>
        [DynamoDBHashKey]
        public string PK { get; set; }

        /// <summary>
        /// DynamoDb Sort Key.
        /// Note: Album dynamodb table holds albums, artists and genres. Using generic sort key name.
        /// </summary>
        [DynamoDBRangeKey]
        public string SK { get; set; }

        public Guid GenreId
        {
            get => _genreId ?? Guid.Parse(SK.Replace("genre#", ""));
            set => _genreId = value;
        }

        [DynamoDBProperty("Genre")]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
