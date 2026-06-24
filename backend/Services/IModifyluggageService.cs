using snoopy_airlines_backend.Domain.View;

namespace snoopy_airlines_backend.Services
{
    public interface IModifyluggageService
    {
        Task<IReadOnlyCollection<PassengerView>> GetPassengersByConfirmationAsync(
            string confirmationNumber,
            CancellationToken cancellationToken
        );

        Task<FlightLuggageView> GetFlightLuggageInfoByConfirmationAsync(
            string confirmationNumber,
            CancellationToken cancellationToken
        );

        Task<ModifyLuggageInfo> GetLuggageInfoAsync(
            string confirmationNumber,
            CancellationToken cancellationToken
        );
    }
}