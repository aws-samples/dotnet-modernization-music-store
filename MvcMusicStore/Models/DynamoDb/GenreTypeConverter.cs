using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcMusicStore.Models.DynamoDb
{
    public class GenreTypeConverter : IPropertyConverter
    {
        public object FromEntry(DynamoDBEntry entry)
        {
            Primitive primitive = entry as Primitive;
            if (primitive == null || !(primitive.Value is String) || string.IsNullOrEmpty((string)primitive.Value))
                throw new ArgumentOutOfRangeException();

            return JsonConvert.DeserializeObject<Genre>(primitive);
        }

        public DynamoDBEntry ToEntry(object value)
        {
            Genre genre = value as Genre;
            if (genre == null) throw new ArgumentNullException();

            string genreData = JsonConvert.SerializeObject(genre);

            DynamoDBEntry entry = new Primitive {
                Value = genreData
            };

            return entry;
        }
    }
}