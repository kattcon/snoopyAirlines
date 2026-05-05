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

        public Task<IReadOnlyCollection<Flight>> SearchFlightsAsync(
            int? departureAirportId,
            int? arrivalAirportId,
            DateOnly? departureDate,
            CancellationToken cancellationToken)
        {
            return _flightRepository.SearchAsync(departureAirportId, arrivalAirportId, departureDate, cancellationToken);
        }

        public Task<Flight> SaveFlightAsync(Flight flight, CancellationToken cancellationToken)
        {
            return _flightRepository.SaveAsync(flight, cancellationToken);
        }
    }
}
