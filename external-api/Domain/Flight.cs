namespace SnoopyAirlines.External.Domain
{
    public class Flight
    {
        required public string FlightGUID { get; set; }
        public int RouteId { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        required public string Duration { get; set; }
        required public Airport DepartureAirport { get; set; }
        required public Airport ArrivalAirport { get; set; }
        public decimal TouristPrice { get; set; }
        public decimal FirstClassPrice { get; set; }
        public decimal CarryOnPrice { get; set; }
        public decimal CheckedPrice { get; set; }
    }
}
