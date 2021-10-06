using Amazon.DynamoDBv2.DataModel;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcMusicStore.Models
{
    [DynamoDBTable("Cart")]
    public class Cart
    {
        private Guid? _albumId;

        [Key]
        [DynamoDBIgnore]
        public Guid RecordId { get; set; }

        [DynamoDBHashKey("PK")]
        public string CartId { get; set; }

        [DynamoDBRangeKey("SK")]
        [NotMapped]
        public string SortKey { get; set; }

        public Guid AlbumId
        {
            get
            {
                return _albumId ?? Guid.Parse(SortKey.Replace("album#", ""));
            }
            set { _albumId = value; }
        }

        public int Count { get; set; }

        public System.DateTime DateCreated { get; set; }

        public virtual Album Album { get; set; }
    }
}