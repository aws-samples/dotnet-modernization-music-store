using Amazon.DynamoDBv2.DataModel;

namespace MvcMusicStore.Catalog.Models
{
    // TODO: Map this to the appropriate DynamoDB table (single table design)
    [DynamoDBTable("Artist")]
    public class ArtistModel
    {
        public string ArtistId { get; set; }
        public string Name { get; set; }
    }
}
