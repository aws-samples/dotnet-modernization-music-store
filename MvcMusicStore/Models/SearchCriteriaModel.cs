using MvcMusicStore.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Linq;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace MvcMusicStore.Models
{
    public class SearchCriteriaModel
    {
        [Required]
        [Display(Name = "Search")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 3)]
        public string SearchTerm { get; set; }

        [HiddenInput(DisplayValue = false)]
        public string ErrorMessage { get; set; }

        internal async Task<SearchResultsViewModel> Search()
        {
            Task<List<Album>> albums = SearchAlbums(this.SearchTerm);
            Task<List<Artist>> artists = SearchArtists(this.SearchTerm);
            Task<List<Genre>> genres = SearchGenres(this.SearchTerm);
            
            // Run all queries in parallel
            await Task.WhenAll(albums, artists, genres);

            return new SearchResultsViewModel
            {
                Albums = albums.Result,
                Artists = artists.Result,
                Genres = genres.Result
            };
        }

        private static async Task<List<T>> RunAsyncQuery<T>(Func<MusicStoreEntities, IQueryable<T>> queryCallback)
        {
            using (var storeDb = new MusicStoreEntities())
            {
                return await queryCallback(storeDb).ToListAsync();
            }
        }

        private static Task<List<Album>> SearchAlbums(string searchTerm) =>
            RunAsyncQuery(storeDB => 
                from album in storeDB.Albums
                where album.Title.Contains(searchTerm)
                select album
            );


        private static Task<List<Artist>> SearchArtists(string searchTerm) =>
            RunAsyncQuery(storeDB =>
                from artist in storeDB.Artists
                where artist.Name.Contains(searchTerm)
                select artist
            );

        private static Task<List<Genre>> SearchGenres(string searchTerm) =>
            RunAsyncQuery(storeDB =>
                from genre in storeDB.Genres
                where genre.Name.Contains(searchTerm)
                select genre
            );
    }
}