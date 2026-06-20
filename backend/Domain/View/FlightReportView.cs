namespace SnoopyAirlines.Domain.View

{
    public class FlightReportView
    {
        public string ReservationNumber { get; set; } = null!;
        public string CardHolderName { get; set; } = null!;
        public int SequenceNumber { get; set; }
        public string DepartureCity { get; set; } = null!;
        public string ArrivalCity { get; set; } = null!;
        public string DepartureAirportCode { get; set; } = null!;
        public string ArrivalAirportCode { get; set; } = null!;
        public DateOnly DepartureDate { get; set; }
        public TimeOnly DepartureTime { get; set; }
        public DateOnly ArrivalDate { get; set; }
        public TimeOnly ArrivalTime { get; set; }
        public int DurationMinutes { get; set; }
        public string AirplaneModel { get; set; } = null!;
        public int PassengerCount { get; set; }
    }
}