using System.Globalization;
using SnoopyAirlines.Domain;
using SnoopyAirlines.Repositories;

namespace SnoopyAirlines.Services
{
    public class ReportService : IReportService
    {
        private static readonly CultureInfo MonthCulture = CultureInfo.GetCultureInfo("es-CR");

        private readonly IReportRepository _reportRepository;

        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public Task<MonthlyRevenueFilterOptions> GetMonthlyRevenueFilterOptionsAsync(CancellationToken cancellationToken)
        {
            return _reportRepository.GetMonthlyRevenueFilterOptionsAsync(cancellationToken);
        }

        public async Task<MonthlyRevenueReport> GetMonthlyRevenueReportAsync(
            int? year,
            int? originAirportId,
            int? destinationAirportId,
            int? partnerAirlineId,
            CancellationToken cancellationToken)
        {
            if (year.HasValue && (year.Value < 1 || year.Value > 9999))
            {
                throw new ArgumentOutOfRangeException(nameof(year), "year must be between 1 and 9999.");
            }

            ValidatePositiveIdIfProvided(originAirportId, nameof(originAirportId));
            ValidatePositiveIdIfProvided(destinationAirportId, nameof(destinationAirportId));
            ValidateNonNegativeIdIfProvided(partnerAirlineId, nameof(partnerAirlineId));

            var rows = await _reportRepository.GetMonthlyRevenueBreakdownAsync(
                year,
                originAirportId,
                destinationAirportId,
                partnerAirlineId,
                cancellationToken);

            var normalizedRows = rows
                .Select(row => new MonthlyRevenueReportRow
                {
                    MonthNumber = row.MonthNumber,
                    MonthLabel = MonthCulture.TextInfo.ToTitleCase(MonthCulture.DateTimeFormat.GetMonthName(row.MonthNumber)),
                    FlightCount = row.FlightCount,
                    FirstClassPassengers = row.FirstClassPassengers,
                    EconomyPassengers = row.EconomyPassengers,
                    TotalPassengers = row.TotalPassengers,
                    TicketRevenue = row.TicketRevenue,
                    LuggageRevenue = row.LuggageRevenue,
                    TotalRevenue = row.TotalRevenue,
                })
                .ToList();

            return new MonthlyRevenueReport
            {
                Year = year,
                Rows = normalizedRows,
            };
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

        private static void ValidatePositiveIdIfProvided(int? id, string parameterName)
        {
            if (id.HasValue && id.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, $"{parameterName} must be greater than 0.");
            }
        }

        private static void ValidateNonNegativeIdIfProvided(int? id, string parameterName)
        {
            if (id.HasValue && id.Value < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, $"{parameterName} must be greater than or equal to 0.");
            }
        }
    }
}
