namespace SnoopyAirlines.Domain
{
    public class Flight
    {
        public Guid Guid { get; set; }
        public int RouteId { get; set; }
        public DateTime DepartureAt { get; set; }
        public DateTime ArrivalAt { get; set; }
        public string Status { get; set; } = "scheduled";
        public DateTime CreatedAt { get; set; }
        public Route? Route { get; set; }
    }
}
