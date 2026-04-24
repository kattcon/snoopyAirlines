namespace SnoopyAirlines.Domain.User
{
    public class PendingUser
    {
        public int? Id { get; set; }
        required public string IdentificationNumber { get; set; }
        required public string Email { get; set; }
        required public string FirstName { get; set; }
        required public string LastNameOne { get; set; }
        public string? LastNameTwo { get; set; }
        required public UserRole Type { get; set; }
    }
}
