using Amazon.DynamoDBv2.DataModel;
using MvcMusicStore.Models.DynamoDb;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace MvcMusicStore.Models
{
    [Bind(Exclude = "AlbumId")]
    [DynamoDBTable("Albums")]
    public class Album
    {
        private string _genreGUID;
        private string _artistGUID;

        [ScaffoldColumn(false)]
        [DynamoDBProperty]
        // DynamoDB stream will manage the AlbumId and maintain backward compatibility in RDS database.
        public int AlbumId { get; set; }

        [DynamoDBHashKey("PK")]
        public string UniqueId { get; set; }
        
        [DisplayName("GenreGUID")]
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

        [DisplayName("ArtistGUID")]
        [DynamoDBIgnore]
        public string ArtistGUID {
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

        [DynamoDBIgnore]
        public int GenreId { get; set; }

        [DynamoDBIgnore]
        public int ArtistId { get; set; }

        [Required(ErrorMessage = "An Album Title is required")]
        [StringLength(160)]
        [DynamoDBProperty]
        public string Title { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 100.00,
            ErrorMessage = "Price must be between 0.01 and 100.00")]
        [DynamoDBProperty]
        public decimal Price { get; set; }

        [DisplayName("Album Art URL")]
        [StringLength(1024)]
        [DynamoDBProperty]
        public string AlbumArtUrl { get; set; }

        [DynamoDBProperty(typeof(GenreTypeConverter))]
        public virtual Genre Genre { get; set; }

        [DynamoDBProperty(typeof(ArtistTypeConverter))]
        public virtual Artist Artist { get; set; }
        
        [DynamoDBIgnore]
        public virtual List<OrderDetail> OrderDetails { get; set; }
    }
}