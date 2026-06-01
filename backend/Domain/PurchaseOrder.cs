namespace snoopy_airlines_backend.Domain
{
    public class PurchaseOrder
    {
        public int Id { get; set; }
        public int RouteId { get; set; }
        public DateOnly? IntendedDate { get; set; }
        public string SeatClass { get; set; }
        public List<Passenger> Passengers { get; set; } = new ();
    }
}
