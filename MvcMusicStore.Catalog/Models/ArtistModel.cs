using Amazon.DynamoDBv2.DataModel;

namespace MvcMusicStore.Catalog.Models
{
    // TODO: Map this to the appropriate DynamoDB table (single table design)
    [DynamoDBTable("Albums")]
    public class ArtistModel
    {
        public string ArtistId { get; set; }

        [DynamoDBProperty("Artist")]
        public string Name { get; set; }
    }
}
