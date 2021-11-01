using MvcMusicStore.CatalogApi;
using MvcMusicStore.CatalogApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace MvcMusicStore.CatalogApi.Controllers
{
    [RoutePrefix("api/catalog")]
    public class CatalogController : ApiController
    {
        CatalogService client = new CatalogService();

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
        public IHttpActionResult Albums(string idlist = null, string genreName = null)
        {
            List<AlbumModel> albums = new List<AlbumModel>();

            if (!string.IsNullOrEmpty(idlist))
            {
                var idArray = idlist.ToUpper().Split(',');
                albums.AddRange(client.AlbumsByIdList(idArray));
            }
            else if (!string.IsNullOrEmpty(genreName))
            {
                albums.AddRange(client.AlbumsByGenre(genreName));
            }

            return Ok(albums);
        }
    }
}