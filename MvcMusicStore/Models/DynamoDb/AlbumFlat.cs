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
        private string _genreGUID;
        private string _albumGUID;
        private string _artistGUID;
        private string _gs1sk;
        private string _gs2pk;
        private string _gs2sk;

        [DynamoDBHashKey("PK")]
        public string UniqueId { get; set; }

        [DynamoDBRangeKey("SK")]
        public string AlbumGUID
        {
            get
            {
                return _albumGUID ?? UniqueId;
            }
            set
            {
                _albumGUID = value;
            }
        }


        public string GS1PK { get; set; }

        public string GS1SK
        {
            get
            {
                return _gs1sk ?? $"ALBUM#{UniqueId}";
            }
            set
            {
                _gs1sk = value;
            }
        }


        public string GS2PK
        {
            get
            {
                return _gs2pk ?? "ALBUM";
            }
            set
            {
                _gs2pk = value;
            }
        }

        public string GS2SK
        {
            get
            {
                return _gs2sk ?? $"ALBUM#{UniqueId}";
            }
            set
            {
                _gs2sk = value;
            }
        }

        [DynamoDBIgnore]
        public string GenreGUID
        {
            get
            {

                if (!string.IsNullOrEmpty(_genreGUID))
                {
                    return _genreGUID;
                }

                if (Genre != null)
                {
                    return Genre.GenreGUID;
                }

                return null;
            }
            set
            {
                _genreGUID = value;
            }
        }

        [DynamoDBIgnore]
        public string ArtistGUID
        {
            get
            {

                if (!string.IsNullOrEmpty(_artistGUID))
                {
                    return _artistGUID;
                }

                if (Artist != null)
                {
                    return Artist.ArtistGUID;
                }

                return null;
            }
            set
            {
                _artistGUID = value;
            }
        }
    }
}