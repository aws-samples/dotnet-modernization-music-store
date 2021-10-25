using MvcMusicStore.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using MvcMusicStore.DataAccess;

namespace MvcMusicStore.Models
{
    public class SearchCriteriaModel
    {
        private static readonly Dictionary<string, Type> indexModelMap = new Dictionary<string, Type>()
        {
            ["genres"] = typeof(Genre),
            ["artists"] = typeof(Artist),
            ["albums"] = typeof(Album),
            ["tracks"] = typeof(Track)
        };

        [Required]
        [Display(Name = "Search")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 3)]
        public string SearchTerm { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string ErrorMessage { get; set; }


        private readonly OpenSearchHelper osHelper = new OpenSearchHelper(indexModelMap);

        internal async Task<SearchResultsViewModel> Search()
        {
            Dictionary<Type, IEnumerable<object>> groupedSearchResults = await osHelper.Search(this.SearchTerm);

            var tracks = OpenSearchHelper.ResultsOfType<Track>(groupedSearchResults);
            var trackAlbums = await GetTrackAlbums(tracks);

            return new SearchResultsViewModel
            {
                Albums = OpenSearchHelper.ResultsOfType<Album>(groupedSearchResults) // Albums returned by search (album title)...
                                .Concat(trackAlbums) // together with albums returned by track search...
                                .GroupBy(a => a.AlbumId).Select(g => g.FirstOrDefault()) // excluding duplicates.
                                .ToList(),

                Artists = OpenSearchHelper.ResultsOfType<Artist>(groupedSearchResults).ToList(),
                Genres = OpenSearchHelper.ResultsOfType<Genre>(groupedSearchResults).ToList()
            };
        }

        private async Task<IEnumerable<Album>> GetTrackAlbums(IEnumerable<Track> tracks)
        {
            var albumIds = tracks.Select(t => t.AlbumId.ToString()).Distinct(); // deduplicated abum IDs
            
            Dictionary<Type, IEnumerable<object>> results = await osHelper.MultiGetSameType("albums", albumIds);
            
            return OpenSearchHelper.ResultsOfType<Album>(results);
        }
    }
}