using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.Airlines;
using SnoopyAirlines.Repositories;

namespace SnoopyAirlines.Services.PartnerAirlines
{
    public class ExternalFlightSearchService : IExternalFlightSearchService
    {
        private const int MaxConcurrentPartnerSearches = 4;

        private readonly IFlightRepository _flightRepository;
        private readonly IPartnerAirlineRepository _partnerAirlineRepository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ExternalFlightSearchService> _logger;

        public ExternalFlightSearchService(
            IFlightRepository flightRepository,
            IPartnerAirlineRepository partnerAirlineRepository,
            IHttpClientFactory httpClientFactory,
            ILogger<ExternalFlightSearchService> logger)
        {
            _flightRepository = flightRepository;
            _partnerAirlineRepository = partnerAirlineRepository;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IReadOnlyCollection<ExternalFlight>> SearchConnectionsAsync(
            ExternalFlightSearchQuery flightQuery,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(flightQuery.Destination)
                || flightQuery.Legs.Count == 0
                || flightQuery.MaxTimeWindow < TimeSpan.Zero)
            {
                return Array.Empty<ExternalFlight>();
            }

            var partnerAirlines = await _partnerAirlineRepository.GetAllAsync(cancellationToken);
            if (partnerAirlines.Count == 0)
            {
                return Array.Empty<ExternalFlight>();
            }

            var queryLegs = flightQuery.Legs
                .Where(leg => !string.IsNullOrWhiteSpace(leg.Origin))
                .Distinct()
                .ToList();
            if (queryLegs.Count == 0)
            {
                return Array.Empty<ExternalFlight>();
            }

            var earliestDeparture = queryLegs.Min(leg => leg.Time);
            var latestDeparture = queryLegs.Max(leg => leg.Time.Add(flightQuery.MaxTimeWindow));
            var legsByOrigin = queryLegs
                .GroupBy(leg => leg.Origin, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group => group.Key,
                    group => group.ToList(),
                    StringComparer.OrdinalIgnoreCase);
            var resultsByGuid = new Dictionary<Guid, ExternalFlight>();
            using var partnerSearchSemaphore = new SemaphoreSlim(MaxConcurrentPartnerSearches);
            var partnerSearchTasks = partnerAirlines
                .Select(partnerAirline => SearchPartnerFlightsThrottledAsync(
                    partnerAirline,
                    flightQuery.Destination,
                    earliestDeparture,
                    latestDeparture,
                    flightQuery.QuantityOfPassengers,
                    partnerSearchSemaphore,
                    cancellationToken))
                .ToList();

            var partnerSearchResults = await Task.WhenAll(partnerSearchTasks);

            foreach (var partnerSearchResult in partnerSearchResults)
            {
                foreach (var partnerFlight in partnerSearchResult.Flights)
                {
                    if (partnerFlight.DepartureAirport is null
                        || !legsByOrigin.TryGetValue(partnerFlight.DepartureAirport.Code, out var originLegs))
                    {
                        continue;
                    }

                    if (!originLegs.Any(
                            leg => IsWithinLegWindow(
                                partnerFlight.DepartureTime,
                                leg,
                                flightQuery.MaxTimeWindow)))
                    {
                        continue;
                    }

                    var externalFlight = await CreateExternalFlightAsync(
                        partnerSearchResult.PartnerAirline,
                        partnerFlight,
                        flightQuery.Destination,
                        cancellationToken);

                    if (externalFlight is not null)
                    {
                        resultsByGuid[externalFlight.Guid] = externalFlight;
                    }
                }
            }

            return resultsByGuid.Values
                .OrderBy(flight => flight.DepartureAt)
                .ThenBy(flight => flight.ArrivalAt)
                .ToList();
        }

        private async Task<PartnerFlightSearchResult> SearchPartnerFlightsThrottledAsync(
            PartnerAirline partnerAirline,
            string destination,
            DateTime earliestDeparture,
            DateTime latestDeparture,
            int quantityOfPassengers,
            SemaphoreSlim semaphore,
            CancellationToken cancellationToken)
        {
            await semaphore.WaitAsync(cancellationToken);

            try
            {
                var flights = await SearchPartnerFlightsAsync(
                    partnerAirline,
                    destination,
                    earliestDeparture,
                    latestDeparture,
                    quantityOfPassengers,
                    cancellationToken);

                return new PartnerFlightSearchResult(partnerAirline, flights);
            }
            finally
            {
                semaphore.Release();
            }
        }

        private static bool IsWithinLegWindow(
            DateTime departureTime,
            ExternalFlightSearchQueryLeg leg,
            TimeSpan maxTimeWindow)
        {
            return departureTime >= leg.Time
                && departureTime <= leg.Time.Add(maxTimeWindow);
        }

        private async Task<ExternalFlight?> CreateExternalFlightAsync(
            PartnerAirline partnerAirline,
            PartnerFlight partnerFlight,
            string destination,
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
                    partnerFlight.ArrivalAirport.Code,
                    destination,
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

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

            var flightGuid = await _flightRepository.MaterializeExternalFlightAsync(
                externalFlight,
                cancellationToken);
            externalFlight.Guid = flightGuid;

            return externalFlight;
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

        private static FlightAirport ToFlightAirport(PartnerAirport airport)
        {
            return new FlightAirport
            {
                Code = airport.Code,
                Name = airport.Name,
                City = airport.City
            };
        }

        private class PartnerFlightsResponse
        {
            public IReadOnlyCollection<PartnerFlight> Flights { get; set; } = Array.Empty<PartnerFlight>();
        }

        private sealed record PartnerFlightSearchResult(
            PartnerAirline PartnerAirline,
            IReadOnlyCollection<PartnerFlight> Flights);

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
