namespace SnoopyAirlines.External.Domain
{
    public class Route
    {
        public int Id { get; set; }
        public TimeOnly DepartureTime { get; set; }
        public TimeOnly ArrivalTime { get; set; }
        required public RouteFrequency Frequency { get; set; }
        public int DurationMinutes { get; set; }
        required public Airport DepartureAirport { get; set; }
        required public Airport ArrivalAirport { get; set; }
        public decimal TouristPrice { get; set; }
        public decimal FirstClassPrice { get; set; }
        public decimal CarryOnPrice { get; set; }
        public decimal CheckedPrice { get; set; }
    }
}
