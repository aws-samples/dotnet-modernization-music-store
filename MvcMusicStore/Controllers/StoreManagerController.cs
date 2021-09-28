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

        public ViewResult Details(string id)
        {
            var album = context.Query<AlbumFlat>(id, QueryOperator.Equal, new[] { id });

            return View(album.FirstOrDefault());
        }

        //
        // GET: /StoreManager/Create

        public async Task<ActionResult> Create()
        {

            //Get all genres and artists items.
            var genres = context.Query<Genre>("GENRE", QueryOperator.BeginsWith, new[] { "GENRE" });
            var artists = context.Query<Artist>("ARTIST", QueryOperator.BeginsWith, new[] { "ARTIST" });

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
            var genres = context.Query<Genre>("GENRE", QueryOperator.BeginsWith, new[] { "GENRE" });
            var artists = context.Query<Artist>("ARTIST", QueryOperator.BeginsWith, new[] { "ARTIST" });


            if (ModelState.IsValid)
            {
                album.UniqueId = "A#" + Guid.NewGuid().ToString();
                album.Genre = genres.FirstOrDefault(g => g.GenreGUID == album.GenreGUID);
                album.Artist = artists.FirstOrDefault(ar => ar.ArtistGUID == album.ArtistGUID);

                context.Save(album);
                return RedirectToAction("Index");
            }

            ViewBag.GenreGUID = new SelectList(genres, "GenreGUID", "Name", album.GenreGUID);
            ViewBag.ArtistGUID = new SelectList(artists, "ArtistGUID", "Name", album.ArtistGUID);
            return View(album);
        }

        //
        // GET: /StoreManager/Edit/5

        public ActionResult Edit(string id)
        {
            //Album album = db.Albums.Find(id);

            var album = context.Query<AlbumFlat>(id, QueryOperator.Equal, new[] { id }).FirstOrDefault();

            //Get all genres and artists items.
            var genres = context.Query<Genre>("GENRE", QueryOperator.BeginsWith, new[] { "GENRE" });
            var artists = context.Query<Artist>("ARTIST", QueryOperator.BeginsWith, new[] { "ARTIST" });

            ViewBag.GenreGUID = new SelectList(genres, "GenreGUID", "Name", album.GenreGUID);
            ViewBag.ArtistGUID = new SelectList(artists, "ArtistGUID", "Name", album.ArtistGUID);
            return View(album);
        }

        //
        // POST: /StoreManager/Edit/5

        [HttpPost]
        public async Task<ActionResult> Edit(AlbumFlat album)
        {

            //Get all genres and artists items.
            var genres = context.Query<Genre>("GENRE", QueryOperator.BeginsWith, new[] { "GENRE" });
            var artists = context.Query<Artist>("ARTIST", QueryOperator.BeginsWith, new[] { "ARTIST" });

            if (ModelState.IsValid)
            {
                album.Genre = genres.FirstOrDefault(g => g.GenreGUID == album.GenreGUID);
                album.Artist = artists.FirstOrDefault(ar => ar.ArtistGUID == album.ArtistGUID);

                await context.SaveAsync(album);

                return RedirectToAction("Index");
            }

            ViewBag.GenreGUID = new SelectList(genres, "GenreGUID", "Name", album.GenreGUID);
            ViewBag.ArtistGUID = new SelectList(artists, "ArtistGUID", "Name", album.ArtistGUID);
            return View(album);
        }

        //
        // GET: /StoreManager/Delete/5

        public async Task<ActionResult> Delete(string id)
        {
            //Album album = db.Albums.Find(id);
            var album = context.Query<AlbumFlat>(id, QueryOperator.Equal, new[] { id }).FirstOrDefault();

            return View(album);
        }

        //
        // POST: /StoreManager/Delete/5

        [HttpPost, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            //Album album = db.Albums.Find(id);
            //db.Albums.Remove(album);
            //db.SaveChanges();

            context.Delete<AlbumFlat>(id,id);

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}