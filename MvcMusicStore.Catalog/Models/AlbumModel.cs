using Amazon.DynamoDBv2.DataModel;

namespace MvcMusicStore.Catalog.Models
{
    // TODO: Map this to the appropriate DynamoDB table (single table design)
    [DynamoDBTable("Album")]
    public class AlbumModel
    {       

       
        public string AlbumId { get; set; }

        public string GenreId { get; set; }

        public string ArtistId { get; set; }

        public string Title { get; set; }

        public decimal Price { get; set; }

        public string AlbumArtUrl { get; set; }
    }
}
