namespace SnoopyAirlines.Domain.Intake
{
    public class UserUpdateIntake
    {
        required public string FirstName { get; set; }
        required public string LastNameOne { get; set; }
        public string? LastNameTwo { get; set; }
    }
}