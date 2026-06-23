using snoopy_airlines_backend.Domain;

namespace snoopy_airlines_backend.Repositories
{
    public interface IReportRepository
    {
        Task<IReadOnlyCollection<MonthlyRevenueReportRow>> GetMonthlyRevenueBreakdownAsync(int year, CancellationToken cancellationToken);
    }
}