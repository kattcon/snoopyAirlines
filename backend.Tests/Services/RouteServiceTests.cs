using Microsoft.Extensions.Configuration;
using Xunit;
using Moq;
using SnoopyAirlines.Services;
using SnoopyAirlines.Domain;
using SnoopyAirlines.Repositories;

namespace SnoopyAirlines.Tests
{
    public class RouteServiceTests
    {
        private readonly Mock<IRouteRepository> _mockRepository = new();
        private readonly RouteService _routeService;

        public RouteServiceTests()
        {
            _routeService = new RouteService(_mockRepository.Object);
        }

        /// <summary>
        /// Test 1: Verifica que se retornen vuelos directos cuando no se solicitan escalas
        /// </summary>
        [Fact]
        public async Task SearchFlights_WithoutStopovers_ReturnOnlyDirectFlights()
        {
            // Arrange
            var query = new RouteQuery
            {
                Origin = "SJO",
                Destination = "MIA",
                EarliestDeparture = new DateTime(2026, 6, 15, 6, 0, 0),
                LatestDeparture = new DateTime(2026, 6, 15, 22, 0, 0),
                QuantityOfPassengers = 2,
                IncludeStopovers = false
            };

            var route = new Route
            {
                Id = 1,
                DepartureAirport = new RouteAirport { Code = "SJO", Name = "San José", City = "San José" },
                ArrivalAirport = new RouteAirport { Code = "MIA", Name = "Miami", City = "Miami" },
                DepartureTime = new TimeOnly(8, 0),
                ArrivalTime = new TimeOnly(10, 30),
                DurationMinutes = 150,
                Frequency = new RouteFrequency { Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = false, Sunday = false },
                PriceEconomyClass = 299,
                PriceFirstClass = 599,
                PriceCarryOnBaggage = 0,
                PriceCheckedBaggage = 25
            };

            _mockRepository
                .Setup(r => r.GetRoutesAsync(It.IsAny<RouteSearchQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { route });

            // Act
            var results = await _routeService.SearchFlightsAsync(query, CancellationToken.None);

            // Assert
            Assert.NotEmpty(results);
            Assert.All(results, flight => Assert.False(flight.HasStopover));
            Assert.True(results.All(f => f.DepartureAirport.Code == "SJO" && f.ArrivalAirport.Code == "MIA"));
        }

        /// <summary>
        /// Test 2: Verifica que las escalas menores a 1 hora sean filtradas (restricción mínima)
        /// </summary>
        [Fact]
        public async Task SearchFlights_WithStopoverLessThanOneHour_StopoverIsFiltered()
        {
            // Arrange
            var query = new RouteQuery
            {
                Origin = "SJO",
                Destination = "LAX",
                EarliestDeparture = new DateTime(2026, 6, 15, 6, 0, 0),
                LatestDeparture = new DateTime(2026, 6, 15, 22, 0, 0),
                QuantityOfPassengers = 1,
                IncludeStopovers = true
            };

            var firstLeg = new Route
            {
                Id = 1,
                DepartureAirport = new RouteAirport { Code = "SJO", Name = "San José", City = "San José" },
                ArrivalAirport = new RouteAirport { Code = "MIA", Name = "Miami", City = "Miami" },
                DepartureTime = new TimeOnly(8, 0),
                ArrivalTime = new TimeOnly(10, 30),
                DurationMinutes = 150,
                Frequency = new RouteFrequency { Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = false, Sunday = false },
                PriceEconomyClass = 299,
                PriceFirstClass = 599,
                PriceCarryOnBaggage = 0,
                PriceCheckedBaggage = 25
            };

            var secondLeg = new Route
            {
                Id = 2,
                DepartureAirport = new RouteAirport { Code = "MIA", Name = "Miami", City = "Miami" },
                ArrivalAirport = new RouteAirport { Code = "LAX", Name = "Los Ángeles", City = "Los Ángeles" },
                DepartureTime = new TimeOnly(11, 0), // Solo 30 minutos después de la llegada (10:30)
                ArrivalTime = new TimeOnly(12, 30),
                DurationMinutes = 90,
                Frequency = new RouteFrequency { Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = false, Sunday = false },
                PriceEconomyClass = 249,
                PriceFirstClass = 499,
                PriceCarryOnBaggage = 0,
                PriceCheckedBaggage = 25
            };

            _mockRepository
                .Setup(r => r.GetRoutesAsync(
                    It.Is<RouteSearchQuery>(q => q.Origin == "SJO" && string.IsNullOrWhiteSpace(q.Destination)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { firstLeg });

            _mockRepository
                .Setup(r => r.GetRoutesAsync(
                    It.Is<RouteSearchQuery>(q => q.Destination == "LAX" && string.IsNullOrWhiteSpace(q.Origin)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { secondLeg });

            _mockRepository
                .Setup(r => r.GetRoutesAsync(
                    It.Is<RouteSearchQuery>(q => q.Origin == "SJO" && q.Destination == "LAX"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<Route>());

            // Act
            var results = await _routeService.SearchFlightsAsync(query, CancellationToken.None);

            // Assert
            var stopoverFlights = results.Where(f => f.HasStopover && f.StopoverAirport?.Code == "MIA").ToList();
            Assert.DoesNotContain(stopoverFlights, flight => flight.StopoverDuration != null && flight.StopoverDuration.Contains("00:30"));
        }

        /// <summary>
        /// Test 3: Verifica que las escalas mayores a 12 horas sean filtradas (restricción máxima)
        /// </summary>
        [Fact]
        public async Task SearchFlights_WithStopoverMoreThanTwelveHours_StopoverIsFiltered()
        {
            // Arrange
            var query = new RouteQuery
            {
                Origin = "SJO",
                Destination = "CDG",
                EarliestDeparture = new DateTime(2026, 6, 15, 6, 0, 0),
                LatestDeparture = new DateTime(2026, 6, 16, 22, 0, 0),
                QuantityOfPassengers = 2,
                IncludeStopovers = true
            };

            var firstLeg = new Route
            {
                Id = 3,
                DepartureAirport = new RouteAirport { Code = "SJO", Name = "San José", City = "San José" },
                ArrivalAirport = new RouteAirport { Code = "BOG", Name = "Bogotá", City = "Bogotá" },
                DepartureTime = new TimeOnly(8, 0),
                ArrivalTime = new TimeOnly(11, 0),
                DurationMinutes = 180,
                Frequency = new RouteFrequency { Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = true, Sunday = true },
                PriceEconomyClass = 199,
                PriceFirstClass = 399,
                PriceCarryOnBaggage = 0,
                PriceCheckedBaggage = 25
            };

            var secondLeg = new Route
            {
                Id = 4,
                DepartureAirport = new RouteAirport { Code = "BOG", Name = "Bogotá", City = "Bogotá" },
                ArrivalAirport = new RouteAirport { Code = "CDG", Name = "París", City = "París" },
                DepartureTime = new TimeOnly(23, 30), // 12:30 horas después de la llegada (11:00 + 12:30)
                ArrivalTime = new TimeOnly(15, 0), // Día siguiente
                DurationMinutes = 840,
                Frequency = new RouteFrequency { Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = true, Sunday = true },
                PriceEconomyClass = 649,
                PriceFirstClass = 1249,
                PriceCarryOnBaggage = 0,
                PriceCheckedBaggage = 25
            };

            _mockRepository
                .Setup(r => r.GetRoutesAsync(
                    It.Is<RouteSearchQuery>(q => q.Origin == "SJO" && string.IsNullOrWhiteSpace(q.Destination)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { firstLeg });

            _mockRepository
                .Setup(r => r.GetRoutesAsync(
                    It.Is<RouteSearchQuery>(q => q.Destination == "CDG" && string.IsNullOrWhiteSpace(q.Origin)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { secondLeg });

            _mockRepository
                .Setup(r => r.GetRoutesAsync(
                    It.Is<RouteSearchQuery>(q => q.Origin == "SJO" && q.Destination == "CDG"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<Route>());

            // Act
            var results = await _routeService.SearchFlightsAsync(query, CancellationToken.None);

            // Assert
            var stopoverFlights = results.Where(f => f.HasStopover && f.StopoverAirport?.Code == "BOG").ToList();
            Assert.DoesNotContain(stopoverFlights, flight => flight.StopoverDuration != null && flight.StopoverDuration.Contains("12:30"));
        }

        [Fact]
        public async Task SearchFlights_WithArrivalWindow_PassesArrivalWindowAndFiltersDirectFlights()
        {
            // Arrange
            var query = new RouteQuery
            {
                Origin = "SJO",
                Destination = "MIA",
                EarliestDeparture = new DateTime(2026, 6, 15, 6, 0, 0),
                LatestDeparture = new DateTime(2026, 6, 15, 22, 0, 0),
                EarliestArrival = new DateTime(2026, 6, 15, 10, 0, 0),
                LatestArrival = new DateTime(2026, 6, 15, 11, 0, 0),
                QuantityOfPassengers = 1,
                IncludeStopovers = false
            };

            var matchingRoute = new Route
            {
                Id = 5,
                DepartureAirport = new RouteAirport { Code = "SJO", Name = "San Jose", City = "San Jose" },
                ArrivalAirport = new RouteAirport { Code = "MIA", Name = "Miami", City = "Miami" },
                DepartureTime = new TimeOnly(8, 0),
                ArrivalTime = new TimeOnly(10, 30),
                DurationMinutes = 150,
                Frequency = new RouteFrequency { Monday = true },
                PriceEconomyClass = 299,
                PriceFirstClass = 599,
                PriceCarryOnBaggage = 0,
                PriceCheckedBaggage = 25
            };

            var lateRoute = new Route
            {
                Id = 6,
                DepartureAirport = new RouteAirport { Code = "SJO", Name = "San Jose", City = "San Jose" },
                ArrivalAirport = new RouteAirport { Code = "MIA", Name = "Miami", City = "Miami" },
                DepartureTime = new TimeOnly(9, 0),
                ArrivalTime = new TimeOnly(12, 30),
                DurationMinutes = 210,
                Frequency = new RouteFrequency { Monday = true },
                PriceEconomyClass = 299,
                PriceFirstClass = 599,
                PriceCarryOnBaggage = 0,
                PriceCheckedBaggage = 25
            };

            RouteSearchQuery? capturedQuery = null;
            _mockRepository
                .Setup(r => r.GetRoutesAsync(It.IsAny<RouteSearchQuery>(), It.IsAny<CancellationToken>()))
                .Callback<RouteSearchQuery, CancellationToken>((routeSearchQuery, _) => capturedQuery = routeSearchQuery)
                .ReturnsAsync([matchingRoute, lateRoute]);

            // Act
            var results = await _routeService.SearchFlightsAsync(query, CancellationToken.None);

            // Assert
            var flight = Assert.Single(results);
            Assert.Equal(matchingRoute.Id, flight.RouteId);

            Assert.NotNull(capturedQuery);
            var arrivalWindow = Assert.Single(capturedQuery.ArrivalWindows);
            Assert.True(arrivalWindow.SameDayDepartureFrequency.Monday);
            Assert.True(arrivalWindow.PreviousDayDepartureFrequency.Sunday);
            Assert.Equal(new TimeOnly(10, 0), arrivalWindow.EarliestArrival);
            Assert.Equal(new TimeOnly(11, 0), arrivalWindow.LatestArrival);
        }

        [Fact]
        public async Task SearchFlights_WithOvernightArrivalWindow_UsesPreviousDayDepartureFrequency()
        {
            // Arrange
            var query = new RouteQuery
            {
                Origin = "SJO",
                Destination = "MIA",
                EarliestDeparture = new DateTime(2026, 6, 15, 20, 0, 0),
                LatestDeparture = new DateTime(2026, 6, 15, 23, 0, 0),
                EarliestArrival = new DateTime(2026, 6, 16, 0, 30, 0),
                LatestArrival = new DateTime(2026, 6, 16, 2, 0, 0),
                QuantityOfPassengers = 1,
                IncludeStopovers = false
            };

            var overnightRoute = new Route
            {
                Id = 7,
                DepartureAirport = new RouteAirport { Code = "SJO", Name = "San Jose", City = "San Jose" },
                ArrivalAirport = new RouteAirport { Code = "MIA", Name = "Miami", City = "Miami" },
                DepartureTime = new TimeOnly(22, 30),
                ArrivalTime = new TimeOnly(1, 15),
                DurationMinutes = 165,
                Frequency = new RouteFrequency { Monday = true },
                PriceEconomyClass = 299,
                PriceFirstClass = 599,
                PriceCarryOnBaggage = 0,
                PriceCheckedBaggage = 25
            };

            RouteSearchQuery? capturedQuery = null;
            _mockRepository
                .Setup(r => r.GetRoutesAsync(It.IsAny<RouteSearchQuery>(), It.IsAny<CancellationToken>()))
                .Callback<RouteSearchQuery, CancellationToken>((routeSearchQuery, _) => capturedQuery = routeSearchQuery)
                .ReturnsAsync(new[] { overnightRoute });

            // Act
            var results = await _routeService.SearchFlightsAsync(query, CancellationToken.None);

            // Assert
            var flight = Assert.Single(results);
            Assert.Equal(new DateTime(2026, 6, 16, 1, 15, 0), flight.ArrivalTime);

            Assert.NotNull(capturedQuery);
            var arrivalWindow = Assert.Single(capturedQuery.ArrivalWindows);
            Assert.True(arrivalWindow.SameDayDepartureFrequency.Tuesday);
            Assert.True(arrivalWindow.PreviousDayDepartureFrequency.Monday);
        }
    }
}
