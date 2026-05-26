using System.ComponentModel.DataAnnotations;

namespace SnoopyAirlines.Domain.Intake
{
    public class AirplaneUpdateIntake : IValidatableObject
    {
        private const int MaximumAllowedSeats = 999;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Las filas de turista deben ser un número positivo")]
        required public int TouristRows { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Las columnas de turista deben ser un número positivo")]
        required public int TouristColumns { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Las filas de primera clase deben ser un número positivo")]
        required public int FirstClassRows { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Las columnas de primera clase deben ser un número positivo")]
        required public int FirstClassColumns { get; set; }

        [Required]
        [Range(1, double.MaxValue, ErrorMessage = "El peso máximo debe ser un número positivo")]
        required public float MaxWeight { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var totalSeats = (TouristRows * TouristColumns) + (FirstClassRows * FirstClassColumns);

            if (totalSeats > MaximumAllowedSeats)
            {
                yield return new ValidationResult(
                    $"El total de asientos no puede ser mayor a {MaximumAllowedSeats}",
                    new[] { nameof(TouristRows), nameof(TouristColumns), nameof(FirstClassRows), nameof(FirstClassColumns) }
                );
            }
        }
    }
}
