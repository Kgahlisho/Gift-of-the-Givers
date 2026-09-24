using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;


namespace GiftofGivers.Functions
{
    public class GenerateTaxCertificateFunction
    {
        private readonly ILogger _logger;
        public GenerateTaxCertificateFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GenerateTaxCertificateFunction>();
        }

        // (Post) Postman and Get (Browser via route values)

        [Function("GenerateTaxCertificate")]
        public async Task<HttpResponseData> Run(
    [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post",
        Route = "GenerateTaxCertificate/{donorName?}/{amount?}")] HttpRequestData req,
            string? donorName,
            string? amount)   //string no binding
        {
            _logger.LogInformation("GenerateTaxCertificate triggered via {Method}.", req.Method);

            DonationRequest donation;

            if (req.Method == "POST")
            {
                try
                {
                    donation = await JsonSerializer.DeserializeAsync<DonationRequest>(
                        req.Body,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? new DonationRequest();
                }
                catch (JsonException)
                {
                    var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                    await bad.WriteAsJsonAsync(new { error = "Invalid JSON payload." });
                    return bad;
                }
            }
            else
            {
                //Broswer-friend;y get function

                donation = new DonationRequest
                {
                    DonorName = donorName ?? string.Empty,
                    Amount = decimal.TryParse(amount, out var parsedAmount) ? parsedAmount : 0
                };
            }

            if (string.IsNullOrWhiteSpace(donation.DonorName) || donation.Amount <= 0)
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteAsJsonAsync(new { error = "DonorName and a positive Amount are required." });
                return bad;
            }

            var certificate = new
            {
                CertificateReference = $"GOTG-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(100000, 999999)}",
                DonorName = donation.IsAnonymous ? "Anonymous Donor" : donation.DonorName,
                donation.Amount,
                Currency = string.IsNullOrWhiteSpace(donation.Currency) ? "ZAR" : donation.Currency,
                Frequency = string.IsNullOrWhiteSpace(donation.Frequency) ? "OneTime" : donation.Frequency,
                DateIssued = DateTime.UtcNow,
                Status = "Dummy certificate generated - prototype only"
            };


            _logger.LogInformation("Generated {Ref} for {Donor}.", certificate.CertificateReference, certificate.DonorName);
            _logger.LogInformation("Validation passed for donor {Donor}, proceeding to generate certificate.", donation.DonorName);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(certificate);
            return response;
        }
    }

    public class DonationRequest
    {
        public string DonorName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Currency { get; set; }
        public string? Frequency { get; set; }
        public bool IsAnonymous { get; set; }
    }
}

