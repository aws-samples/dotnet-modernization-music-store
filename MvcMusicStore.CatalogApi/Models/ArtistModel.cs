using Amazon.DynamoDBv2.DataModel;

namespace MvcMusicStore.CatalogApi.Models
{
    // TODO: Map this to the appropriate DynamoDB table (single table design)
    [DynamoDBTable("Catalog")]
    public class ArtistModel
    {
        public string ArtistId { get; set; }

        [DynamoDBProperty("Artist")]
        public string Name { get; set; }
    }
}
