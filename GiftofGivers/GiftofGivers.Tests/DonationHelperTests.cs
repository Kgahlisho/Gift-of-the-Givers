using GiftofGivers.Helpers;
using System;
using Xunit;

namespace GiftofGivers.Tests
{
    public class DonationHelperTests
    {
        [Fact]
        public void GenerateCertificateNumber_ReturnsCorrectFormat()
        {
            // Act
            var reference = DonationHelper.GenerateCertificateNumber();

            // Assert
            Assert.NotNull(reference);
            // Should look like: GOTG-2026-123456
            Assert.StartsWith($"GOTG-{DateTime.UtcNow.Year}-", reference);
            Assert.Equal(16, reference.Length); // 5 + 4 + 1 + 6 = 16 characters
        }

        [Theory]
        [InlineData(100, 0, 100)]       // No fee
        [InlineData(100, 10, 110)]      // 10% fee
        [InlineData(150.50, 5, 158.02)] // Changed to .02 for Banker's Rounding
        public void CalculateTotal_ReturnsCorrectAmount(decimal amount, decimal fee, decimal expectedTotal)
        {
            // Act
            var total = DonationHelper.CalculateTotal(amount, fee);

            // Assert
            Assert.Equal(expectedTotal, total);
        }

        [Fact]
        public void CalculateTotal_NegativeAmount_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                DonationHelper.CalculateTotal(-50m));
        }

        [Theory]
        [InlineData(1500, "zar", "ZAR 1,500.00")]
        [InlineData(500.5, "USD", "USD 500.50")]
        public void FormatCurrency_ReturnsCorrectFormattedString(decimal amount, string currencyCode, string expected)
        {
            // Act
            var formatted = DonationHelper.FormatCurrency(amount, currencyCode);

            // Assert
            Assert.Equal(expected, formatted);
        }
    }
}