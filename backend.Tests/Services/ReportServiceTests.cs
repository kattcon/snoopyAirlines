using Moq;
using SnoopyAirlines.Domain;
using SnoopyAirlines.Repositories;
using SnoopyAirlines.Services;
using Xunit;

namespace backend.Tests.Services
{
    public class ReportServiceTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(10000)]
        public async Task TestGetMonthlyRevenueReportAsyncInvalidYearThrowsArgumentOutOfRangeException(int year)
        {
            var mockRepository = new Mock<IReportRepository>();
            var service = new ReportService(mockRepository.Object);

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                service.GetMonthlyRevenueReportAsync(year, null, null, null, CancellationToken.None));
        }

        [Fact]
        public async Task TestGetMonthlyRevenueReportAsyncMapsRowsAndAddsSpanishMonthLabel()
        {
            const int selectedYear = 2026;

            var repositoryRows = new List<MonthlyRevenueReportRow>
            {
                new()
                {
                    MonthNumber = 2,
                    FlightCount = 1,
                    FirstClassPassengers = 1,
                    EconomyPassengers = 0,
                    TotalPassengers = 1,
                    TicketRevenue = 1800.00m,
                    LuggageRevenue = 150.00m,
                    TotalRevenue = 1950.00m,
                }
            };

            var mockRepository = new Mock<IReportRepository>();
            mockRepository
                .Setup(repository => repository.GetMonthlyRevenueBreakdownAsync(selectedYear, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(repositoryRows);

            var service = new ReportService(mockRepository.Object);

            var report = await service.GetMonthlyRevenueReportAsync(selectedYear, null, null, null, CancellationToken.None);

            Assert.Equal(selectedYear, report.Year);
            Assert.Single(report.Rows);

            var row = report.Rows.Single();
            Assert.Equal(2, row.MonthNumber);
            Assert.Equal("Febrero", row.MonthLabel);
            Assert.Equal(1, row.FlightCount);
            Assert.Equal(1, row.FirstClassPassengers);
            Assert.Equal(0, row.EconomyPassengers);
            Assert.Equal(1, row.TotalPassengers);
            Assert.Equal(1800.00m, row.TicketRevenue);
            Assert.Equal(150.00m, row.LuggageRevenue);
            Assert.Equal(1950.00m, row.TotalRevenue);

            mockRepository.Verify(
                repository => repository.GetMonthlyRevenueBreakdownAsync(selectedYear, null, null, null, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task TestGetMonthlyRevenueReportAsyncAllFiltersEmptyReturnsReportWithNullYear()
        {
            var mockRepository = new Mock<IReportRepository>();
            mockRepository
                .Setup(repository => repository.GetMonthlyRevenueBreakdownAsync(null, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<MonthlyRevenueReportRow>());

            var service = new ReportService(mockRepository.Object);

            var report = await service.GetMonthlyRevenueReportAsync(null, null, null, null, CancellationToken.None);

            Assert.Null(report.Year);
            Assert.Empty(report.Rows);
        }

        private static readonly IReadOnlyCollection<AirlineDetailedReportRow> EmptyRows =
            Array.Empty<AirlineDetailedReportRow>();

        [Fact]
        public async Task TestGetAirlineDetailedReportNullFiltersPassesNullsToRepository()
        {
            var mockRepo = new Mock<IReportRepository>();
            mockRepo
                .Setup(r => r.GetAirlineDetailedReportAsync(
                    null, null, null, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmptyRows);

            var service = new ReportService(mockRepo.Object);

            await service.GetAirlineDetailedReportAsync(null, null, null, null, null, null, CancellationToken.None);

            mockRepo.Verify(r => r.GetAirlineDetailedReportAsync(
                null, null, null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestGetAirlineDetailedReportEmptyStringFiltersPassesNullsToRepository()
        {
            var mockRepo = new Mock<IReportRepository>();
            mockRepo
                .Setup(r => r.GetAirlineDetailedReportAsync(
                    null, null, null, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmptyRows);

            var service = new ReportService(mockRepo.Object);

            await service.GetAirlineDetailedReportAsync("", "", "", null, null, "", CancellationToken.None);

            mockRepo.Verify(r => r.GetAirlineDetailedReportAsync(
                null, null, null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestGetAirlineDetailedReportWhitespaceFiltersPassesNullsToRepository()
        {
            var mockRepo = new Mock<IReportRepository>();
            mockRepo
                .Setup(r => r.GetAirlineDetailedReportAsync(
                    null, null, null, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmptyRows);

            var service = new ReportService(mockRepo.Object);

            await service.GetAirlineDetailedReportAsync("   ", "  ", "  ", null, null, "   ", CancellationToken.None);

            mockRepo.Verify(r => r.GetAirlineDetailedReportAsync(
                null, null, null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestGetAirlineDetailedReportLowercaseOriginPassesUppercaseToRepository()
        {
            var mockRepo = new Mock<IReportRepository>();
            mockRepo
                .Setup(r => r.GetAirlineDetailedReportAsync(
                    "SJO", null, null, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmptyRows);

            var service = new ReportService(mockRepo.Object);

            await service.GetAirlineDetailedReportAsync("sjo", null, null, null, null, null, CancellationToken.None);

            mockRepo.Verify(r => r.GetAirlineDetailedReportAsync(
                "SJO", null, null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestGetAirlineDetailedReportOriginWithWhitespacePassesTrimmedUppercaseToRepository()
        {
            var mockRepo = new Mock<IReportRepository>();
            mockRepo
                .Setup(r => r.GetAirlineDetailedReportAsync(
                    "MAD", null, null, null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmptyRows);

            var service = new ReportService(mockRepo.Object);

            await service.GetAirlineDetailedReportAsync(" mad ", null, null, null, null, null, CancellationToken.None);

            mockRepo.Verify(r => r.GetAirlineDetailedReportAsync(
                "MAD", null, null, null, null, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestGetAirlineDetailedReportValidSeatClassPassesTrimmedToRepository()
        {
            var mockRepo = new Mock<IReportRepository>();
            mockRepo
                .Setup(r => r.GetAirlineDetailedReportAsync(
                    null, null, "economy", null, null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmptyRows);

            var service = new ReportService(mockRepo.Object);

            await service.GetAirlineDetailedReportAsync(null, null, " economy ", null, null, null, CancellationToken.None);

            mockRepo.Verify(r => r.GetAirlineDetailedReportAsync(
                null, null, "economy", null, null, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestGetAirlineDetailedReportReturnsRowsFromRepository()
        {
            var expectedRows = new List<AirlineDetailedReportRow>
            {
                new()
                {
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

            var result = await service.GetAirlineDetailedReportAsync(null, null, null, null, null, null, CancellationToken.None);

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
            var dateFrom = new DateOnly(2026, 6, 1);
            var dateTo = new DateOnly(2026, 6, 30);

            var mockRepo = new Mock<IReportRepository>();
            mockRepo
                .Setup(r => r.GetAirlineDetailedReportAsync(
                    null, null, null, dateFrom, dateTo, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(EmptyRows);

            var service = new ReportService(mockRepo.Object);

            await service.GetAirlineDetailedReportAsync(null, null, null, dateFrom, dateTo, null, CancellationToken.None);

            mockRepo.Verify(r => r.GetAirlineDetailedReportAsync(
                null, null, null, dateFrom, dateTo, null, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
