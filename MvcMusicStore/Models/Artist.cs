using Amazon.DynamoDBv2.DataModel;

namespace MvcMusicStore.Models
{
    [DynamoDBTable("Albums")]
    public class Artist
    {
        [DynamoDBHashKey("PK")]
        public int ArtistId { get; set; }
        [DynamoDBProperty("Artist")]
        public string Name { get; set; }
    }
}