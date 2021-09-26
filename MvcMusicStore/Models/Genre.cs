using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;

namespace MvcMusicStore.Models
{
    [DynamoDBTable("Genre")]
    public partial class Genre
    {
        [DynamoDBHashKey("PK")]
        public int GenreId { get; set; }
        
        [DynamoDBProperty("Artist")]
        public string Name { get; set; }
        
        public string Description { get; set; }

        [DynamoDBIgnore]
        public List<Album> Albums { get; set; }
    }
}
