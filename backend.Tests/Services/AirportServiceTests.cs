using Microsoft.Extensions.Configuration;
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
        public async Task CreateAirportAsync_WhenCodeAlreadyExists_ThrowsInvalidOperationException()
        {
            // Arrange
            var airport = new Airport { Name = "Aeropuerto Internacional", Code = "SJO", CityId = 1 };

            var mockSection = new Mock<IConfigurationSection>();
            mockSection.Setup(s => s["DefaultConnection"]).Returns("Server=fake;");
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c.GetSection("ConnectionStrings")).Returns(mockSection.Object);

            var mockRepo = new Mock<AirportRepository>(mockConfig.Object);
            mockRepo.Setup(r => r.AirportCodeExistsAsync(airport.Code, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

            var service = new AirportService(mockRepo.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateAirportAsync(airport, CancellationToken.None));
        }

        [Fact]
        public async Task CreateAirportAsync_WhenCodeDoesNotExist_ReturnsCreatedAirport()
        {
            // Arrange
            var airport = new Airport { Name = "Aeropuerto Internacional", Code = "SJO", CityId = 1 };
            var expectedAirport = new Airport { Id = 1, Name = "Aeropuerto Internacional", Code = "SJO", CityId = 1 };

            var mockSection = new Mock<IConfigurationSection>();
            mockSection.Setup(s => s["DefaultConnection"]).Returns("Server=fake;");
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c.GetSection("ConnectionStrings")).Returns(mockSection.Object);

            var mockRepo = new Mock<AirportRepository>(mockConfig.Object);
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
        public async Task UpdateAirportNameAsync_WhenNameIsEmpty_ThrowsArgumentException()
        {
            // Arrange
            var service = new AirportService(null!);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.UpdateAirportNameAsync(1, "   ", CancellationToken.None));
        }
    }
}
