using MvcMusicStore.CatalogApi;
using MvcMusicStore.CatalogApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace MvcMusicStore.CatalogApi.Controllers
{
    [RoutePrefix("api/catalog")]
    public class CatalogController : ApiController
    {
        CatalogService client = new CatalogService();

        [HttpGet]
        [Route("genres")]
        public async Task<IHttpActionResult> Genres(string name = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                return Ok(await client.Genres());
            }
            else
            {
                return Ok(new List<GenreModel> { await client.GenreByName(name) });
            }
        }

        // Method expects one or more AlbumIds, comma separated or a genre name
        [HttpGet]
        [Route("albums")]
        public async Task<IHttpActionResult> Albums(string idlist = null, string genreName = null)
        {
            List<AlbumModel> albums = new List<AlbumModel>();

            if (!string.IsNullOrEmpty(idlist))
            {
                var idArray = idlist.ToUpper().Split(',');
                albums.AddRange(await client.AlbumsByIdList(idArray) );
            }
            else if (!string.IsNullOrEmpty(genreName))
            {
                albums.AddRange(await client.AlbumsByGenre(genreName));
            }

            return Ok(albums);
        }
    }
}