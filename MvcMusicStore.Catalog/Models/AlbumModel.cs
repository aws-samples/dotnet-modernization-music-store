using Amazon.DynamoDBv2.DataModel;
using System;

namespace MvcMusicStore.Catalog.Models
{
    // TODO: Map this to the appropriate DynamoDB table (single table design)
    [DynamoDBTable("Albums")]
    public class AlbumModel
    {
        /// <summary>
        /// DynamoDB partition Key.
        /// Note: Album dynamodb table holds albums, artists and genres. Using generic partition key name.
        /// value: album#{albumId}
        /// </summary>
        [DynamoDBHashKey]
        public string PK { get; set; }

        /// <summary>
        /// DynamoDb Sort Key.
        /// Note: Album dynamodb table holds albums, artists and genres. Using generic sort key name.
        /// value: genre#{genreId}
        /// </summary>
        [DynamoDBRangeKey]
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

        /// <summary>
        /// This Global seconday index Partition Key stores GenreId (prepended by genre#). 
        /// </summary>
        [DynamoDBGlobalSecondaryIndexHashKey]
        public string GS1PK { get; set; }

        /// <summary>
        /// This Global Seconday Index Partition Key stores keyword 'ALBUM'.
        /// </summary>
        [DynamoDBGlobalSecondaryIndexHashKey]
        public string GS2PK { get; set; }

        /// <summary>
        /// This Global Seconday Index Sort Key stores album Id (prepended by album#).
        /// </summary>
        [DynamoDBGlobalSecondaryIndexRangeKey]
        public string GS1SK { get; set; }

        /// <summary>
        /// This Global Seconday Index Sort Key stores album Id (prepended by album#).
        /// </summary>
        [DynamoDBGlobalSecondaryIndexRangeKey]
        public string GS2SK { get; set; }

        public string Title { get; set; }

        public decimal Price { get; set; }

        public string AlbumArtUrl { get; set; }

        public GenreModel Genre { get; set; }

        public ArtistModel Artist { get; set; }
    }
}
