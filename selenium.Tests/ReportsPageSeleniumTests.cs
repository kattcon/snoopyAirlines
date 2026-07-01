using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace selenium.Tests;

public class ReportsPageSeleniumTests
{
    private static string FrontendBaseUrl =>
        Environment.GetEnvironmentVariable("E2E_BASE_URL")?.TrimEnd('/')
        ?? "http://localhost:8080";

    [Fact]
    public void AdminReportsPage_ShowsCardsAndExportButton()
    {
        if (!IsSeleniumEnabled())
        {
            return;
        }

        var options = new ChromeOptions();
        options.AddArgument("--headless=new");
        options.AddArgument("--window-size=1600,1000");

        using var driver = new ChromeDriver(options);
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

        driver.Navigate().GoToUrl($"{FrontendBaseUrl}/admin/reports");

        wait.Until(d => d.FindElement(By.XPath("//h3[contains(normalize-space(.), 'Reportes')]")));
        wait.Until(d => d.FindElement(By.XPath("//h4[contains(normalize-space(.), 'Ingresos por mes')]")));

        driver.FindElement(By.XPath("//h4[contains(normalize-space(.), 'Ingresos por mes')]/ancestor::button[1]")).Click();

        wait.Until(d => d.FindElement(By.XPath("//button[contains(normalize-space(.), 'Exportar XLSX')]")));
    }

    private static bool IsSeleniumEnabled()
    {
        var runSelenium = Environment.GetEnvironmentVariable("RUN_SELENIUM_TESTS");

        return string.Equals(runSelenium, "true", StringComparison.OrdinalIgnoreCase);
    }
}
