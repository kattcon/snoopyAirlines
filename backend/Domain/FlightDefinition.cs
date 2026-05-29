namespace SnoopyAirlines.Domain
{
    public class FlightDefinition
    {
        public int Id { get; set; }
        public TimeOnly DepartureTime { get; set; }
        public TimeOnly ArrivalTime { get; set; }
        public FlightFrequency Frequency { get; set; } = new();
        public int DurationMinutes { get; set; }
        public FlightDefinitionAirport DepartureAirport { get; set; } = null!;
        public FlightDefinitionAirport ArrivalAirport { get; set; } = null!;
        public decimal PriceEconomyClass { get; set; }
        public decimal PriceFirstClass { get; set; }
        public decimal CarryOnPrice { get; set; }
        public decimal CheckedPrice { get; set; }
    }

    public class FlightDefinitionAirport
    {
        required public string Code { get; set; }
        required public string Name { get; set; }
        required public string City { get; set; }
    }
}
