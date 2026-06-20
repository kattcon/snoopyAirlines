namespace SnoopyAirlines.Domain

{
    public class FlightCustomerReport
    {
        public string ReservationNumber { get; set; }
        public string CardHolderName { get; set; }
        public string SequenceNumber { get; set; }
        public string DepartureCity { get; set; }
        public string ArrivalCity { get; set; }
        public string DepartureAirportCode { get; set; }
        public string ArrivalAirportCode { get; set; }
        public DateOnly DepartureDate { get; set; }
        public TimeOnly DepartureTime { get; set; }
        public DateOnly ArrivalDate { get; set; }
        public TimeOnly ArrivalTime { get; set; }
        public int DurationMinutes { get; set; }
        public string AirplaneModel { get; set; }
        public int PassengerCount { get; set; }
    }
}