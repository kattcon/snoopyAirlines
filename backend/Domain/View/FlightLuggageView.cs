using System.ComponentModel.DataAnnotations;

namespace snoopy_airlines_backend.Domain.View
{
    public class FlightLuggageView
    {
        [Required] public float PriceCheckedBaggage { get; set; }
        [Required] public float CheckedBaggagePriceMultiplier { get; set; }
    }
}