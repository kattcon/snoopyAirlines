using Moq;
using Xunit;
using snoopy_airlines_backend.Domain.Intake;
using snoopy_airlines_backend.Domain.View;
using snoopy_airlines_backend.Repositories;
using snoopy_airlines_backend.Services;
using SnoopyAirlines.Domain.Intake;
using SnoopyAirlines.Repositories;

namespace backend.Tests.Services
{
    public class ModifyLuggageServiceTests
    {
        private readonly Mock<IPassengerLuggageRepository> _passengerRepository = new();
        private readonly Mock<IFlightLuggageRepository> _flightRepository = new();
        private readonly Mock<IModifyLuggageRepository> _modifyRepository = new();

        private ModifyLuggageService CreateService()
        {
            return new ModifyLuggageService(
                _passengerRepository.Object,
                _flightRepository.Object,
                _modifyRepository.Object);
        }

        [Fact]
        public async Task TestGetPassengersByConfirmationReturnsPassengers()
        {
            // Arrange
            var service = CreateService();

            var passengers = new List<PassengerView>
            {
                new PassengerView
                {
                    Id = 1,
                    FirstName = "Juan",
                    LastName = "Perez",
                    CheckedLuggage = 2
                }
            };

            _passengerRepository
                .Setup(r => r.GetPassengersByConfirmationAsync(
                    "ABC123",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(passengers);

            // Act
            var result = await service.GetPassengersByConfirmationAsync(
                "ABC123",
                CancellationToken.None);

            // Assert
            Assert.Single(result);
            Assert.Equal("Juan", result.First().FirstName);

            _passengerRepository.Verify(
                r => r.GetPassengersByConfirmationAsync(
                    "ABC123",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task TestGetPassengersByConfirmationEmptyConfirmationNumber()
        {
            // Arrange
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GetPassengersByConfirmationAsync(
                    "",
                    CancellationToken.None));

            _passengerRepository.Verify(
                r => r.GetPassengersByConfirmationAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task TestGetPassengersByConfirmationWhenReservationDoesNotExist()
        {
            // Arrange
            var service = CreateService();

            _passengerRepository
                .Setup(r => r.GetPassengersByConfirmationAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PassengerView>());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.GetPassengersByConfirmationAsync(
                    "ABC123",
                    CancellationToken.None));

            _passengerRepository.Verify(
                r => r.GetPassengersByConfirmationAsync(
                    "ABC123",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task TestGetFlightLuggageInfoReturnsFlightInformation()
        {
            // Arrange
            var service = CreateService();

            var luggageInfo = new List<FlightLuggageView>
            {
                new FlightLuggageView
                {
                    PriceCheckedBaggage = 35,
                    CheckedBaggagePriceMultiplier = 1.5m,
                    WeightLimitCheckedBaggage = 23
                }
            };

            _flightRepository
                .Setup(r => r.GetFlightLuggageInfoByConfirmationAsync(
                    "ABC123",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(luggageInfo);

            // Act
            var result = await service.GetFlightLuggageInfoByConfirmationAsync(
                "ABC123",
                CancellationToken.None);

            // Assert
            Assert.Single(result);
            Assert.Equal(23, result.First().WeightLimitCheckedBaggage);

            _flightRepository.Verify(
                r => r.GetFlightLuggageInfoByConfirmationAsync(
                    "ABC123",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task TestGetFlightLuggageInfoEmptyConfirmationNumber()
        {
            // Arrange
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GetFlightLuggageInfoByConfirmationAsync(
                    "",
                    CancellationToken.None));

            _flightRepository.Verify(
                r => r.GetFlightLuggageInfoByConfirmationAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task TestGetLuggageInfoReturnsCombinedInformation()
        {
            // Arrange
            var service = CreateService();

            var passengers = new List<PassengerView>
            {
                new PassengerView
                {
                    Id = 1,
                    FirstName = "Juan",
                    LastName = "Perez",
                    CheckedLuggage = 1
                }
            };

            var luggageInfo = new List<FlightLuggageView>
            {
                new FlightLuggageView
                {
                    PriceCheckedBaggage = 35,
                    CheckedBaggagePriceMultiplier = 1.5m,
                    WeightLimitCheckedBaggage = 23
                }
            };

            _passengerRepository
                .Setup(r => r.GetPassengersByConfirmationAsync(
                    "ABC123",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(passengers);

            _flightRepository
                .Setup(r => r.GetFlightLuggageInfoByConfirmationAsync(
                    "ABC123",
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(luggageInfo);

            // Act
            var result = await service.GetLuggageInfoAsync(
                "ABC123",
                CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Passengers);
            Assert.Single(result.LuggageInfo);

            _passengerRepository.Verify(
                r => r.GetPassengersByConfirmationAsync(
                    "ABC123",
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _flightRepository.Verify(
                r => r.GetFlightLuggageInfoByConfirmationAsync(
                    "ABC123",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task TestGetLuggageInfoWhenPassengersDoNotExistThrowsException()
        {
            // Arrange
            var service = CreateService();

            _passengerRepository
                .Setup(r => r.GetPassengersByConfirmationAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PassengerView>());

            _flightRepository
                .Setup(r => r.GetFlightLuggageInfoByConfirmationAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FlightLuggageView>());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.GetLuggageInfoAsync(
                    "ABC123",
                    CancellationToken.None));

            _passengerRepository.Verify(
                r => r.GetPassengersByConfirmationAsync(
                    "ABC123",
                    It.IsAny<CancellationToken>()),
                Times.Once);

            _flightRepository.Verify(
                r => r.GetFlightLuggageInfoByConfirmationAsync(
                    "ABC123",
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task TestUpdateLuggageCallsRepository()
        {
            // Arrange
            var service = CreateService();

            var request = new ModifyLuggageRequest
            {
                ConfirmationNumber = "ABC123",
                TotalAmountPaid = 50,
                Passengers = new List<PassengerLuggageIntake>
                {
                    new PassengerLuggageIntake()
                }
            };

            // Act
            await service.UpdateLuggageAsync(
                request,
                CancellationToken.None);

            // Assert
            _modifyRepository.Verify(
                r => r.UpdateLuggageAsync(
                    request,
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task TestUpdateLuggageEmptyConfirmationNumber()
        {
            // Arrange
            var service = CreateService();

            var request = new ModifyLuggageRequest
            {
                ConfirmationNumber = "",
                TotalAmountPaid = 50,
                Passengers = new List<PassengerLuggageIntake>
                {
                    new PassengerLuggageIntake()
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.UpdateLuggageAsync(
                    request,
                    CancellationToken.None));

            _modifyRepository.Verify(
                r => r.UpdateLuggageAsync(
                    It.IsAny<ModifyLuggageRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task TestUpdateLuggageWithoutPassengers()
        {
            // Arrange
            var service = CreateService();

            var request = new ModifyLuggageRequest
            {
                ConfirmationNumber = "ABC123",
                TotalAmountPaid = 50,
                Passengers = new List<PassengerLuggageIntake>()
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.UpdateLuggageAsync(
                    request,
                    CancellationToken.None));

            _modifyRepository.Verify(
                r => r.UpdateLuggageAsync(
                    It.IsAny<ModifyLuggageRequest>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}