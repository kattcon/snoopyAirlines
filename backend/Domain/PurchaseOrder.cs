using System.ComponentModel.DataAnnotations;

namespace snoopy_airlines_backend.Domain
{
    public class PurchaseOrder
    {
        public int Id { get; set; }
        public int FlightId { get; set; }
        public string SeatClass { get; set; }
        public string Status { get; set; } = "pending";
        public List<Passenger> Passengers { get; set; } = new ();
    }
}
