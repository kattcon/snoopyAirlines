using SnoopyAirlines.External.Domain;
using SnoopyAirlines.External.Domain.View;
using SnoopyAirlines.External.Repositories;
using DomainRoute = SnoopyAirlines.External.Domain.Route;

namespace SnoopyAirlines.External.Services
{
    public class RouteService
    {
        private static readonly Guid FlightGuidNamespace = new("9e991ddc-7c58-4e29-a379-503fc594c1e2");

        private readonly RouteRepository _routeRepository;

        public RouteService(RouteRepository routeRepository)
        {
            _routeRepository = routeRepository;
        }

        public async Task<IReadOnlyCollection<Flight>> GetFlights(
            RouteQuery routeQuery,
            CancellationToken cancellationToken = default)
        {
            var repositoryQuery = ToRouteSearchQuery(routeQuery);
            var routes = await _routeRepository.GetRoutes(
                repositoryQuery,
                cancellationToken);

            return CreateFlights(routes, routeQuery);
        }

        public async Task<FlightsResponse> Get(
            RouteQuery routeQuery,
            CancellationToken cancellationToken = default)
        {
            var flights = await GetFlights(routeQuery, cancellationToken);

            return new FlightsResponse
            {
                Flights = flights
            };
        }

        private static RouteSearchQuery ToRouteSearchQuery(RouteQuery routeQuery)
        {
            return new RouteSearchQuery
            {
                Destination = routeQuery.Destination,
                QuantityOfPassengers = routeQuery.QuantityOfPassengers,
                ArrivalWindows = CreateArrivalWindows(routeQuery)
            };
        }

        private static IReadOnlyCollection<RouteArrivalWindow> CreateArrivalWindows(RouteQuery routeQuery)
        {
            if (routeQuery.EarliestArrival is null || routeQuery.LatestArrival is null)
            {
                return Array.Empty<RouteArrivalWindow>();
            }

            var earliestArrival = routeQuery.EarliestArrival.Value;
            var latestArrival = routeQuery.LatestArrival.Value;

            var windows = new List<RouteArrivalWindow>();
            var currentDate = earliestArrival.Date;
            var endDate = latestArrival.Date;

            while (currentDate <= endDate)
            {
                windows.Add(new RouteArrivalWindow
                {
                    SameDayDepartureFrequency = CreateFrequency(currentDate.DayOfWeek),
                    PreviousDayDepartureFrequency = CreateFrequency(currentDate.AddDays(-1).DayOfWeek),
                    EarliestArrival = currentDate == earliestArrival.Date
                        ? TimeOnly.FromDateTime(earliestArrival)
                        : TimeOnly.MinValue,
                    LatestArrival = currentDate == latestArrival.Date
                        ? TimeOnly.FromDateTime(latestArrival)
                        : new TimeOnly(23, 59, 59)
                });

                currentDate = currentDate.AddDays(1);
            }

            return windows;
        }

        private static IReadOnlyCollection<Flight> CreateFlights(
            IReadOnlyCollection<DomainRoute> routes,
            RouteQuery routeQuery)
        {
            if (routeQuery.EarliestArrival is null || routeQuery.LatestArrival is null)
            {
                return Array.Empty<Flight>();
            }

            var flights = new List<Flight>();
            var earliestArrival = routeQuery.EarliestArrival.Value;
            var latestArrival = routeQuery.LatestArrival.Value;
            var currentDate = earliestArrival.Date;
            var endDate = latestArrival.Date;

            while (currentDate <= endDate)
            {
                foreach (var route in routes)
                {
                    var departureDate = ArrivesNextDay(route)
                        ? currentDate.AddDays(-1)
                        : currentDate;

                    if (!OccursOn(route.Frequency, departureDate.DayOfWeek))
                    {
                        continue;
                    }

                    var arrivalTime = currentDate.Add(route.ArrivalTime.ToTimeSpan());

                    if (arrivalTime < earliestArrival || arrivalTime > latestArrival)
                    {
                        continue;
                    }

                    var departureTime = departureDate.Add(route.DepartureTime.ToTimeSpan());
                    flights.Add(CreateFlight(route, departureTime));
                }

                currentDate = currentDate.AddDays(1);
            }

            return flights
                .OrderBy(flight => flight.ArrivalTime)
                .ThenBy(flight => flight.DepartureTime)
                .ThenBy(flight => flight.FlightGUID, StringComparer.Ordinal)
                .ToList();
        }

        private static Flight CreateFlight(DomainRoute route, DateTime departureTime)
        {
            var arrivalDate = ArrivesNextDay(route)
                ? departureTime.Date.AddDays(1)
                : departureTime.Date;
            var arrivalTime = arrivalDate.Add(route.ArrivalTime.ToTimeSpan());

            return new Flight
            {
                FlightGUID = CreateFlightGuid(route.Id, departureTime.Date),
                RouteId = route.Id,
                DepartureTime = departureTime,
                ArrivalTime = arrivalTime,
                Duration = FormatDuration(route.DurationMinutes),
                DepartureAirport = route.DepartureAirport,
                ArrivalAirport = route.ArrivalAirport,
                TouristPrice = route.TouristPrice,
                FirstClassPrice = route.FirstClassPrice,
                CarryOnPrice = route.CarryOnPrice,
                CheckedPrice = route.CheckedPrice
            };
        }

        private static bool ArrivesNextDay(DomainRoute route)
        {
            return route.ArrivalTime < route.DepartureTime;
        }

        private static bool OccursOn(RouteFrequency frequency, DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => frequency.Monday,
                DayOfWeek.Tuesday => frequency.Tuesday,
                DayOfWeek.Wednesday => frequency.Wednesday,
                DayOfWeek.Thursday => frequency.Thursday,
                DayOfWeek.Friday => frequency.Friday,
                DayOfWeek.Saturday => frequency.Saturday,
                DayOfWeek.Sunday => frequency.Sunday,
                _ => false
            };
        }

        private static RouteFrequency CreateFrequency(DayOfWeek dayOfWeek)
        {
            return new RouteFrequency
            {
                Monday = dayOfWeek == DayOfWeek.Monday,
                Tuesday = dayOfWeek == DayOfWeek.Tuesday,
                Wednesday = dayOfWeek == DayOfWeek.Wednesday,
                Thursday = dayOfWeek == DayOfWeek.Thursday,
                Friday = dayOfWeek == DayOfWeek.Friday,
                Saturday = dayOfWeek == DayOfWeek.Saturday,
                Sunday = dayOfWeek == DayOfWeek.Sunday
            };
        }

        private static string CreateFlightGuid(int routeId, DateTime departureDate)
        {
            var bytes = FlightGuidNamespace.ToByteArray();
            var idBytes = BitConverter.GetBytes(routeId);
            var dateNumber = departureDate.Year * 10000 + departureDate.Month * 100 + departureDate.Day;
            var dateBytes = BitConverter.GetBytes(dateNumber);

            Array.Copy(idBytes, 0, bytes, bytes.Length - idBytes.Length - dateBytes.Length, idBytes.Length);
            Array.Copy(dateBytes, 0, bytes, bytes.Length - dateBytes.Length, dateBytes.Length);

            return new Guid(bytes).ToString();
        }

        private static string FormatDuration(int durationMinutes)
        {
            var hours = durationMinutes / 60;
            var minutes = durationMinutes % 60;

            return $"{hours:00}:{minutes:00}";
        }
    }
}
