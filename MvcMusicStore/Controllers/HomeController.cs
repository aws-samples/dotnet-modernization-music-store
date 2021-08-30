using MvcMusicStore.Common.Models;
using MvcMusicStore.Database;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace MvcMusicStore.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        AlbumRepository albumRepository = new AlbumRepository();

        public ActionResult Index()
        {
            // Get most popular albums
            var albums = GetTopSellingAlbums(5);

            return View(albums);
        }

        private List<Album> GetTopSellingAlbums(int count)
        {
            // Group the order details by album and return
            // the albums with the highest count

            return albumRepository.GetTopSellingAlbums(count);
        }
    }
}