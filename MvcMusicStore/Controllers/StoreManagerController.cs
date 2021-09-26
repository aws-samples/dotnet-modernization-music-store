using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using MvcMusicStore.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MvcMusicStore.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class StoreManagerController : Controller
    {
        private MusicStoreEntities db = new MusicStoreEntities();
        private AmazonDynamoDBClient dynamoClient;
        private DynamoDBContext context;

        public StoreManagerController()
        {
            dynamoClient = new AmazonDynamoDBClient();
            context = new DynamoDBContext(dynamoClient);
        }

        //
        // GET: /StoreManager/

        public async Task<ViewResult> Index()
        {
            //Get all items from dynamo DB.
            var conditions = new List<ScanCondition>();

            var albums = await context.ScanAsync<Album>(conditions).GetRemainingAsync();
            //var albums = db.Albums.Include(a => a.Genre).Include(a => a.Artist);
            return View(albums.ToList());
        }

        //
        // GET: /StoreManager/Details/5

        public ViewResult Details(int id)
        {
            string albumId = id.ToString();

            //Album album = db.Albums.Find(id);
            var album = context.Load<Album>(id);

            return View(album);
        }

        //
        // GET: /StoreManager/Create

        public async Task<ActionResult> Create()
        {

            //Get all items from dynamo DB.
            var conditions = new List<ScanCondition>();

            var genres = await context.ScanAsync<Genre>(conditions).GetRemainingAsync();
            var artists = await context.ScanAsync<Artist>(conditions).GetRemainingAsync();

            ViewBag.GenreId = new SelectList(genres, "GenreId", "Name");
            ViewBag.ArtistId = new SelectList(artists, "ArtistId", "Name");
            return View();
        }

        //
        // POST: /StoreManager/Create

        [HttpPost]
        public ActionResult Create(Album album)
        {
            if (ModelState.IsValid)
            {
                //db.Albums.Add(album);
                //db.SaveChanges();
                context.Save(album);
                return RedirectToAction("Index");
            }

            ViewBag.GenreId = new SelectList(db.Genres, "GenreId", "Name", album.GenreId);
            ViewBag.ArtistId = new SelectList(db.Artists, "ArtistId", "Name", album.ArtistId);
            return View(album);
        }

        //
        // GET: /StoreManager/Edit/5

        public async Task<ActionResult> Edit(int id)
        {
            //Album album = db.Albums.Find(id);

            var albumQuery = await context.QueryAsync<Album>(id).GetRemainingAsync();

            Album album = albumQuery.FirstOrDefault();

            ViewBag.GenreId = new SelectList(db.Genres, "GenreId", "Name", album.GenreId);
            ViewBag.ArtistId = new SelectList(db.Artists, "ArtistId", "Name", album.ArtistId);
            return View(album);
        }

        //
        // POST: /StoreManager/Edit/5

        [HttpPost]
        public ActionResult Edit(Album album)
        {
            if (ModelState.IsValid)
            {
                //db.Entry(album).State = System.Data.Entity.EntityState.Modified;
                //db.SaveChanges();

                context.Save(album);

                return RedirectToAction("Index");
            }
            ViewBag.GenreId = new SelectList(db.Genres, "GenreId", "Name", album.GenreId);
            ViewBag.ArtistId = new SelectList(db.Artists, "ArtistId", "Name", album.ArtistId);
            return View(album);
        }

        //
        // GET: /StoreManager/Delete/5

        public async Task<ActionResult> Delete(int id)
        {
            //Album album = db.Albums.Find(id);
            var albumQuery = await context.QueryAsync<Album>(id).GetRemainingAsync();

            Album album = albumQuery.FirstOrDefault();

            return View(album);
        }

        //
        // POST: /StoreManager/Delete/5

        [HttpPost, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            //Album album = db.Albums.Find(id);
            //db.Albums.Remove(album);
            //db.SaveChanges();

            context.Delete<Album>(id);

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}