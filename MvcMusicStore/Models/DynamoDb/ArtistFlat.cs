using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcMusicStore.Models.DynamoDb
{
    [DynamoDBTable("AlbumFlat")]
    public class ArtistFlat : Artist
    {
        [DynamoDBHashKey("PK")]
        public string Type { get; set; }
    }
}