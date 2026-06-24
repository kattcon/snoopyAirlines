using System.ComponentModel.DataAnnotations;

namespace snoopy_airlines_backend.Domain.View
{
    public class ModifyLuggageInfo
    {
        [Required]
        public IReadOnlyCollection<PassengerView> Passengers { get; set; }
        [Required]
        public FlightLuggageView LuggageInfo { get; set; }
    }
}