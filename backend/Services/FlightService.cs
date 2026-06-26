using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Repositories;
using DomainRoute = SnoopyAirlines.Domain.Route;

namespace SnoopyAirlines.Services
{
    public class FlightService : IFlightService
    {
        private readonly IRouteService _routeService;
        private readonly IFlightRepository _flightRepository;

        public FlightService(
            IRouteService routeService,
            IFlightRepository flightRepository)
        {
            _routeService = routeService;
            _flightRepository = flightRepository;
        }

        public async Task<IReadOnlyCollection<FlightResponse>> Search(
            FlightQuery flightQuery,
            CancellationToken cancellationToken)
        {
            var repositoryQuery = ToRouteSearchQuery(flightQuery);
            var routes = await _routeService.SearchRoutesAsync(repositoryQuery, cancellationToken);
            var directFlights = await CreateFlightsAsync(routes, flightQuery, cancellationToken);

            if (!flightQuery.IncludeStopovers)
            {
                return directFlights;
            }

            var connectingFlights = await CreateConnectingFlightsAsync(flightQuery, cancellationToken);

            return directFlights
                .Concat(connectingFlights)
                .OrderBy(flight => flight.DepartureTime)
                .ThenBy(flight => flight.RouteId)
                .ToList();
        }

        private static RouteSearchQuery ToRouteSearchQuery(FlightQuery flightQuery)
        {
            return new RouteSearchQuery
            {
                Origin = flightQuery.Origin,
                Destination = flightQuery.Destination,
                QuantityOfPassengers = flightQuery.QuantityOfPassengers,
                IncludeStopovers = flightQuery.IncludeStopovers,
                DepartureWindows = CreateDepartureWindows(flightQuery),
                ArrivalWindows = CreateArrivalWindows(flightQuery)
            };
        }

        private static IReadOnlyCollection<RouteDepartureWindow> CreateDepartureWindows(FlightQuery flightQuery)
        {
            if (flightQuery.EarliestDeparture is null || flightQuery.LatestDeparture is null)
            {
                return Array.Empty<RouteDepartureWindow>();
            }

            var earliestDeparture = flightQuery.EarliestDeparture.Value;
            var latestDeparture = flightQuery.LatestDeparture.Value;
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

        private static IReadOnlyCollection<RouteArrivalWindow> CreateArrivalWindows(FlightQuery flightQuery)
        {
            if (flightQuery.EarliestArrival is null && flightQuery.LatestArrival is null)
            {
                return Array.Empty<RouteArrivalWindow>();
            }

            var startDate = flightQuery.EarliestArrival?.Date
                ?? flightQuery.EarliestDeparture?.Date
                ?? flightQuery.LatestArrival!.Value.Date;
            var endDate = flightQuery.LatestArrival?.Date
                ?? flightQuery.LatestDeparture?.Date.AddDays(1)
                ?? flightQuery.EarliestArrival!.Value.Date;

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
                    EarliestArrival = flightQuery.EarliestArrival is { } earliestArrival
                                      && currentDate == earliestArrival.Date
                        ? TimeOnly.FromDateTime(earliestArrival)
                        : TimeOnly.MinValue,
                    LatestArrival = flightQuery.LatestArrival is { } latestArrival
                                    && currentDate == latestArrival.Date
                        ? TimeOnly.FromDateTime(latestArrival)
                        : new TimeOnly(23, 59, 59)
                });

                currentDate = currentDate.AddDays(1);
            }

