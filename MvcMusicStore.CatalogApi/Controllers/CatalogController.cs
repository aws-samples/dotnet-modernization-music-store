using MvcMusicStore.CatalogApi;
using MvcMusicStore.CatalogApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MvcMusicStore.CatalogApi.Controllers
{
    /* Added by CTA: RoutePrefix attribute is no longer supported */
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogController : ControllerBase
    {
        CatalogService client = new CatalogService();
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Genres(string name = null)
        {
            if (string.IsNullOrEmpty(name))
            {
                return Ok(await client.Genres());
            }
            else
            {
                return Ok(new List<GenreModel>{await client.GenreByName(name)});
            }
        }

        // Method expects one or more AlbumIds, comma separated or a genre name
        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Albums(string idlist = null, string genreName = null)
        {
            List<AlbumModel> albums = new List<AlbumModel>();
            if (!string.IsNullOrEmpty(idlist))
            {
                var idArray = idlist.ToUpper().Split(',');
                albums.AddRange(await client.AlbumsByIdList(idArray));
            }
            else if (!string.IsNullOrEmpty(genreName))
            {
                albums.AddRange(await client.AlbumsByGenre(genreName));
            }

            return Ok(albums);
        }
    }
}