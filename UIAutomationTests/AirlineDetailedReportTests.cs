using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace UIAutomationTests
{
    public class AirlineDetailedReportTests
    {
        IWebDriver _driver;

        private static readonly string AdminEmail =
            Environment.GetEnvironmentVariable("SNOOPY_TEST_EMAIL") ?? "admin@snoopyairlines.com";
        private static readonly string AdminPassword =
            Environment.GetEnvironmentVariable("SNOOPY_TEST_PASSWORD")
            ?? throw new InvalidOperationException("Set the SNOOPY_TEST_PASSWORD environment variable before running this test.");

        [SetUp]
        public void Setup()
        {
            _driver = new ChromeDriver();
        }

        [Test]
        public void TestAdminUserCanSeeReports()
        {
            // Arrange
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            var js = (IJavaScriptExecutor)_driver;

            _driver.Manage().Window.Maximize();

            // Act
            _driver.Navigate().GoToUrl("http://localhost:8080/login");

            IWebElement emailInput =
                _driver.FindElement(By.XPath("//input[@placeholder='nombre@correo.com']"));
            emailInput.SendKeys(AdminEmail);

            IWebElement passwordInput =
                _driver.FindElement(By.XPath("//input[@placeholder='Contraseña']"));
            passwordInput.SendKeys(AdminPassword);

            IWebElement loginButton =
                _driver.FindElement(By.XPath("//button[contains(text(),'Iniciar sesión')]"));
            js.ExecuteScript("arguments[0].click();", loginButton);

            wait.Until(d => d.Url.Contains("/admin"));

            _driver.Navigate().GoToUrl("http://localhost:8080/admin/reports");

            // Assert
            IWebElement reportCard =
                wait.Until(d => d.FindElement(By.XPath("//h4[contains(text(),'Vuelo detallado')]")));

            Assert.That(reportCard.Displayed, Is.True);
        }

        [TearDown]
        public void TearDown()
        {
            _driver.Quit();
            _driver.Dispose();
        }
    }
}
