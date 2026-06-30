using System.ComponentModel.DataAnnotations;

namespace snoopy_airlines_backend.Domain.View
{
    public class PassengerView
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public int CheckedLuggage { get; set; }
    }
}
