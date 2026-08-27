using GiftofGivers.Data;
using GiftofGivers.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GiftofGivers.Pages.Volunteer
{
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public RegisterModel(ApplicationDbContext db) => _db = db;

        [BindProperty]
        public VolunteerSignup Volunteer { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Volunteer.SubmittedOn = DateTime.UtcNow;

            _db.VolunteerSignups.Add(Volunteer);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Thank you for registering as a volunteer! We'll be in touch soon.";
            return RedirectToPage();
        }
    }
}
