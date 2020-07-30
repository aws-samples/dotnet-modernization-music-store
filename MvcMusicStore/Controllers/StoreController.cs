using MvcMusicStore.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace MvcMusicStore.Controllers
{
    public class StoreController : Controller
    {
        //MusicStoreEntities storeDB = new MusicStoreEntities();
        private readonly MusicStoreEntities storeDB;

        public StoreController(MusicStoreEntities musicStoreEntities)
        {
            storeDB = musicStoreEntities;
        }
        //
        // GET: /Store/

        public ActionResult Index()
        {
            var genres = storeDB.Genres.ToList();

            return View(genres);
        }

        //
        // GET: /Store/Browse?genre=Disco

        public ActionResult Browse(string genre)
        {
            // Retrieve Genre and its Associated Albums from database
            var genreModel = storeDB.Genres.Include("Albums")
                .Single(g => g.Name == genre);

            return View(genreModel);
        }

        //
        // GET: /Store/Details/5

        public ActionResult Details(int id)
        {
            var album = storeDB.Albums.Find(id);

            return View(album);
        }
    }
}