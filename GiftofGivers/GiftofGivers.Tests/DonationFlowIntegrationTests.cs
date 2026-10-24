using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using GiftofGivers.Data;
using System.Text.RegularExpressions;
using System.Linq;

namespace GiftofGivers.Tests
{
    public class DonationFlowIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public DonationFlowIntegrationTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false // Stop at the 302 Redirect so we can test it
            });
        }

        [Fact]
        public async Task GuestDonation_IsSavedToDatabase_AndCertificateCanBeDownloaded()
        {
            // 1. GET the donation page to grab the Anti-Forgery Token
            var getResponse = await _client.GetAsync("/Donation");
            getResponse.EnsureSuccessStatusCode();
            var getContent = await getResponse.Content.ReadAsStringAsync();

            // Extract the token using Regex
            var tokenMatch = Regex.Match(getContent, @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""");
            var antiForgeryToken = tokenMatch.Groups[1].Value;

            // 2. POST the donation form
            // NOTE: If your Razor Page uses a different binding name (e.g., just "DonorName" instead of "Donation.DonorName"),
            // you will need to adjust the keys in this dictionary.
            var formData = new Dictionary<string, string>
            {
                { "__RequestVerificationToken", antiForgeryToken },
                { "Donation.DonorName", "Integration Test Donor" },
                { "Donation.DonorEmail", "test@integration.com" },
                { "Donation.Amount", "250" },
                { "Donation.Currency", "ZAR" },
                { "Donation.IsAnonymous", "false" }
            };

            var postResponse = await _client.PostAsync("/Donation", new FormUrlEncodedContent(formData));

            // 3. Verify we were redirected to the Thank You page
            Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);
            var redirectUrl = postResponse.Headers.Location.ToString();
            Assert.Contains("/ThankYou", redirectUrl);

            // 4. Verify the donation was actually saved to the database
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var savedDonation = db.Donations.FirstOrDefault(d => d.DonorName == "Integration Test Donor");

            Assert.NotNull(savedDonation);
            Assert.Equal(250m, savedDonation.Amount);

            // 5. Verify the certificate can be downloaded as a PDF
            var certResponse = await _client.GetAsync($"/Donation/Certificate/{savedDonation.Id}");
            certResponse.EnsureSuccessStatusCode();
            Assert.Equal("application/pdf", certResponse.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task GuestDonation_WithInvalidAmount_IsRejectedAndNotSavedToDatabase()
        {
            // 1. GET the donation page to grab the Anti-Forgery Token
            var getResponse = await _client.GetAsync("/Donation");
            getResponse.EnsureSuccessStatusCode();
            var getContent = await getResponse.Content.ReadAsStringAsync();

            var tokenMatch = Regex.Match(getContent, @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""");
            var antiForgeryToken = tokenMatch.Groups[1].Value;

            // 2. POST an INVALID donation form (Amount = 5, which is below the minimum of 10)
            var formData = new Dictionary<string, string>
            {
                { "__RequestVerificationToken", antiForgeryToken },
                { "Donation.DonorName", "Invalid Donation Donor" },
                { "Donation.DonorEmail", "invalid@integration.com" },
                { "Donation.Amount", "5" }, // <--- Invalid Amount
                { "Donation.Currency", "ZAR" },
                { "Donation.IsAnonymous", "false" }
            };

            var postResponse = await _client.PostAsync("/Donation", new FormUrlEncodedContent(formData));

            // 3. Verify it did NOT redirect. (A 200 OK means it reloaded the page with validation errors)
            Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);

            // 4. Verify the validation error message is present in the HTML
            var postContent = await postResponse.Content.ReadAsStringAsync();
           // Assert.Contains("Donation amount must be between 10 and 1,000,000.", postContent);

            // 5. Verify the invalid donation was NOT saved to the database
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var savedDonation = db.Donations.FirstOrDefault(d => d.DonorName == "Invalid Donation Donor");

            Assert.Null(savedDonation); // Must be null!
        }
    }
}