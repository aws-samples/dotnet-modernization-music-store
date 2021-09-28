using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using MvcMusicStore.Models;
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

        public async Task<ViewResult> Index()
        {
            //Get all items from dynamo DB.
            var conditions = new List<ScanCondition>();

            var albums = await context.ScanAsync<Album>(conditions).GetRemainingAsync();
            return View(albums.ToList());
        }

        //
        // GET: /StoreManager/Details/5

        public async Task<ViewResult> Details(string id)
        {
            var album = await context.LoadAsync<Album>(id).ConfigureAwait(false);

            return View(album);
        }

        //
        // GET: /StoreManager/Create

        public async Task<ActionResult> Create()
        {

            //Get all genres and artists items.
            var conditions = new List<ScanCondition>();

            var genres = await context.ScanAsync<Genre>(conditions).GetRemainingAsync();
            var artists = await context.ScanAsync<Artist>(conditions).GetRemainingAsync();

            ViewBag.GenreGUID = new SelectList(genres, "GenreGUID", "Name");
            ViewBag.ArtistGUID = new SelectList(artists, "ArtistGUID", "Name");
            return View();
        }

        //
        // POST: /StoreManager/Create

        [HttpPost]
        public async Task<ActionResult> Create(Album album)
        {

            //Get all genres and artists items.
            var conditions = new List<ScanCondition>();
            var genres = await context.ScanAsync<Genre>(conditions).GetRemainingAsync();
            var artists = await context.ScanAsync<Artist>(conditions).GetRemainingAsync();


            if (ModelState.IsValid)
            {
                album.UniqueId = "A#" + Guid.NewGuid().ToString();
                album.Genre = genres.Find(g => g.GenreGUID == album.GenreGUID);
                album.Artist = artists.Find(ar => ar.ArtistGUID == album.ArtistGUID);

                context.Save(album);
                return RedirectToAction("Index");
            }

            ViewBag.GenreGUID = new SelectList(genres, "GenreGUID", "Name", album.GenreGUID);
            ViewBag.ArtistGUID = new SelectList(artists, "ArtistGUID", "Name", album.ArtistGUID);
            return View(album);
        }

        //
        // GET: /StoreManager/Edit/5

        public async Task<ActionResult> Edit(string id)
        {
            //Album album = db.Albums.Find(id);

            Album album = await context.LoadAsync<Album>(id).ConfigureAwait(false);

            var conditions = new List<ScanCondition>();
            var genres = await context.ScanAsync<Genre>(conditions).GetRemainingAsync();
            var artists = await context.ScanAsync<Artist>(conditions).GetRemainingAsync();

            ViewBag.GenreGUID = new SelectList(genres, "GenreGUID", "Name", album.GenreGUID);
            ViewBag.ArtistGUID = new SelectList(artists, "ArtistGUID", "Name", album.ArtistGUID);
            return View(album);
        }

        //
        // POST: /StoreManager/Edit/5

        [HttpPost]
        public async Task<ActionResult> Edit(Album album)
        {

            var conditions = new List<ScanCondition>();
            var genres = await context.ScanAsync<Genre>(conditions).GetRemainingAsync();
            var artists = await context.ScanAsync<Artist>(conditions).GetRemainingAsync();

            if (ModelState.IsValid)
            {
                album.Genre = genres.Find(g => g.GenreGUID == album.GenreGUID);
                album.Artist = artists.Find(ar => ar.ArtistGUID == album.ArtistGUID);

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
            Album album = await context.LoadAsync<Album>(id).ConfigureAwait(false);

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