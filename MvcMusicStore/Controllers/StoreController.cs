using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MvcMusicStore.Models;
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
        public ActionResult Browse(string genre)
        {
            // Retrieve the genre and its Associated Albums from database.
            // If the genre is not found, then return an empty list of albums.
            var genreModel = catalogSvc.GetGenreByName(genre);
            var albums = genreModel != null
                ? catalogSvc.GetAlbumsByGenre(genreModel.GenreId).OrderBy(a => a.Title).ToList()
                : new List<Album>();

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