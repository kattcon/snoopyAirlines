using SnoopyAirlines.Domain.Airlines;

namespace SnoopyAirlines.Repositories
{
    public interface IPartnerAirlineRepository
    {
        Task<IReadOnlyCollection<PartnerAirline>> GetAllAsync(CancellationToken cancellationToken);
    }
}
