namespace snoopy_airlines_backend.Domain
{
    public class PurchaseOrder
    {
        public int Id { get; set; }
        public string SeatClass { get; set; } = string.Empty;
        public List<PurchaseOrderRoute> Routes { get; set; } = new ();
        public List<Passenger> Passengers { get; set; } = new ();
    }
}
