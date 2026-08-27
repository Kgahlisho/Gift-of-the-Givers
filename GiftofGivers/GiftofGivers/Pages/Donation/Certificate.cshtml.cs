using GiftofGivers.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GiftofGivers.Pages.Donation
{
    // Generates a placeholder tax certificate as a downloadable PDF.
    // This page never renders HTML - OnGet always returns a FileResult.
    public class CertificateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public CertificateModel(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var donation = await _db.Donations.FindAsync(id);
            if (donation is null) return NotFound();

            var pdfBytes = GiftofGivers.Services.TaxCertificateGenerator.Generate(donation);
            return File(pdfBytes, "application/pdf", $"{donation.CertificateReference}.pdf");
        }
    }
}
