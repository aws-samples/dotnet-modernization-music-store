using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MvcMusicStore.Models
{
    public partial class Genre
    {
        [Key]
        [DynamoDBRangeKey("SK")]
        public Guid GenreId { get; set; }
        
        [DynamoDBProperty]
        public string Name { get; set; }
        
        public string Description { get; set; }

        [DynamoDBIgnore]
        public List<Album> Albums { get; set; }
    }
}
