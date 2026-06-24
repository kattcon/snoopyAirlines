using snoopy_airlines_backend.Domain.View;

namespace snoopy_airlines_backend.Services
{
    public interface IModifyLuggageService
    {
        Task<ModifyLuggageInfo> GetLuggageInfoAsync(
            string confirmationNumber,
            CancellationToken cancellationToken
        );
    }
}