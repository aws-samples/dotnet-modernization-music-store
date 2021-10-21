using MvcMusicStore.Models;
using System.Collections.Generic;

namespace MvcMusicStore.ViewModels
{
    public class StoreBrowseViewModel
    {
        public Genre Genre { get; set; }
        public List<Album> Albums { get; set; }

    }
}