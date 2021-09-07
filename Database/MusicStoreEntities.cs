using System.Data.Entity;
using MvcMusicStore.Common.Models;

namespace MvcMusicStore.Database
{
    public class MusicStoreEntities : DbContext
    {

        public MusicStoreEntities() : base("name=MusicStoreEntities")
        { 
        
        }

        public DbSet<Album> Albums
        {
            get;
            set;
        }

        public DbSet<Genre> Genres
        {
            get;
            set;
        }

        public DbSet<Artist> Artists
        {
            get;
            set;
        }
    }
}