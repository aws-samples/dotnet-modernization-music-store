using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MvcMusicStore.Models
{
    public class Cart
    {
        private Guid? _albumId;

        [Key]
        public Guid RecordId { get; set; }

        public string CartId { get; set; }

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
    }
}