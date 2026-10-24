using GiftofGivers.Models;
using GiftofGivers.Services;
using QuestPDF.Infrastructure;
using System;
using System.Text;
using Xunit;

namespace GiftofGivers.Tests
{
    public class CertificateTests
    {

        static CertificateTests()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        [Fact]
        public void GenerateCertificate_ReturnsValidPdfBytes()
        {
            // Arrange
            var donation = new Donation
            {
                DonorName = "Test Donor",
                Amount = 500m,
                CertificateReference = "GOTG-2026-000001",
                DonationDate = DateTime.UtcNow
            };

            // Act
            byte[] pdfBytes = TaxCertificateGenerator.Generate(donation);

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.True(pdfBytes.Length > 0);

            // PDF files always start with the "%PDF" magic number
            string header = Encoding.ASCII.GetString(pdfBytes, 0, 4);
            Assert.Equal("%PDF", header);
        }
    }
}