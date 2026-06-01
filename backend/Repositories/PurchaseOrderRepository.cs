using Dapper;
using Microsoft.Data.SqlClient;
using snoopy_airlines_backend.Domain;

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
                    INSERT INTO PurchaseOrder(routeId, intendedDate, seatClass)
                    OUTPUT INSERTED.id
                    VALUES (@RouteId, @IntendedDate, @SeatClass);
                    """, ToParameters(order), transaction);

                order.Id = orderId;

                foreach (var passenger in order.Passengers)
                {
                    passenger.PurchaseOrderId = order.Id;
                    var passengerId = await connection.ExecuteScalarAsync<int>("""
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
            var orders = await connection.QueryAsync<PurchaseOrderRecord>("""
                SELECT
                    id AS Id,
                    routeId AS RouteId,
                    intendedDate AS IntendedDate,
                    seatClass AS SeatClass
                FROM PurchaseOrder
                """);

            return orders.Select(ToPurchaseOrder).ToList().AsReadOnly();
        }

        public async Task<PurchaseOrder?> GetPurchaseOrderByIdAsync(int id, CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT
                    id AS Id,
                    routeId AS RouteId,
                    intendedDate AS IntendedDate,
                    seatClass AS SeatClass
                FROM PurchaseOrder
                WHERE id = @Id;

                SELECT
                    id AS Id,
                    purchaseOrderId AS PurchaseOrderId,
                    gender AS Gender,
                    firstName AS FirstName,
                    lastName AS LastName,
                    birthDay AS BirthDay,
                    birthMonth AS BirthMonth,
                    birthYear AS BirthYear,
                    nationality AS Nationality
                FROM Passenger
                WHERE purchaseOrderId = @Id
                ORDER BY id;
                """;

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            using var results = await connection.QueryMultipleAsync(
                new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));

            var orderRecord = await results.ReadSingleOrDefaultAsync<PurchaseOrderRecord>();
            if (orderRecord is null)
            {
                return null;
            }

            var order = ToPurchaseOrder(orderRecord);
            var passengers = await results.ReadAsync<Passenger>();
            order.Passengers = passengers.ToList();

            return order;
        }

        private static object ToParameters(PurchaseOrder order)
        {
            return new
            {
                order.RouteId,
                IntendedDate = order.IntendedDate?.ToDateTime(TimeOnly.MinValue),
                order.SeatClass
            };
        }

        private static PurchaseOrder ToPurchaseOrder(PurchaseOrderRecord order)
        {
            return new PurchaseOrder
            {
                Id = order.Id,
                RouteId = order.RouteId,
                IntendedDate = order.IntendedDate.HasValue ? DateOnly.FromDateTime(order.IntendedDate.Value) : null,
                SeatClass = order.SeatClass
            };
        }

        private class PurchaseOrderRecord
        {
            public int Id { get; set; }
            public int RouteId { get; set; }
            public DateTime? IntendedDate { get; set; }
            required public string SeatClass { get; set; }
        }
    }
}
