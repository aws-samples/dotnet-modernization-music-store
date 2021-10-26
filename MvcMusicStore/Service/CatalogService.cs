using MvcMusicStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcMusicStore.Service
{
    [Obsolete("We have fully migrated to HttpCatalogService")]
    public class CatalogService : ICatalogService
    {
        private MusicStoreEntities storeDb;

        public CatalogService()
        {
            storeDb = new MusicStoreEntities();
        }

        // Retrieves a particular album by its Guid
        public Album GetAlbumById(Guid id)
        {
            return storeDb.Albums.Find(id);
        }

        // Given a list of guids, retrieves the corresponding albums
        public List<Album> GetAlbums(List<Guid> ids)
        {
            return storeDb.Albums.Where(a => ids.Contains(a.AlbumId)).ToList();
        }

        // Retrieves all the albums for a particular album
        public List<Album> GetAlbumsByGenre(Guid id)
        {
            return storeDb.Albums.Where(a => a.GenreId == id).ToList();
        }

        // Retrieves a genre by its name
        public Genre GetGenre(Guid id)
        {
            return storeDb.Genres.Include("Albums").Single(g => g.GenreId == id);
        }

        // Retrieves a list of all available Genres
        public List<Genre> GetGenres()
        {
            return storeDb.Genres.ToList();
        }
    }
}