            return windows;
        }

        private async Task<IReadOnlyCollection<FlightResponse>> CreateFlightsAsync(
            IReadOnlyCollection<DomainRoute> routes,
            FlightQuery flightQuery,
            CancellationToken cancellationToken)
        {
            if (flightQuery.EarliestDeparture is null || flightQuery.LatestDeparture is null)
            {
                return Array.Empty<FlightResponse>();
            }

            var flights = new List<FlightResponse>();
            var earliestDeparture = flightQuery.EarliestDeparture.Value;
            var latestDeparture = flightQuery.LatestDeparture.Value;
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

                    if (!IsWithinArrivalBounds(arrivalTime, flightQuery))
                    {
                        continue;
                    }

                    var flightGuid = await _flightRepository.MaterializeInternalFlightAsync(
                        route.Id,
                        departureTime,
                        cancellationToken);

                    flights.Add(CreateFlightResponse(route, departureTime, flightGuid));
                }

                currentDate = currentDate.AddDays(1);
            }

            return flights
                .OrderBy(flight => flight.DepartureTime)
                .ThenBy(flight => flight.RouteId)
                .ToList();
        }

        private static FlightResponse CreateFlightResponse(
            DomainRoute route,
            DateTime departureTime,
            Guid flightGuid)
        {
            return new FlightResponse
            {
                RouteId = route.Id,
                Routes =
                [
                    CreateFlightRouteResponse(1, route.Id, departureTime, flightGuid)
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

        private static bool IsWithinArrivalBounds(DateTime arrivalTime, FlightQuery flightQuery)
        {
            if (flightQuery.EarliestArrival is { } earliestArrival && arrivalTime < earliestArrival)
            {
                return false;
            }

            if (flightQuery.LatestArrival is { } latestArrival && arrivalTime > latestArrival)
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

        private async Task<IReadOnlyCollection<FlightResponse>> CreateConnectingFlightsAsync(
            FlightQuery flightQuery,
            CancellationToken cancellationToken)
        {
            const int MinStopoverMinutes = 60;
            const int MaxStopoverMinutes = 720;

            var firstLegQuery = new RouteSearchQuery
            {
                Origin = flightQuery.Origin
            };

            var secondLegQuery = new RouteSearchQuery
            {
                Destination = flightQuery.Destination,
                ArrivalWindows = CreateArrivalWindows(flightQuery)
            };

            var firstDefs = await _routeService.SearchRoutesAsync(firstLegQuery, cancellationToken);
            var secondDefs = await _routeService.SearchRoutesAsync(secondLegQuery, cancellationToken);

            var results = new List<FlightResponse>();

            if (flightQuery.EarliestDeparture is null || flightQuery.LatestDeparture is null)
            {
                return results;
            }

            var earliestDeparture = flightQuery.EarliestDeparture.Value;
            var latestDeparture = flightQuery.LatestDeparture.Value;
            var currentDate = earliestDeparture.Date;
            var endDate = latestDeparture.Date;

            while (currentDate <= endDate)
            {
                foreach (var first in firstDefs)
                {
                    if (!OccursOn(first.Frequency, currentDate.DayOfWeek))
                    {
                        continue;
                    }

                    var dep1 = currentDate.Add(first.DepartureTime.ToTimeSpan());
                    var arr1 = CalculateArrivalTime(first, dep1);

                    if (dep1 < earliestDeparture || dep1 > latestDeparture)
                    {
                        continue;
                    }

                    var candidates = secondDefs.Where(
                        second => second.DepartureAirport!.Code == first.ArrivalAirport!.Code);

                    foreach (var second in candidates)
                    {
                        for (var dayOffset = 0; dayOffset <= 1; dayOffset++)
                        {
                            var secondDate = arr1.Date.AddDays(dayOffset);
                            if (!OccursOn(second.Frequency, secondDate.DayOfWeek))
                            {
                                continue;
                            }

                            var dep2 = secondDate.Add(second.DepartureTime.ToTimeSpan());
                            var arr2 = CalculateArrivalTime(second, dep2);

                            if (!IsWithinArrivalBounds(arr2, flightQuery))
                            {
                                continue;
                            }

                            var connectionMinutes = (int)(dep2 - arr1).TotalMinutes;
                            if (connectionMinutes < MinStopoverMinutes || connectionMinutes > MaxStopoverMinutes)
                            {
                                continue;
                            }

                            var firstFlightGuid = await _flightRepository.MaterializeInternalFlightAsync(
                                first.Id,
                                dep1,
                                cancellationToken);
                            var secondFlightGuid = await _flightRepository.MaterializeInternalFlightAsync(
                                second.Id,
                                dep2,
                                cancellationToken);
                            var totalDurationMinutes = (int)(arr2 - dep1).TotalMinutes;

                            results.Add(new FlightResponse
                            {
                                RouteId = first.Id,
                                Routes =
                                [
                                    CreateFlightRouteResponse(1, first.Id, dep1, firstFlightGuid),
                                    CreateFlightRouteResponse(2, second.Id, dep2, secondFlightGuid)
                                ],
                                DepartureTime = dep1,
                                ArrivalTime = arr2,
                                Duration = FormatDuration(totalDurationMinutes),
                                DepartureAirport = new AirportResponse
                                {
                                    Code = first.DepartureAirport!.Code,
                                    Name = first.DepartureAirport.Name,
                                    City = first.DepartureAirport.City
                                },
                                ArrivalAirport = new AirportResponse
                                {
                                    Code = second.ArrivalAirport!.Code,
                                    Name = second.ArrivalAirport.Name,
                                    City = second.ArrivalAirport.City
                                },
                                HasStopover = true,
                                StopoverAirport = new AirportResponse
                                {
                                    Code = first.ArrivalAirport!.Code,
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
            DateTime departureTime,
            Guid flightGuid)
        {
            return new FlightRouteResponse
            {
                SequenceNumber = sequenceNumber,
                RouteId = routeId,
                FlightGuid = flightGuid.ToString(),
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
