namespace SnoopyAirlines.Domain
{
    public class City
    {
        public int Id { get; set; }
        required public string Name { get; set; }
        public int CountryId { get; set; }
    }
}
