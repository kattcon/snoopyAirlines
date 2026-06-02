using System.ComponentModel.DataAnnotations;

namespace SnoopyAirlines.Domain.Intake
{
    public class PurchaseOrderIntake
    {
        [Required] public int RouteId { get; set; }
        [Required] public DateOnly IntendedDate { get; set; }
        [Required] public string SeatClass { get; set; }
        [Required] public List<PassengerIntake> Passengers { get; set; }
    }
}
