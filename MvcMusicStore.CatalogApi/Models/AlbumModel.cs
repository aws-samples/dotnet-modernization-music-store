using Amazon.DynamoDBv2.DataModel;
using System;

namespace MvcMusicStore.CatalogApi.Models
{
    // TODO: Map this to the appropriate DynamoDB table (single table design)
    [DynamoDBTable("Catalog")]
    public class AlbumModel
    {
        /// <summary>
        /// DynamoDB partition Key.
        /// Note: Catalog table holds albums, artists and genres. Using generic partition key name.
        /// value: album#{albumId}
        /// </summary>
        [DynamoDBHashKey]
        [DynamoDBGlobalSecondaryIndexRangeKey]
        public string PK { get; set; }

        /// <summary>
        /// DynamoDb Sort Key.
        /// Note: Catalog table holds albums, artists and genres. Using generic sort key name.
        /// value: genre#{genreId}
        /// </summary>
        [DynamoDBRangeKey]
        [DynamoDBGlobalSecondaryIndexHashKey]
        public string SK { get; set; }

        private Guid? _albumId;
        
        private Guid? _genreId;

        public Guid AlbumId
        {
            get => _albumId ?? Guid.Parse(PK.Replace("album#", ""));
            set => _albumId = value;
        }

        public Guid GenreId
        {
            get => _genreId ?? Guid.Parse(SK.Replace("genre#", ""));
            set => _genreId = value;
        }

        public Guid ArtistId { get; set; }

        public string Title { get; set; }

        public decimal Price { get; set; }

        public string AlbumArtUrl { get; set; }

        public GenreModel Genre { get; set; }

        public ArtistModel Artist { get; set; }
    }
}
