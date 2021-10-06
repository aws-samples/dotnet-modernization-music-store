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
        public DbSet<Cart> Carts { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<MusicStoreEntities>(null);

            base.OnModelCreating(modelBuilder);
        }

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