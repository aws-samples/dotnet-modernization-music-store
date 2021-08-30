using MvcMusicStore.Common.Models;
using MvcMusicStore.Database;
using System.Linq;
using System.Web.Mvc;

namespace MvcMusicStore.Controllers
{
    public class StoreController : Controller
    {
        AlbumRepository albumRepository = new AlbumRepository();

        //
        // GET: /Store/

        public ActionResult Index()
        {
            var genres = albumRepository.GetAllGenres();

            return View(genres);
        }

        //
        // GET: /Store/Browse?genre=Disco

        public ActionResult Browse(string genre)
        {

            var genreModel = albumRepository.GetGenreWithAlbum(genre);

            return View(genreModel);
        }

        //
        // GET: /Store/Details/5

        public ActionResult Details(int id)
        {
            var album = albumRepository.GetAlbumById(id);

            return View(album);
        }

        //
        // GET: /Store/GenreMenu

        [ChildActionOnly]
        public ActionResult GenreMenu()
        {
            var genres = albumRepository.GetAllGenres();

            return PartialView(genres);
        }

    }
}