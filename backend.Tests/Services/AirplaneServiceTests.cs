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
        public async Task TestExistsByModelReturnsTrueWhenModelExists()
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
        public async Task TestCreateAirplaneReturnsCreatedAirplane()
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
        public async Task TestGetAirplaneByIdReturnsNullWhenNotFound()
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
        public async Task TestGetAirplaneByModelReturnsAirplaneWhenFound()
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

        [Fact]
        public async Task TestDeleteAirplaneAirplaneNotFoundThrowsKeyNotFoundException()
        {
            // Arrange
            var mockRepo = new Mock<IAirplaneRepository>();
            mockRepo.Setup(r => r.GetAirplaneByIdAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Airplane?)null);

            var service = new AirplaneService(mockRepo.Object);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.DeleteAirplaneAsync(1, CancellationToken.None));
        }

        [Fact]
        public async Task TestDeleteAirplaneAirplaneHasPurchasesCallsSoftDelete()
        {
            // Arrange
            var airplane = new Airplane { Id = 1, Model = "Boeing 737" };

            var mockRepo = new Mock<IAirplaneRepository>();
            mockRepo.Setup(r => r.GetAirplaneByIdAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(airplane);
            mockRepo.Setup(r => r.AirplaneHasPurchasesAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

            var service = new AirplaneService(mockRepo.Object);

            // Act
            await service.DeleteAirplaneAsync(1, CancellationToken.None);

            // Assert
            mockRepo.Verify(r => r.SoftDeleteAirplaneAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            mockRepo.Verify(r => r.HardDeleteAirplaneAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task TestDeleteAirplaneAirplaneHasNoPurchasesCallsHardDelete()
        {
            // Arrange
            var airplane = new Airplane { Id = 1, Model = "Boeing 737" };

            var mockRepo = new Mock<IAirplaneRepository>();
            mockRepo.Setup(r => r.GetAirplaneByIdAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(airplane);
            mockRepo.Setup(r => r.AirplaneHasPurchasesAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);

            var service = new AirplaneService(mockRepo.Object);

            // Act
            await service.DeleteAirplaneAsync(1, CancellationToken.None);

            // Assert
            mockRepo.Verify(r => r.HardDeleteAirplaneAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            mockRepo.Verify(r => r.SoftDeleteAirplaneAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}