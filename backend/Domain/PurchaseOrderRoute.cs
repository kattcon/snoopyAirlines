namespace snoopy_airlines_backend.Domain
{
    public class PurchaseOrderRoute
    {
        public int PurchaseOrderId { get; set; }
        public int SequenceNumber { get; set; }
        public Guid FlightGuid { get; set; }
        public int? RouteId { get; set; }
        public DateOnly? IntendedDate { get; set; }
    }
}
