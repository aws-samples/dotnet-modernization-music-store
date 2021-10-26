using MvcMusicStore.Catalog;
using MvcMusicStore.Catalog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MvcMusicStore.Api.Controllers
{
    [RoutePrefix("api/catalog")]
    public class CatalogController : ApiController
    {
        MusicStoreDBClient client = new MusicStoreDBClient();

        [HttpGet]
        [Route("genres")]
        public IHttpActionResult Genres(string genreId = null)
        {
            if (string.IsNullOrEmpty(genreId))
            {
                return Ok(client.Genres());
            }
            else
            {
                return Ok(new List<GenreModel> { client.GenreById(genreId.ToUpper()) });
            }
        }

        // Method expects one or more AlbumIds, comma separated
        [HttpGet]
        [Route("albums")]
        public IHttpActionResult Albums(string idlist = null, string genreid = null)
        {
            if (!string.IsNullOrEmpty(idlist))
            {
                var idArray = idlist.ToUpper().Split(',');
                return Ok(client.AlbumsByIdList(idArray));
            }
            else if (!string.IsNullOrEmpty(genreid))
            {
                return Ok(client.AlbumsByGenre(genreid.ToUpper()));
            }
            return Ok(client.Albums());
        }
    }
}