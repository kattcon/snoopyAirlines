namespace SnoopyAirlines.External.Domain
{
    public class ApiUser
    {
        public int Id { get; set; }
        required public string ApiKey { get; set; }
        required public string Name { get; set; }
        required public string Role { get; set; }
    }
}
