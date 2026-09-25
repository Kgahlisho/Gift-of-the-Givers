namespace GiftofGivers.Helpers
{
  
    public static class DonationHelper
    {

        // Generates a certificate reference number in the format GOTG-{year}-{6-digit random}.
        // Centralises the format used across the web app and the Azure Function.

        public static string GenerateCertificateNumber()
        {
            return $"GOTG-{DateTime.UtcNow:yyyy}-{Random.Shared.Next(100000, 999999)}";
        }


        // Calculates the total donation amount including an optional processing fee.

        public static decimal CalculateTotal(decimal amount, decimal feePercentage = 0)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Donation amount cannot be negative.");
            var fee = amount * (feePercentage / 100m);
            return Math.Round(amount + fee, 2);
        }


        // Formats a donation amount with its currency code for display, e.g. "ZAR 1,500.00".

        public static string FormatCurrency(decimal amount, string currencyCode)
        {
            return $"{currencyCode.ToUpperInvariant()} {amount:N2}";
        }
    }
}