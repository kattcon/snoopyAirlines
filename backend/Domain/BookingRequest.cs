namespace SnoopyAirlines.Domain
{
    public class BookingRequest
    {
        public int PurchaseOrderId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string CardBrand { get; set; } = string.Empty;
        public string CardLastFour { get; set; } = string.Empty;
        public string CardHolderName { get; set; } = string.Empty;
    }
}
