using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using snoopy_airlines_backend.Domain.Intake;
using snoopy_airlines_backend.Domain.View;
using System.Data;

namespace snoopy_airlines_backend.Repositories
{
    public class ModifyLuggageRepository : IModifyLuggageRepository
    {
        private readonly string _connectionString;

        public ModifyLuggageRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        public async Task UpdateLuggageAsync(
            ModifyLuggageRequest modifyLuggageRequest,
            CancellationToken cancellationToken)
        {

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var passengerTable = new DataTable();
            passengerTable.Columns.Add("Id", typeof(int));
            passengerTable.Columns.Add("NewCheckedLuggage", typeof(int));

            foreach (var passenger in modifyLuggageRequest.Passengers)
            {
                passengerTable.Rows.Add(passenger.Id, passenger.NewCheckedLuggage);
            }

            var commandDefinition = new CommandDefinition(
                "dbo.UpdateBookingLuggage",
                new
                {
                    ConfirmationCode = modifyLuggageRequest.ConfirmationNumber,
                    Passengers = passengerTable.AsTableValuedParameter("dbo.PassengerLuggageType")
                },
                commandType: System.Data.CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            );

            await connection.ExecuteAsync(commandDefinition);
        }
    }
}
