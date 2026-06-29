using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Repositories;
using SnoopyAirlines.Services.PartnerAirlines;
using DomainRoute = SnoopyAirlines.Domain.Route;

namespace SnoopyAirlines.Services
{
    public class FlightService : IFlightService
    {
        private const int MinStopoverMinutes = 60;
        private const int MaxStopoverMinutes = 720;

        private readonly IRouteService _routeService;
        private readonly IFlightRepository _flightRepository;
        private readonly IExternalFlightSearchService _externalFlightSearchService;

        public FlightService(
            IRouteService routeService,
            IFlightRepository flightRepository,
            IExternalFlightSearchService externalFlightSearchService)
        {
            _routeService = routeService;
            _flightRepository = flightRepository;
            _externalFlightSearchService = externalFlightSearchService;
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

            var connectionOriginOptions = await CreateConnectionOriginOptionsAsync(
                flightQuery,
                cancellationToken);
            var connectingFlights = await CreateConnectingFlightsAsync(
                flightQuery,
                connectionOriginOptions,
                cancellationToken);
            var externalFlights = await _externalFlightSearchService.SearchConnectionsAsync(
                new ExternalFlightSearchQuery
                {
                    Destination = flightQuery.Destination ?? string.Empty,
                    QuantityOfPassengers = flightQuery.QuantityOfPassengers ?? 1,
                    MaxTimeWindow = TimeSpan.FromMinutes(MaxStopoverMinutes - MinStopoverMinutes),
                    Legs = connectionOriginOptions
                        .Select(option => option.ExternalSearchLeg)
                        .Distinct()
                        .ToList()
                },
                cancellationToken);
            var externalConnectingFlights = await CreateExternalConnectingFlightsAsync(
                flightQuery,
                connectionOriginOptions,
                externalFlights,
                cancellationToken);

            return directFlights
                .Concat(connectingFlights)
                .Concat(externalConnectingFlights)
                .OrderBy(flight => flight.DepartureTime)
                .ThenBy(flight => flight.RouteId)
                .ToList();
        }

        public async Task<FlightSearchResult> GetFlightReportByConfirmationAsync(
            string confirmationNumber,
            string lastNames,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(confirmationNumber))
            {
                throw new ArgumentException("El número de reservación es obligatorio.", nameof(confirmationNumber));
            }

            if (string.IsNullOrWhiteSpace(lastNames))
            {
                throw new ArgumentException("Los apellidos son obligatorios.", nameof(lastNames));
            }

            var flightReport = await _flightRepository.GetFlightReportByConfirmationAsync(
                confirmationNumber,
                lastNames,
                cancellationToken);

            if (flightReport.Legs.Count == 0)
            {
                throw new InvalidOperationException("No se encontró ningún reporte de vuelo para el número de reservación y apellidos proporcionados.");
            }

            return flightReport;
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

        private async Task<IReadOnlyCollection<ExternalFlightOriginOption>> CreateConnectionOriginOptionsAsync(
            FlightQuery flightQuery,
            CancellationToken cancellationToken)
        {
            var firstLegQuery = new RouteSearchQuery
            {
                Origin = flightQuery.Origin,
                QuantityOfPassengers = flightQuery.QuantityOfPassengers
            };

            var firstDefs = await _routeService.SearchRoutesAsync(firstLegQuery, cancellationToken);
            var results = new List<ExternalFlightOriginOption>();

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
                    if (!OccursOn(first.Frequency, currentDate.DayOfWeek)
                        || first.DepartureAirport is null
                        || first.ArrivalAirport is null)
                    {
                        continue;
                    }

                    if (string.Equals(
                            first.ArrivalAirport.Code,
                            flightQuery.Destination,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var dep1 = currentDate.Add(first.DepartureTime.ToTimeSpan());
                    var arr1 = CalculateArrivalTime(first, dep1);

                    if (dep1 < earliestDeparture || dep1 > latestDeparture)
                    {
                        continue;
                    }

                    results.Add(new ExternalFlightOriginOption
                    {
                        Origin = first.ArrivalAirport.Code,
                        ExternalSearchLeg = new ExternalFlightSearchQueryLeg(
                            first.ArrivalAirport.Code,
                            arr1.AddMinutes(MinStopoverMinutes)),
                        FirstLeg = first,
                        FirstDeparture = dep1,
                        FirstArrival = arr1,
                        EarliestDeparture = arr1.AddMinutes(MinStopoverMinutes),
                        LatestDeparture = arr1.AddMinutes(MaxStopoverMinutes)
                    });
                }

                currentDate = currentDate.AddDays(1);
            }

            return results;
        }

