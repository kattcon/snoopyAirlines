namespace SnoopyAirlines.Domain

{
    public class FlightCustomerReport
    {
        public string ReservationNumber { get; set; } = null!;
        public string CardHolderName { get; set; } = null!;
        public string SequenceNumber { get; set; } = null!;
        public string DepartureCity { get; set; } = null!;
        public string ArrivalCity { get; set; } = null!;
        public string DepartureAirportCode { get; set; } = null!;
        public string ArrivalAirportCode { get; set; } = null!;
        public Date DepartureDate { get; set; } = null!;
        public Time DepartureTime { get; set; } = null!;
        public Date ArrivalDate { get; set; } = null!;
        public Time ArrivalTime { get; set; } = null!;
        public int DurationMinutes { get; set; } = null!;
        public string AirplaneModel { get; set; } = null!;
        public int PassengerCount { get; set; } = null!;
    }
}