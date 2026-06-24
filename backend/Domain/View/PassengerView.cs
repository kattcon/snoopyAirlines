using System.ComponentModel.DataAnnotations;

namespace snoopy_airlines_backend.Domain.View
{
    public class PassengerView
    {
        [Required] public string FirstName { get; set; }
        [Required] public string LastName { get; set; }
        [Required] public int CheckedLuggage { get; set; }
    }
}
