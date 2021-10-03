using Npgsql;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.Pluralization;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace MvcMusicStore.Models
{
    public class PostgresMusicStoreContext : MusicStoreEntities
    {
        public PostgresMusicStoreContext() : base("name=MusicStorePostgres") { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Set schema name matching SSQL db name + schema (dbo) name, from the connection string.
            NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder(this.Database.Connection.ConnectionString);
            modelBuilder.HasDefaultSchema(builder.SearchPath);

            // Ensure PG SQL statements have lower cased column names
            modelBuilder.Conventions.Add(new ColumnNameLowerCaseConvention());

            // Ensure PG SQL statements have lower cased table names
            modelBuilder.Types().Configure(c => TableNameToLowerCase(c));

            base.OnModelCreating(modelBuilder);
        }

        static void TableNameToLowerCase(ConventionTypeConfiguration config)
        {
            IPluralizationService pluralizationService
                = (IPluralizationService)DbConfiguration.DependencyResolver.GetService(typeof(IPluralizationService), null);

            string pluralizedTalbeName = pluralizationService.Pluralize(config.ClrType.Name);
            config.ToTable(pluralizedTalbeName.ToLowerInvariant());
        }
    }

    class ColumnNameLowerCaseConvention : IStoreModelConvention<EdmProperty>
    {
        public void Apply(EdmProperty property, DbModel model) =>
            property.Name = property.Name.ToLowerInvariant();
    }
}