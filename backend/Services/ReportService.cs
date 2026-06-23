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

        public async Task<MonthlyRevenueReport> GetMonthlyRevenueReportAsync(int year, CancellationToken cancellationToken)
        {
            if (year < 1 || year > 9999)
            {
                throw new ArgumentOutOfRangeException(nameof(year), "year must be between 1 and 9999.");
            }

            var rows = await _reportRepository.GetMonthlyRevenueBreakdownAsync(year, cancellationToken);
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
    }
}