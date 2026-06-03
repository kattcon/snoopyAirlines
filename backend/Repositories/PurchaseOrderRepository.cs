using Dapper;
using Microsoft.Data.SqlClient;
using snoopy_airlines_backend.Domain;

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
            using var transaction = connection.BeginTransaction();

            try
            {
                var orderId = await connection.ExecuteScalarAsync<int>("""
                    INSERT INTO PurchaseOrder(seatClass)
                    OUTPUT INSERTED.id
                    VALUES (@SeatClass);
                    """, new { order.SeatClass }, transaction);

                order.Id = orderId;

                foreach (var route in order.Routes.OrderBy(route => route.SequenceNumber))
                {
                    route.PurchaseOrderId = order.Id;
                    await connection.ExecuteAsync("""
                        INSERT INTO dbo.PurchaseOrderRoute(PurchaseOrderId, SequenceNumber, RouteId, IntendedDate)
                        VALUES (@PurchaseOrderId, @SequenceNumber, @RouteId, @IntendedDate);
                        """, ToParameters(route), transaction);
                }

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

        public async Task<PurchaseOrderEmailData?> GetPurchaseOrderDetailsAsync(
            int purchaseOrderId,
            CancellationToken cancellationToken)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var rows = await connection.QueryAsync<PurchaseOrderDetailRow>("""
                SELECT * FROM GetPurchaseOrderDetails(@PurchaseOrderId)
                """, new { PurchaseOrderId = purchaseOrderId });
            
            var list = rows.ToList();

            if (list.Count == 0)
            {
                return null;
            }

            var first = list[0];

            return new PurchaseOrderEmailData
            {
                PurchaseOrderId  = first.PurchaseOrderId,
                SeatClass = first.SeatClass,
                DepartureTime = first.DepartureTime,
                ArrivalTime = first.ArrivalTime,
                DepartureAirportName = first.DepartureAirportName,
                DepartureAirportCode = first.DepartureAirportCode,
                DepartureCityName = first.DepartureCityName,
                ArrivalAirportName = first.ArrivalAirportName,
                ArrivalAirportCode = first.ArrivalAirportCode,
                ArrivalCityName = first.ArrivalCityName,
                AirplaneModel = first.AirplaneModel,
                Passengers = list.Select(r => new PassengerEmailData
                {
                    FirstName = r.FirstName,
                    LastName = r.LastName,
                    Gender = r.Gender,
                    Nationality = r.Nationality,
                    BirthDay = r.BirthDay,
                    BirthMonth = r.BirthMonth,
                    BirthYear = r.BirthYear
                }).ToList(),
            };
        }

    }
}
