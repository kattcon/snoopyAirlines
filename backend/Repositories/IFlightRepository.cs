using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.View;

namespace SnoopyAirlines.Repositories
{
    public interface IFlightRepository
    {
        Task<Flight?> GetByGuidAsync(Guid flightGuid, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<Flight>> GetByGuidsAsync(
            IReadOnlyCollection<Guid> flightGuids,
            CancellationToken cancellationToken);

        Task<Guid> MaterializeInternalFlightAsync(
            int routeId,
            DateTime departureAt,
            CancellationToken cancellationToken);

        Task<Guid> MaterializeExternalFlightAsync(
            ExternalFlight flight,
            CancellationToken cancellationToken);

        Task<IReadOnlyCollection<FlightReportView>> GetFlightReportByConfirmationAsync(
            string confirmationNumber,
            string lastNames,
            CancellationToken cancellationToken);
    }
}
