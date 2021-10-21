using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon;
using MvcMusicStore.Catalog.Models;
using Amazon.DynamoDBv2.DocumentModel;

namespace MvcMusicStore.Catalog
{
    public class MusicStoreDBClient
    {
        DynamoDBContext context;

        public MusicStoreDBClient(string region = "us-east-1")
        {
            var dynamoClient = new AmazonDynamoDBClient(RegionEndpoint.GetBySystemName(region));
            context = new DynamoDBContext(dynamoClient);
        }

        public IEnumerable<Genre> Genres()
        {
            return context.Scan<Genre>();
        }

        public Genre GenreByName(string name)
        {
            return context.Scan<Genre>(new ScanCondition("Name", ScanOperator.Equal, name)).FirstOrDefault();
        }

        public IEnumerable<AlbumModel> Albums()
        {
            return context.Scan<AlbumModel>();
        }

        public AlbumModel AlbumById(string id)
        {
            return context.Load<AlbumModel>(id);
        }
        public IEnumerable<AlbumModel> AlbumsByGenre(string genreId)
        {
            return context.Scan<AlbumModel>(new ScanCondition("GenreId", ScanOperator.Equal, genreId));
        }

        public IEnumerable<AlbumModel> AlbumsById(IEnumerable<string> ids)
        {
            foreach(var id in ids)
            {
                yield return AlbumById(id);
            }
        }

    }
}
