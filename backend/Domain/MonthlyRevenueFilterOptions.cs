namespace SnoopyAirlines.Domain
{
    public class MonthlyRevenueFilterOptions
    {
        public IReadOnlyCollection<MonthlyRevenueAirportFilterOption> Origins { get; set; } = Array.Empty<MonthlyRevenueAirportFilterOption>();
        public IReadOnlyCollection<MonthlyRevenueAirportFilterOption> Destinations { get; set; } = Array.Empty<MonthlyRevenueAirportFilterOption>();
        public IReadOnlyCollection<MonthlyRevenueAirlineFilterOption> Airlines { get; set; } = Array.Empty<MonthlyRevenueAirlineFilterOption>();
    }

    public class MonthlyRevenueAirportFilterOption
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class MonthlyRevenueAirlineFilterOption
    {
        public int Id { get; set; }
        public string Model { get; set; } = string.Empty;
    }
}