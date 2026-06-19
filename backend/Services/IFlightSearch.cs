namespace SnoopyAirlines.Services
{
    public interface IFlightSearch
    {
        Task<IReadOnlyCollection<FlightReportView>> GetFlightReportByConfirmationAsync(string email, string password, CancellationToken cancellationToken);
    }
}