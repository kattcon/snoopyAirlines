namespace SnoopyAirlines.Domain.Intake
{
    public class AdminUserUpdateIntake
    {
        required public string FirstName { get; set; }
        required public string LastNameOne { get; set; }
        public string? LastNameTwo { get; set; }
        required public string IdentificationNumber { get; set; }
        required public string Type { get; set; }
    }
}
