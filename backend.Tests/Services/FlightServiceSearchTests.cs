using Moq;
using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Repositories;
using SnoopyAirlines.Services;
using SnoopyAirlines.Services.PartnerAirlines;
using Xunit;
using DomainRoute = SnoopyAirlines.Domain.Route;

namespace SnoopyAirlines.Tests
{
    public class FlightServiceSearchTests
    {
        private readonly Mock<IRouteService> _routeService = new();
        private readonly Mock<IFlightRepository> _flightRepository = new();
        private readonly Mock<IExternalFlightSearchService> _externalFlightSearchService = new();
        private readonly FlightService _flightService;

        public FlightServiceSearchTests()
        {
            _flightRepository
                .Setup(r => r.MaterializeInternalFlightAsync(
                    It.IsAny<int>(),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.Parse("11111111-1111-1111-1111-111111111111"));

            _externalFlightSearchService
                .Setup(s => s.SearchConnectionsAsync(
                    It.IsAny<ExternalFlightSearchQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<ExternalFlight>());

            _flightService = new FlightService(
                _routeService.Object,
                _flightRepository.Object,
                _externalFlightSearchService.Object);
        }

        [Fact]
        public async Task Search_WithoutStopovers_ReturnsOnlyDirectFlights()
        {
            var query = new FlightQuery
            {
                Origin = "SJO",
                Destination = "MIA",
                EarliestDeparture = new DateTime(2026, 6, 15, 6, 0, 0),
                LatestDeparture = new DateTime(2026, 6, 15, 22, 0, 0),
                QuantityOfPassengers = 2,
                IncludeStopovers = false
            };

            var route = CreateRoute(1, "SJO", "MIA", new TimeOnly(8, 0), new TimeOnly(10, 30));
            SetupRoutes(_ => true, [route]);

            var results = await _flightService.Search(query, CancellationToken.None);

            Assert.NotEmpty(results);
            Assert.All(results, flight => Assert.False(HasStopover(flight)));
            Assert.True(results.All(f => DepartureAirportCode(f) == "SJO" && ArrivalAirportCode(f) == "MIA"));
        }

        [Fact]
        public async Task Search_WithStopoverLessThanOneHour_FiltersConnection()
        {
            var query = new FlightQuery
            {
                Origin = "SJO",
                Destination = "LAX",
                EarliestDeparture = new DateTime(2026, 6, 15, 6, 0, 0),
                LatestDeparture = new DateTime(2026, 6, 15, 22, 0, 0),
                QuantityOfPassengers = 1,
                IncludeStopovers = true
            };

            var firstLeg = CreateRoute(1, "SJO", "MIA", new TimeOnly(8, 0), new TimeOnly(10, 30));
            var secondLeg = CreateRoute(2, "MIA", "LAX", new TimeOnly(11, 0), new TimeOnly(12, 30));

            SetupRoutes(q => q.Origin == "SJO" && q.Destination == "LAX", []);
            SetupRoutes(q => q.Origin == "SJO" && q.Destination is null, [firstLeg]);
            SetupRoutes(q => q.Origin is null && q.Destination == "LAX", [secondLeg]);

            var results = await _flightService.Search(query, CancellationToken.None);

            Assert.DoesNotContain(results, flight =>
                HasStopover(flight)
                && StopoverAirportCode(flight) == "MIA"
                && StopoverDuration(flight) == "00:30");
        }

        [Fact]
        public async Task Search_WithStopoverMoreThanTwelveHours_FiltersConnection()
        {
            var query = new FlightQuery
            {
                Origin = "SJO",
                Destination = "CDG",
                EarliestDeparture = new DateTime(2026, 6, 15, 6, 0, 0),
                LatestDeparture = new DateTime(2026, 6, 16, 22, 0, 0),
                QuantityOfPassengers = 2,
                IncludeStopovers = true
            };

            var firstLeg = CreateRoute(3, "SJO", "BOG", new TimeOnly(8, 0), new TimeOnly(11, 0));
            var secondLeg = CreateRoute(4, "BOG", "CDG", new TimeOnly(23, 30), new TimeOnly(15, 0), 840);

            SetupRoutes(q => q.Origin == "SJO" && q.Destination == "CDG", []);
            SetupRoutes(q => q.Origin == "SJO" && q.Destination is null, [firstLeg]);
            SetupRoutes(q => q.Origin is null && q.Destination == "CDG", [secondLeg]);

            var results = await _flightService.Search(query, CancellationToken.None);

            Assert.DoesNotContain(results, flight =>
                HasStopover(flight)
                && StopoverAirportCode(flight) == "BOG"
                && StopoverDuration(flight) == "12:30");
        }

        [Fact]
        public async Task Search_WithNextDayStopoverWithinTwelveHours_ReturnsConnectingFlight()
        {
            var query = new FlightQuery
            {
                Origin = "SJO",
                Destination = "JFK",
                EarliestDeparture = new DateTime(2026, 6, 15, 20, 0, 0),
                LatestDeparture = new DateTime(2026, 6, 15, 23, 59, 0),
                QuantityOfPassengers = 1,
                IncludeStopovers = true
            };

            var firstLeg = CreateRoute(10, "SJO", "MIA", new TimeOnly(22, 30), new TimeOnly(1, 0));
            var secondLeg = CreateRoute(11, "MIA", "JFK", new TimeOnly(3, 30), new TimeOnly(6, 30));
            secondLeg.Frequency = new RouteFrequency { Tuesday = true };

            SetupRoutes(q => q.Origin == "SJO" && q.Destination == "JFK", []);
            SetupRoutes(q => q.Origin == "SJO" && q.Destination is null, [firstLeg]);
            SetupRoutes(q => q.Origin is null && q.Destination == "JFK", [secondLeg]);

            var results = await _flightService.Search(query, CancellationToken.None);

            var nextDayConnection = results.SingleOrDefault(f =>
                HasStopover(f)
                && DepartureAirportCode(f) == "SJO"
                && ArrivalAirportCode(f) == "JFK"
                && StopoverAirportCode(f) == "MIA");

            Assert.NotNull(nextDayConnection);
            Assert.Equal("02:30", StopoverDuration(nextDayConnection!));
            Assert.Equal(new DateTime(2026, 6, 15, 22, 30, 0), nextDayConnection!.Flights.First().DepartureTime);
            Assert.Equal(new DateTime(2026, 6, 16, 6, 30, 0), nextDayConnection.Flights.Last().ArrivalTime);
        }

        [Fact]
        public async Task Search_WithArrivalWindow_PassesArrivalWindowAndFiltersDirectFlights()
        {
            var query = new FlightQuery
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

            var matchingRoute = CreateRoute(5, "SJO", "MIA", new TimeOnly(8, 0), new TimeOnly(10, 30));
            var lateRoute = CreateRoute(6, "SJO", "MIA", new TimeOnly(9, 0), new TimeOnly(12, 30), 210);
            RouteSearchQuery? capturedQuery = null;

            _routeService
                .Setup(r => r.SearchRoutesAsync(It.IsAny<RouteSearchQuery>(), It.IsAny<CancellationToken>()))
                .Callback<RouteSearchQuery, CancellationToken>((routeSearchQuery, _) => capturedQuery = routeSearchQuery)
                .ReturnsAsync([matchingRoute, lateRoute]);

            var results = await _flightService.Search(query, CancellationToken.None);

            var flight = Assert.Single(results);
            Assert.Equal(matchingRoute.Id, flight.Flights.Single().RouteId);
            Assert.NotNull(capturedQuery);
            var arrivalWindow = Assert.Single(capturedQuery.ArrivalWindows);
            Assert.True(arrivalWindow.SameDayDepartureFrequency.Monday);
            Assert.True(arrivalWindow.PreviousDayDepartureFrequency.Sunday);
            Assert.Equal(new TimeOnly(10, 0), arrivalWindow.EarliestArrival);
            Assert.Equal(new TimeOnly(11, 0), arrivalWindow.LatestArrival);
        }

        [Fact]
        public async Task Search_WithOvernightArrivalWindow_UsesPreviousDayDepartureFrequency()
        {
            var query = new FlightQuery
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

            var overnightRoute = CreateRoute(7, "SJO", "MIA", new TimeOnly(22, 30), new TimeOnly(1, 15), 165);
            RouteSearchQuery? capturedQuery = null;

            _routeService
                .Setup(r => r.SearchRoutesAsync(It.IsAny<RouteSearchQuery>(), It.IsAny<CancellationToken>()))
                .Callback<RouteSearchQuery, CancellationToken>((routeSearchQuery, _) => capturedQuery = routeSearchQuery)
                .ReturnsAsync([overnightRoute]);

            var results = await _flightService.Search(query, CancellationToken.None);

            var flight = Assert.Single(results);
            Assert.Equal(new DateTime(2026, 6, 16, 1, 15, 0), flight.Flights.Last().ArrivalTime);
            Assert.NotNull(capturedQuery);
            var arrivalWindow = Assert.Single(capturedQuery.ArrivalWindows);
            Assert.True(arrivalWindow.SameDayDepartureFrequency.Tuesday);
            Assert.True(arrivalWindow.PreviousDayDepartureFrequency.Monday);
        }

        private static bool HasStopover(FlightResponse flight)
        {
            return flight.Flights.Count > 1;
        }

        private static string DepartureAirportCode(FlightResponse flight)
        {
            return flight.Flights.First().DepartureAirport.Code;
        }

        private static string ArrivalAirportCode(FlightResponse flight)
        {
            return flight.Flights.Last().ArrivalAirport.Code;
        }

        private static string? StopoverAirportCode(FlightResponse flight)
        {
            return HasStopover(flight) ? flight.Flights.First().ArrivalAirport.Code : null;
        }

        private static string? StopoverDuration(FlightResponse flight)
        {
            if (!HasStopover(flight))
            {
                return null;
            }

            var orderedLegs = flight.Flights.OrderBy(leg => leg.SequenceNumber).ToArray();
            var stopover = orderedLegs[1].DepartureTime - orderedLegs[0].ArrivalTime;
            return stopover.ToString(@"hh\:mm");
        }

        private void SetupRoutes(
            Func<RouteSearchQuery, bool> predicate,
            IReadOnlyCollection<DomainRoute> routes)
        {
            _routeService
                .Setup(r => r.SearchRoutesAsync(
                    It.Is<RouteSearchQuery>(q => predicate(q)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(routes);
        }

        private static DomainRoute CreateRoute(
            int id,
            string origin,
            string destination,
            TimeOnly departureTime,
            TimeOnly arrivalTime,
            int durationMinutes = 150)
        {
            return new DomainRoute
            {
                Id = id,
                DepartureAirport = new RouteAirport { Code = origin, Name = origin, City = origin },
                ArrivalAirport = new RouteAirport { Code = destination, Name = destination, City = destination },
                DepartureTime = departureTime,
                ArrivalTime = arrivalTime,
                DurationMinutes = durationMinutes,
                Frequency = new RouteFrequency
                {
                    Monday = true,
                    Tuesday = true,
                    Wednesday = true,
                    Thursday = true,
                    Friday = true,
                    Saturday = true,
                    Sunday = true
                },
                PriceEconomyClass = 299,
                PriceFirstClass = 599,
                PriceCarryOnBaggage = 0,
                PriceCheckedBaggage = 25
            };
        }
    }
}
