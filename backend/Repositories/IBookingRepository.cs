using SnoopyAirlines.Domain;
using snoopy_airlines_backend.Domain;

namespace SnoopyAirlines.Repositories
{
    public interface IBookingRepository
    {
        Task<Booking> BookAsync(BookingRequest bookingRequest, CancellationToken cancellationToken);
        Task<Booking?> GetByGuidAsync(Guid bookingGuid, CancellationToken cancellationToken);
        Task<PurchaseOrderEmailData?> GetBookingItineraryDetailsAsync(Guid bookingGuid, CancellationToken cancellationToken);
    }
}
