using Amazon.DynamoDBv2.DataModel;
using System;

namespace MvcMusicStore.CatalogApi.Models
{
    // TODO: Map this to the appropriate DynamoDB table (single table design)
    [DynamoDBTable("Catalog")]
    public class AlbumModel
    {
        /// <summary>
        /// Partition Key holds either albumId to fetch specific album or genreId to get albums by Genre.
        /// Note: Catalog table holds albums, artists and genres. Using generic partition key name.
        /// Example value: album#{albumId} or genre#{genreId}
        /// </summary>
        [DynamoDBHashKey]
        public string PartitionKey { get; set; }

        /// <summary>
        /// DynamoDb Sort Key holds either keywork "metadata" to get album metadata or albumId to get albums by Genre.
        /// Note: Catalog table holds albums, artists and genres. Using generic sort key name.
        /// </summary>
        [DynamoDBRangeKey]
        public string SortKey { get; set; }

        private Guid? _albumId;
        
        public Guid AlbumId
        {
            get => _albumId ?? Guid.Parse(PartitionKey.Replace("album#", "").Replace("genre#",""));
            set => _albumId = value;
        }

        public Guid GenreId { get; set; }

        public Guid ArtistId { get; set; }

        public string Title { get; set; }

        public decimal Price { get; set; }

        public string AlbumArtUrl { get; set; }

        public GenreModel Genre { get; set; }

        public ArtistModel Artist { get; set; }
    }
}
