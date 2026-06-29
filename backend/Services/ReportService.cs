using SnoopyAirlines.Domain;
using SnoopyAirlines.Repositories;

namespace SnoopyAirlines.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public Task<IReadOnlyCollection<AirlineDetailedReportRow>> GetAirlineDetailedReportAsync(
            string? origin,
            string? destination,
            string? seatClass,
            DateOnly? dateFrom,
            DateOnly? dateTo,
            CancellationToken cancellationToken)
        {
            return _reportRepository.GetAirlineDetailedReportAsync(
                string.IsNullOrWhiteSpace(origin) ? null : origin.Trim().ToUpperInvariant(),
                string.IsNullOrWhiteSpace(destination) ? null : destination.Trim().ToUpperInvariant(),
                string.IsNullOrWhiteSpace(seatClass) ? null : seatClass.Trim(),
                dateFrom,
                dateTo,
                cancellationToken);
        }
    }
}
