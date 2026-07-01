using SnoopyAirlines.Domain;

namespace SnoopyAirlines.Services
{
    public interface IReportService
    {
        Task<MonthlyRevenueFilterOptions> GetMonthlyRevenueFilterOptionsAsync(CancellationToken cancellationToken);

        Task<MonthlyRevenueReport> GetMonthlyRevenueReportAsync(
            int? year,
            int? originAirportId,
            int? destinationAirportId,
            int? partnerAirlineId,
            CancellationToken cancellationToken);

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
