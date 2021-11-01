using Amazon.DynamoDBv2.DataModel;
using System;

namespace MvcMusicStore.CatalogApi.Models
{
    // TODO: Map this to the appropriate DynamoDB table (single table design)
    [DynamoDBTable("Catalog")]
    public class GenreModel
    {
        private string _name;

        /// <summary>
        /// Partition Key holds keyword GENRE to get all available Genres.
        /// Note: Using generic property name because Catalog table stores albums, artists and genres.
        /// </summary>
        [DynamoDBHashKey]
        public string PartitionKey { get; set; }

        /// <summary>
        /// Sort Key holds GenereName.
        /// Note: Using generic property name because Catalog table stores albums, artists and genres.
        /// </summary>
        [DynamoDBRangeKey]
        public string SortKey { get; set; }

        public string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(_name))
                {
                    return _name;
                }
                return SortKey;
            }
            set { _name = value; }
        }

    }
}
