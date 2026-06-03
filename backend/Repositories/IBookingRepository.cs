using SnoopyAirlines.Domain;

namespace SnoopyAirlines.Repositories
{
    public interface IBookingRepository
    {
        Task<Booking> BookAsync(BookingRequest bookingRequest, CancellationToken cancellationToken);
        Task<Booking?> GetByGuidAsync(Guid bookingGuid, CancellationToken cancellationToken);
    }
}
