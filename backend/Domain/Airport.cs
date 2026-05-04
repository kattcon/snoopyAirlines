namespace SnoopyAirlines.Domain
{
    public class Airport
    {
        public int Id { get; set; }
        required public string Name { get; set; }
        required public string Code { get; set; }
        public int CityId { get; set; }
    }
}
