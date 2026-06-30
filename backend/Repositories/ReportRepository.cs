using Dapper;
using Microsoft.Data.SqlClient;
using SnoopyAirlines.Domain;

namespace SnoopyAirlines.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly string _connectionString;

        public ReportRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        public async Task<IReadOnlyCollection<AirlineDetailedReportRow>> GetAirlineDetailedReportAsync(
            string? origin,
            string? destination,
            string? seatClass,
            DateOnly? dateFrom,
            DateOnly? dateTo,
            string? airline,
            CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT
                    Fecha,
                    Origen,
                    Destino,
                    FlightCode,
                    PasajerosPrimeraClase,
                    PasajerosEconomia,
                    Aerolinea,
                    VentaPasajeros,
                    VentaEquipajes,
                    TotalVenta
                FROM dbo.GetAirlineDetailedReport(@Origin, @Destination, @SeatClass, @DateFrom, @DateTo, @AirlineName)
                ORDER BY Fecha DESC, Origen, Destino;
                """;

            await using var connection = new SqlConnection(_connectionString);
            var rows = await connection.QueryAsync<AirlineDetailedReportRow>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        Origin = origin,
                        Destination = destination,
                        SeatClass = seatClass,
                        DateFrom = dateFrom.HasValue ? dateFrom.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                        DateTo = dateTo.HasValue ? dateTo.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null,
                        AirlineName = airline
                    },
                    cancellationToken: cancellationToken));

            return rows.ToList();
        }
    }
}
