using snoopy_airlines_backend.Domain;

namespace snoopy_airlines_backend.Repositories
{
    public interface IReportRepository
    {
        Task<MonthlyRevenueFilterOptions> GetMonthlyRevenueFilterOptionsAsync(CancellationToken cancellationToken);

        Task<IReadOnlyCollection<MonthlyRevenueReportRow>> GetMonthlyRevenueBreakdownAsync(
            int? year,
            int? originAirportId,
            int? destinationAirportId,
            int? airplaneId,
            CancellationToken cancellationToken);
    }
}