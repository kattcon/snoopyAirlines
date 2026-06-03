namespace SnoopyAirlines.Domain
{
    public class ItineraryLeg
    {
        public Guid BookingGuid { get; set; }
        public int SequenceNumber { get; set; }
        public Guid FlightGuid { get; set; }
    }
}
