using Dapper;
using Microsoft.Data.SqlClient;
using snoopy_airlines_backend.Domain;
using System.Text;

namespace snoopy_airlines_backend.Repositories
{
    public class PurchaseOrderRepository : IPurchaseOrderRepository
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
            using var transaction = connection.BeginTransaction(System.Data.IsolationLevel.Serializable);

            try
            {
                await EnsureRoutesExistAsync(connection, transaction, order.Routes, cancellationToken);

                // Prepare all batch SQL and parameters
                var batchSql = new StringBuilder();
                var batchParameters = new DynamicParameters();

                // 1. Insert PurchaseOrder
                batchSql.AppendLine("""
                    DECLARE @PurchaseOrderId int;

                    INSERT INTO PurchaseOrder(seatClass)
                    VALUES (@SeatClass);

                    SET @PurchaseOrderId = CAST(SCOPE_IDENTITY() AS int);
                    SELECT @PurchaseOrderId;
                    """);
                batchParameters.Add("@SeatClass", order.SeatClass);

                // 2. Prepare bulk route insert
                if (order.Routes.Any())
                {
                    var routes = order.Routes.OrderBy(route => route.SequenceNumber).ToList();
                    var routeValuesClauses = string.Join(",", routes.Select((_, i) => 
                        $"(@PurchaseOrderId, @SequenceNumber{i}, @RouteId{i}, @IntendedDate{i})"));

                    batchSql.AppendLine($"""
                        INSERT INTO dbo.PurchaseOrderRoute(PurchaseOrderId, SequenceNumber, RouteId, IntendedDate)
                        VALUES {routeValuesClauses};
                        """);

                    for (int i = 0; i < routes.Count; i++)
                    {
                        var route = routes[i];
                        batchParameters.Add($"@SequenceNumber{i}", route.SequenceNumber);
                        batchParameters.Add($"@RouteId{i}", route.RouteId);
                        batchParameters.Add($"@IntendedDate{i}", route.IntendedDate?.ToDateTime(TimeOnly.MinValue));
                    }
                }

                // 3. Prepare bulk passenger insert
                if (order.Passengers.Any())
                {
                    var passengerValuesClauses = string.Join(",", order.Passengers.Select((_, i) => 
                        $"(@PurchaseOrderId, @Gender{i}, @FirstName{i}, @LastName{i}, @BirthDay{i}, @BirthMonth{i}, @BirthYear{i}, @Nationality{i}, @CarryOnLuggage{i}, @CheckedLuggage{i})"));

                    batchSql.AppendLine($"""
                        INSERT INTO Passenger(purchaseOrderId, gender, FirstName, LastName, birthDay, birthMonth, birthYear, nationality, CarryOnLuggage, CheckedLuggage)
                        OUTPUT INSERTED.id
                        VALUES {passengerValuesClauses};
                        """);

                    for (int i = 0; i < order.Passengers.Count; i++)
                    {
                        var passenger = order.Passengers[i];
                        batchParameters.Add($"@Gender{i}", passenger.Gender);
                        batchParameters.Add($"@FirstName{i}", passenger.FirstName);
                        batchParameters.Add($"@LastName{i}", passenger.LastName);
                        batchParameters.Add($"@BirthDay{i}", passenger.BirthDay);
                        batchParameters.Add($"@BirthMonth{i}", passenger.BirthMonth);
                        batchParameters.Add($"@BirthYear{i}", passenger.BirthYear);
                        batchParameters.Add($"@Nationality{i}", passenger.Nationality);
                        batchParameters.Add($"@CarryOnLuggage{i}", passenger.CarryOnLuggage);
                        batchParameters.Add($"@CheckedLuggage{i}", passenger.CheckedLuggage);
                    }
                }

                // Execute all statements in one batch
                using var results = await connection.QueryMultipleAsync(
                    new CommandDefinition(batchSql.ToString(), batchParameters, transaction, cancellationToken: cancellationToken));

                order.Id = (await results.ReadAsync<int>()).First();

                if (order.Passengers.Any())
                {
                    var passengerIds = (await results.ReadAsync<int>()).ToList();
                    for (int i = 0; i < order.Passengers.Count; i++)
                    {
                        order.Passengers[i].Id = passengerIds[i];
                    }
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

        private static async Task EnsureRoutesExistAsync(
            SqlConnection connection,
            SqlTransaction transaction,
            IReadOnlyCollection<PurchaseOrderRoute> routes,
            CancellationToken cancellationToken)
        {
            var routeIds = routes
                .Select(route => route.RouteId)
                .Distinct()
                .ToArray();

            var existingRouteIds = await connection.QueryAsync<int>(
                new CommandDefinition("""
                    SELECT id
                    FROM dbo.[route] WITH (UPDLOCK, HOLDLOCK)
                    WHERE id IN @RouteIds
                    AND is_deleted = 0;
                    """,
                    new { RouteIds = routeIds },
                    transaction,
                    cancellationToken: cancellationToken));

            var missingRouteIds = routeIds.Except(existingRouteIds).ToArray();
            if (missingRouteIds.Length > 0)
            {
                throw new InvalidOperationException(
                    $"Route(s) not found: {string.Join(", ", missingRouteIds)}.");
            }
        }

        public async Task<IReadOnlyCollection<PurchaseOrder>> GetPurchaseOrdersAsync(CancellationToken cancellationToken)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            using var results = await connection.QueryMultipleAsync(
                new CommandDefinition("""
                SELECT
                    id AS Id,
                    seatClass AS SeatClass
                FROM PurchaseOrder
                ORDER BY id;

                SELECT
                    PurchaseOrderId AS PurchaseOrderId,
                    SequenceNumber AS SequenceNumber,
                    RouteId AS RouteId,
                    IntendedDate AS IntendedDate
                FROM dbo.PurchaseOrderRoute
                ORDER BY PurchaseOrderId, SequenceNumber;
                """, cancellationToken: cancellationToken));

            var orders = (await results.ReadAsync<PurchaseOrderRecord>()).Select(ToPurchaseOrder).ToList();
            var orderLookup = orders.ToDictionary(order => order.Id);
            var routeRecords = await results.ReadAsync<PurchaseOrderRouteRecord>();

            foreach (var routeRecord in routeRecords)
            {
                if (orderLookup.TryGetValue(routeRecord.PurchaseOrderId, out var order))
                {
                    order.Routes.Add(ToPurchaseOrderRoute(routeRecord));
                }
            }

            return orders.AsReadOnly();
        }

        public async Task<PurchaseOrder?> GetPurchaseOrderByIdAsync(int id, CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT
                    id AS Id,
                    seatClass AS SeatClass
                FROM PurchaseOrder
                WHERE id = @Id;

                SELECT
                    PurchaseOrderId AS PurchaseOrderId,
                    SequenceNumber AS SequenceNumber,
                    RouteId AS RouteId,
                    IntendedDate AS IntendedDate
                FROM dbo.PurchaseOrderRoute
                WHERE PurchaseOrderId = @Id
                ORDER BY SequenceNumber;

                SELECT
                    id AS Id,
                    purchaseOrderId AS PurchaseOrderId,
                    gender AS Gender,
                    firstName AS FirstName,
                    lastName AS LastName,
                    birthDay AS BirthDay,
                    birthMonth AS BirthMonth,
                    birthYear AS BirthYear,
                    nationality AS Nationality,
                    carryOnLuggage AS CarryOnLuggage,
                    checkedLuggage AS CheckedLuggage
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
            var routes = await results.ReadAsync<PurchaseOrderRouteRecord>();
            order.Routes = routes.Select(ToPurchaseOrderRoute).ToList();

            var passengers = await results.ReadAsync<Passenger>();
            order.Passengers = passengers.ToList();

            return order;
        }

        private static object ToParameters(PurchaseOrderRoute route)
        {
            return new
            {
                route.PurchaseOrderId,
                route.SequenceNumber,
                route.RouteId,
                IntendedDate = route.IntendedDate?.ToDateTime(TimeOnly.MinValue)
            };
        }

        private static PurchaseOrder ToPurchaseOrder(PurchaseOrderRecord order)
        {
            return new PurchaseOrder
            {
                Id = order.Id,
                SeatClass = order.SeatClass
            };
        }

        private static PurchaseOrderRoute ToPurchaseOrderRoute(PurchaseOrderRouteRecord route)
        {
            return new PurchaseOrderRoute
            {
                PurchaseOrderId = route.PurchaseOrderId,
                SequenceNumber = route.SequenceNumber,
                RouteId = route.RouteId,
                IntendedDate = route.IntendedDate.HasValue ? DateOnly.FromDateTime(route.IntendedDate.Value) : null
            };
        }

        private class PurchaseOrderRecord
        {
            public int Id { get; set; }
            public string SeatClass { get; set; } = string.Empty;
        }

        private class PurchaseOrderRouteRecord
        {
            public int PurchaseOrderId { get; set; }
            public int SequenceNumber { get; set; }
            public int RouteId { get; set; }
            public DateTime? IntendedDate { get; set; }
        }
    }
}
