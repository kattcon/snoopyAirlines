namespace snoopy_airlines_backend.Domain
{
    public class PurchaseOrderEmailData
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
            public List<PassengerEmailData> Passengers { get; set; } = [];
    
            public string BaseFare { get; set; } = string.Empty;
            public string Taxes { get; set; } = string.Empty;
            public string TravelInsurance { get; set; } = string.Empty;
            public string Total { get; set; } = string.Empty;
            public string PaymentMethod { get; set; } = string.Empty;
            public string PurchaseDate { get; set; } = string.Empty;
            public string TransactionId { get; set; } = string.Empty;
        }
}