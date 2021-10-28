using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon;
using MvcMusicStore.CatalogApi.Models;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;

namespace MvcMusicStore.CatalogApi
{
    /**
     * TODO - These methods are just stubs and should be re-mapped 
     **/
    public class CatalogService
    {
        DynamoDBContext context;
        AmazonDynamoDBClient dynamoClient;

        // TODO - The "How" of configuring the DynamoDB client should be determined. We are
        // running this on an EC2 instance with a default profile, and will deploy to EC2 with execution
        // role. We can probably just use the SDK credentials as supplied in search order - eg through
        // web.config / appsettings.json
        //
        // Note - ideally this would also be all async. We can investigate using the object context
        // async pattern if we have time.
        public CatalogService()
        {
            dynamoClient = new AmazonDynamoDBClient();
            context = new DynamoDBContext(dynamoClient);
        }

        public IEnumerable<GenreModel> Genres()
        {
            return context.Query<GenreModel>("metadata", QueryOperator.BeginsWith, new[] { "genre#" });
        }   

        public GenreModel GenreById(string genreId)
        {
            return context.Query<GenreModel>("metadata", QueryOperator.Equal, new[] { $"genre#{genreId}" }).FirstOrDefault();
        }

        public AlbumModel AlbumById(string id)
        {
            return context.Query<AlbumModel>($"album#{id}", QueryOperator.BeginsWith, new[] { "metadata" }).FirstOrDefault();
        }
        public IEnumerable<AlbumModel> AlbumsByGenre(string genreId)
        {
            return context.Query<AlbumModel>($"genre#{genreId}", QueryOperator.BeginsWith, new[] { "album#" });
        }

        public IEnumerable<AlbumModel> AlbumsByIdList(IEnumerable<string> ids)
        {
            var albumBatch = context.CreateBatchGet<AlbumModel>();

            ids.ToList().ForEach(i => albumBatch.AddKey(i, "metadata"));

            albumBatch.Execute();

            return albumBatch.Results;
        }

    }
}
