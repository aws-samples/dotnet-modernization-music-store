using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcMusicStore.Models.DynamoDb
{
    public class ArtistTypeConverter : IPropertyConverter
    {
        public object FromEntry(DynamoDBEntry entry)
        {
            Primitive primitive = entry as Primitive;
            if (primitive == null || !(primitive.Value is String) || string.IsNullOrEmpty((string)primitive.Value))
                throw new ArgumentOutOfRangeException();

            return JsonConvert.DeserializeObject<Artist>(primitive);
        }

        public DynamoDBEntry ToEntry(object value)
        {
            Artist artist = value as Artist;
            if (artist == null) throw new ArgumentNullException();

            string artistData = JsonConvert.SerializeObject(artist);

            DynamoDBEntry entry = new Primitive
            {
                Value = artistData
            };

            return entry;
        }
    }
}