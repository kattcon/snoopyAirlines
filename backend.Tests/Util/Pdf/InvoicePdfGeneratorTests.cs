using snoopy_airlines_backend.Domain;
using SnoopyAirlines.Util.Pdf;
using Xunit;

namespace backend.Tests.Util.Pdf
{
    public class InvoicePdfGeneratorTests
    {
        [Fact]
        public void GenerateInvoiceReturnsPdfAttachment()
        {
            // Arrange
            var generator = new InvoicePdfGenerator();
            var data = CreatePurchaseOrderEmailData();

            // Act
            var attachment = generator.GenerateInvoice(data);

            // Assert
            Assert.Equal("Factura-SA-123456.pdf", attachment.FileName);
            Assert.Equal("application/pdf", attachment.Format);

            using var stream = attachment.OpenRead();
            var header = new byte[4];
            var read = stream.Read(header, 0, header.Length);

            Assert.Equal(4, read);
            Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(header));
        }

        private static PurchaseOrderEmailData CreatePurchaseOrderEmailData()
        {
            return new PurchaseOrderEmailData
            {
                ConfirmationCode = "SA-123456",
                Email = "traveler@example.com",
                BookingStatus = "Confirmed",
                TotalAmount = 1234.50m,
                CardBrand = "Visa",
                CardLastFour = "4242",
                CardHolderName = "Jane Doe",
                CreatedAt = new DateTime(2026, 5, 20, 13, 15, 0),
                ConfirmedAt = new DateTime(2026, 5, 20, 14, 30, 0),
                PurchaseOrderId = 42,
                SeatClass = "Economy",
                DepartureAirportName = "Juan Santamaria International Airport",
                DepartureAirportCode = "SJO",
                DepartureCityName = "San Jose",
                DepartureAt = new DateTime(2026, 6, 15, 8, 0, 0),
                ArrivalAirportName = "Los Angeles International Airport",
                ArrivalAirportCode = "LAX",
                ArrivalCityName = "Los Angeles",
                ArrivalAt = new DateTime(2026, 6, 15, 12, 30, 0),
                BaseFare = "1,234.50",
                Taxes = "0.00",
                TravelInsurance = "0.00",
                Total = "1,234.50",
                PaymentMethod = "Visa terminada en 4242",
                PurchaseDate = "20/05/2026 14:30",
                TransactionId = "11111111-2222-3333-4444-555555555555",
                Passengers =
                [
                    new PassengerEmailData
                    {
                        FirstName = "Jane",
                        LastName = "Doe",
                        Gender = "Female",
                        Nationality = "Costa Rican",
                        BirthDay = "15",
                        BirthMonth = "March",
                        BirthYear = "1990"
                    }
                ]
            };
        }
    }
}
