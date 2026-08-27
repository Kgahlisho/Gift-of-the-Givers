using GiftofGivers.Data;
using GiftofGivers.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GiftofGivers.Pages.Donation
{
    public class ThankYouModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public ThankYouModel(ApplicationDbContext db) => _db = db;

        public GiftofGivers.Models.Donation Donation { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var donation = await _db.Donations.FindAsync(id);
            if (donation is null) return NotFound();

            Donation = donation;
            return Page();
        }
    }
}