using Moq;
using snoopy_airlines_backend.Domain;
using SnoopyAirlines.Domain;
using SnoopyAirlines.Repositories;
using SnoopyAirlines.Services;
using SnoopyAirlines.Util.Email;
using SnoopyAirlines.Util.Email.Templates;
using SnoopyAirlines.Util.Pdf;
using Xunit;

namespace backend.Tests.Services
{
    public class BookingServiceTests
    {
        private readonly Mock<IBookingRepository> _bookingRepository = new();
        private readonly Mock<IEmailSender> _emailSender = new();
        private readonly Mock<IInvoicePdfGenerator> _invoicePdfGenerator = new();

        public BookingServiceTests()
        {
            _emailSender
                .Setup(s => s.SendAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEmailTemplate<PurchaseOrderEmailData>>(),
                    It.IsAny<PurchaseOrderEmailData>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _emailSender
                .Setup(s => s.SendAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEmailTemplate<PurchaseOrderEmailData>>(),
                    It.IsAny<PurchaseOrderEmailData>(),
                    It.IsAny<Attachment>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _invoicePdfGenerator
                .Setup(generator => generator.GenerateInvoice(It.IsAny<PurchaseOrderEmailData>()))
                .Returns(CreateInvoiceAttachment());
        }

        [Fact]
        public async Task TestBookAsyncNormalizesRequestAndReturnsBooking()
        {
            // Arrange
            var booking = CreateBooking();
            BookingRequest? capturedRequest = null;

            _bookingRepository
                .Setup(r => r.BookAsync(It.IsAny<BookingRequest>(), It.IsAny<CancellationToken>()))
                .Callback<BookingRequest, CancellationToken>((request, _) => capturedRequest = request)
                .ReturnsAsync(booking);

            SetupBookingItineraryDetails(booking.Guid, booking.PurchaseOrderId);
            var service = CreateService();

            var request = new BookingRequest
            {
                PurchaseOrderId = booking.PurchaseOrderId,
                Email = "  traveler@example.com  ",
                CardBrand = "  Visa  ",
                CardLastFour = "  4242  ",
                CardHolderName = "  Jane Doe  "
            };

            // Act
            var result = await service.BookAsync(request, CancellationToken.None);

            // Assert
            Assert.Same(booking, result);
            Assert.NotNull(capturedRequest);
            Assert.Equal(booking.PurchaseOrderId, capturedRequest!.PurchaseOrderId);
            Assert.Equal("traveler@example.com", capturedRequest.Email);
            Assert.Equal("Visa", capturedRequest.CardBrand);
            Assert.Equal("4242", capturedRequest.CardLastFour);
            Assert.Equal("Jane Doe", capturedRequest.CardHolderName);
        }

        [Fact]
        public async Task TestBookAsyncSendsConfirmationAndItineraryEmails()
        {
            // Arrange
            var booking = CreateBooking(totalAmount: 1234.50m);
            var sentMessages = new List<(string To, IEmailTemplate<PurchaseOrderEmailData> Template, PurchaseOrderEmailData Data, Attachment? Attachment)>();

            SetupBooking(booking);
            SetupBookingItineraryDetails(booking.Guid, booking.PurchaseOrderId);

            _emailSender
                .Setup(s => s.SendAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEmailTemplate<PurchaseOrderEmailData>>(),
                    It.IsAny<PurchaseOrderEmailData>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, IEmailTemplate<PurchaseOrderEmailData>, PurchaseOrderEmailData, CancellationToken>(
                    (to, template, data, _) => sentMessages.Add((to, template, data, null)))
                .Returns(Task.CompletedTask);

            _emailSender
                .Setup(s => s.SendAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEmailTemplate<PurchaseOrderEmailData>>(),
                    It.IsAny<PurchaseOrderEmailData>(),
                    It.IsAny<Attachment>(),
                    It.IsAny<CancellationToken>()))
                .Callback<string, IEmailTemplate<PurchaseOrderEmailData>, PurchaseOrderEmailData, Attachment, CancellationToken>(
                    (to, template, data, attachment, _) => sentMessages.Add((to, template, data, attachment)))
                .Returns(Task.CompletedTask);

            var service = CreateService();

            // Act
            await service.BookAsync(CreateValidRequest(booking.PurchaseOrderId), CancellationToken.None);

            // Assert
            Assert.Equal(2, sentMessages.Count);
            Assert.All(sentMessages, message =>
            {
                Assert.Equal(booking.Email, message.To);
            });

            Assert.Contains(sentMessages, message =>
                message.Template is BookingConfirmationEmail
                && message.Attachment is not null
                && message.Attachment.FileName == "Factura-SA-123456.pdf"
                && message.Attachment.Format == "application/pdf"
                && message.Template.Subject == "Confirmaci\u00f3n de reserva - Snoopy Airlines"
                && GetTemplateParameter(message.Template, message.Data, "TOTAL") == "1,234.50"
                && GetTemplateParameter(message.Template, message.Data, "TRANSACTION_ID") == booking.Guid.ToString());

            Assert.Contains(sentMessages, message =>
                message.Template is BookingItineraryEmail
                && message.Attachment is null
                && message.Template.Subject == "Itinerario de viaje - Snoopy Airlines"
                && GetTemplateParameter(message.Template, message.Data, "DEPARTURE_CITY") == "San Jose"
                && GetTemplateParameter(message.Template, message.Data, "PASSENGERS_HTML").Contains("Jane Doe"));

            _invoicePdfGenerator.Verify(
                generator => generator.GenerateInvoice(It.Is<PurchaseOrderEmailData>(
                    data => data.ConfirmationCode == booking.ConfirmationCode
                        && data.Total == "1,234.50")),
                Times.Once);
        }

        [Fact]
        public async Task TestBookAsyncInvalidCardLastFourThrowsArgumentException()
        {
            // Arrange
            var service = CreateService();
            var request = CreateValidRequest();
            request.CardLastFour = "12A4";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.BookAsync(request, CancellationToken.None));

            _bookingRepository.Verify(
                r => r.BookAsync(It.IsAny<BookingRequest>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _bookingRepository.Verify(
                r => r.GetBookingItineraryDetailsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never);
            _emailSender.Verify(
                s => s.SendAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEmailTemplate<PurchaseOrderEmailData>>(),
                    It.IsAny<PurchaseOrderEmailData>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
            _emailSender.Verify(
                s => s.SendAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEmailTemplate<PurchaseOrderEmailData>>(),
                    It.IsAny<PurchaseOrderEmailData>(),
                    It.IsAny<Attachment>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
            _invoicePdfGenerator.Verify(
                generator => generator.GenerateInvoice(It.IsAny<PurchaseOrderEmailData>()),
                Times.Never);
        }

        [Fact]
        public async Task TestBookAsyncWhenPurchaseOrderDetailsMissingThrowsInvalidOperationException()
        {
            // Arrange
            var booking = CreateBooking();

            SetupBooking(booking);
            SetupMissingBookingItineraryDetails(booking.Guid);

            var service = CreateService();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.BookAsync(CreateValidRequest(booking.PurchaseOrderId), CancellationToken.None));

            Assert.Equal($"Booking {booking.Guid} not found.", exception.Message);
            _emailSender.Verify(
                s => s.SendAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEmailTemplate<PurchaseOrderEmailData>>(),
                    It.IsAny<PurchaseOrderEmailData>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
            _emailSender.Verify(
                s => s.SendAsync(
                    It.IsAny<string>(),
                    It.IsAny<IEmailTemplate<PurchaseOrderEmailData>>(),
                    It.IsAny<PurchaseOrderEmailData>(),
                    It.IsAny<Attachment>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
            _invoicePdfGenerator.Verify(
                generator => generator.GenerateInvoice(It.IsAny<PurchaseOrderEmailData>()),
                Times.Never);
        }

        private BookingService CreateService()
        {
            return new BookingService(
                _bookingRepository.Object,
                _emailSender.Object,
                _invoicePdfGenerator.Object);
        }

        private void SetupBooking(Booking booking)
        {
            _bookingRepository
                .Setup(r => r.BookAsync(It.IsAny<BookingRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(booking);
        }

        private void SetupBookingItineraryDetails(Guid bookingGuid, int purchaseOrderId)
        {
            _bookingRepository
                .Setup(r => r.GetBookingItineraryDetailsAsync(bookingGuid, It.IsAny<CancellationToken>()))
                .ReturnsAsync(CreatePurchaseOrderEmailData(purchaseOrderId));
        }

        private void SetupMissingBookingItineraryDetails(Guid bookingGuid)
        {
            _bookingRepository
                .Setup(r => r.GetBookingItineraryDetailsAsync(bookingGuid, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PurchaseOrderEmailData?)null);
        }

        private static BookingRequest CreateValidRequest(int purchaseOrderId = 42)
        {
            return new BookingRequest
            {
                PurchaseOrderId = purchaseOrderId,
                Email = "traveler@example.com",
                CardBrand = "Visa",
                CardLastFour = "4242",
                CardHolderName = "Jane Doe"
            };
        }

        private static Booking CreateBooking(
            int purchaseOrderId = 42,
            string email = "traveler@example.com",
            decimal totalAmount = 250m)
        {
            return new Booking
            {
                Guid = Guid.Parse("11111111-2222-3333-4444-555555555555"),
                PurchaseOrderId = purchaseOrderId,
                Itinerary =
                [
                    new ItineraryLeg
                    {
                        BookingGuid = Guid.Parse("11111111-2222-3333-4444-555555555555"),
                        SequenceNumber = 1,
                        FlightGuid = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee")
                    }
                ],
                ConfirmationCode = "SA-123456",
                Email = email,
                Status = "Confirmed",
                TotalAmount = totalAmount,
                CardBrand = "Visa",
                CardLastFour = "4242",
                CardHolderName = "Jane Doe",
                CreatedAt = new DateTime(2026, 5, 20, 13, 15, 0),
                ConfirmedAt = new DateTime(2026, 5, 20, 14, 30, 0)
            };
        }

        private static PurchaseOrderEmailData CreatePurchaseOrderEmailData(int purchaseOrderId)
        {
            return new PurchaseOrderEmailData
            {
                PurchaseOrderId = purchaseOrderId,
                SeatClass = "Economy",
                DepartureAt = new DateTime(2026, 6, 15, 8, 0, 0),
                ArrivalAt = new DateTime(2026, 6, 15, 12, 30, 0),
                DepartureAirportName = "Juan Santamaria International Airport",
                DepartureAirportCode = "SJO",
                DepartureCityName = "San Jose",
                ArrivalAirportName = "Los Angeles International Airport",
                ArrivalAirportCode = "LAX",
                ArrivalCityName = "Los Angeles",
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

        private static string GetTemplateParameter<T>(
            IEmailTemplate<T> template,
            T data,
            string name)
        {
            return template
                .GetParameters(data)
                .Single(parameter => parameter.Name == name)
                .Value;
        }

        private static Attachment CreateInvoiceAttachment()
        {
            return Attachment.FromBytes(
                "Factura-SA-123456.pdf",
                "application/pdf",
                [0x25, 0x50, 0x44, 0x46]);
        }
    }
}
