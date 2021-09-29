using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using MvcMusicStore.Models;
using MvcMusicStore.Models.DynamoDb;
using System;
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

        public ViewResult Index()
        {
            //Get all items from dynamo DB.
            //var conditions = new List<ScanCondition>();

            IEnumerable<AlbumFlat> albums = context.Query<AlbumFlat>("ALBUM", QueryOperator.BeginsWith, new[] { "ALBUM" },
                new DynamoDBOperationConfig { IndexName = "GS2PK-GS2SK-index" });

            return View(albums.ToList());
        }

        //
        // GET: /StoreManager/Details/5

        public ViewResult Details(Guid id)
        {
            var album = context.Query<AlbumFlat>(id.ToString().ToUpper(), QueryOperator.Equal, new[] { "METADATA" });

            return View(album.FirstOrDefault());
        }

        //
        // GET: /StoreManager/Create

        public ActionResult Create()
        {

            //Get all genres and artists items.
            var genres = context.Query<GenreFlat>("GENRE", QueryOperator.BeginsWith, new[] { "GENRE" });
            var artists = context.Query<ArtistFlat>("ARTIST", QueryOperator.BeginsWith, new[] { "ARTIST" });

            ViewBag.GenreGUID = new SelectList(genres, "GenreGUID", "Name");
            ViewBag.ArtistGUID = new SelectList(artists, "ArtistGUID", "Name");
            return View();
        }

        //
        // POST: /StoreManager/Create

        [HttpPost]
        public ActionResult Create(AlbumFlat album)
        {

            //Get all genres and artists items.
            var genres = context.Query<GenreFlat>("GENRE", QueryOperator.BeginsWith, new[] { "GENRE" });
            var artists = context.Query<ArtistFlat>("ARTIST", QueryOperator.BeginsWith, new[] { "ARTIST" });


            if (ModelState.IsValid)
            {
                album.AlbumId = Guid.NewGuid();
                album.Genre = genres.FirstOrDefault(g => g.GenreId == album.GenreId);
                album.Artist = artists.FirstOrDefault(ar => ar.ArtistId == album.ArtistId);

                context.Save(album);
                return RedirectToAction("Index");
            }

            ViewBag.GenreGUID = new SelectList(genres, "GenreGUID", "Name", album.GenreId);
            ViewBag.ArtistGUID = new SelectList(artists, "ArtistGUID", "Name", album.ArtistId);
            return View(album);
        }

        //
        // GET: /StoreManager/Edit/5

        public ActionResult Edit(Guid id)
        {
            //Album album = db.Albums.Find(id);

            var album = context.Query<AlbumFlat>(id.ToString().ToUpper(), QueryOperator.Equal, new[] { id }).FirstOrDefault();

            //Get all genres and artists items.
            var genres = context.Query<GenreFlat>("GENRE", QueryOperator.BeginsWith, new[] { "GENRE" });
            var artists = context.Query<ArtistFlat>("ARTIST", QueryOperator.BeginsWith, new[] { "ARTIST" });

            ViewBag.GenreGUID = new SelectList(genres, "GenreGUID", "Name", album.GenreId);
            ViewBag.ArtistGUID = new SelectList(artists, "ArtistGUID", "Name", album.ArtistId);
            return View(album);
        }

        //
        // POST: /StoreManager/Edit/5

        [HttpPost]
        public async Task<ActionResult> Edit(AlbumFlat album)
        {

            //Get all genres and artists items.
            var genres = context.Query<GenreFlat>("GENRE", QueryOperator.BeginsWith, new[] { "GENRE" });
            var artists = context.Query<ArtistFlat>("ARTIST", QueryOperator.BeginsWith, new[] { "ARTIST" });

            if (ModelState.IsValid)
            {
                album.Genre = genres.FirstOrDefault(g => g.GenreId == album.GenreId);
                album.Artist = artists.FirstOrDefault(ar => ar.ArtistId == album.ArtistId);

                await context.SaveAsync(album);

                return RedirectToAction("Index");
            }

            ViewBag.GenreGUID = new SelectList(genres, "GenreGUID", "Name", album.GenreId);
            ViewBag.ArtistGUID = new SelectList(artists, "ArtistGUID", "Name", album.ArtistId);
            return View(album);
        }

        //
        // GET: /StoreManager/Delete/5

        public ActionResult Delete(Guid id)
        {
            //Album album = db.Albums.Find(id);
            var album = context.Query<AlbumFlat>(id.ToString().ToUpper(), QueryOperator.Equal, new[] { id }).FirstOrDefault();

            return View(album);
        }

        //
        // POST: /StoreManager/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(Guid id)
        {
            //Album album = db.Albums.Find(id);
            //db.Albums.Remove(album);
            //db.SaveChanges();

            context.Delete<AlbumFlat>(id.ToString().ToUpper(), "METADATA");

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}