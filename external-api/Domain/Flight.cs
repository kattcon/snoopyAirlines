namespace SnoopyAirlines.External.Domain
{
    public class Flight
    {
        required public string FlightGUID { get; set; }
        required public string DepartureTime { get; set; }
        required public string ArrivalTime { get; set; }
        required public FlightFrequency Frequency { get; set; }
        required public string Duration { get; set; }
        required public Airport DepartureAirport { get; set; }
        required public Airport ArrivalAirport { get; set; }
        public decimal TouristPrice { get; set; }
        public decimal FirstClassPrice { get; set; }
        public decimal CarryOnPrice { get; set; }
        public decimal CheckedPrice { get; set; }
    }
}
