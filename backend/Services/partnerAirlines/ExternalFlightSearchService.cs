using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.Airlines;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Repositories;
using DomainRoute = SnoopyAirlines.Domain.Route;

namespace SnoopyAirlines.Services.PartnerAirlines
{
    public class ExternalFlightSearchService : IExternalFlightSearchService
    {
        private const int MinStopoverMinutes = 60;
        private const int MaxStopoverMinutes = 720;

        private readonly IRouteService _routeService;
        private readonly IFlightRepository _flightRepository;
        private readonly IPartnerAirlineRepository _partnerAirlineRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ExternalFlightSearchService> _logger;

        public ExternalFlightSearchService(
            IRouteService routeService,
            IFlightRepository flightRepository,
            IPartnerAirlineRepository partnerAirlineRepository,
            IHttpClientFactory httpClientFactory,
            ILogger<ExternalFlightSearchService> logger)
        {
            _routeService = routeService;
            _flightRepository = flightRepository;
            _partnerAirlineRepository = partnerAirlineRepository;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IReadOnlyCollection<FlightResponse>> SearchConnectionsAsync(
            FlightQuery flightQuery,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(flightQuery.Origin)
                || string.IsNullOrWhiteSpace(flightQuery.Destination)
                || flightQuery.EarliestDeparture is null
                || flightQuery.LatestDeparture is null)
            {
                return Array.Empty<FlightResponse>();
            }

            var firstLegs = await _routeService.SearchRoutesAsync(
                new RouteSearchQuery
                {
                    Origin = flightQuery.Origin,
                    QuantityOfPassengers = flightQuery.QuantityOfPassengers
                },
                cancellationToken);

            var partnerAirlines = await _partnerAirlineRepository.GetAllAsync(cancellationToken);
            if (partnerAirlines.Count == 0)
            {
                return Array.Empty<FlightResponse>();
            }

            var results = new List<FlightResponse>();
            var earliestDeparture = flightQuery.EarliestDeparture.Value;
            var latestDeparture = flightQuery.LatestDeparture.Value;
            var currentDate = earliestDeparture.Date;
            var endDate = latestDeparture.Date;

            while (currentDate <= endDate)
            {
                foreach (var firstLeg in firstLegs)
                {
                    if (!OccursOn(firstLeg.Frequency, currentDate.DayOfWeek)
                        || firstLeg.ArrivalAirport is null
                        || firstLeg.DepartureAirport is null)
                    {
                        continue;
                    }

                    if (string.Equals(
                            firstLeg.ArrivalAirport.Code,
                            flightQuery.Destination,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var firstDeparture = currentDate.Add(firstLeg.DepartureTime.ToTimeSpan());
                    if (firstDeparture < earliestDeparture || firstDeparture > latestDeparture)
                    {
                        continue;
                    }

                    var firstArrival = CalculateArrivalTime(firstLeg, firstDeparture);
                    var connectionEarliestDeparture = firstArrival.AddMinutes(MinStopoverMinutes);
                    var connectionLatestDeparture = firstArrival.AddMinutes(MaxStopoverMinutes);

                    foreach (var partnerAirline in partnerAirlines)
                    {
                        var partnerFlights = await SearchPartnerFlightsAsync(
                            partnerAirline,
                            flightQuery.Destination,
                            connectionEarliestDeparture,
                            connectionLatestDeparture,
                            flightQuery.QuantityOfPassengers ?? 1,
                            cancellationToken);

                        foreach (var partnerFlight in partnerFlights)
                        {
                            var connection = await CreateConnectionAsync(
                                firstLeg,
                                firstDeparture,
                                firstArrival,
                                partnerAirline,
                                partnerFlight,
                                flightQuery,
                                cancellationToken);

                            if (connection is not null)
                            {
                                results.Add(connection);
                            }
                        }
                    }
                }

                currentDate = currentDate.AddDays(1);
            }

            return results
                .OrderBy(flight => flight.DepartureTime)
                .ThenBy(flight => flight.RouteId)
                .ThenBy(flight => flight.ArrivalTime)
                .ToList();
        }

        private async Task<FlightResponse?> CreateConnectionAsync(
            DomainRoute firstLeg,
            DateTime firstDeparture,
            DateTime firstArrival,
            PartnerAirline partnerAirline,
            PartnerFlight partnerFlight,
            FlightQuery flightQuery,
            CancellationToken cancellationToken)
        {
            if (partnerFlight.DepartureAirport is null
                || partnerFlight.ArrivalAirport is null
                || string.IsNullOrWhiteSpace(partnerFlight.FlightGUID)
                || !Guid.TryParse(partnerFlight.FlightGUID, out var externalFlightGuid))
            {
                return null;
            }

            if (!string.Equals(
                    partnerFlight.DepartureAirport.Code,
                    firstLeg.ArrivalAirport!.Code,
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (!string.Equals(
                    partnerFlight.ArrivalAirport.Code,
                    flightQuery.Destination,
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var connectionMinutes = (int)(partnerFlight.DepartureTime - firstArrival).TotalMinutes;
            if (connectionMinutes < MinStopoverMinutes || connectionMinutes > MaxStopoverMinutes)
            {
                return null;
            }

            if (!IsWithinArrivalBounds(partnerFlight.ArrivalTime, flightQuery))
            {
                return null;
            }

            var firstFlightGuid = await _flightRepository.MaterializeInternalFlightAsync(
                firstLeg.Id,
                firstDeparture,
                cancellationToken);

            var externalFlight = new ExternalFlight
            {
                Guid = externalFlightGuid,
                PartnerAirlineId = partnerAirline.Id,
                PartnerAirline = partnerAirline,
                DepartureAt = partnerFlight.DepartureTime,
                ArrivalAt = partnerFlight.ArrivalTime,
                DepartureAirport = ToFlightAirport(partnerFlight.DepartureAirport),
                ArrivalAirport = ToFlightAirport(partnerFlight.ArrivalAirport),
                TouristPrice = partnerFlight.TouristPrice,
                FirstClassPrice = partnerFlight.FirstClassPrice,
                CarryOnPrice = partnerFlight.CarryOnPrice,
                CheckedPrice = partnerFlight.CheckedPrice
            };

            var secondFlightGuid = await _flightRepository.MaterializeExternalFlightAsync(
                externalFlight,
                cancellationToken);

            var totalDurationMinutes = (int)(partnerFlight.ArrivalTime - firstDeparture).TotalMinutes;

            return new FlightResponse
            {
                RouteId = firstLeg.Id,
                Routes =
                [
                    CreateFlightRouteResponse(1, firstLeg.Id, firstDeparture, firstFlightGuid),
                    CreateFlightRouteResponse(2, 0, partnerFlight.DepartureTime, secondFlightGuid)
                ],
                DepartureTime = firstDeparture,
                ArrivalTime = partnerFlight.ArrivalTime,
                Duration = FormatDuration(totalDurationMinutes),
                DepartureAirport = ToAirportResponse(firstLeg.DepartureAirport!),
                ArrivalAirport = ToAirportResponse(partnerFlight.ArrivalAirport),
                HasStopover = true,
                StopoverAirport = ToAirportResponse(firstLeg.ArrivalAirport!),
                StopoverDuration = FormatDuration(connectionMinutes),
                TouristPrice = firstLeg.PriceEconomyClass + partnerFlight.TouristPrice,
                FirstClassPrice = firstLeg.PriceFirstClass + partnerFlight.FirstClassPrice,
                CarryOnPrice = firstLeg.PriceCarryOnBaggage + partnerFlight.CarryOnPrice,
                CheckedPrice = firstLeg.PriceCheckedBaggage + partnerFlight.CheckedPrice
            };
        }

        private async Task<IReadOnlyCollection<PartnerFlight>> SearchPartnerFlightsAsync(
            PartnerAirline partnerAirline,
            string destination,
            DateTime earliestDeparture,
            DateTime latestDeparture,
            int quantityOfPassengers,
            CancellationToken cancellationToken)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                using var response = await client.GetAsync(
                    BuildPartnerSearchUri(
                        partnerAirline,
                        destination,
                        earliestDeparture,
                        latestDeparture,
                        quantityOfPassengers),
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Partner airline {PartnerAirlineName} flight search failed with status {StatusCode}.",
                        partnerAirline.Name,
                        response.StatusCode);

                    return Array.Empty<PartnerFlight>();
                }

                var flightsResponse = await response.Content.ReadFromJsonAsync<PartnerFlightsResponse>(
                    cancellationToken: cancellationToken);

                return flightsResponse?.Flights ?? Array.Empty<PartnerFlight>();
            }
            catch (Exception exception) when (
                exception is HttpRequestException
                || exception is JsonException
                || (exception is TaskCanceledException && !cancellationToken.IsCancellationRequested))
            {
                _logger.LogWarning(
                    exception,
                    "Partner airline {PartnerAirlineName} flight search failed.",
                    partnerAirline.Name);

                return Array.Empty<PartnerFlight>();
            }
        }

        private static Uri BuildPartnerSearchUri(
            PartnerAirline partnerAirline,
            string destination,
            DateTime earliestDeparture,
            DateTime latestDeparture,
            int quantityOfPassengers)
        {
            var endpoint = NormalizePartnerEndpoint(partnerAirline.Host);
            var queryParameters = new List<string>();

            AddQueryParameter(queryParameters, "destination", destination);
            AddQueryParameter(queryParameters, "earliestDeparture", FormatDateTime(earliestDeparture));
            AddQueryParameter(queryParameters, "latestDeparture", FormatDateTime(latestDeparture));
            AddQueryParameter(
                queryParameters,
                "quantityOfPassengers",
                quantityOfPassengers.ToString(CultureInfo.InvariantCulture));
            AddQueryParameter(queryParameters, "apiKey", partnerAirline.ApiKey);

            return new Uri($"{endpoint}?{string.Join("&", queryParameters)}");
        }

        private static string NormalizePartnerEndpoint(string host)
        {
            var endpoint = host.Trim().TrimEnd('/');

            return endpoint.EndsWith("/api/external", StringComparison.OrdinalIgnoreCase)
                ? endpoint
                : $"{endpoint}/api/external";
        }

        private static void AddQueryParameter(
            ICollection<string> queryParameters,
            string name,
            string value)
        {
            queryParameters.Add($"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value)}");
        }

        private static string FormatDateTime(DateTime dateTime)
        {
            return dateTime.ToString("O", CultureInfo.InvariantCulture);
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
            var arrivalDate = route.ArrivalTime < route.DepartureTime
                ? departureTime.Date.AddDays(1)
                : departureTime.Date;

            return arrivalDate.Add(route.ArrivalTime.ToTimeSpan());
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

        private static AirportResponse ToAirportResponse(RouteAirport airport)
        {
            return new AirportResponse
            {
                Code = airport.Code,
                Name = airport.Name,
                City = airport.City
            };
        }

        private static AirportResponse ToAirportResponse(PartnerAirport airport)
        {
            return new AirportResponse
            {
                Code = airport.Code,
                Name = airport.Name,
                City = airport.City
            };
        }

        private static FlightAirport ToFlightAirport(PartnerAirport airport)
        {
            return new FlightAirport
            {
                Code = airport.Code,
                Name = airport.Name,
                City = airport.City
            };
        }

        private static string FormatDuration(int durationMinutes)
        {
            var hours = durationMinutes / 60;
            var minutes = durationMinutes % 60;

            return $"{hours:00}:{minutes:00}";
        }

        private class PartnerFlightsResponse
        {
            public IReadOnlyCollection<PartnerFlight> Flights { get; set; } = Array.Empty<PartnerFlight>();
        }

        private class PartnerFlight
        {
            public string? FlightGUID { get; set; }
            public int RouteId { get; set; }
            public DateTime DepartureTime { get; set; }
            public DateTime ArrivalTime { get; set; }
            public string? Duration { get; set; }
            public PartnerAirport? DepartureAirport { get; set; }
            public PartnerAirport? ArrivalAirport { get; set; }
            public decimal TouristPrice { get; set; }
            public decimal FirstClassPrice { get; set; }
            public decimal CarryOnPrice { get; set; }
            public decimal CheckedPrice { get; set; }
        }

        private class PartnerAirport
        {
            required public string Code { get; set; }
            required public string Name { get; set; }
            required public string City { get; set; }
        }
    }
}
