using SnoopyAirlines.Domain;

namespace SnoopyAirlines.Repositories
{
    public interface IReportRepository
    {
        Task<IReadOnlyCollection<AirlineDetailedReportRow>> GetAirlineDetailedReportAsync(
            string? origin,
            string? destination,
            string? seatClass,
            DateOnly? dateFrom,
            DateOnly? dateTo,
            CancellationToken cancellationToken);
    }
}
