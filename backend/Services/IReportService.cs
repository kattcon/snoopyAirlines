using SnoopyAirlines.Domain;

namespace SnoopyAirlines.Services
{
    public interface IReportService
    {
        Task<IReadOnlyCollection<AirlineDetailedReportRow>> GetAirlineDetailedReportAsync(
            string? origin,
            string? destination,
            string? seatClass,
            DateOnly? dateFrom,
            DateOnly? dateTo,
            string? airline,
            CancellationToken cancellationToken);
    }
}
