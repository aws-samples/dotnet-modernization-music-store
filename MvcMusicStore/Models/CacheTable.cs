using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MvcMusicStore.Models
{
    [Table("MusicStoreCache")]
    public class CacheTable
    {
        [Column(TypeName = "nvarchar(449)")]
        [Required]
        [Key]
        [MaxLength(449)]
        public string Id { get; set; }

        [Column(TypeName = "varbinary(max)")]
        [Required]
        public byte[] Value { get; set; }
        
        public DateTimeOffset ExpiresAtTime { get; set; }
        
        public long? SlidingExpirationInSeconds { get; set; }
        public DateTimeOffset? AbsoluteExpiration { get; set; }
    }
}
