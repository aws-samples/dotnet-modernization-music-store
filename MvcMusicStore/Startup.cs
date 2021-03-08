using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MvcMusicStore.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;

namespace MvcMusicStoreCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews()
                .AddJsonOptions(jsonOptions => jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = null);

            services.AddScoped(_ => new MusicStoreEntities(Configuration.GetConnectionString("MusicStoreEntities")));

            services.AddHttpContextAccessor();

            // distributed cache is used as a backing store for session.
            if (!string.IsNullOrEmpty(Configuration.GetConnectionString("redisConnection")))
            {
                // persist cache in redis.
                services.AddDistributedRedisCache(options =>
                {
                    options.Configuration = Configuration.GetConnectionString("redisConnection");
                });
            }
            else
            {
                // add in memory cache.
                services.AddDistributedMemoryCache();
            }

            services.AddSession();
            
            services.AddScoped<ShoppingCart>();

            services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("IdentityConnection")));

            // Add a DbContext to store your Database Keys
            services.AddDbContext<DataProtectionKeysContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("IdentityConnection")));

            // using Microsoft.AspNetCore.DataProtection;
            services.AddDataProtection()
                .PersistKeysToDbContext<DataProtectionKeysContext>();

            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options => options.LoginPath = "/Account/Logon");

            services.AddScoped<IPasswordHasher<User>, SQLPasswordHasher<User>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                // EF Core
                var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.Migrate();

                // Data protection keys
                var dataProtectionKeysContext = serviceScope.ServiceProvider.GetRequiredService<DataProtectionKeysContext>();
                dataProtectionKeysContext.Database.Migrate();

                // EF 6.4 (MusicStore)
                System.Data.Entity.Database.SetInitializer(new MvcMusicStore.Models.SampleData());
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
