using SnoopyAirlines.Domain.Airlines;

namespace SnoopyAirlines.Domain
{
    public abstract class Flight
    {
        public Guid Guid { get; set; }
        public DateTime DepartureAt { get; set; }
        public DateTime ArrivalAt { get; set; }
        public string Status { get; set; } = "scheduled";
        public DateTime CreatedAt { get; set; }
    }

    public class InternalFlight : Flight
    {
        public int RouteId { get; set; }
        public Route? Route { get; set; }
    }

    public class ExternalFlight : Flight
    {
        public int PartnerAirlineId { get; set; }
        public PartnerAirline? PartnerAirline { get; set; }
        required public FlightAirport DepartureAirport { get; set; }
        required public FlightAirport ArrivalAirport { get; set; }
        public decimal TouristPrice { get; set; }
        public decimal FirstClassPrice { get; set; }
        public decimal CarryOnPrice { get; set; }
        public decimal CheckedPrice { get; set; }
    }

    public class FlightAirport
    {
        required public string Code { get; set; }
        required public string Name { get; set; }
        required public string City { get; set; }
    }
}
