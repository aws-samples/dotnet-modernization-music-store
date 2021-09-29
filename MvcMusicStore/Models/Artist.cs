using Amazon.DynamoDBv2.DataModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace MvcMusicStore.Models
{
    public class Artist
    {
        [Key]
        [DynamoDBRangeKey("SK")]
        public Guid ArtistId { get; set; }
        
        [DynamoDBProperty]
        public string Name { get; set; }
    }
}