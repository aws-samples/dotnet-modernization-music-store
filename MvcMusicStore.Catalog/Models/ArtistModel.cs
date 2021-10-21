using Amazon.DynamoDBv2.DataModel;

namespace MvcMusicStore.Catalog.Models
{
    [DynamoDBTable("Artist")]
    public class ArtistModel
    {
        public string ArtistId { get; set; }
        public string Name { get; set; }
    }
}
