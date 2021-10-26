using MvcMusicStore.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using MvcMusicStore.Service;
using MvcMusicStore.ViewModels;

namespace MvcMusicStore.Controllers
{
    public class StoreController : Controller
    {
        ICatalogService catalogSvc = new HttpCatalogService();
        //
        // GET: /Store/
        public ActionResult Index()
        {
            var genres = catalogSvc.GetGenres();
            return View(genres);
        }

        //
        // GET: /Store/Browse?genre=Disco
        public ActionResult Browse(Guid genreId)
        {
            // Retrieve Genre and its Associated Albums from database
            var genreModel = catalogSvc.GetGenre(genreId);
            var albums = catalogSvc.GetAlbumsByGenre(genreId).OrderBy(a => a.Title).ToList();
            var viewModel = new StoreBrowseViewModel{Genre = genreModel, Albums = albums};
            return View(viewModel);
        }

        //
        // GET: /Store/Details/5
        public ActionResult Details(Guid id)
        {
            var album = catalogSvc.GetAlbumById(id);
            return View(album);
        }

        //
        // GET: /Store/GenreMenu
        [ChildActionOnly]
        public ActionResult GenreMenu()
        {
            var genres = catalogSvc.GetGenres();
            return PartialView(genres);
        }
    }
}