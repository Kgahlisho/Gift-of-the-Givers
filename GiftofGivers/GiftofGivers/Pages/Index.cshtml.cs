using GiftofGivers.Data;
using GiftofGivers.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GiftofGivers.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db) => _db = db;

        public List<ProjectUpdate> LatestUpdates { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Show the latest relief updates on the public homepage for transparency
            LatestUpdates = await _db.ProjectUpdates
                .OrderByDescending(p => p.PostedOn)
                .Take(3)
                .ToListAsync();
        }
    }
}
