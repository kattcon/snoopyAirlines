namespace SnoopyAirlines.Domain
{
    public class Booking
    {
        public Guid Guid { get; set; }
        public int PurchaseOrderId { get; set; }
        public Guid FlightGuid { get; set; }
        public string ConfirmationCode { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string? CardBrand { get; set; }
        public string? CardLastFour { get; set; }
        public string? CardHolderName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ConfirmedAt { get; set; }
    }
}
