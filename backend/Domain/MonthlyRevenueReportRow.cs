namespace snoopy_airlines_backend.Domain
{
    public class MonthlyRevenueReportRow
    {
        public int MonthNumber { get; set; }
        public string MonthLabel { get; set; } = string.Empty;
        public int FlightCount { get; set; }
        public int FirstClassPassengers { get; set; }
        public int EconomyPassengers { get; set; }
        public int TotalPassengers { get; set; }
        public decimal TicketRevenue { get; set; }
        public decimal LuggageRevenue { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}