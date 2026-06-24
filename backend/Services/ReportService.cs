using System.Globalization;
using snoopy_airlines_backend.Domain;
using snoopy_airlines_backend.Repositories;

namespace snoopy_airlines_backend.Services
{
    public class ReportService
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
            int? airplaneId,
            CancellationToken cancellationToken)
        {
            if (year.HasValue && (year.Value < 1 || year.Value > 9999))
            {
                throw new ArgumentOutOfRangeException(nameof(year), "year must be between 1 and 9999.");
            }

            ValidatePositiveIdIfProvided(originAirportId, nameof(originAirportId));
            ValidatePositiveIdIfProvided(destinationAirportId, nameof(destinationAirportId));
            ValidatePositiveIdIfProvided(airplaneId, nameof(airplaneId));

            var rows = await _reportRepository.GetMonthlyRevenueBreakdownAsync(
                year,
                originAirportId,
                destinationAirportId,
                airplaneId,
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

        private static void ValidatePositiveIdIfProvided(int? id, string parameterName)
        {
            if (id.HasValue && id.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, $"{parameterName} must be greater than 0.");
            }
        }
    }
}