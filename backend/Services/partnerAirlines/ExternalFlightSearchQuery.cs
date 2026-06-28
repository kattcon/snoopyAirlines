namespace SnoopyAirlines.Services.PartnerAirlines
{
    public class ExternalFlightSearchQuery
    {
        required public string Destination { get; set; }
        public int QuantityOfPassengers { get; set; } = 1;
        public TimeSpan MaxTimeWindow { get; set; }
        public IReadOnlyList<ExternalFlightSearchQueryLeg> Legs { get; set; } = Array.Empty<ExternalFlightSearchQueryLeg>();
    }

    public readonly record struct ExternalFlightSearchQueryLeg(
        string Origin,
        DateTime Time);
}
