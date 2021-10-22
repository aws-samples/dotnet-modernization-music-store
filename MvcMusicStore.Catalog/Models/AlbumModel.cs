using Amazon.DynamoDBv2.DataModel;
using System;

namespace MvcMusicStore.Catalog.Models
{
    // TODO: Map this to the appropriate DynamoDB table (single table design)
    [DynamoDBTable("Album")]
    public class AlbumModel
    {
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

        public string ArtistId { get; set; }

        public string Title { get; set; }

        public decimal Price { get; set; }

        public string AlbumArtUrl { get; set; }

        public GenreModel Genre { get; set; }

        public ArtistModel Artist { get; set; }
    }
}
