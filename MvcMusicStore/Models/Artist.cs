using Amazon.DynamoDBv2.DataModel;

namespace MvcMusicStore.Models
{
    [DynamoDBTable("Artists")]
    public class Artist
    {
        [DynamoDBHashKey("PK")]
        public string ArtistGUID { get; set; }

        [DynamoDBIgnore]
        public int ArtistId { get; set; }
        
        [DynamoDBProperty("Artist")]
        public string Name { get; set; }
    }
}