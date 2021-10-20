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
        
        [StringLength(160)]
        public string AlbumTitle { get; set; }
        
        [Range(0.01, 100.00, ErrorMessage = "Price must be between 0.01 and 100.00")]
        public decimal AlbumPrice { get; set; }
    }
}