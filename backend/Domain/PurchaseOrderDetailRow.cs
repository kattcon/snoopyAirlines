namespace snoopy_airlines_backend.Domain
{
    public class PurchaseOrderDetailRow
    {
        public int PurchaseOrderId { get; set; }
        public string SeatClass { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string DepartureAirportName { get; set; } = string.Empty;
        public string DepartureAirportCode { get; set; } = string.Empty;
        public string DepartureCityName { get; set; } = string.Empty;
        public string ArrivalAirportName { get; set; } = string.Empty;
        public string ArrivalAirportCode { get; set; } = string.Empty;
        public string ArrivalCityName { get; set; } = string.Empty;
        public string AirplaneModel { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public string BirthDay { get; set; } = string.Empty;
        public string BirthMonth { get; set; } = string.Empty;
        public string BirthYear { get; set; } = string.Empty;
    }
}