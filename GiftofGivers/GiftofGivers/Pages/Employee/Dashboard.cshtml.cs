using GiftofGivers.Data;
using GiftofGivers.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GiftofGivers.Pages.Employee
{
    [Authorize(Roles = "Employee")] // only Employees can access this page
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [BindProperty]
        public ProjectUpdate NewUpdate { get; set; } = new();

        public List<ProjectUpdate> Updates { get; set; } = new();
        public List<VolunteerSignup> Volunteers { get; set; } = new();
        public List<GiftofGivers.Models.Donation> RecentDonations { get; set; } = new();

        // GET: /Employee/Dashboard
        public async Task OnGetAsync()
        {
            await LoadDashboardDataAsync();
        }

        // POST: /Employee/Dashboard?handler=PostUpdate
        public async Task<IActionResult> OnPostPostUpdateAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (string.IsNullOrWhiteSpace(NewUpdate.Title) || string.IsNullOrWhiteSpace(NewUpdate.Description))
            {
                TempData["Error"] = "Title and description are required.";
                return RedirectToPage();
            }

            NewUpdate.PostedByUserId = user?.Id ?? "unknown";
            NewUpdate.PostedByName = user?.FullName ?? user?.Email;
            NewUpdate.PostedOn = DateTime.UtcNow;

            _db.ProjectUpdates.Add(NewUpdate);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Update posted successfully.";
            return RedirectToPage();
        }

        private async Task LoadDashboardDataAsync()
        {
            Updates = await _db.ProjectUpdates.OrderByDescending(u => u.PostedOn).ToListAsync();
            Volunteers = await _db.VolunteerSignups.OrderByDescending(v => v.SubmittedOn).ToListAsync();
            RecentDonations = await _db.Donations.OrderByDescending(d => d.DonationDate).Take(10).ToListAsync();
        }
    }
}
