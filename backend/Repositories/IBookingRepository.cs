using SnoopyAirlines.Domain;
using snoopy_airlines_backend.Domain;

namespace SnoopyAirlines.Repositories
{
    public interface IBookingRepository
    {
        Task<Booking> BookAsync(BookingRequest bookingRequest, CancellationToken cancellationToken);
        Task<Booking?> GetByGuidAsync(Guid bookingGuid, CancellationToken cancellationToken);
        Task<PurchaseOrderEmailData?> GetBookingItineraryDetailsAsync(Guid bookingGuid, CancellationToken cancellationToken);
        Task<string?> GetEmailByConfirmationCodeAsync(string confirmationCode, CancellationToken cancellationToken);
        Task StoreCancellationTokenAsync(string confirmationCode, string tokenHash, DateTime expiresAt, CancellationToken cancellationToken);
        Task<bool> CancelByTokenHashAsync(string tokenHash, CancellationToken cancellationToken);
    }
}
