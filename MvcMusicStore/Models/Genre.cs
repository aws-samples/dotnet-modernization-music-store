using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;

namespace MvcMusicStore.Models
{
    [DynamoDBTable("AlbumFlat")]
    public partial class Genre
    {
        [DynamoDBIgnore]
        public int GenreId { get; set; }

        [DynamoDBHashKey("PK")]
        public string Type { get; set; }

        [DynamoDBRangeKey("SK")]
        public string GenreGUID { get; set; }

        [DynamoDBProperty("Genre")]
        public string Name { get; set; }
        
        public string Description { get; set; }

        [DynamoDBIgnore]
        public List<Album> Albums { get; set; }
    }
}
