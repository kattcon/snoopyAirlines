using System.ComponentModel.DataAnnotations;

namespace SnoopyAirlines.Domain.Intake
{
    public class AdminUserUpdateIntake
    {
        [Required(ErrorMessage = "El nombre es requerido.")]
        required public string FirstName { get; set; }

        [Required(ErrorMessage = "El primer apellido es requerido.")]
        required public string LastNameOne { get; set; }

        public string? LastNameTwo { get; set; }

        [Required(ErrorMessage = "El número de identificación es requerido.")]
        required public string IdentificationNumber { get; set; }

        [Required(ErrorMessage = "El tipo de usuario es requerido.")]
        required public string Type { get; set; }
    }
}
