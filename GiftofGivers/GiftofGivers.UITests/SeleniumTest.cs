using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit;

namespace GiftofGivers.UITests
{
    public class SeleniumTest : IDisposable
    {
        protected readonly IWebDriver Driver;
        protected readonly string BaseUrl;

        public SeleniumTest()
        {
            // Allows you to override the URL via environment variable if your port changes
            BaseUrl = Environment.GetEnvironmentVariable("GOTG_BASE_URL") ?? "https://localhost:7209";

            var options = new ChromeOptions();
            // Accept self-signed SSL certificates (common for localhost)
            options.AcceptInsecureCertificates = true;

            Driver = new ChromeDriver(options);
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        public void Dispose()
        {
            Driver.Quit();
            Driver.Dispose();
        }
    }
}