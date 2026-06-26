using System.ComponentModel.DataAnnotations;

namespace snoopy_airlines_backend.Domain.Intake
{
    public class PassengerLuggageIntake
    {
        [Required] public int Id { get; set; }
        [Required] public int NewCheckedLuggage { get; set; }
    }
}
