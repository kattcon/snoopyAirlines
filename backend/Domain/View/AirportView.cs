namespace SnoopyAirlines.Domain.View
{
    public class AirportView
    {
        public int Id { get; set; }
        required public string Name { get; set; }
        required public string Code { get; set; }
        required public string CityName { get; set; }
        required public string CountryName { get; set; }
    }
}
