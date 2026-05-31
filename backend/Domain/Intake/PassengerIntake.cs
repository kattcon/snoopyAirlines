using System.ComponentModel.DataAnnotations;

namespace SnoopyAirlines.Domain.Intake
{
    public class PassengerIntake
    {
        [Required] public string Gender { get; set; }
        [Required] public string FirstName { get; set; }
        [Required] public string LastName { get; set; }
        [Required] public string BirthDay { get; set; }
        [Required] public string BirthMonth { get; set; }
        [Required] public string BirthYear { get; set; }
        [Required] public string Nationality { get; set; }
    }
}
