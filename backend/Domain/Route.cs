
namespace SnoopyAirlines.Domain
{
    public class Route
    {
        public int Id { get; set; }
        public int AirplaneId { get; set; }
        public int DepartureAirportId { get; set; }
        public int ArrivalAirportId { get; set; }
        public TimeOnly DepartureTime { get; set; }
        public TimeOnly ArrivalTime { get; set; }
        public RouteFrequency Frequency { get; set; } = new();
        public int DurationMinutes { get; set; }
        public decimal PriceFirstClass { get; set; }
        public decimal PriceEconomyClass { get; set; }
        public decimal PriceCarryOnBaggage { get; set; }
        public decimal PriceCheckedBaggage { get; set; }
        public int WeightLimitCarryOnBaggage { get; set; }
        public int WeightLimitCheckedBaggage { get; set; }
        public decimal CheckedBaggagePriceMultiplier { get; set; }
        public string? FlightCode { get; set; }
        public RouteAirport? DepartureAirport { get; set; }
        public RouteAirport? ArrivalAirport { get; set; }
    }

    public class RouteAirport
    {
        required public string Code { get; set; }
        required public string Name { get; set; }
        required public string City { get; set; }
    }
}
