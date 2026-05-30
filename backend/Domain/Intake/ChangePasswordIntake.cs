namespace SnoopyAirlines.Domain.Intake
{
    public class ChangePasswordIntake
    {
        required public string CurrentPassword { get; set; }
        required public string NewPassword { get; set; }
    }
}