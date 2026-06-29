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
        public async Task TestGetAirlineDetailedReport_NullFilters_PassesNullsToRepository()
        {
            // Arrange
            var mockRepo = new Mock<IReportRepository>();
            mockRepo
                .Setup(r => r.GetAirlineDetailedReportAsync(
                    null, null, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmptyRows);

            var service = new ReportService(mockRepo.Object);

            // Act
            await service.GetAirlineDetailedReportAsync(null, null, null, null, null, CancellationToken.None);

            // Assert
            mockRepo.Verify(r => r.GetAirlineDetailedReportAsync(
                null, null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestGetAirlineDetailedReport_EmptyStringFilters_PassesNullsToRepository()
        {
            // Arrange
            var mockRepo = new Mock<IReportRepository>();
            mockRepo
                .Setup(r => r.GetAirlineDetailedReportAsync(
                    null, null, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmptyRows);

            var service = new ReportService(mockRepo.Object);

            // Act
            await service.GetAirlineDetailedReportAsync("", "", "", null, null, CancellationToken.None);

            // Assert
            mockRepo.Verify(r => r.GetAirlineDetailedReportAsync(
                null, null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestGetAirlineDetailedReport_WhitespaceFilters_PassesNullsToRepository()
        {
            // Arrange
            var mockRepo = new Mock<IReportRepository>();
            mockRepo
                .Setup(r => r.GetAirlineDetailedReportAsync(
                    null, null, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmptyRows);

            var service = new ReportService(mockRepo.Object);

            // Act
            await service.GetAirlineDetailedReportAsync("   ", "  ", "  ", null, null, CancellationToken.None);

            // Assert
            mockRepo.Verify(r => r.GetAirlineDetailedReportAsync(
                null, null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestGetAirlineDetailedReport_LowercaseOrigin_PassesUppercaseToRepository()
        {
            // Arrange
            var mockRepo = new Mock<IReportRepository>();
            mockRepo
                .Setup(r => r.GetAirlineDetailedReportAsync(
                    "SJO", null, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmptyRows);

            var service = new ReportService(mockRepo.Object);

            // Act
            await service.GetAirlineDetailedReportAsync("sjo", null, null, null, null, CancellationToken.None);

            // Assert
            mockRepo.Verify(r => r.GetAirlineDetailedReportAsync(
                "SJO", null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestGetAirlineDetailedReport_OriginWithWhitespace_PassesTrimmedUppercaseToRepository()
        {
            // Arrange
            var mockRepo = new Mock<IReportRepository>();
            mockRepo
                .Setup(r => r.GetAirlineDetailedReportAsync(
                    "MAD", null, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmptyRows);

            var service = new ReportService(mockRepo.Object);

            // Act
            await service.GetAirlineDetailedReportAsync(" mad ", null, null, null, null, CancellationToken.None);

            // Assert
            mockRepo.Verify(r => r.GetAirlineDetailedReportAsync(
                "MAD", null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestGetAirlineDetailedReport_ValidSeatClass_PassesTrimmedToRepository()
        {
            // Arrange
            var mockRepo = new Mock<IReportRepository>();
            mockRepo
                .Setup(r => r.GetAirlineDetailedReportAsync(
                    null, null, "economy", null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmptyRows);

            var service = new ReportService(mockRepo.Object);

            // Act
            await service.GetAirlineDetailedReportAsync(null, null, " economy ", null, null, CancellationToken.None);

            // Assert
            mockRepo.Verify(r => r.GetAirlineDetailedReportAsync(
                null, null, "economy", null, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestGetAirlineDetailedReport_ReturnsRowsFromRepository()
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
                    null, null, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedRows);

            var service = new ReportService(mockRepo.Object);

            // Act
            var result = await service.GetAirlineDetailedReportAsync(null, null, null, null, null, CancellationToken.None);

            // Assert
            Assert.Single(result);
            var row = result.First();
            Assert.Equal("SJO", row.Origen);
            Assert.Equal("ATL", row.Destino);
            Assert.Equal(1, row.PasajerosEconomia);
            Assert.Equal(587.5m, row.TotalVenta);
        }

        [Fact]
        public async Task TestGetAirlineDetailedReport_WithDateFilters_PassesDateFiltersToRepository()
        {
            // Arrange
            var dateFrom = new DateOnly(2026, 6, 1);
            var dateTo = new DateOnly(2026, 6, 30);

            var mockRepo = new Mock<IReportRepository>();
            mockRepo
                .Setup(r => r.GetAirlineDetailedReportAsync(
                    null, null, null, dateFrom, dateTo, It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmptyRows);

            var service = new ReportService(mockRepo.Object);

            // Act
            await service.GetAirlineDetailedReportAsync(null, null, null, dateFrom, dateTo, CancellationToken.None);

            // Assert
            mockRepo.Verify(r => r.GetAirlineDetailedReportAsync(
                null, null, null, dateFrom, dateTo, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
