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
                await EnsureFlightsExistAsync(connection, transaction, order.Routes, cancellationToken);

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

                // 2. Prepare bulk flight insert
                if (order.Routes.Any())
                {
                    var routes = order.Routes.OrderBy(route => route.SequenceNumber).ToList();
                    var routeValuesClauses = string.Join(",", routes.Select((_, i) => 
                        $"(@PurchaseOrderId, @SequenceNumber{i}, @FlightGuid{i})"));

                    batchSql.AppendLine($"""
                        INSERT INTO dbo.purchaseOrder_flight(PurchaseOrderId, SequenceNumber, FlightGuid)
                        VALUES {routeValuesClauses};
                        """);

                    for (int i = 0; i < routes.Count; i++)
                    {
                        var route = routes[i];
                        batchParameters.Add($"@SequenceNumber{i}", route.SequenceNumber);
                        batchParameters.Add($"@FlightGuid{i}", route.FlightGuid);
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
                return await GetPurchaseOrderByIdAsync(order.Id, cancellationToken) ?? order;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }

        }

        private static async Task EnsureFlightsExistAsync(
            SqlConnection connection,
            SqlTransaction transaction,
            IReadOnlyCollection<PurchaseOrderRoute> routes,
            CancellationToken cancellationToken)
        {
            var flightGuids = routes
                .Select(route => route.FlightGuid)
                .Distinct()
                .ToArray();

            var existingFlightGuids = await connection.QueryAsync<Guid>(
                new CommandDefinition("""
                    SELECT guid
                    FROM dbo.flight WITH (UPDLOCK, HOLDLOCK)
                    WHERE guid IN @FlightGuids
                      AND status = 'scheduled';
                    """,
                    new { FlightGuids = flightGuids },
                    transaction,
                    cancellationToken: cancellationToken));

            var missingFlightGuids = flightGuids.Except(existingFlightGuids).ToArray();
            if (missingFlightGuids.Length > 0)
            {
                throw new InvalidOperationException(
                    $"Flight(s) not found or unavailable: {string.Join(", ", missingFlightGuids)}.");
            }
        }

        public async Task<IReadOnlyCollection<PurchaseOrder>> GetPurchaseOrdersAsync(CancellationToken cancellationToken)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            using var results = await connection.QueryMultipleAsync(
                new CommandDefinition("""
                SELECT
                    purchase_order.id AS Id,
                    purchase_order.seatClass AS SeatClass,
                    amount_breakdown.TicketTotalAmount,
                    amount_breakdown.CarryOnLuggageTotalAmount,
                    amount_breakdown.CheckedLuggageTotalAmount,
                    amount_breakdown.TotalAmount
                FROM PurchaseOrder purchase_order
                OUTER APPLY dbo.GetPurchaseOrderAmountBreakdown(purchase_order.Id) amount_breakdown
                ORDER BY purchase_order.id;

                SELECT
                    PurchaseOrderId AS PurchaseOrderId,
                    SequenceNumber AS SequenceNumber,
                    FlightGuid AS FlightGuid,
                    flight_internal.route_id AS RouteId,
                    CAST(flight.departure_at AS date) AS IntendedDate
                FROM dbo.purchaseOrder_flight purchase_order_flight
                INNER JOIN dbo.flight flight
                    ON flight.guid = purchase_order_flight.FlightGuid
                LEFT JOIN dbo.flight_internal flight_internal
                    ON flight_internal.flight_guid = flight.guid
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
                    purchase_order.id AS Id,
                    purchase_order.seatClass AS SeatClass,
                    amount_breakdown.TicketTotalAmount,
                    amount_breakdown.CarryOnLuggageTotalAmount,
                    amount_breakdown.CheckedLuggageTotalAmount,
                    amount_breakdown.TotalAmount
                FROM PurchaseOrder purchase_order
                OUTER APPLY dbo.GetPurchaseOrderAmountBreakdown(purchase_order.Id) amount_breakdown
                WHERE purchase_order.id = @Id;

                SELECT
                    PurchaseOrderId AS PurchaseOrderId,
                    SequenceNumber AS SequenceNumber,
                    FlightGuid AS FlightGuid,
                    flight_internal.route_id AS RouteId,
                    CAST(flight.departure_at AS date) AS IntendedDate
                FROM dbo.purchaseOrder_flight purchase_order_flight
                INNER JOIN dbo.flight flight
                    ON flight.guid = purchase_order_flight.FlightGuid
                LEFT JOIN dbo.flight_internal flight_internal
                    ON flight_internal.flight_guid = flight.guid
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
                route.FlightGuid,
                route.RouteId,
                IntendedDate = route.IntendedDate?.ToDateTime(TimeOnly.MinValue)
            };
        }

        private static PurchaseOrder ToPurchaseOrder(PurchaseOrderRecord order)
        {
            return new PurchaseOrder
            {
                Id = order.Id,
                SeatClass = order.SeatClass,
                TicketTotalAmount = order.TicketTotalAmount,
                CarryOnLuggageTotalAmount = order.CarryOnLuggageTotalAmount,
                CheckedLuggageTotalAmount = order.CheckedLuggageTotalAmount,
                TotalAmount = order.TotalAmount
            };
        }

        private static PurchaseOrderRoute ToPurchaseOrderRoute(PurchaseOrderRouteRecord route)
        {
            return new PurchaseOrderRoute
            {
                PurchaseOrderId = route.PurchaseOrderId,
                SequenceNumber = route.SequenceNumber,
                FlightGuid = route.FlightGuid,
                RouteId = route.RouteId,
                IntendedDate = route.IntendedDate.HasValue ? DateOnly.FromDateTime(route.IntendedDate.Value) : null
            };
        }

        private class PurchaseOrderRecord
        {
            public int Id { get; set; }
            public string SeatClass { get; set; } = string.Empty;
            public decimal TicketTotalAmount { get; set; }
            public decimal CarryOnLuggageTotalAmount { get; set; }
            public decimal CheckedLuggageTotalAmount { get; set; }
            public decimal TotalAmount { get; set; }
        }

        private class PurchaseOrderRouteRecord
        {
            public int PurchaseOrderId { get; set; }
            public int SequenceNumber { get; set; }
            public Guid FlightGuid { get; set; }
            public int? RouteId { get; set; }
            public DateTime? IntendedDate { get; set; }
        }
    }
}
