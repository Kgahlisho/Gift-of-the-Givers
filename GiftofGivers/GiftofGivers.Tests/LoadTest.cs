using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using GiftofGivers.Data;
using System.Net;

namespace GiftofGivers.Tests
{
    public class LoadTest : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly CustomWebApplicationFactory<Program> _factory;
        private readonly ITestOutputHelper _output;

        public LoadTest(CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
        {
            _factory = factory;
            _output = output;
        }

        [Fact]
        public async Task Run_LoadTest_15Users_10Iterations()
        {
            // 1. Increased users from 5 to 15
            int virtualUsers = 15;
            int iterationsPerUser = 10;
            var responseTimes = new ConcurrentBag<long>();
            int errorCount = 0;

            var tasks = Enumerable.Range(1, virtualUsers).Select(async userId =>
            {
                var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
                for (int i = 1; i <= iterationsPerUser; i++)
                {
                    var stopwatch = Stopwatch.StartNew();
                    try
                    {
                        var homeResponse = await client.GetAsync("/");
                        if (!homeResponse.IsSuccessStatusCode) System.Threading.Interlocked.Increment(ref errorCount);

                        var donationGet = await client.GetAsync("/Donation");
                        if (!donationGet.IsSuccessStatusCode) System.Threading.Interlocked.Increment(ref errorCount);
                        var html = await donationGet.Content.ReadAsStringAsync();

                        var tokenMatch = Regex.Match(html, @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""");
                        var formData = new Dictionary<string, string>
                        {
                            { "__RequestVerificationToken", tokenMatch.Groups[1].Value },
                            { "Donation.DonorName", $"Load Test User {userId}-{i}" },
                            { "Donation.Amount", "50" },
                            { "Donation.Currency", "ZAR" }
                        };
                        var postResponse = await client.PostAsync("/Donation", new FormUrlEncodedContent(formData));
                        if (postResponse.StatusCode != HttpStatusCode.Redirect) System.Threading.Interlocked.Increment(ref errorCount);
                    }
                    catch { System.Threading.Interlocked.Increment(ref errorCount); }
                    finally { stopwatch.Stop(); responseTimes.Add(stopwatch.ElapsedMilliseconds); }
                }
            });

            await Task.WhenAll(tasks);

            var sortedTimes = responseTimes.OrderBy(t => t).ToList();
            long p95 = sortedTimes[(int)Math.Ceiling(0.95 * sortedTimes.Count) - 1];
            long avg = (long)sortedTimes.Average();
            long max = sortedTimes.Last();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            int savedDonations = db.Donations.Count(d => d.DonorName.StartsWith("Load Test User"));

            _output.WriteLine("--------------------------------------------------");
            _output.WriteLine($"Total Requests: {responseTimes.Count * 3}");
            _output.WriteLine($"Errors: {errorCount}");
            _output.WriteLine($"Saved Donations: {savedDonations}");
            _output.WriteLine($"Average Response Time: {avg} ms");
            _output.WriteLine($"P95 Response Time: {p95} ms");
            _output.WriteLine($"Max Response Time: {max} ms");
            _output.WriteLine("--------------------------------------------------");

            Assert.Equal(0, errorCount);
            Assert.True(p95 < 2000);
            // 2. Update the expected count to 150
            Assert.Equal(150, savedDonations);
        }
    }
}