using Amazon.DynamoDBv2.DataModel;
using MvcMusicStore.Models.DynamoDb;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace MvcMusicStore.Models
{
    [Bind(Exclude = "AlbumId")]
    public class Album
    {
        [ScaffoldColumn(false)]
        [Key]
        [DynamoDBHashKey("PK")]
        public Guid AlbumId { get; set; }

        [DisplayName("Genre")]
        [DynamoDBIgnore]
        public Guid GenreId { get; set; }

        [DisplayName("Artist")]
        [DynamoDBIgnore]
        public Guid ArtistId { get; set; }

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