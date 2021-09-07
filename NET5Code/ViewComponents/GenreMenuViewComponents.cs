using Microsoft.AspNetCore.Mvc;
using MvcMusicStore.Database;
using System.Linq;

namespace MvcMusicStore.ViewComponents
{
    public class GenreMenuViewComponent : ViewComponent
    {
        private readonly MusicStoreEntities storeDB;

        public GenreMenuViewComponent(MusicStoreEntities musicStoreEntities)
        {
            this.storeDB = musicStoreEntities;
        }

        public IViewComponentResult Invoke()
        {
            var genres = storeDB.Genres.ToList();

            return View(genres);
        }
    }
}