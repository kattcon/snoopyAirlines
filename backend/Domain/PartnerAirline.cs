namespace SnoopyAirlines.Domain.Airlines
{
    public class PartnerAirline
    {
        public int Id { get; set; }
        required public string Name { get; set; }
        required public string Host { get; set; }
        required public string ApiKey { get; set; }
    }
}
