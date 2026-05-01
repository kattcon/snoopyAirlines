using SnoopyAirlines.External.Domain;
using SnoopyAirlines.External.Domain.View;
using SnoopyAirlines.External.Repositories;

namespace SnoopyAirlines.External.Services
{
    public class FlightService
    {
        private readonly FlightRepository _flightRepository;

        public FlightService(FlightRepository flightRepository)
        {
            _flightRepository = flightRepository;
        }

        public Task<IReadOnlyCollection<Flight>> GetFlights(
            FlightQuery flightQuery,
            CancellationToken cancellationToken = default)
        {
            return _flightRepository.GetFlights(flightQuery, cancellationToken);
        }

        public async Task<FlightsResponse> Get(
            FlightQuery flightQuery,
            CancellationToken cancellationToken = default)
        {
            var flights = await GetFlights(flightQuery, cancellationToken);

            return new FlightsResponse
            {
                Flights = flights
            };
        }
    }
}
