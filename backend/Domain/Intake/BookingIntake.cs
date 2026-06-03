using System.ComponentModel.DataAnnotations;

namespace SnoopyAirlines.Domain.Intake
{
    public class BookingIntake
    {
        [Required] public int PurchaseOrderId { get; set; }
        [Required] public string Email { get; set; } = string.Empty;
        [Required] public string CardBrand { get; set; } = string.Empty;
        [Required] public string CardLastFour { get; set; } = string.Empty;
        [Required] public string CardHolderName { get; set; } = string.Empty;
    }
}
