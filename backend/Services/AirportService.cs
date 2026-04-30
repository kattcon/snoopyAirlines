using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Repositories;

namespace SnoopyAirlines.Services
{
    public class AirportService
    {
        private readonly AirportRepository _airportRepository;

        public AirportService(AirportRepository airportRepository)
        {
            _airportRepository = airportRepository;
        }

        public Task<IReadOnlyCollection<Country>> GetCountriesAsync(CancellationToken cancellationToken)
        {
            return _airportRepository.GetCountriesAsync(cancellationToken);
        }

        public Task<IReadOnlyCollection<City>> GetCitiesByCountryAsync(int countryId, CancellationToken cancellationToken)
        {
            return _airportRepository.GetCitiesByCountryAsync(countryId, cancellationToken);
        }

        public async Task<Airport> CreateAirportAsync(Airport airport, CancellationToken cancellationToken)
        {
            var codeExists = await _airportRepository.AirportCodeExistsAsync(airport.Code, cancellationToken);

            if (codeExists)
            {
                throw new InvalidOperationException($"An airport with code '{airport.Code}' already exists.");
            }

            return await _airportRepository.CreateAirportAsync(airport, cancellationToken);
        }

        public Task<IReadOnlyCollection<AirportView>> GetAirportsAsync(string? search, CancellationToken cancellationToken)
        {
            return _airportRepository.GetAirportsAsync(search, cancellationToken);
        }
    }
}
