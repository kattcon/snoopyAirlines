using System.Globalization;
using System.Net.Http.Json;
using SnoopyAirlines.External.Domain;
using SnoopyAirlines.External.Domain.View;

namespace SnoopyAirlines.External.Services
{
    public class RouteService
    {
        private readonly HttpClient _httpClient;

        public RouteService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IReadOnlyCollection<Flight>> GetFlights(
            RouteQuery routeQuery,
            CancellationToken cancellationToken = default)
        {
            var response = await Get(routeQuery, cancellationToken);

            return response.Flights;
        }

        public async Task<FlightsResponse> Get(
            RouteQuery routeQuery,
            CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync(
                BuildSearchPath(routeQuery),
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var backendResponse = await response.Content.ReadFromJsonAsync<BackendFlightsResponse>(
                    cancellationToken: cancellationToken)
                ?? new BackendFlightsResponse { Flights = Array.Empty<BackendFlightResponse>() };

            return new FlightsResponse
            {
                Flights = backendResponse.Flights
                    .SelectMany(flight => flight.Flights)
                    .Select(ToExternalFlight)
                    .ToList()
            };
        }

        private static string BuildSearchPath(RouteQuery routeQuery)
        {
            var queryParameters = new List<string>();

            AddQueryParameter(queryParameters, "detination", routeQuery.Destination);
            AddQueryParameter(queryParameters, "earliestDeparture", FormatDateTime(routeQuery.EarliestDeparture));
            AddQueryParameter(queryParameters, "latestDeparture", FormatDateTime(routeQuery.LatestDeparture));
            AddQueryParameter(
                queryParameters,
                "quantityOfPassengers",
                routeQuery.QuantityOfPassengers?.ToString(CultureInfo.InvariantCulture));

            return $"Flight/search?{string.Join("&", queryParameters)}";
        }

        private static void AddQueryParameter(
            ICollection<string> queryParameters,
            string name,
            string? value)
        {
            if (value is null)
            {
                return;
            }

            queryParameters.Add($"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value)}");
        }

        private static string? FormatDateTime(DateTime? dateTime)
        {
            return dateTime?.ToString("O", CultureInfo.InvariantCulture);
        }

        private static Flight ToExternalFlight(BackendFlightLegResponse flight)
        {
            return new Flight
            {
                FlightGUID = flight.FlightGuid ?? string.Empty,
                RouteId = flight.RouteId ?? 0,
                DepartureTime = flight.DepartureTime,
                ArrivalTime = flight.ArrivalTime,
                Duration = flight.Duration ?? string.Empty,
                DepartureAirport = flight.DepartureAirport
                    ?? new Airport { Code = string.Empty, Name = string.Empty, City = string.Empty },
                ArrivalAirport = flight.ArrivalAirport
                    ?? new Airport { Code = string.Empty, Name = string.Empty, City = string.Empty },
                TouristPrice = flight.TouristPrice,
                FirstClassPrice = flight.FirstClassPrice,
                CarryOnPrice = flight.CarryOnPrice,
                CheckedPrice = flight.CheckedPrice
            };
        }

        private class BackendFlightsResponse
        {
            public IReadOnlyCollection<BackendFlightResponse> Flights { get; set; } =
                Array.Empty<BackendFlightResponse>();
        }

        private class BackendFlightResponse
        {
            public IReadOnlyCollection<BackendFlightLegResponse> Flights { get; set; } =
                Array.Empty<BackendFlightLegResponse>();
        }

        private class BackendFlightLegResponse
        {
            public int? RouteId { get; set; }
            public string? FlightGuid { get; set; }
            public DateTime DepartureTime { get; set; }
            public DateTime ArrivalTime { get; set; }
            public string? Duration { get; set; }
            public Airport? DepartureAirport { get; set; }
            public Airport? ArrivalAirport { get; set; }
            public decimal TouristPrice { get; set; }
            public decimal FirstClassPrice { get; set; }
            public decimal CarryOnPrice { get; set; }
            public decimal CheckedPrice { get; set; }
        }
    }
}
