using System.ComponentModel.DataAnnotations;

namespace SnoopyAirlines.Domain.Intake
{
    public sealed class CancelBookingIntake
    {
        [Required]
        public string Token { get; set; } = string.Empty;
    }
}