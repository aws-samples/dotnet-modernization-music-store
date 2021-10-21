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
 
        ICatalogService catalogSvc = new CatalogService();


        //
        // GET: /Store/

        public ActionResult Index()
        {
            var genres = catalogSvc.GetGenres();

            return View(genres);
        }

        //
        // GET: /Store/Browse?genre=Disco

        public ActionResult Browse(string genre)
        {
            // Retrieve Genre and its Associated Albums from database
            var genreModel = catalogSvc.GetGenreByName(genre);
            var albums = catalogSvc.GetAlbumsByGenre(genreModel.GenreId).OrderBy(a => a.Title).ToList();
            

            var viewModel = new StoreBrowseViewModel
            {
                Genre = genreModel,
                Albums = albums
            };


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