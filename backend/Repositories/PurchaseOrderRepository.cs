using Dapper;
using Microsoft.Data.SqlClient;
using snoopy_airlines_backend.Domain;
using SnoopyAirlines.domain;

namespace snoopy_airlines_backend.Repositories
{
    public class PurchaseOrderRepository
    {
        private readonly string _connectionString;
        public PurchaseOrderRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("defaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        }

        public async Task<PurchaseOrder> CreatePurchaseOrderAsync(PurchaseOrder order, CancellationToken cancellationToken)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            using var transaction = connection.BeginTransaction();

            try
            {
                var orderId = await connection.ExecuteScalarAsync<int>("""
                    INSERT INTO PurchaseOrder(flightId, seatClass, status )
                    OUTPUT INSERTED.id
                    VALUES (@FlightId, @SeatClass, @Status);
                    """, order, transaction);

                order.Id = orderId;

                foreach (var passenger in order.Passengers)
                {
                    var passengerId = passenger.PurchaseOrderId = order.Id;
                    await connection.ExecuteScalarAsync<int>("""
                        INSERT INTO Passenger(purchaseOrderId, gender, FirstName, LastName, birthDay, birthMonth, birthYear, nationality)
                        OUTPUT INSERTED.id
                        VALUES (@PurchaseOrderId, @Gender, @FirstName, @LastName, @BirthDay, @BirthMonth, @BirthYear, @Nationality);
                        """, passenger, transaction);
                    passenger.Id = passengerId;
                }

                await transaction.CommitAsync(cancellationToken);
                return order;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }

        }

        public async Task<IReadOnlyCollection<PurchaseOrder>> GetPurchaseOrdersAsync(CancellationToken cancellationToken)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            var order = await connection.QueryAsync<PurchaseOrder>("""
                SELECT * FROM PurchaseOrder
                """);
            return order.ToList().AsReadOnly();
        }
    }
}
