using GiftofGivers.Data;
using GiftofGivers.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GiftofGivers.Pages.Donation
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [BindProperty]
        public GiftofGivers.Models.Donation Donation { get; set; } = new();

        [BindProperty]
        public bool DonateAsGuest { get; set; }

        // GET: /Donation
        // Works for logged-in Donors AND anonymous guests
        public void OnGet()
        {
            Donation = new GiftofGivers.Models.Donation();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.GetUserAsync(User);

            if (user != null && !DonateAsGuest)
            {
                Donation.UserId = user.Id;
                Donation.DonorName = user.FullName ?? user.Email ?? "Registered Donor";
                Donation.DonorEmail = user.Email;
                Donation.IsAnonymous = false;
            }
            else
            {
                Donation.IsAnonymous = string.IsNullOrWhiteSpace(Donation.DonorName)
                    || (DonateAsGuest && string.IsNullOrWhiteSpace(Donation.DonorEmail));
                Donation.DonorName = string.IsNullOrWhiteSpace(Donation.DonorName) ? "Anonymous Donor" : Donation.DonorName;
            }

            Donation.DonationDate = DateTime.UtcNow;
            Donation.CertificateReference = $"GOTG-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(100000, 999999)}";

            _db.Donations.Add(Donation);
            await _db.SaveChangesAsync();

            return RedirectToPage("ThankYou", new { id = Donation.Id });
        }
    }
}
