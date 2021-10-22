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
    /**
     * TODO - These methods are just stubs and should be re-mapped 
     **/
    public class MusicStoreDBClient
    {
        DynamoDBContext context;

        // TODO - The "How" of configuring the DynamoDB client should be determined. We are
        // running this on an EC2 instance with a default profile, and will deploy to EC2 with execution
        // role. We can probably just use the SDK credentials as supplied in search order - eg through
        // web.config / appsettings.json
        //
        // Note - ideally this would also be all async. We can investigate using the object context
        // async pattern if we have time.
        public MusicStoreDBClient()
        {
            var dynamoClient = new AmazonDynamoDBClient();
            context = new DynamoDBContext(dynamoClient);
        }

        public IEnumerable<GenreModel> Genres()
        {
            return context.Query<GenreModel>("GENRE", QueryOperator.BeginsWith, new[] { "genre#" });
        }

        public GenreModel GenreByName(Guid genreId)
        {
            return context.Query<GenreModel>("GENRE", QueryOperator.Equal, new[] { $"genre#{genreId}" }).FirstOrDefault();
        }

        public AlbumModel AlbumById(Guid id)
        {
            return context.Load<AlbumModel>($"album#{id}");
        }
        public IEnumerable<AlbumModel> AlbumsByGenre(Guid genreId)
        {
            return context.Query<AlbumModel>(
                $"genre#{genreId}", 
                QueryOperator.BeginsWith, 
                new[] { "album#" }, 
                new DynamoDBOperationConfig { IndexName = "album-genre" });
        }

        public IEnumerable<AlbumModel> AlbumsByIdList(IEnumerable<Guid> ids)
        {
            var albumBatch = context.CreateBatchGet<AlbumModel>();

            ids.ToList().ForEach(i => albumBatch.AddKey(i));

            albumBatch.Execute();

            return albumBatch.Results;
        }

    }
}
