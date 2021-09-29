using System;
using System.ComponentModel.DataAnnotations;

namespace MvcMusicStore.Models
{
    public class Artist
    {
        [Key]
        public Guid ArtistId { get; set; }
        public string Name { get; set; }
    }
}