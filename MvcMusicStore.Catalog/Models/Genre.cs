using Amazon.DynamoDBv2.DataModel;

namespace MvcMusicStore.Catalog.Models
{
    [DynamoDBTable("Genre")]
    public class Genre
    {
        [DynamoDBHashKey]
        public string GenreId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
