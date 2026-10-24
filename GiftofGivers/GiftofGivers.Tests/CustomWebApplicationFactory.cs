using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using GiftofGivers.Data;
using System;
using System.Linq;
using System.IO;

namespace GiftofGivers.Tests
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        // 1. Create a unique DB file name for this specific test class run
        private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"GOTG_Test_{Guid.NewGuid()}.db");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // 2. Use the unique DB path
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlite($"Data Source={_dbPath}");
                });

                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<ApplicationDbContext>();

                    // Since the name is unique, EnsureDeleted isn't strictly necessary, but good practice
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();
                }
            });
        }

        // 3. Clean up the temp file when tests finish
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (File.Exists(_dbPath))
            {
                try { File.Delete(_dbPath); } catch { /* Ignore if locked */ }
            }
        }
    }
}