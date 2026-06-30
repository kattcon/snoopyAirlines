using Moq;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Repositories;
using SnoopyAirlines.Services;
using SnoopyAirlines.Services.PartnerAirlines;
using Xunit;

namespace backend.Tests.Services
{
    public class FlightServiceReportTests
    {
        private readonly Mock<IFlightRepository> _flightRepository = new();

        [Fact]
        public async Task TestGetFlightReportEmptyConfirmationNumber()
        {
            // Arrange
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GetFlightReportByConfirmationAsync("", "Cruz Ruiz", CancellationToken.None));

            _flightRepository.Verify(
                r => r.GetFlightReportByConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task TestGetFlightReportWhitespaceConfirmationNumber()
        {
            // Arrange
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GetFlightReportByConfirmationAsync("   ", "Cruz Ruiz", CancellationToken.None));

            _flightRepository.Verify(
                r => r.GetFlightReportByConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task TestGetFlightReportNullConfirmationNumber()
        {
            // Arrange
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GetFlightReportByConfirmationAsync(null!, "Cruz Ruiz", CancellationToken.None));
        }

        [Fact]
        public async Task TestGetFlightReportEmptyLastNames()
        {
            // Arrange
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GetFlightReportByConfirmationAsync("69314F9CF6F9", "", CancellationToken.None));

            _flightRepository.Verify(
                r => r.GetFlightReportByConfirmationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task TestGetFlightReportWhitespaceLastNames()
        {
            // Arrange
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GetFlightReportByConfirmationAsync("69314F9CF6F9", "   ", CancellationToken.None));
        }

        [Fact]
        public async Task TestGetFlightReportRepositoryReturnsEmptyLegs()
        {
            // Arrange
            SetupRepositoryResult("69314F9CF6F9", "Cruz Ruiz", new FlightSearchResult
            {
                Legs = [],
                Passengers = []
            });
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.GetFlightReportByConfirmationAsync("69314F9CF6F9", "Cruz Ruiz", CancellationToken.None));
        }

        [Fact]
        public async Task TestGetFlightReportSingleLegSuccess()
        {
            // Arrange
            var expectedLeg = CreateFlightReportView();
            SetupRepositoryResult("69314F9CF6F9", "Cruz Ruiz", new FlightSearchResult
            {
                Legs = [expectedLeg],
                Passengers = []
            });
            var service = CreateService();

            // Act
            var result = await service.GetFlightReportByConfirmationAsync(
                "69314F9CF6F9", "Cruz Ruiz", CancellationToken.None);

            // Assert
            Assert.Single(result.Legs);
            Assert.Equal(expectedLeg.ReservationNumber, result.Legs.First().ReservationNumber);
            Assert.Equal(expectedLeg.CardHolderName, result.Legs.First().CardHolderName);
        }

        [Fact]
        public async Task TestGetFlightReportMultipleLegsSuccess()
        {
            // Arrange
            var firstLeg = CreateFlightReportView(sequenceNumber: 1);
            var secondLeg = CreateFlightReportView(sequenceNumber: 2);
            SetupRepositoryResult("69314F9CF6F9", "Cruz Ruiz", new FlightSearchResult
            {
                Legs = [firstLeg, secondLeg],
                Passengers = []
            });
            var service = CreateService();

            // Act
            var result = await service.GetFlightReportByConfirmationAsync(
                "69314F9CF6F9", "Cruz Ruiz", CancellationToken.None);

            // Assert
            Assert.Equal(2, result.Legs.Count);
            Assert.Equal(1, result.Legs.First().SequenceNumber);
            Assert.Equal(2, result.Legs.Last().SequenceNumber);
        }

        [Fact]
        public async Task TestGetFlightReportIncludesPassengers()
        {
            // Arrange
            var leg = CreateFlightReportView();
            var passenger = CreatePassengerReportView();
            SetupRepositoryResult("69314F9CF6F9", "Cruz Ruiz", new FlightSearchResult
            {
                Legs = [leg],
                Passengers = [passenger]
            });
            var service = CreateService();

            // Act
            var result = await service.GetFlightReportByConfirmationAsync(
                "69314F9CF6F9", "Cruz Ruiz", CancellationToken.None);

            // Assert
            Assert.Single(result.Passengers);
            Assert.Equal(passenger.FirstName, result.Passengers.First().FirstName);
            Assert.Equal(passenger.LastName, result.Passengers.First().LastName);
        }

        [Fact]
        public async Task TestGetFlightReportCallsRepositoryWithCorrectParameters()
        {
            // Arrange
            SetupRepositoryResult("69314F9CF6F9", "Cruz Ruiz", new FlightSearchResult
            {
                Legs = [CreateFlightReportView()],
                Passengers = []
            });
            var service = CreateService();

            // Act
            await service.GetFlightReportByConfirmationAsync("69314F9CF6F9", "Cruz Ruiz", CancellationToken.None);

            // Assert
            _flightRepository.Verify(
                r => r.GetFlightReportByConfirmationAsync("69314F9CF6F9", "Cruz Ruiz", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        private FlightService CreateService()
        {
            return new FlightService(
                Mock.Of<IRouteService>(),
                _flightRepository.Object,
                Mock.Of<IExternalFlightSearchService>());
        }

        private void SetupRepositoryResult(
            string confirmationNumber,
            string lastNames,
            FlightSearchResult result)
        {
            _flightRepository
                .Setup(r => r.GetFlightReportByConfirmationAsync(
                    confirmationNumber, lastNames, It.IsAny<CancellationToken>()))
                .ReturnsAsync(result);
        }

        private static FlightReportView CreateFlightReportView(int sequenceNumber = 1)
        {
            return new FlightReportView
            {
                ReservationNumber = "69314F9CF6F9",
                CardHolderName = "Javier Cruz Ruiz",
                SequenceNumber = sequenceNumber,
                DepartureCity = "San Jose",
                ArrivalCity = "Miami",
                DepartureAirportCode = "SJO",
                ArrivalAirportCode = "MIA",
                DepartureDate = new DateOnly(2026, 7, 5),
                DepartureTime = new TimeOnly(7, 45),
                ArrivalDate = new DateOnly(2026, 7, 5),
                ArrivalTime = new TimeOnly(12, 0),
                DurationMinutes = 255,
                AirplaneModel = "Boeing 737-800",
                PassengerCount = 2
            };
        }

        private static PassengerReportView CreatePassengerReportView()
        {
            return new PassengerReportView
            {
                FirstName = "Javier",
                LastName = "Cruz Ruiz",
                Gender = "M",
                Nationality = "Costa Rica",
                BirthDay = "01",
                BirthMonth = "01",
                BirthYear = "2000",
                CarryOnLuggage = 1,
                CheckedLuggage = 1
            };
        }
    }
}
