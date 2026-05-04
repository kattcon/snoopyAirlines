
namespace SnoopyAirlines.Domain
{
    public class Flight
    {
        public int Id { get; set; }
        public int AirplaneId { get; set; }
        public int DepartureAirportId { get; set; }
        public int ArrivalAirportId { get; set; }
        public TimeOnly DepartureTime { get; set; }
        public TimeOnly ArrivalTime { get; set; }
        public FlightFrequency Frequency { get; set; } = new();
        public int DurationMinutes { get; set; }
        public decimal PriceFirstClass { get; set; }
        public decimal PriceEconomyClass { get; set; }
        public decimal PriceCarryOnBaggage { get; set; }
        public decimal PriceCheckedBaggage { get; set; }
        public int WeightLimitCarryOnBaggage { get; set; }
        public int WeightLimitCheckedBaggage { get; set; }
        public decimal CheckedBaggagePriceMultiplier { get; set; }
    }
}
