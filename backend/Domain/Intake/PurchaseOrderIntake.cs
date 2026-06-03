using System.ComponentModel.DataAnnotations;

namespace SnoopyAirlines.Domain.Intake
{
    public class PurchaseOrderIntake
    {
        [Required] public List<PurchaseOrderRouteIntake> Routes { get; set; } = new();
        [Required] public string SeatClass { get; set; } = string.Empty;
        [Required] public List<PassengerIntake> Passengers { get; set; } = new();
    }

    public class PurchaseOrderRouteIntake
    {
        public int? SequenceNumber { get; set; }
        [Required] public int RouteId { get; set; }
        [Required] public DateOnly? IntendedDate { get; set; }
    }
}
