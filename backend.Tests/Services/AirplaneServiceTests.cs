using Moq;
using SnoopyAirlines.domain;
using SnoopyAirlines.Domain.Intake;
using SnoopyAirlines.Repositories;
using SnoopyAirlines.Services;
using Xunit;

namespace backend.Tests.Services
{
    public class AirplaneServiceTests
    {
        [Fact]
        public async Task TestExistsByModel_ReturnsTrue_WhenModelExists()
        {
            // Arrange
            var mockRepo = new Mock<IAirplaneRepository>();
            mockRepo.Setup(r => r.ExistsByModelAsync("Boeing 737", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

            var service = new AirplaneService(mockRepo.Object);

            // Act
            var result = await service.ExistsByModelAsync("Boeing 737", CancellationToken.None);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task TestCreateAirplane_ReturnsCreatedAirplane()
        {
            // Arrange
            var airplane = new Airplane { Model = "Boeing 737", TouristRows = 20, TouristColumns = 6 };
            var expectedAirplane = new Airplane { Id = 1, Model = "Boeing 737", TouristRows = 20, TouristColumns = 6 };

            var mockRepo = new Mock<IAirplaneRepository>();
            mockRepo.Setup(r => r.CreateAirplaneAsync(airplane, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedAirplane);

            var service = new AirplaneService(mockRepo.Object);

            // Act
            var result = await service.CreateAirplaneAsync(airplane, CancellationToken.None);

            // Assert
            Assert.Equal(expectedAirplane.Id, result.Id);
            Assert.Equal(expectedAirplane.Model, result.Model);
        }

        [Fact]
        public async Task TestGetAirplaneById_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var mockRepo = new Mock<IAirplaneRepository>();
            mockRepo.Setup(r => r.GetAirplaneByIdAsync(99, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Airplane?)null);

            var service = new AirplaneService(mockRepo.Object);

            // Act
            var result = await service.GetAirplaneByIdAsync(99, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task TestGetAirplaneByModel_ReturnsAirplane_WhenFound()
        {
            // Arrange
            var expectedAirplane = new Airplane { Id = 1, Model = "Airbus A320" };

            var mockRepo = new Mock<IAirplaneRepository>();
            mockRepo.Setup(r => r.GetAirplaneByModelAsync("Airbus A320", It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedAirplane);

            var service = new AirplaneService(mockRepo.Object);

            // Act
            var result = await service.GetAirplaneByModelAync("Airbus A320", CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Airbus A320", result.Model);
        }
    }
}