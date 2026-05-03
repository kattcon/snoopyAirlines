using Dapper;
using Microsoft.Data.SqlClient;
using SnoopyAirlines.domain;
using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.View;

namespace SnoopyAirlines.Repositories
{
    public class AirplaneRepository
    {
        public Task<Airplane> CreateAirplaneAsync(Airplane airplane, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Airplane created: Model {airplane.Model}");
            return Task.FromResult(airplane);
        }

        public Task<IReadOnlyCollection<Airplane>> GetAirplanesAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine($"Airplanes retrieved");
            return Task.FromResult<IReadOnlyCollection<Airplane>>(Array.Empty<Airplane>()); //  cambiar por consulta
        }
    }
}