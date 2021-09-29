using Amazon.DynamoDBv2.DataModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace MvcMusicStore.Models
{
    [DynamoDBTable("Cart")]
    public class Cart
    {
        private string _albumId;

        [DynamoDBHashKey("PK")]
        public string CartId { get; set; }

        [DynamoDBHashKey("SK")]
        public string AlbumId
        {
            get {
                return _albumId.Replace("album#", "");
            }
            set
            {
                _albumId = value; 
            }
        }
        
        public int Count { get; set; }
        
        public System.DateTime DateCreated { get; set; }

        public virtual Album Album { get; set; }
    }
}