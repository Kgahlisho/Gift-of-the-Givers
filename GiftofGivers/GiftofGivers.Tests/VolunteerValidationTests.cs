using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using GiftofGivers.Models;
using Xunit;

namespace GiftofGivers.Tests
{
    public class VolunteerValidationTests
    {
        [Fact]
        public void VolunteerSignup_WithValidData_IsValid()
        {
            var volunteer = new VolunteerSignup
            {
                FullName = "John Doe",
                Email = "john@example.com",
                PhoneNumber = "0821234567",
                Skills = "First Aid, Logistics",
                Availability = "Weekends"
            };

            var context = new ValidationContext(volunteer);
            var results = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(volunteer, context, results, true);

            Assert.True(isValid);
            Assert.Empty(results);
        }

        [Fact]
        public void VolunteerSignup_WithInvalidEmail_IsInvalid()
        {
            var volunteer = new VolunteerSignup
            {
                FullName = "John Doe",
                Email = "not-an-email",
                Skills = "Driving",
                Availability = "Full-time"
            };

            var context = new ValidationContext(volunteer) { MemberName = nameof(VolunteerSignup.Email) };
            var results = new List<ValidationResult>();

            Validator.TryValidateProperty(volunteer.Email, context, results);

            Assert.NotEmpty(results);
        }

        [Fact]
        public void VolunteerSignup_WithMissingSkills_IsInvalid()
        {
            var volunteer = new VolunteerSignup
            {
                FullName = "John Doe",
                Email = "john@example.com",
                Availability = "Weekends"
                // Skills is intentionally left empty to trigger [Required]
            };

            var context = new ValidationContext(volunteer) { MemberName = nameof(VolunteerSignup.Skills) };
            var results = new List<ValidationResult>();

            Validator.TryValidateProperty(volunteer.Skills, context, results);

            Assert.NotEmpty(results);
        }
    }
}