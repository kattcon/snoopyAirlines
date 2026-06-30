namespace SnoopyAirlines.Domain
{
    public class MonthlyRevenueReport
    {
        public int? Year { get; set; }
        public IReadOnlyCollection<MonthlyRevenueReportRow> Rows { get; set; } = Array.Empty<MonthlyRevenueReportRow>();
    }
}