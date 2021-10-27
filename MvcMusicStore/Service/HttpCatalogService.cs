using MvcMusicStore.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace MvcMusicStore.Service
{
    public class HttpCatalogService : ICatalogService
    {
        // Structures in support of web calls
        private static string baseUrl = $"{ConfigurationManager.AppSettings["CatalogApi"]}/api/Catalog";
        private HttpRequestHelper httpHelper;

        public HttpCatalogService()
        {        
            httpHelper = new HttpRequestHelper(baseUrl);
        }

        // Retrieves a particular album by its Guid
        public Album GetAlbumById(Guid id)
        {
            var album = httpHelper.MakeHttpCall<List<Album>>("Albums", null, $"idList={id}");
            return album.FirstOrDefault();
        }

        // Given a list of guids, retrieves the corresponding albums
        public List<Album> GetAlbums(List<Guid> ids)
        {
            var idList = string.Join(",", ids);
            var albums = httpHelper.MakeHttpCall<List<Album>>("Albums", null, $"idList={idList}");
            return albums;
        }

        // Retrieves all the albums for a particular album
        public List<Album> GetAlbumsByGenre(Guid id)
        {
            var albums = httpHelper.MakeHttpCall<List<Album>>("Albums", null, $"genreid={id}");
            return albums;
        }

        // Retrieves a genre by its name
        public Genre GetGenreById(Guid genreId)
        {
            return httpHelper.MakeHttpCall<List<Genre>>("Genres", null, $"genre={genreId}").FirstOrDefault();
        }

        // Retrieves a list of all available Genres
        public List<Genre> GetGenres()
        {
            return httpHelper.MakeHttpCall<List<Genre>>("Genres");
        }
    }
}