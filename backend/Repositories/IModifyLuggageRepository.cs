using snoopy_airlines_backend.Domain.Intake;

namespace snoopy_airlines_backend.Repositories
{
    public interface IModifyLuggageRepository
    {
        Task UpdateLuggageAsync(
            ModifyLuggageRequest modifyLuggageRequest,
            CancellationToken cancellationToken
        );
    }
}
