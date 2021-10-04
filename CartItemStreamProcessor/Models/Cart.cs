using System;
using System.ComponentModel.DataAnnotations;

namespace MvcMusicStore.Models
{
    public class Cart
    {
        [Key]
        public Guid RecordId { get; set; }
        public string CartId { get; set; }
        public Guid AlbumId { get; set; }
        public int Count { get; set; }
        public System.DateTime DateCreated { get; set; }
    }
}