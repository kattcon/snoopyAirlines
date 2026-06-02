using System.Globalization;
using snoopy_airlines_backend.Domain;
using snoopy_airlines_backend.Repositories;
using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.EmailTemplate;
using SnoopyAirlines.Repositories;

namespace SnoopyAirlines.Services
{
    public class BookingService
    {
        private readonly BookingRepository _bookingRepository;
        private readonly PurchaseOrderRepository _purchaseOrderRepository;
        private readonly IEmailSender _emailSender;

        public BookingService(
            BookingRepository bookingRepository,
            PurchaseOrderRepository purchaseOrderRepository,
            IEmailSender emailSender)
        {
            _bookingRepository = bookingRepository;
            _purchaseOrderRepository = purchaseOrderRepository;
            _emailSender = emailSender;
        }

        public async Task<Booking> BookAsync(
            BookingRequest bookingRequest,
            CancellationToken cancellationToken)
        {
            var normalizedRequest = NormalizeAndValidate(bookingRequest);
            var booking = await _bookingRepository.BookAsync(normalizedRequest, cancellationToken);

            await SendBookingConfirmationEmailAsync(booking, cancellationToken);

            return booking;
        }

        public Task<Booking?> GetByGuidAsync(Guid bookingGuid, CancellationToken cancellationToken)
        {
            return _bookingRepository.GetByGuidAsync(bookingGuid, cancellationToken);
        }

        private async Task SendBookingConfirmationEmailAsync(
            Booking booking,
            CancellationToken cancellationToken)
        {
            var data = await GetPurchaseOrderEmailDataAsync(
                booking.PurchaseOrderId,
                cancellationToken);

            ApplyBookingDetails(data, booking);

            var confirmationHtml = BookingConfirmationEmail.Build(data);
            var itineraryHtml = BookingItineraryEmail.Build(data);

            await _emailSender.SendAsync(
                booking.Email,
                "Confirmación de reserva - Snoopy Airlines",
                confirmationHtml,
                cancellationToken,
                isHtml: true);

            await _emailSender.SendAsync(
                booking.Email,
                "Itinerario de viaje - Snoopy Airlines",
                itineraryHtml,
                cancellationToken,
                isHtml: true);
        }

        private async Task<PurchaseOrderEmailData> GetPurchaseOrderEmailDataAsync(
            int purchaseOrderId,
            CancellationToken cancellationToken)
        {
            var data = await _purchaseOrderRepository.GetPurchaseOrderDetailsAsync(
                purchaseOrderId,
                cancellationToken);

            return data ?? throw new InvalidOperationException(
                $"Purchase order {purchaseOrderId} not found.");
        }

        private static void ApplyBookingDetails(
            PurchaseOrderEmailData data,
            Booking booking)
        {
            data.BaseFare = FormatMoney(booking.TotalAmount);
            data.Taxes = FormatMoney(0);
            data.TravelInsurance = FormatMoney(0);
            data.Total = FormatMoney(booking.TotalAmount);
            data.PaymentMethod = FormatPaymentMethod(booking);
            data.PurchaseDate = FormatPurchaseDate(booking);
            data.TransactionId = booking.Guid.ToString();
        }

        private static string FormatMoney(decimal amount)
        {
            return amount.ToString("N2", CultureInfo.InvariantCulture);
        }

        private static string FormatPaymentMethod(Booking booking)
        {
            if (string.IsNullOrWhiteSpace(booking.CardBrand)
                || string.IsNullOrWhiteSpace(booking.CardLastFour))
            {
                return "{{PAYMENT_METHOD}}";
            }

            return $"{booking.CardBrand} terminada en {booking.CardLastFour}";
        }

        private static string FormatPurchaseDate(Booking booking)
        {
            var purchaseDate = booking.ConfirmedAt == default
                ? booking.CreatedAt
                : booking.ConfirmedAt;

            return purchaseDate.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
        }

        private static BookingRequest NormalizeAndValidate(BookingRequest bookingRequest)
        {
            if (bookingRequest.PurchaseOrderId <= 0)
            {
                throw new ArgumentException("purchaseOrderId must be greater than 0.");
            }

            var email = bookingRequest.Email.Trim();
            var cardBrand = bookingRequest.CardBrand.Trim();
            var cardLastFour = bookingRequest.CardLastFour.Trim();
            var cardHolderName = bookingRequest.CardHolderName.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("email is required.");
            }

            if (string.IsNullOrWhiteSpace(cardBrand))
            {
                throw new ArgumentException("cardBrand is required.");
            }

            if (!cardLastFour.All(char.IsDigit) || cardLastFour.Length != 4)
            {
                throw new ArgumentException("cardLastFour must contain exactly 4 digits.");
            }

            if (string.IsNullOrWhiteSpace(cardHolderName))
            {
                throw new ArgumentException("cardHolderName is required.");
            }

            return new BookingRequest
            {
                PurchaseOrderId = bookingRequest.PurchaseOrderId,
                Email = email,
                CardBrand = cardBrand,
                CardLastFour = cardLastFour,
                CardHolderName = cardHolderName
            };
        }
    }
}
