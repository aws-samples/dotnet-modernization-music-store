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
            // TODO: Stub - Implement with the "right" version of the single table design
            return context.Scan<GenreModel>();
        }

        public GenreModel GenreByName(string name)
        {
            // TODO: Stub - Implement with the "right" version of the single table design
            return context.Scan<GenreModel>(new ScanCondition("Name", ScanOperator.Equal, name)).FirstOrDefault();
        }

        public IEnumerable<AlbumModel> Albums()
        {
            // TODO: Stub - Implement with the "right" version of the single table design
            return context.Scan<AlbumModel>();
        }

        public AlbumModel AlbumById(string id)
        {
            // TODO: Stub - Implement with the "right" version of the single table design
            return context.Load<AlbumModel>(id);
        }
        public IEnumerable<AlbumModel> AlbumsByGenre(string genreId)
        {
            // TODO: Stub - Implement with the "right" version of the single table design
            return context.Scan<AlbumModel>(new ScanCondition("GenreId", ScanOperator.Equal, genreId));
        }

        public IEnumerable<AlbumModel> AlbumsByIdList(IEnumerable<string> ids)
        {
            // TODO: Stub - Implement with the "right" version of the single table design
            foreach (var id in ids)
            {
                yield return AlbumById(id);
            }
        }

    }
}
