using Amazon.DynamoDBv2.DataModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace MvcMusicStore.Models.DynamoDb
{
    [DynamoDBTable("AlbumFlat")]
    public class AlbumFlat : Album
    {
        private Guid? _gs1PK;
        private string _gs1SK;
        private string _gs2SK;

        [DynamoDBRangeKey("SK")]
        public string Type { get; set; } = "METADATA";

        public Guid GS1PK
        {
            get { return _gs1PK ?? GenreId; }
            set { _gs1PK = value; }
        }

        public string GS1SK 
        { 
            get { return _gs1SK ?? $"ALBUM#{AlbumId}"; }
            set { _gs1SK = value; }
        }

        public string GS2PK { get; set; } = "ALBUM";

        public string GS2SK
        {
            get { return _gs2SK ?? $"ALBUM#{AlbumId}"; }
            set { _gs2SK = value; }
        }
    }
}