using GiftofGivers.Data;
using GiftofGivers.Models;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;
// Add services to the container.
builder.Services.AddRazorPages();

// Azure SQL when a SQL Server connection string is configured (App Service connection string
// "DefaultConnection"); otherwise SQLite for local development and the automated tests.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var useSqlServer = !string.IsNullOrWhiteSpace(connectionString) &&
    (connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) ||
     connectionString.Contains("Data Source=tcp:", StringComparison.OrdinalIgnoreCase));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (useSqlServer)
        options.UseSqlServer(connectionString!);
    else
        options.UseSqlite(connectionString ?? "Data Source=gotg.db");
});

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // relaxed for prototype
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();


//policy for employee role

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Employee", policy => policy.RequireRole("Employee"));
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Employee", "Employee");
});

var app = builder.Build();

//we seed thatdata here and the roles aswell
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();
    // SQLite (local/tests): create schema directly. Azure SQL: schema is applied by the pipeline's
    // EF Core migration script, so we must NOT call EnsureCreated there.
    if (db.Database.IsSqlite())
        db.Database.EnsureCreated();
    await SeedData.InitializeAsync(services);
}


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

public partial class Program { }
