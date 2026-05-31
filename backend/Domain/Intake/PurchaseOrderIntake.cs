using System.ComponentModel.DataAnnotations;

namespace SnoopyAirlines.Domain.Intake
{
    public class PurchaseOrderIntake
    {
        [Required] public int FlightId { get; set; }
        [Required] public string SeatClass { get; set; }
        [Required] public List<PassengerIntake> Passengers { get; set; }
    }
}
