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

            return await response.Content.ReadFromJsonAsync<FlightsResponse>(
                    cancellationToken: cancellationToken)
                ?? new FlightsResponse { Flights = Array.Empty<Flight>() };
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
    }
}
