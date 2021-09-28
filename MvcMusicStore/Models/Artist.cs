using Amazon.DynamoDBv2.DataModel;

namespace MvcMusicStore.Models
{
    [DynamoDBTable("AlbumFlat")]
    public class Artist
    {
        [DynamoDBHashKey("PK")]
        public string Type { get; set; }

        [DynamoDBRangeKey("SK")]
        public string ArtistGUID { get; set; }

        [DynamoDBIgnore]
        public int ArtistId { get; set; }
        
        [DynamoDBProperty("Artist")]
        public string Name { get; set; }
    }
}