using SnoopyAirlines.Domain;
using SnoopyAirlines.Repositories;

namespace SnoopyAirlines.Services
{
    public class FlightService
    {
        private readonly FlightRepository _flightRepository;

        public FlightService(FlightRepository flightRepository)
        {
            _flightRepository = flightRepository;
        }

        public Task<IReadOnlyCollection<Flight>> GetFlightsAsync(CancellationToken cancellationToken)
        {
            return _flightRepository.GetAllAsync(cancellationToken);
        }

        public Task<Flight> CreateFlightAsync(Flight flight, CancellationToken cancellationToken)
        {
            return _flightRepository.CreateAsync(flight, cancellationToken);
        }
    }
}
