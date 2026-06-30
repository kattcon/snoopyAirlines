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
            string? airline,
            CancellationToken cancellationToken)
        {
            return _reportRepository.GetAirlineDetailedReportAsync(
                string.IsNullOrWhiteSpace(origin) ? null : origin.Trim().ToUpperInvariant(),
                string.IsNullOrWhiteSpace(destination) ? null : destination.Trim().ToUpperInvariant(),
                string.IsNullOrWhiteSpace(seatClass) ? null : seatClass.Trim(),
                dateFrom,
                dateTo,
                string.IsNullOrWhiteSpace(airline) ? null : airline.Trim(),
                cancellationToken);
        }
    }
}
