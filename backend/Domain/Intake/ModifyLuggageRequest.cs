using snoopy_airlines_backend.Domain.View;
using SnoopyAirlines.Domain.Intake;
using System.ComponentModel.DataAnnotations;

namespace snoopy_airlines_backend.Domain.Intake
{
    public class ModifyLuggageRequest
    {
        [Required] public string ConfirmationNumber { get; set; }
        [Required] public IReadOnlyCollection<PassengerLuggageIntake> Passengers { get; set; }
    }
}
