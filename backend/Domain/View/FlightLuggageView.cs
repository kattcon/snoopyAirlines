using System.ComponentModel.DataAnnotations;

namespace snoopy_airlines_backend.Domain.View
{
    public class FlightLuggageView
    {
        [Required] public float priceCheckedLuggage { get; set; }
        [Required] public float checkedLuggageMultiplier { get; set; }
    }
}