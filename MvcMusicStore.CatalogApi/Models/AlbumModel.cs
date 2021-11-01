using Amazon.DynamoDBv2.DataModel;
using System;

namespace MvcMusicStore.CatalogApi.Models
{
    // TODO: Map this to the appropriate DynamoDB table (single table design)
    [DynamoDBTable("Catalog")]
    public class AlbumModel
    {
        /// <summary>
        /// Partition Key holds AlbumTitle (prepended by album#), example: album#{albumName}
        /// Note: Using generic property name because Catalog table stores albums, artists and genres.
        /// </summary>
        [DynamoDBHashKey]
        public string PartitionKey { get; set; }

        /// <summary>
        /// Sort key holds artist Name (prepended by artist#) to uniquely identify the Album.
        /// Note: Using generic property name because Catalog table stores albums, artists and genres.
        /// </summary>
        [DynamoDBRangeKey()]
        public string SortKey { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey]
        public string GenreName { get; set; }

        [DynamoDBGlobalSecondaryIndexHashKey]
        public Guid AlbumId { get; set; }

        public Guid ArtistId { get; set; }

        public string Title => PartitionKey.Replace("album#", "");

        public string ArtistName => SortKey.Replace("artist#", "");

        public decimal Price { get; set; }

        public string AlbumArtUrl { get; set; }

        public GenreModel Genre => new GenreModel { Name = this.GenreName };

        public ArtistModel Artist => new ArtistModel { ArtistId = this.ArtistId, Name = this.ArtistName };
    }
}
