using MvcMusicStore.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcMusicStore.Database
{
    public class AlbumRepository
    {
        MusicStoreEntities storeDB = new MusicStoreEntities();

        public Album GetAlbumById(int id)
        { 
            return storeDB.Albums.Find(id);
        }

        public List<Genre> GetAllGenres()
        { 
            return storeDB.Genres.ToList();
        }

        /// <summary>
        /// // Retrieve Genre and its Associated Albums from database
        /// </summary>
        /// <param name="genre"></param>
        /// <returns></returns>
        public Genre GetGenreWithAlbum(string genre)
        { 
            return storeDB.Genres.Include("Albums")
                .Single(g => g.Name == genre);
        }

        public List<Album> GetTopSellingAlbums(int count)
        {
            return storeDB.Albums
                .Take(count)
                .ToList();
        }
    }
}
