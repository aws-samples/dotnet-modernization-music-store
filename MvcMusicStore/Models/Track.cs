using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace MvcMusicStore.Models
{
    [Bind(Exclude = "TrackId")]
    public class Track
    {
        [ScaffoldColumn(false)]
        [Key]
        public Guid TrackId { get; set; }

        [DisplayName("Album")]
        public Guid AlbumId { get; set; }

        [Required(ErrorMessage = "A Track Title is required")]
        [StringLength(160)]
        public string Title { get; set; }

        [StringLength(10 * 1024)]
        public string Lyrics { get; set; }

        public virtual Album Album { get; set; }
    }
}