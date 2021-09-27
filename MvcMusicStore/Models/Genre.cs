using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;

namespace MvcMusicStore.Models
{
    [DynamoDBTable("Genres")]
    public partial class Genre
    {
        [DynamoDBHashKey("PK")]
        public int GenreId { get; set; }
        
        [DynamoDBProperty("Genre")]
        public string Name { get; set; }
        
        public string Description { get; set; }

        [DynamoDBIgnore]
        public List<Album> Albums { get; set; }
    }
}
