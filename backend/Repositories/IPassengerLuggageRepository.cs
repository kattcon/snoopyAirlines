using snoopy_airlines_backend.Domain.View;

namespace snoopy_airlines_backend.Repositories
{
    public interface IPassengerLuggageRepository
    {
        Task<IReadOnlyCollection<PassengerView>> GetPassengersByConfirmationAsync(
            string confirmationNumber,
            CancellationToken cancellationToken);
    }
}
