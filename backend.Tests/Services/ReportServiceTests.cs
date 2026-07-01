using Moq;
using SnoopyAirlines.Domain;
using SnoopyAirlines.Repositories;
using SnoopyAirlines.Services;
using Xunit;

namespace backend.Tests.Services
{
    public class ReportServiceTests
    {
        private static readonly IReadOnlyCollection<AirlineDetailedReportRow> EmptyRows =
            Array.Empty<AirlineDetailedReportRow>();

        [Fact]
        public async Task TestGetAirlineDetailedReportNullFiltersPassesNullsToRepository()
        {
            // Arrange
            var mockRepo = new Mock<IReportRepository>();
            mockRepo
                .Setup(r => r.GetAirlineDetailedReportAsync(
                    null, null, null, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmptyRows);

            var service = new ReportService(mockRepo.Object);

            // Act
            await service.GetAirlineDetailedReportAsync(null, null, null, null, null, null, CancellationToken.None);

            // Assert
            mockRepo.Verify(r => r.GetAirlineDetailedReportAsync(
                null, null, null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestGetAirlineDetailedReportEmptyStringFiltersPassesNullsToRepository()
        {
            // Arrange
            var mockRepo = new Mock<IReportRepository>();
            mockRepo
                .Setup(r => r.GetAirlineDetailedReportAsync(
                    null, null, null, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmptyRows);

            var service = new ReportService(mockRepo.Object);

            // Act
            await service.GetAirlineDetailedReportAsync("", "", "", null, null, "", CancellationToken.None);

            // Assert
            mockRepo.Verify(r => r.GetAirlineDetailedReportAsync(
                null, null, null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestGetAirlineDetailedReportWhitespaceFiltersPassesNullsToRepository()
        {
            // Arrange
            var mockRepo = new Mock<IReportRepository>();
            mockRepo
                .Setup(r => r.GetAirlineDetailedReportAsync(
                    null, null, null, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmptyRows);

            var service = new ReportService(mockRepo.Object);

            // Act
            await service.GetAirlineDetailedReportAsync("   ", "  ", "  ", null, null, "   ", CancellationToken.None);

            // Assert
            mockRepo.Verify(r => r.GetAirlineDetailedReportAsync(
                null, null, null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestGetAirlineDetailedReportLowercaseOriginPassesUppercaseToRepository()
        {
            // Arrange
            var mockRepo = new Mock<IReportRepository>();
            mockRepo
                .Setup(r => r.GetAirlineDetailedReportAsync(
                    "SJO", null, null, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmptyRows);

            var service = new ReportService(mockRepo.Object);

            // Act
            await service.GetAirlineDetailedReportAsync("sjo", null, null, null, null, null, CancellationToken.None);

            // Assert
            mockRepo.Verify(r => r.GetAirlineDetailedReportAsync(
                "SJO", null, null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestGetAirlineDetailedReportOriginWithWhitespacePassesTrimmedUppercaseToRepository()
        {
            // Arrange
            var mockRepo = new Mock<IReportRepository>();
            mockRepo
                .Setup(r => r.GetAirlineDetailedReportAsync(
                    "MAD", null, null, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmptyRows);

            var service = new ReportService(mockRepo.Object);

            // Act
            await service.GetAirlineDetailedReportAsync(" mad ", null, null, null, null, null, CancellationToken.None);

            // Assert
            mockRepo.Verify(r => r.GetAirlineDetailedReportAsync(
                "MAD", null, null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestGetAirlineDetailedReportValidSeatClassPassesTrimmedToRepository()
        {
            // Arrange
            var mockRepo = new Mock<IReportRepository>();
            mockRepo
                .Setup(r => r.GetAirlineDetailedReportAsync(
                    null, null, "economy", null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmptyRows);

            var service = new ReportService(mockRepo.Object);

            // Act
            await service.GetAirlineDetailedReportAsync(null, null, " economy ", null, null, null, CancellationToken.None);

            // Assert
            mockRepo.Verify(r => r.GetAirlineDetailedReportAsync(
                null, null, "economy", null, null, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestGetAirlineDetailedReportReturnsRowsFromRepository()
        {
            // Arrange
            var expectedRows = new List<AirlineDetailedReportRow>
            {
                new() {
                    Fecha = new DateOnly(2026, 7, 9),
                    Origen = "SJO",
                    Destino = "ATL",
                    PasajerosPrimeraClase = 0,
                    PasajerosEconomia = 1,
                    Aerolinea = "Snoopy Airlines",
                    VentaPasajeros = 400,
                    VentaEquipajes = 187.5m,
                    TotalVenta = 587.5m
                }
            };

            var mockRepo = new Mock<IReportRepository>();
            mockRepo
                .Setup(r => r.GetAirlineDetailedReportAsync(
                    null, null, null, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedRows);

            var service = new ReportService(mockRepo.Object);

            // Act
            var result = await service.GetAirlineDetailedReportAsync(null, null, null, null, null, null, CancellationToken.None);

            // Assert
            Assert.Single(result);
            var row = result.First();
            Assert.Equal("SJO", row.Origen);
            Assert.Equal("ATL", row.Destino);
            Assert.Equal(1, row.PasajerosEconomia);
            Assert.Equal(587.5m, row.TotalVenta);
        }

        [Fact]
        public async Task TestGetAirlineDetailedReportWithDateFiltersPassesDateFiltersToRepository()
        {
            // Arrange
            var dateFrom = new DateOnly(2026, 6, 1);
            var dateTo = new DateOnly(2026, 6, 30);

            var mockRepo = new Mock<IReportRepository>();
            mockRepo
                .Setup(r => r.GetAirlineDetailedReportAsync(
                    null, null, null, dateFrom, dateTo, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmptyRows);

            var service = new ReportService(mockRepo.Object);

            // Act
            await service.GetAirlineDetailedReportAsync(null, null, null, dateFrom, dateTo, null, CancellationToken.None);

            // Assert
            mockRepo.Verify(r => r.GetAirlineDetailedReportAsync(
                null, null, null, dateFrom, dateTo, null, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
