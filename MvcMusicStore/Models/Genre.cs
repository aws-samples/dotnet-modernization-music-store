using System;
using System.Collections.Generic;

namespace MvcMusicStore.Models
{
    public partial class Genre
    {
        public Guid GenreId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Album> Albums { get; set; }
    }
}
