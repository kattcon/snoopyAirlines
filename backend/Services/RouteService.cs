using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Repositories;
using DomainRoute = SnoopyAirlines.Domain.Route;

namespace SnoopyAirlines.Services
{
    public class RouteService
    {
        private static readonly Guid FlightGuidNamespace = new("9e991ddc-7c58-4e29-a379-503fc594c1e2");
        private readonly IRouteRepository _routeRepository;

        public RouteService(IRouteRepository routeRepository)
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
            var directFlights = CreateFlights(routes, routeQuery);

            if (!routeQuery.IncludeStopovers)
            {
                return directFlights;
            }

            var connectingFlights = await CreateConnectingFlightsAsync(routeQuery, cancellationToken);

            return directFlights
                .Concat(connectingFlights)
                .OrderBy(f => f.DepartureTime)
                .ThenBy(f => f.FlightGUID, StringComparer.Ordinal)
                .ToList();
        }

        public Task SaveRouteAsync(DomainRoute route, CancellationToken cancellationToken)
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
                IncludeStopovers = routeQuery.IncludeStopovers,
                DepartureWindows = CreateDepartureWindows(routeQuery),
                ArrivalWindows = CreateArrivalWindows(routeQuery)
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

        private static IReadOnlyCollection<RouteArrivalWindow> CreateArrivalWindows(RouteQuery routeQuery)
        {
            if (routeQuery.EarliestArrival is null && routeQuery.LatestArrival is null)
            {
                return Array.Empty<RouteArrivalWindow>();
            }

            var startDate = routeQuery.EarliestArrival?.Date
                ?? routeQuery.EarliestDeparture?.Date
                ?? routeQuery.LatestArrival!.Value.Date;
            var endDate = routeQuery.LatestArrival?.Date
                ?? routeQuery.LatestDeparture?.Date.AddDays(1)
                ?? routeQuery.EarliestArrival!.Value.Date;

            if (endDate < startDate)
            {
                return Array.Empty<RouteArrivalWindow>();
            }

            var windows = new List<RouteArrivalWindow>();
            var currentDate = startDate;

            while (currentDate <= endDate)
            {
                windows.Add(new RouteArrivalWindow
                {
                    SameDayDepartureFrequency = CreateFrequency(currentDate.DayOfWeek),
                    PreviousDayDepartureFrequency = CreateFrequency(currentDate.AddDays(-1).DayOfWeek),
                    EarliestArrival = routeQuery.EarliestArrival is { } earliestArrival
                                      && currentDate == earliestArrival.Date
                        ? TimeOnly.FromDateTime(earliestArrival)
                        : TimeOnly.MinValue,
                    LatestArrival = routeQuery.LatestArrival is { } latestArrival
                                    && currentDate == latestArrival.Date
                        ? TimeOnly.FromDateTime(latestArrival)
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
                    var arrivalTime = CalculateArrivalTime(route, departureTime);

                    if (departureTime < earliestDeparture || departureTime > latestDeparture)
                    {
                        continue;
                    }

                    if (!IsWithinArrivalBounds(arrivalTime, routeQuery))
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
            return new FlightResponse
            {
                FlightGUID = CreateFlightGuid(route.Id, departureTime.Date),
                RouteId = route.Id,
                Routes =
                [
                    CreateFlightRouteResponse(1, route.Id, departureTime)
                ],
                DepartureTime = departureTime,
                ArrivalTime = CalculateArrivalTime(route, departureTime),
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

        private static DateTime CalculateArrivalTime(DomainRoute route, DateTime departureTime)
        {
            var arrivalDate = ArrivesNextDay(route)
                ? departureTime.Date.AddDays(1)
                : departureTime.Date;

            return arrivalDate.Add(route.ArrivalTime.ToTimeSpan());
        }

        private static bool ArrivesNextDay(DomainRoute route)
        {
            return route.ArrivalTime < route.DepartureTime;
        }

        private static bool IsWithinArrivalBounds(DateTime arrivalTime, RouteQuery routeQuery)
        {
            if (routeQuery.EarliestArrival is { } earliestArrival && arrivalTime < earliestArrival)
            {
                return false;
            }

            if (routeQuery.LatestArrival is { } latestArrival && arrivalTime > latestArrival)
            {
                return false;
            }

            return true;
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

        private static string CreateConnectingFlightGuid(int firstDefinitionId, int secondDefinitionId, DateTime departureDate)
        {
            var bytes = FlightGuidNamespace.ToByteArray();
            var idBytes1 = BitConverter.GetBytes(firstDefinitionId);
            var idBytes2 = BitConverter.GetBytes(secondDefinitionId);
            var dateNumber = departureDate.Year * 10000 + departureDate.Month * 100 + departureDate.Day;
            var dateBytes = BitConverter.GetBytes(dateNumber);

            // copy both ids and date into namespace bytes to create a unique GUID
            var offset = bytes.Length - idBytes1.Length - idBytes2.Length - dateBytes.Length;
            Array.Copy(idBytes1, 0, bytes, offset, idBytes1.Length);
            Array.Copy(idBytes2, 0, bytes, offset + idBytes1.Length, idBytes2.Length);
            Array.Copy(dateBytes, 0, bytes, bytes.Length - dateBytes.Length, dateBytes.Length);

            return new Guid(bytes).ToString();
        }

        private async Task<IReadOnlyCollection<FlightResponse>> CreateConnectingFlightsAsync(
            RouteQuery routeQuery,
            CancellationToken cancellationToken)
        {
            const int MinStopoverMinutes = 60;     // 1 hora
            const int MaxStopoverMinutes = 720;    // 12 horas

            var firstLegQuery = new RouteSearchQuery
            {
                Origin = routeQuery.Origin
            };

            var secondLegQuery = new RouteSearchQuery
            {
                Destination = routeQuery.Destination,
                ArrivalWindows = CreateArrivalWindows(routeQuery)
            };

            var firstDefs = await _routeRepository.GetRoutesAsync(firstLegQuery, cancellationToken);
            var secondDefs = await _routeRepository.GetRoutesAsync(secondLegQuery, cancellationToken);

            var results = new List<FlightResponse>();

            if (routeQuery.EarliestDeparture is null || routeQuery.LatestDeparture is null)
            {
                return results;
            }

            var earliestDeparture = routeQuery.EarliestDeparture.Value;
            var latestDeparture = routeQuery.LatestDeparture.Value;
            var currentDate = earliestDeparture.Date;
            var endDate = latestDeparture.Date;

            while (currentDate <= endDate)
            {
                foreach (var first in firstDefs)
                {
                    if (!OccursOn(first.Frequency, currentDate.DayOfWeek))
                        continue;

                    var dep1 = currentDate.Add(first.DepartureTime.ToTimeSpan());
                    var arr1 = first.ArrivalTime >= first.DepartureTime
                        ? currentDate.Add(first.ArrivalTime.ToTimeSpan())
                        : currentDate.AddDays(1).Add(first.ArrivalTime.ToTimeSpan());

                    if (dep1 < earliestDeparture || dep1 > latestDeparture)
                        continue;

                    var candidates = secondDefs.Where(s => s.DepartureAirport.Code == first.ArrivalAirport.Code);

                    foreach (var second in candidates)
                    {
                        for (int dayOffset = 0; dayOffset <= 1; dayOffset++)
                        {
                            var secondDate = arr1.Date.AddDays(dayOffset);
                            if (!OccursOn(second.Frequency, secondDate.DayOfWeek))
                                continue;

                            var dep2 = secondDate.Add(second.DepartureTime.ToTimeSpan());
                            var arr2 = second.ArrivalTime >= second.DepartureTime
                                ? secondDate.Add(second.ArrivalTime.ToTimeSpan())
                                : secondDate.AddDays(1).Add(second.ArrivalTime.ToTimeSpan());

                            var connectionMinutes = (int)(dep2 - arr1).TotalMinutes;
                            if (connectionMinutes < MinStopoverMinutes || connectionMinutes > MaxStopoverMinutes)
                                continue;

                            if (dep2 < earliestDeparture || dep2 > latestDeparture)
                                continue;

                            if (!IsWithinArrivalBounds(arr2, routeQuery))
                                continue;

                            var totalDurationMinutes = (int)(arr2 - dep1).TotalMinutes;

                            results.Add(new FlightResponse
                            {
                                FlightGUID = CreateConnectingFlightGuid(first.Id, second.Id, dep1.Date),
                                RouteId = first.Id,
                                Routes =
                                [
                                    CreateFlightRouteResponse(1, first.Id, dep1),
                                    CreateFlightRouteResponse(2, second.Id, dep2)
                                ],
                                DepartureTime = dep1,
                                ArrivalTime = arr2,
                                Duration = FormatDuration(totalDurationMinutes),
                                DepartureAirport = new AirportResponse
                                {
                                    Code = first.DepartureAirport.Code,
                                    Name = first.DepartureAirport.Name,
                                    City = first.DepartureAirport.City
                                },
                                ArrivalAirport = new AirportResponse
                                {
                                    Code = second.ArrivalAirport.Code,
                                    Name = second.ArrivalAirport.Name,
                                    City = second.ArrivalAirport.City
                                },
                                HasStopover = true,
                                StopoverAirport = new AirportResponse
                                {
                                    Code = first.ArrivalAirport.Code,
                                    Name = first.ArrivalAirport.Name,
                                    City = first.ArrivalAirport.City
                                },
                                StopoverDuration = FormatDuration(connectionMinutes),
                                TouristPrice = first.PriceEconomyClass + second.PriceEconomyClass,
                                FirstClassPrice = first.PriceFirstClass + second.PriceFirstClass,
                                CarryOnPrice = first.PriceCarryOnBaggage + second.PriceCarryOnBaggage,
                                CheckedPrice = first.PriceCheckedBaggage + second.PriceCheckedBaggage
                            });
                        }
                    }
                }

                currentDate = currentDate.AddDays(1);
            }

            return results;
        }

        private static FlightRouteResponse CreateFlightRouteResponse(
            int sequenceNumber,
            int routeId,
            DateTime departureTime)
        {
            return new FlightRouteResponse
            {
                SequenceNumber = sequenceNumber,
                RouteId = routeId,
                IntendedDate = DateOnly.FromDateTime(departureTime.Date)
            };
        }

        private static string FormatDuration(int durationMinutes)
        {
            var hours = durationMinutes / 60;
            var minutes = durationMinutes % 60;

            return $"{hours:00}:{minutes:00}";
        }
    }
}
