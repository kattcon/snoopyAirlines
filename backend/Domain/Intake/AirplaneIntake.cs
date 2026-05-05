using System.ComponentModel.DataAnnotations;

namespace SnoopyAirlines.Domain.Intake
{
    public class AirplaneIntake : IValidatableObject
    {
        [Required]
        [RegularExpression(@"^\S+$", ErrorMessage = "The model cannot contain blank characters")]
        required public string Model { get; set; }
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Tourist rows must be a non-negative number")]
        required public int TouristRows { get; set; }
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Tourist seats must be a non-negative number")]
        required public int TouristColumns { get; set; }
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Firstclass rows must be a non-negative number")]
        required public int FirstclassRows { get; set; }
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Firstclass seats must be a non-negative number")]
        required public int FirstclassColumns { get; set; }
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Max weight must be a non-negative number")]
        required public float MaxWeight { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var totalSeats = (TouristRows * TouristColumns) + (FirstclassRows * FirstclassColumns);
            if (totalSeats >= 1000)
            {

                yield return new ValidationResult($"Total seats cannot be greater than 999",
                    new[] {nameof(TouristRows), nameof(TouristColumns), 
                           nameof(FirstclassColumns), nameof(FirstclassRows) }
                );
            }
        }
    }
}
