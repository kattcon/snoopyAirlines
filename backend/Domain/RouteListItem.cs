namespace snoopy_airlines_backend.Domain
{
    public class RouteListItem
    {
        public int Id { get; set; }
        public int AirplaneId { get; set; }
        public string AirplaneModel { get; set; } = string.Empty;

        public int DepartureAirportId { get; set; }
        public string DepartureAirportCode { get; set; } = string.Empty;
        public string DepartureAirportName { get; set; } = string.Empty;
        public string DepartureAirportCity { get; set; } = string.Empty;

        public int ArrivalAirportId { get; set; }
        public string ArrivalAirportCode { get; set; } = string.Empty;
        public string ArrivalAirportName { get; set; } = string.Empty;
        public string ArrivalAirportCity { get; set; } = string.Empty;

        public string DepartureTime { get; set; }
        public string ArrivalTime { get; set; }
    }
}