        private async Task<IReadOnlyCollection<FlightResponse>> CreateExternalConnectingFlightsAsync(
            FlightQuery flightQuery,
            IReadOnlyCollection<ExternalFlightOriginOption> originOptions,
            IReadOnlyCollection<ExternalFlight> externalFlights,
            CancellationToken cancellationToken)
        {
            var externalFlightsByLeg = externalFlights
                .GroupBy(
                    flight => flight.DepartureAirport.Code,
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToList(),
                    StringComparer.OrdinalIgnoreCase);
            var results = new List<FlightResponse>();

            foreach (var originOption in originOptions)
            {
                if (!externalFlightsByLeg.TryGetValue(originOption.Origin, out var candidates))
                {
                    continue;
                }

                var first = originOption.FirstLeg;
                foreach (var candidate in candidates)
                {
                    if (candidate.DepartureAt < originOption.EarliestDeparture
                        || candidate.DepartureAt > originOption.LatestDeparture
                        || !IsWithinArrivalBounds(candidate.ArrivalAt, flightQuery))
                    {
                        continue;
                    }

                    var firstFlightGuid = await _flightRepository.MaterializeInternalFlightAsync(
                        first.Id,
                        originOption.FirstDeparture,
                        cancellationToken);
                    var firstRoute = CreateFlightRouteResponse(
                        1,
                        first.Id,
                        originOption.FirstDeparture,
                        firstFlightGuid);
                    var secondRoute = new FlightRouteResponse
                    {
                        SequenceNumber = 2,
                        RouteId = 0,
                        FlightGuid = candidate.Guid.ToString(),
                        IntendedDate = DateOnly.FromDateTime(candidate.DepartureAt.Date)
                    };
                    var connectionMinutes = (int)(candidate.DepartureAt - originOption.FirstArrival).TotalMinutes;
                    var totalDurationMinutes = (int)(candidate.ArrivalAt - originOption.FirstDeparture).TotalMinutes;

                    results.Add(new FlightResponse
                    {
                        RouteId = first.Id,
                        Routes =
                        [
                            firstRoute,
                            secondRoute
                        ],
                        DepartureTime = originOption.FirstDeparture,
                        ArrivalTime = candidate.ArrivalAt,
                        Duration = FormatDuration(totalDurationMinutes),
                        DepartureAirport = new AirportResponse
                        {
                            Code = first.DepartureAirport!.Code,
                            Name = first.DepartureAirport.Name,
                            City = first.DepartureAirport.City
                        },
                        ArrivalAirport = ToAirportResponse(candidate.ArrivalAirport),
                        HasStopover = true,
                        StopoverAirport = new AirportResponse
                        {
                            Code = first.ArrivalAirport!.Code,
                            Name = first.ArrivalAirport.Name,
                            City = first.ArrivalAirport.City
                        },
                        StopoverDuration = FormatDuration(connectionMinutes),
                        TouristPrice = first.PriceEconomyClass + candidate.TouristPrice,
                        FirstClassPrice = first.PriceFirstClass + candidate.FirstClassPrice,
                        CarryOnPrice = first.PriceCarryOnBaggage + candidate.CarryOnPrice,
                        CheckedPrice = first.PriceCheckedBaggage + candidate.CheckedPrice
                    });
                }
            }

            return results;
        }

        private async Task<IReadOnlyCollection<FlightResponse>> CreateConnectingFlightsAsync(
            FlightQuery flightQuery,
            IReadOnlyCollection<ExternalFlightOriginOption> originOptions,
            CancellationToken cancellationToken)
        {
            var secondLegQuery = new RouteSearchQuery
            {
                Destination = flightQuery.Destination,
                ArrivalWindows = CreateArrivalWindows(flightQuery),
                QuantityOfPassengers = flightQuery.QuantityOfPassengers
            };

            var secondDefs = await _routeService.SearchRoutesAsync(secondLegQuery, cancellationToken);
            var results = new List<FlightResponse>();

            foreach (var originOption in originOptions)
            {
                var first = originOption.FirstLeg;
                var candidates = secondDefs.Where(
                    second => string.Equals(
                        second.DepartureAirport?.Code,
                        originOption.Origin,
                        StringComparison.OrdinalIgnoreCase));

                foreach (var second in candidates)
                {
                    for (var dayOffset = 0; dayOffset <= 1; dayOffset++)
                    {
                        var secondDate = originOption.FirstArrival.Date.AddDays(dayOffset);
                        if (!OccursOn(second.Frequency, secondDate.DayOfWeek))
                        {
                            continue;
                        }

                        var dep2 = secondDate.Add(second.DepartureTime.ToTimeSpan());
                        var arr2 = CalculateArrivalTime(second, dep2);

                        if (dep2 < originOption.EarliestDeparture || dep2 > originOption.LatestDeparture)
                        {
                            continue;
                        }

                        if (!IsWithinArrivalBounds(arr2, flightQuery))
                        {
                            continue;
                        }

                        var connectionMinutes = (int)(dep2 - originOption.FirstArrival).TotalMinutes;
                        var firstFlightGuid = await _flightRepository.MaterializeInternalFlightAsync(
                            first.Id,
                            originOption.FirstDeparture,
                            cancellationToken);
                        var secondFlightGuid = await _flightRepository.MaterializeInternalFlightAsync(
                            second.Id,
                            dep2,
                            cancellationToken);
                        var totalDurationMinutes = (int)(arr2 - originOption.FirstDeparture).TotalMinutes;

                        results.Add(new FlightResponse
                        {
                            RouteId = first.Id,
                            Routes =
                            [
                                CreateFlightRouteResponse(1, first.Id, originOption.FirstDeparture, firstFlightGuid),
                                CreateFlightRouteResponse(2, second.Id, dep2, secondFlightGuid)
                            ],
                            DepartureTime = originOption.FirstDeparture,
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

        private static AirportResponse ToAirportResponse(FlightAirport airport)
        {
            return new AirportResponse
            {
                Code = airport.Code,
                Name = airport.Name,
                City = airport.City
            };
        }

        private class ExternalFlightOriginOption
        {
            required public string Origin { get; set; }
            public ExternalFlightSearchQueryLeg ExternalSearchLeg { get; set; }
            required public DomainRoute FirstLeg { get; set; }
            public DateTime FirstDeparture { get; set; }
            public DateTime FirstArrival { get; set; }
            public DateTime EarliestDeparture { get; set; }
            public DateTime LatestDeparture { get; set; }
        }
    }
}
