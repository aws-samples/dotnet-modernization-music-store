using System;

namespace MvcMusicStore.Models
{
    public class OrderDetail
    {
        public Guid OrderDetailId { get; set; }
        public Guid OrderId { get; set; }
        public Guid AlbumId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public virtual Album Album { get; set; }
        public virtual Order Order { get; set; }
    }
}
