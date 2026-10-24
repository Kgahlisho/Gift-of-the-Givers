using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace GiftofGivers.UITests
{
    public class UIScenarioTest : SeleniumTest
    {
        [Fact]
        public void GuestDonation_WithValidation_AndCertificateLink()
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(5));

            // 1. Go to Donation page
            Driver.Navigate().GoToUrl($"{BaseUrl}/Donation");

            // 2. Submit invalid amount
            // Note: If this fails, it means your input ID is just "Amount" instead of "Donation_Amount"
            var amountInput = wait.Until(d => d.FindElement(By.Id("Donation_Amount")));
            amountInput.Clear();
            amountInput.SendKeys("5");

            var submitBtn = Driver.FindElement(By.CssSelector("button[type='submit']"));
            submitBtn.Click();

            // 3. Verify validation error (Page reloads here!)
            Assert.Contains("/Donation", Driver.Url);
            var validationText = wait.Until(d => d.FindElement(By.CssSelector(".validation-summary-errors, .field-validation-error"))).Text;
            Assert.False(string.IsNullOrWhiteSpace(validationText));

            // 4. RE-FIND ELEMENTS because the page reloaded!
            var nameInput = wait.Until(d => d.FindElement(By.Id("Donation_DonorName")));
            nameInput.Clear();
            nameInput.SendKeys("Selenium Donor");

            amountInput = wait.Until(d => d.FindElement(By.Id("Donation_Amount")));
            amountInput.Clear();
            amountInput.SendKeys("150");

            submitBtn = wait.Until(d => d.FindElement(By.CssSelector("button[type='submit']")));
            submitBtn.Click();

            // 5. Verify redirect to Thank You page
            wait.Until(d => d.Url.Contains("/ThankYou"));

            // 6. Verify the certificate link
            var certLink = wait.Until(d => d.FindElement(By.PartialLinkText("Certificate")));
            Assert.NotNull(certLink);
            Assert.Contains("/Donation/Certificate/", certLink.GetAttribute("href"));
        }

        [Fact]
        public void EmployeeLogin_NavigatesToDashboard()
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(5));

            // 1. Go to your CUSTOM Login page
            Driver.Navigate().GoToUrl($"{BaseUrl}/Account/Login");

            // 2. Enter Credentials
            var emailInput = wait.Until(d => d.FindElement(By.Id("Input_Email")));
            var passwordInput = Driver.FindElement(By.Id("Input_Password"));

            emailInput.SendKeys("employee@giftofthegivers.org");

            var password = Environment.GetEnvironmentVariable("GOTG_EMPLOYEE_PASSWORD") ?? "Employee@123";
            passwordInput.SendKeys(password);

            var loginBtn = Driver.FindElement(By.CssSelector("button[type='submit']"));
            loginBtn.Click();

            // 3. Verify successful login by checking for the "Log out" button in your layout
            var logoutBtn = wait.Until(d => d.FindElement(By.XPath("//button[contains(text(), 'Log out')]")));
            Assert.NotNull(logoutBtn);

            // 4. Verify we can navigate to the Employee Dashboard
            Driver.Navigate().GoToUrl($"{BaseUrl}/Employee/Dashboard");
            Assert.Contains("/Employee/Dashboard", Driver.Url);
        }
    }
}