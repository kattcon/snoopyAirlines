namespace SnoopyAirlines.Domain.Intake
{
    public class AirportIntake
    {
        required public string Name { get; set; }
        required public string Code { get; set; }
        public int CityId { get; set; }
    }
}
