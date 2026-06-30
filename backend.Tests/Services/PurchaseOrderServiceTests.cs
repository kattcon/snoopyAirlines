using Moq;
using snoopy_airlines_backend.Domain;
using snoopy_airlines_backend.Repositories;
using snoopy_airlines_backend.Services;
using Xunit;

namespace backend.Tests.Services
{
    public class PurchaseOrderServiceTests
    {
        private readonly Mock<IPurchaseOrderRepository> _purchaseOrderRepository = new();

        [Fact]
        public async Task TestCreatePurchaseOrderAsyncNormalizesOrderAndReturnsSavedOrder()
        {
            // Arrange
            var cancellationToken = new CancellationTokenSource().Token;
            var savedOrder = CreateValidOrder();
            savedOrder.Id = 42;
            PurchaseOrder? capturedOrder = null;

            _purchaseOrderRepository
                .Setup(repository => repository.CreatePurchaseOrderAsync(
                    It.IsAny<PurchaseOrder>(),
                    cancellationToken))
                .Callback<PurchaseOrder, CancellationToken>((order, _) => capturedOrder = order)
                .ReturnsAsync(savedOrder);

            var service = CreateService();
            var order = CreateValidOrder();
            order.SeatClass = "  Economy  ";
            order.Routes =
            [
                new PurchaseOrderRoute
                {
                    SequenceNumber = 20,
                    FlightGuid = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    RouteId = 200,
                    IntendedDate = new DateOnly(2026, 7, 2)
                },
                new PurchaseOrderRoute
                {
                    SequenceNumber = 10,
                    FlightGuid = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    RouteId = 100,
                    IntendedDate = new DateOnly(2026, 7, 1)
                }
            ];

            // Act
            var result = await service.CreatePurchaseOrderAsync(order, cancellationToken);

            // Assert
            Assert.Same(savedOrder, result);
            Assert.NotNull(capturedOrder);
            Assert.Equal("Economy", capturedOrder!.SeatClass);
            Assert.Collection(
                capturedOrder.Routes,
                route =>
                {
                    Assert.Equal(1, route.SequenceNumber);
                    Assert.Equal(Guid.Parse("11111111-1111-1111-1111-111111111111"), route.FlightGuid);
                    Assert.Equal(100, route.RouteId);
                    Assert.Equal(new DateOnly(2026, 7, 1), route.IntendedDate);
                },
                route =>
                {
                    Assert.Equal(2, route.SequenceNumber);
                    Assert.Equal(Guid.Parse("22222222-2222-2222-2222-222222222222"), route.FlightGuid);
                    Assert.Equal(200, route.RouteId);
                    Assert.Equal(new DateOnly(2026, 7, 2), route.IntendedDate);
                });
            Assert.Same(order.Passengers, capturedOrder.Passengers);
        }

        [Fact]
        public async Task TestCreatePurchaseOrderAsyncWithBlankSeatClassThrowsArgumentException()
        {
            // Arrange
            var service = CreateService();
            var order = CreateValidOrder();
            order.SeatClass = "   ";

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreatePurchaseOrderAsync(order, CancellationToken.None));

            Assert.Equal("seatClass is required.", exception.Message);
            VerifyCreateWasNotCalled();
        }

        [Fact]
        public async Task TestCreatePurchaseOrderAsyncWithoutRoutesThrowsArgumentException()
        {
            // Arrange
            var service = CreateService();
            var order = CreateValidOrder();
            order.Routes.Clear();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreatePurchaseOrderAsync(order, CancellationToken.None));

            Assert.Equal("At least one route is required.", exception.Message);
            VerifyCreateWasNotCalled();
        }

        [Fact]
        public async Task TestCreatePurchaseOrderAsyncWithoutPassengersThrowsArgumentException()
        {
            // Arrange
            var service = CreateService();
            var order = CreateValidOrder();
            order.Passengers.Clear();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreatePurchaseOrderAsync(order, CancellationToken.None));

            Assert.Equal("At least one passenger is required.", exception.Message);
            VerifyCreateWasNotCalled();
        }

        [Fact]
        public async Task TestCreatePurchaseOrderAsyncWithoutFlightGuidThrowsArgumentException()
        {
            // Arrange
            var service = CreateService();
            var order = CreateValidOrder();
            order.Routes[0].FlightGuid = Guid.Empty;

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
                service.CreatePurchaseOrderAsync(order, CancellationToken.None));

            Assert.Equal("flightGuid is required for every route.", exception.Message);
            VerifyCreateWasNotCalled();
        }

        [Fact]
        public async Task TestGetPurchaseOrdersAsyncReturnsRepositoryOrders()
        {
            // Arrange
            var cancellationToken = new CancellationTokenSource().Token;
            IReadOnlyCollection<PurchaseOrder> orders = [CreateValidOrder()];

            _purchaseOrderRepository
                .Setup(repository => repository.GetPurchaseOrdersAsync(cancellationToken))
                .ReturnsAsync(orders);

            var service = CreateService();

            // Act
            var result = await service.GetPurchaseOrdersAsync(cancellationToken);

            // Assert
            Assert.Same(orders, result);
            _purchaseOrderRepository.Verify(
                repository => repository.GetPurchaseOrdersAsync(cancellationToken),
                Times.Once);
        }

        [Fact]
        public async Task TestGetPurchaseOrderByIdAsyncReturnsRepositoryOrder()
        {
            // Arrange
            const int purchaseOrderId = 42;
            var cancellationToken = new CancellationTokenSource().Token;
            var order = CreateValidOrder();
            order.Id = purchaseOrderId;

            _purchaseOrderRepository
                .Setup(repository => repository.GetPurchaseOrderByIdAsync(
                    purchaseOrderId,
                    cancellationToken))
                .ReturnsAsync(order);

            var service = CreateService();

            // Act
            var result = await service.GetPurchaseOrderByIdAsync(
                purchaseOrderId,
                cancellationToken);

            // Assert
            Assert.Same(order, result);
            _purchaseOrderRepository.Verify(
                repository => repository.GetPurchaseOrderByIdAsync(
                    purchaseOrderId,
                    cancellationToken),
                Times.Once);
        }

        private PurchaseOrderService CreateService()
        {
            return new PurchaseOrderService(_purchaseOrderRepository.Object);
        }

        private void VerifyCreateWasNotCalled()
        {
            _purchaseOrderRepository.Verify(
                repository => repository.CreatePurchaseOrderAsync(
                    It.IsAny<PurchaseOrder>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        private static PurchaseOrder CreateValidOrder()
        {
            return new PurchaseOrder
            {
                SeatClass = "Economy",
                Routes =
                [
                    new PurchaseOrderRoute
                    {
                        SequenceNumber = 1,
                        FlightGuid = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        RouteId = 100,
                        IntendedDate = new DateOnly(2026, 7, 1)
                    }
                ],
                Passengers =
                [
                    new Passenger
                    {
                        Gender = "Female",
                        FirstName = "Jane",
                        LastName = "Doe",
                        BirthDay = "15",
                        BirthMonth = "March",
                        BirthYear = "1990",
                        Nationality = "Costa Rican",
                        CarryOnLuggage = "1",
                        CheckedLuggage = "1"
                    }
                ]
            };
        }
    }
}
