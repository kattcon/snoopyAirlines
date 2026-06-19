using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.View;

namespace SnoopyAirlines.Repositories
{
    public interface IAirportRepository
    {
        Task<IReadOnlyCollection<Country>> GetCountriesAsync(CancellationToken cancellationToken);
        Task<IReadOnlyCollection<City>> GetCitiesByCountryAsync(int countryId, CancellationToken cancellationToken);
        Task<bool> AirportCodeExistsAsync(string code, CancellationToken cancellationToken);
        Task<Airport> CreateAirportAsync(Airport airport, CancellationToken cancellationToken);
        Task<IReadOnlyCollection<AirportView>> GetAirportsAsync(string? search, CancellationToken cancellationToken);
        Task<Airport?> GetAirportByIdAsync(int id, CancellationToken cancellationToken);
        Task<bool> UpdateAirportNameAsync(int id, string name, CancellationToken cancellationToken);
        Task<bool> AirportHasPurchasesAsync(int airportId, CancellationToken cancellationToken);
        Task SoftDeleteAirportAsync(int airportId, CancellationToken cancellationToken);
        Task HardDeleteAirportAsync(int airportId, CancellationToken cancellationToken);
    }
}
