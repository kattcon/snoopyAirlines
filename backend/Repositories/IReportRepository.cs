using SnoopyAirlines.Domain;

namespace SnoopyAirlines.Repositories
{
    public interface IReportRepository
    {
        Task<MonthlyRevenueFilterOptions> GetMonthlyRevenueFilterOptionsAsync(CancellationToken cancellationToken);

        Task<IReadOnlyCollection<MonthlyRevenueReportRow>> GetMonthlyRevenueBreakdownAsync(
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
