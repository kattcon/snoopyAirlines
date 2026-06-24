using snoopy_airlines_backend.Domain.View;
using SnoopyAirlines.Domain.View;

namespace snoopy_airlines_backend.Repositories
{
    public interface IFlightLuggageRepository
    {
        Task<IReadOnlyCollection<PassengerView>> GetFlightLuggageInfoByConfirmationAsync(
            string confirmationNumber,
            CancellationToken cancellationToken
        );
    }
}