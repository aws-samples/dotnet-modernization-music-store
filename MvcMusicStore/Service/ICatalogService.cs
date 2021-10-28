using MvcMusicStore.Models;
using System;
using System.Collections.Generic;

namespace MvcMusicStore.Service
{
    interface ICatalogService
    {
        Album GetAlbumById(Guid id);
        List<Album> GetAlbums(List<Guid> ids);
        List<Album> GetAlbumsByGenre(Guid id);
        Genre GetGenreByName(string name);
        List<Genre> GetGenres();
    }
}