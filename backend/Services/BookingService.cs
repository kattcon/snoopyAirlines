using SnoopyAirlines.Domain;
using SnoopyAirlines.Repositories;

namespace SnoopyAirlines.Services
{
    public class BookingService
    {
        private readonly BookingRepository _bookingRepository;

        public BookingService(BookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public Task<Booking> BookAsync(BookingRequest bookingRequest, CancellationToken cancellationToken)
        {
            var normalizedRequest = NormalizeAndValidate(bookingRequest);
            return _bookingRepository.BookAsync(normalizedRequest, cancellationToken);
        }

        public Task<Booking?> GetByGuidAsync(Guid bookingGuid, CancellationToken cancellationToken)
        {
            return _bookingRepository.GetByGuidAsync(bookingGuid, cancellationToken);
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
