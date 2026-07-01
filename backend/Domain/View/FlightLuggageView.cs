using System.ComponentModel.DataAnnotations;

namespace snoopy_airlines_backend.Domain.View
{
    public class FlightLuggageView
    {
        public decimal PriceCheckedBaggage { get; set; }
        public decimal CheckedBaggagePriceMultiplier { get; set; }
        [Required] public int WeightLimitCheckedBaggage { get; set; }
        public decimal AirplaneMaxWeight { get; set; }
        public decimal BookedLuggageWeight { get; set; }
    }
}
