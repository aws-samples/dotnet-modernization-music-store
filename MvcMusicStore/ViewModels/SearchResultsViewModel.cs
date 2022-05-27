using MvcMusicStore.Models;
using System.Collections.Generic;

namespace MvcMusicStore.ViewModels
{
    public class SearchResultsViewModel
    {
        public List<Album> Albums { get; set; }

        public List<Artist> Artists { get; set; }

        public List<Genre> Genres { get; set; }
    }
}