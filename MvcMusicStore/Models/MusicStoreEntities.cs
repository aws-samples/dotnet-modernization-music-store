using System.Data.Entity;

namespace MvcMusicStore.Models
{
    [DbConfigurationType(typeof(CodeConfig))] // point to the class that inherit from DbConfiguration
    public class MusicStoreEntities : DbContext
    {
        public MusicStoreEntities(string nameOrConnectionString)
        : base(nameOrConnectionString)
        {
        }

        public DbSet<Album> Albums { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
    }

    public class CodeConfig : DbConfiguration
    {
        public CodeConfig()
        {
            SetProviderServices("System.Data.SqlClient",
                  System.Data.Entity.SqlServer.SqlProviderServices.Instance);
        }
    }
}