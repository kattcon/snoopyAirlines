using Moq;
using snoopy_airlines_backend.Domain;
using snoopy_airlines_backend.Repositories;
using snoopy_airlines_backend.Services;
using Xunit;

namespace backend.Tests.Services
{
    public class ReportServiceTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(10000)]
        public async Task GetMonthlyRevenueReportAsync_InvalidYear_ThrowsArgumentOutOfRangeException(int year)
        {
            // Arrange
            var mockRepository = new Mock<IReportRepository>();
            var service = new ReportService(mockRepository.Object);

            // Act + Assert
            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
                service.GetMonthlyRevenueReportAsync(year, CancellationToken.None));
        }

        [Fact]
        public async Task GetMonthlyRevenueReportAsync_MapsRowsAndAddsSpanishMonthLabel()
        {
            // Arrange
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
                .Setup(repository => repository.GetMonthlyRevenueBreakdownAsync(selectedYear, It.IsAny<CancellationToken>()))
                .ReturnsAsync(repositoryRows);

            var service = new ReportService(mockRepository.Object);

            // Act
            var report = await service.GetMonthlyRevenueReportAsync(selectedYear, CancellationToken.None);

            // Assert
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
                repository => repository.GetMonthlyRevenueBreakdownAsync(selectedYear, It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}