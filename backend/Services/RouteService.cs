using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Repositories;
using DomainRoute = SnoopyAirlines.Domain.Route;

namespace SnoopyAirlines.Services
{
    public class RouteService
    {
        private static readonly Guid FlightGuidNamespace = new("9e991ddc-7c58-4e29-a379-503fc594c1e2");
        private readonly RouteRepository _routeRepository;

        public RouteService(RouteRepository routeRepository)
        {
            _routeRepository = routeRepository;
        }

        public Task<IReadOnlyCollection<DomainRoute>> GetRoutesAsync(CancellationToken cancellationToken)
        {
            return _routeRepository.GetAllAsync(cancellationToken);
        }

        public Task<DomainRoute?> GetRouteByIdAsync(int routeId, CancellationToken cancellationToken)
        {
            return _routeRepository.GetByIdAsync(routeId, cancellationToken);
        }

        public Task<IReadOnlyCollection<DomainRoute>> SearchRoutesAsync(
            int? departureAirportId,
            int? arrivalAirportId,
            DateOnly? departureDate,
            CancellationToken cancellationToken)
        {
            return _routeRepository.SearchAsync(departureAirportId, arrivalAirportId, departureDate, cancellationToken);
        }

        public async Task<IReadOnlyCollection<FlightResponse>> SearchFlightsAsync(
            RouteQuery routeQuery,
            CancellationToken cancellationToken)
        {
            var repositoryQuery = ToRouteSearchQuery(routeQuery);
            var routes = await _routeRepository.GetRoutesAsync(repositoryQuery, cancellationToken);
            return CreateFlights(routes, routeQuery);
        }

        public Task<DomainRoute> SaveRouteAsync(DomainRoute route, CancellationToken cancellationToken)
        {
            return _routeRepository.SaveAsync(route, cancellationToken);
        }

        private static RouteSearchQuery ToRouteSearchQuery(RouteQuery routeQuery)
        {
            return new RouteSearchQuery
            {
                Origin = routeQuery.Origin,
                Destination = routeQuery.Destination,
                QuantityOfPassengers = routeQuery.QuantityOfPassengers,
                DepartureWindows = CreateDepartureWindows(routeQuery)
            };
        }

        private static IReadOnlyCollection<RouteDepartureWindow> CreateDepartureWindows(RouteQuery routeQuery)
        {
            if (routeQuery.EarliestDeparture is null || routeQuery.LatestDeparture is null)
            {
                return Array.Empty<RouteDepartureWindow>();
            }

            var earliestDeparture = routeQuery.EarliestDeparture.Value;
            var latestDeparture = routeQuery.LatestDeparture.Value;
            var windows = new List<RouteDepartureWindow>();
            var currentDate = earliestDeparture.Date;
            var endDate = latestDeparture.Date;

            while (currentDate <= endDate)
            {
                windows.Add(new RouteDepartureWindow
                {
                    Frequency = CreateFrequency(currentDate.DayOfWeek),
                    EarliestDeparture = currentDate == earliestDeparture.Date
                        ? TimeOnly.FromDateTime(earliestDeparture)
                        : TimeOnly.MinValue,
                    LatestDeparture = currentDate == latestDeparture.Date
                        ? TimeOnly.FromDateTime(latestDeparture)
                        : new TimeOnly(23, 59, 59)
                });

                currentDate = currentDate.AddDays(1);
            }

            return windows;
        }

        private static IReadOnlyCollection<FlightResponse> CreateFlights(
            IReadOnlyCollection<DomainRoute> routes,
            RouteQuery routeQuery)
        {
            if (routeQuery.EarliestDeparture is null || routeQuery.LatestDeparture is null)
            {
                return Array.Empty<FlightResponse>();
            }

            var flights = new List<FlightResponse>();
            var earliestDeparture = routeQuery.EarliestDeparture.Value;
            var latestDeparture = routeQuery.LatestDeparture.Value;
            var currentDate = earliestDeparture.Date;
            var endDate = latestDeparture.Date;

            while (currentDate <= endDate)
            {
                foreach (var route in routes)
                {
                    if (!OccursOn(route.Frequency, currentDate.DayOfWeek))
                    {
                        continue;
                    }

                    var departureTime = currentDate.Add(route.DepartureTime.ToTimeSpan());

                    if (departureTime < earliestDeparture || departureTime > latestDeparture)
                    {
                        continue;
                    }

                    flights.Add(CreateFlightResponse(route, departureTime));
                }

                currentDate = currentDate.AddDays(1);
            }

            return flights
                .OrderBy(flight => flight.DepartureTime)
                .ThenBy(flight => flight.FlightGUID, StringComparer.Ordinal)
                .ToList();
        }

        private static FlightResponse CreateFlightResponse(DomainRoute route, DateTime departureTime)
        {
            var arrivalDate = route.ArrivalTime >= route.DepartureTime
                ? departureTime.Date
                : departureTime.Date.AddDays(1);

            return new FlightResponse
            {
                FlightGUID = CreateFlightGuid(route.Id, departureTime.Date),
                RouteId = route.Id,
                DepartureTime = departureTime,
                ArrivalTime = arrivalDate.Add(route.ArrivalTime.ToTimeSpan()),
                Duration = FormatDuration(route.DurationMinutes),
                DepartureAirport = new AirportResponse
                {
                    Code = route.DepartureAirport!.Code,
                    Name = route.DepartureAirport.Name,
                    City = route.DepartureAirport.City
                },
                ArrivalAirport = new AirportResponse
                {
                    Code = route.ArrivalAirport!.Code,
                    Name = route.ArrivalAirport.Name,
                    City = route.ArrivalAirport.City
                },
                TouristPrice = route.PriceEconomyClass,
                FirstClassPrice = route.PriceFirstClass,
                CarryOnPrice = route.PriceCarryOnBaggage,
                CheckedPrice = route.PriceCheckedBaggage
            };
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
