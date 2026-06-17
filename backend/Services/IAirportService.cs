using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.View;

namespace SnoopyAirlines.Services
{
    public interface IAirportService
    {
        Task<IReadOnlyCollection<Country>> GetCountriesAsync(CancellationToken cancellationToken);
        Task<IReadOnlyCollection<City>> GetCitiesByCountryAsync(int countryId, CancellationToken cancellationToken);
        Task<Airport> CreateAirportAsync(Airport airport, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<AirportView>> GetAirportsAsync(string? search, CancellationToken cancellationToken);
        Task<Airport?> GetAirportByIdAsync(int airportId, CancellationToken cancellationToken);
        Task UpdateAirportNameAsync(int airportId, string newAirportName, CancellationToken cancellationToken);
        Task DeleteAirportAsync(int airportId, CancellationToken cancellationToken);
    }
}
