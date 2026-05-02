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
            Console.WriteLine($"Airplane created: Model {airplane.ModelNumber}");
            return Task.FromResult(airplane);
        }
    }
}