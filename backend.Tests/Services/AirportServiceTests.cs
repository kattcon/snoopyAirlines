using Moq;
using SnoopyAirlines.Domain;
using SnoopyAirlines.Repositories;
using SnoopyAirlines.Services;
using Xunit;

namespace backend.Tests.Services
{
    public class AirportServiceTests
    {
        [Fact]
        public async Task TestCreateAirportDuplicateCode()
        {
            // Arrange
            var airport = new Airport { Name = "Aeropuerto Internacional", Code = "SJO", CityId = 1 };

            var mockRepo = new Mock<IAirportRepository>();
            mockRepo.Setup(r => r.AirportCodeExistsAsync(airport.Code, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

            var service = new AirportService(mockRepo.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateAirportAsync(airport, CancellationToken.None));
        }

        [Fact]
        public async Task TestCreateAirportSuccess()
        {
            // Arrange
            var airport = new Airport { Name = "Aeropuerto Internacional", Code = "SJO", CityId = 1 };
            var expectedAirport = new Airport { Id = 1, Name = "Aeropuerto Internacional", Code = "SJO", CityId = 1 };

            var mockRepo = new Mock<IAirportRepository>();
            mockRepo.Setup(r => r.AirportCodeExistsAsync(airport.Code, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);
            mockRepo.Setup(r => r.CreateAirportAsync(airport, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(expectedAirport);

            var service = new AirportService(mockRepo.Object);

            // Act
            var result = await service.CreateAirportAsync(airport, CancellationToken.None);

            // Assert
            Assert.Equal(expectedAirport.Id, result.Id);
            Assert.Equal(expectedAirport.Code, result.Code);
        }

        [Fact]
        public async Task TestUpdateAirportEmptyName()
        {
            // Arrange
            var service = new AirportService(null!);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.UpdateAirportNameAsync(1, "   ", CancellationToken.None));
        }

        [Fact]
        public async Task DeleteAirport_AirportNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var mockRepo = new Mock<IAirportRepository>();
            mockRepo.Setup(r => r.GetAirportByIdAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Airport?)null);

            var service = new AirportService(mockRepo.Object);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                service.DeleteAirportAsync(1, CancellationToken.None));
        }

        [Fact]
        public async Task DeleteAirport_AirportHasPurchases_CallsSoftDelete()
        {
            // Arrange
            var airport = new Airport { Id = 1, Name = "Aeropuerto Internacional", Code = "SJO", CityId = 1 };

            var mockRepo = new Mock<IAirportRepository>();
            mockRepo.Setup(r => r.GetAirportByIdAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(airport);
            mockRepo.Setup(r => r.AirportHasPurchasesAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

            var service = new AirportService(mockRepo.Object);

            // Act
            await service.DeleteAirportAsync(1, CancellationToken.None);

            // Assert
            mockRepo.Verify(r => r.SoftDeleteAirportAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            mockRepo.Verify(r => r.HardDeleteAirportAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAirport_AirportHasNoPurchases_CallsHardDelete()
        {
            // Arrange
            var airport = new Airport { Id = 1, Name = "Aeropuerto Internacional", Code = "SJO", CityId = 1 };

            var mockRepo = new Mock<IAirportRepository>();
            mockRepo.Setup(r => r.GetAirportByIdAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(airport);
            mockRepo.Setup(r => r.AirportHasPurchasesAsync(1, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);

            var service = new AirportService(mockRepo.Object);

            // Act
            await service.DeleteAirportAsync(1, CancellationToken.None);

            // Assert
            mockRepo.Verify(r => r.HardDeleteAirportAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            mockRepo.Verify(r => r.SoftDeleteAirportAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
