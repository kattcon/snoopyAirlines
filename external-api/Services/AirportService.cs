using SnoopyAirlines.External.Domain;
using SnoopyAirlines.External.Repositories;

namespace SnoopyAirlines.External.Services
{
    public class AirportService
    {
        private readonly AirportRepository _airportRepository;

        public AirportService(AirportRepository airportRepository)
        {
            _airportRepository = airportRepository;
        }

        public async Task<IReadOnlyCollection<Airport>> GetAllAirports(
            CancellationToken cancellationToken = default)
        {
            return await _airportRepository.GetAllAirports(cancellationToken);
        }

        public async Task<Airport?> GetAirportByCode(
            string code,
            CancellationToken cancellationToken = default)
        {
            return await _airportRepository.GetAirportByCode(code, cancellationToken);
        }
    }
}
