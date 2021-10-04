using Amazon.DynamoDBv2.DataModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace MvcMusicStore.Models
{
    [DynamoDBTable("Cart")]
    public class Cart
    {
        [DynamoDBHashKey("PK")]
        public string CartId { get; set; }

        [DynamoDBRangeKey("SK")]
        public string SortKey { get; set; }

        public Guid AlbumId
        {
            get
            {
                return Guid.Parse(SortKey.Replace("album#", ""));
            }
        }

        public int Count { get; set; }
        
        public System.DateTime DateCreated { get; set; }

        public virtual Album Album { get; set; }
    }
}