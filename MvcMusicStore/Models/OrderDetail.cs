﻿using System;
using System.ComponentModel.DataAnnotations;

namespace MvcMusicStore.Models
{
    public class OrderDetail
    {
        [Key]
        public Guid OrderDetailId { get; set; }
        public Guid OrderId { get; set; }
        public Guid AlbumId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        [StringLength(160)]      
        public virtual Order Order { get; set; }
    }
}
