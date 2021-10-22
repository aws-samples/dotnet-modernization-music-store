using MvcMusicStore.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MvcMusicStore.Service;

namespace MvcMusicStore.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        MusicStoreEntities storeDB = new MusicStoreEntities();
        ICatalogService catalogSvc = new HttpCatalogService();
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
            // Based on placed orders, get the top selling album IDs by quantity
            var topSellingAlbums = storeDB.OrderDetails.GroupBy(d => d.AlbumId, d => d.Quantity).Select(g => new
            { AlbumId = g.Key, Quantity = g.Sum()}).OrderByDescending(s => s.Quantity).Take(count).Select(t => t.AlbumId).ToList();
            return catalogSvc.GetAlbums(topSellingAlbums);
        }
    }
}