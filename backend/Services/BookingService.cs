using System.Globalization;
using snoopy_airlines_backend.Domain;
using SnoopyAirlines.Domain;
using SnoopyAirlines.Repositories;
using SnoopyAirlines.Util.Email;
using SnoopyAirlines.Util.Email.Templates;
using SnoopyAirlines.Util.Pdf;
using System.Security.Cryptography;
using System.Text;

namespace SnoopyAirlines.Services
{
    public class BookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IEmailSender _emailSender;
        private readonly IInvoicePdfGenerator _invoicePdfGenerator;
        private readonly BookingConfirmationEmail _bookingConfirmationEmail;
        private readonly BookingItineraryEmail _bookingItineraryEmail;
        private readonly CancellationRequestEmail _cancellationRequestEmail;

        public BookingService(
            IBookingRepository bookingRepository,
            IEmailSender emailSender,
            IInvoicePdfGenerator invoicePdfGenerator)
        {
            _bookingRepository = bookingRepository;
            _emailSender = emailSender;
            _invoicePdfGenerator = invoicePdfGenerator;
            _bookingConfirmationEmail = new BookingConfirmationEmail();
            _bookingItineraryEmail = new BookingItineraryEmail();
            _cancellationRequestEmail = new CancellationRequestEmail();
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
                booking.Guid,
                cancellationToken);

            ApplyBookingDetails(data, booking);
            var invoice = _invoicePdfGenerator.GenerateInvoice(data);

            await _emailSender.SendAsync(
                booking.Email,
                _bookingConfirmationEmail,
                data,
                invoice,
                cancellationToken);

            await _emailSender.SendAsync(
                booking.Email,
                _bookingItineraryEmail,
                data,
                cancellationToken);
        }

        private async Task<PurchaseOrderEmailData> GetPurchaseOrderEmailDataAsync(
            Guid bookingGuid,
            CancellationToken cancellationToken)
        {
            var data = await _bookingRepository.GetBookingItineraryDetailsAsync(
                bookingGuid,
                cancellationToken);

            return data ?? throw new InvalidOperationException(
                $"Booking {bookingGuid} not found.");
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
            data.Email = booking.Email;
            data.ConfirmationCode = booking.ConfirmationCode;
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

        public async Task<bool> RequestCancellationAsync(
            string confirmationCode,
            CancellationToken cancellationToken)
        {
            var email = await _bookingRepository.GetEmailByConfirmationCodeAsync(
                confirmationCode, cancellationToken);

            if (email is null)
            {
                return false;
            }

            var (rawToken, tokenHash, expiresAt) = CreateCancellationToken();

            await _bookingRepository.StoreCancellationTokenAsync(
                confirmationCode, tokenHash, expiresAt, cancellationToken);
                
            await SendCancellationEmailAsync(
                email,
                confirmationCode,
                rawToken,
                cancellationToken);

            return true;
        }

        private async Task SendCancellationEmailAsync(
            string email,
            string confirmationCode,
            string rawToken,
            CancellationToken cancellationToken)
        {
            var cancelUrl = 
                $"https://snoopyairlines.com/Booking/cancel-booking?token={Uri.EscapeDataString(rawToken)}";

            var data = new CancellationRequestEmailData(confirmationCode, cancelUrl);

            await _emailSender.SendAsync(
                email,
                _cancellationRequestEmail,
                data,
                cancellationToken);
        }

        private static string GenerateRawToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(48))
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');
        }

        private (string RawToken, string TokenHash, DateTime ExpiresAt) CreateCancellationToken()
        {
            var rawToken  = GenerateRawToken();
            var tokenHash = HashToken(rawToken);
            var expiresAt = DateTime.UtcNow.AddHours(24);

            return (rawToken, tokenHash, expiresAt);
        }

        private static string HashToken(string rawToken)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        public async Task<bool> ConfirmCancellationAsync(
            string rawToken,
            CancellationToken cancellationToken)
        {
            var tokenHash = HashToken(rawToken);
            return await _bookingRepository.CancelByTokenHashAsync(tokenHash, cancellationToken);
        }
    }
}
