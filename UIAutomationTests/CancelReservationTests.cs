using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace UIAutomationTests
{
    public class CancelReservationTests
    {
        IWebDriver _driver;

        // TODO: Reemplazar con un codigo de confirmación y apellidos reales
        // de una reservación ACTIVA (no cancelada)
        private const string ConfirmationNumber = "C73C332A465D";
        private const string LastNames = "Junior";

        [SetUp]
        public void Setup()
        {
            _driver = new ChromeDriver();
        }

        [Test]
        public void Search_Reservation_And_Cancel_Test()
        {
            // Arrange
            var URL = "http://localhost:8080/";
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
            var js = (IJavaScriptExecutor)_driver;

            _driver.Manage().Window.Maximize();

            // Act
            // Navega a la landing page
            _driver.Navigate().GoToUrl(URL);

            // Busca la reservacion con número de confirmacion y apellidos
            IWebElement confirmationInput =
                _driver.FindElement(By.XPath("//input[@placeholder='AA0A00AA0AA0']"));
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", confirmationInput);
            confirmationInput.SendKeys(ConfirmationNumber);

            IWebElement lastNamesInput =
                _driver.FindElement(By.XPath("//input[@placeholder='Ej. García Rodríguez']"));
            lastNamesInput.SendKeys(LastNames);

            IWebElement searchButton =
                _driver.FindElement(By.XPath("//button[contains(text(),'Buscar reservación')]"));
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", searchButton);
            wait.Until(d => searchButton.Displayed && searchButton.Enabled);
            js.ExecuteScript("arguments[0].click();", searchButton);

            // Assert
            // Verifica que la pagina de reservacion cargo correctamente
            wait.Until(d => d.FindElements(By.XPath("//h3[contains(text(),'Cancelar reservación')]")).Count > 0);

            IWebElement cancelSectionTitle =
                _driver.FindElement(By.XPath("//h3[contains(text(),'Cancelar reservación')]"));
            Assert.That(cancelSectionTitle.Displayed, Is.True);

            // Click en el boton "Cancelar reservacion"
            IWebElement cancelButton =
                _driver.FindElement(By.XPath("//button[contains(@class,'danger-button') and contains(text(),'Cancelar reservación')]"));
            js.ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", cancelButton);
            js.ExecuteScript("arguments[0].click();", cancelButton);

            // Verifica que aparece el modal de confirmacion
            wait.Until(d => d.FindElements(By.XPath("//h2[contains(text(),'¿Cancelar reservación?')]")).Count > 0);

            IWebElement confirmModalTitle =
                _driver.FindElement(By.XPath("//h2[contains(text(),'¿Cancelar reservación?')]"));
            Assert.That(confirmModalTitle.Displayed, Is.True);

            // Click en "Sí, cancelar reservación"
            IWebElement confirmCancelButton =
                _driver.FindElement(By.XPath("//button[contains(@class,'danger-button-solid')]"));
            js.ExecuteScript("arguments[0].click();", confirmCancelButton);

            // Verifica que aparece el modal de exito
            wait.Until(d => d.FindElements(By.XPath("//h2[contains(text(),'Correo enviado')]")).Count > 0);

            IWebElement successModalTitle =
                _driver.FindElement(By.XPath("//h2[contains(text(),'Correo enviado')]"));
            Assert.That(successModalTitle.Displayed, Is.True);

            IWebElement successModalText =
                _driver.FindElement(By.XPath("//p[contains(text(),'Hemos enviado un correo al titular')]"));
            Assert.That(successModalText.Displayed, Is.True);
        }

        [TearDown]
        public void TearDown()
        {
            _driver.Quit();
            _driver.Dispose();
        }
    }
}