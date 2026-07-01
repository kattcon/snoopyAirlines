namespace snoopy_airlines_backend.Domain
{
    public class PurchaseOrder
    {
        public int Id { get; set; }
        public string SeatClass { get; set; } = string.Empty;
        public decimal TicketTotalAmount { get; set; }
        public decimal CarryOnLuggageTotalAmount { get; set; }
        public decimal CheckedLuggageTotalAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public List<PurchaseOrderRoute> Routes { get; set; } = new ();
        public List<Passenger> Passengers { get; set; } = new ();
    }
}
