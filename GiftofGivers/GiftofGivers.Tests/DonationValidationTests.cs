
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GiftofGivers.Models;
using Xunit;
using System;

namespace GiftofGivers.Tests
{
    public class DonationValidationTests
    {

        private static List<ValidationResult> ValidateModel(Donation donation)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(donation);

            bool isValid = Validator.TryValidateObject(
                donation,
                context,
                results,
                validateAllProperties: true);

            // Diagnostic check: invalid data must fail validation.
            if (!isValid && results.Count == 0)
            {
                throw new InvalidOperationException(
                    "Validation failed, but no validation errors were returned.");
            }

            return results;
        }


        [Fact]
        public void Donation_WithValidAmount_IsValid()
        {
            var donation = new Donation
            {
                DonorName = "Test Donor",
                Amount = 100m
            };

            var results = ValidateModel(donation);

            Assert.Empty(results);
        }


        [Theory]
        [InlineData(0)]
        [InlineData(9.99)]
        [InlineData(1000000.01)]
        public void Donation_WithAmountOutsideRange_IsInvalid(decimal amount)
        {
            var donation = new Donation
            {
                DonorName = "Test Donor",
                Amount = amount
            };

            var results = new List<ValidationResult>();
            var context = new ValidationContext(donation);

            Validator.TryValidateProperty(
                donation.Amount,
                new ValidationContext(donation)
                {
                    MemberName = nameof(Donation.Amount)
                },
                results);

            Assert.NotEmpty(results);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(1000000)]
        public void Donation_WithBoundaryAmount_IsValid(decimal amount)
        {
            var donation = new Donation
            {
                DonorName = "Test Donor",
                Amount = amount
            };

            var results = ValidateModel(donation);

            Assert.Empty(results);
        }


        [Fact]
        public void Donation_WithInvalidEmail_IsInvalid()
        {
            var donation = new Donation
            {
                DonorName = "Test Donor",
                DonorEmail = "invalid-email",
                Amount = 100m
            };

            var results = new List<ValidationResult>();

            var context = new ValidationContext(donation)
            {
                MemberName = nameof(Donation.DonorEmail)
            };

            Validator.TryValidateProperty(
                donation.DonorEmail,
                context,
                results);

            Assert.NotEmpty(results);
        }


        [Fact]
        public void Donation_DefaultName_IsAnonymousDonor()
        {
            var donation = new Donation();

            Assert.Equal("Anonymous Donor", donation.DonorName);
        }


        [Fact]
        public void Donation_AmountProperty_HasRangeAttribute()
        {
            var property = typeof(Donation).GetProperty(nameof(Donation.Amount));

            Assert.NotNull(property);

            var attributes = property!.GetCustomAttributesData();

            Assert.Contains(attributes, attribute =>
                attribute.AttributeType == typeof(RangeAttribute));
        }

    }
}
