using SnoopyAirlines.External.Domain;
using SnoopyAirlines.External.Domain.View;
using SnoopyAirlines.External.Repositories;

namespace SnoopyAirlines.External.Services
{
    public class FlightService
    {
        private static readonly Guid FlightGuidNamespace = new("9e991ddc-7c58-4e29-a379-503fc594c1e2");

        private readonly FlightRepository _flightRepository;

        public FlightService(FlightRepository flightRepository)
        {
            _flightRepository = flightRepository;
        }

        public async Task<IReadOnlyCollection<Flight>> GetFlights(
            FlightQuery flightQuery,
            CancellationToken cancellationToken = default)
        {
            var repositoryQuery = ToFlightDefinitionQuery(flightQuery);
            var flightDefinitions = await _flightRepository.GetFlightDefinitions(
                repositoryQuery,
                cancellationToken);

            return CreateFlights(flightDefinitions, flightQuery);
        }

        public async Task<FlightsResponse> Get(
            FlightQuery flightQuery,
            CancellationToken cancellationToken = default)
        {
            var flights = await GetFlights(flightQuery, cancellationToken);

            return new FlightsResponse
            {
                Flights = flights
            };
        }

        private static FlightDefinitionQuery ToFlightDefinitionQuery(FlightQuery flightQuery)
        {
            return new FlightDefinitionQuery
            {
                Origin = flightQuery.Origin,
                Destination = flightQuery.Destination,
                QuantityOfPassengers = flightQuery.QuantityOfPassengers,
                DepartureWindows = CreateDepartureWindows(flightQuery)
            };
        }

        private static IReadOnlyCollection<FlightDefinitionDepartureWindow> CreateDepartureWindows(FlightQuery flightQuery)
        {
            if (flightQuery.EarliestDeparture is null || flightQuery.LatestDeparture is null)
            {
                return Array.Empty<FlightDefinitionDepartureWindow>();
            }

            var earliestDeparture = flightQuery.EarliestDeparture.Value;
            var latestDeparture = flightQuery.LatestDeparture.Value;

            var windows = new List<FlightDefinitionDepartureWindow>();
            var currentDate = earliestDeparture.Date;
            var endDate = latestDeparture.Date;

            while (currentDate <= endDate)
            {
                windows.Add(new FlightDefinitionDepartureWindow
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

        private static IReadOnlyCollection<Flight> CreateFlights(
            IReadOnlyCollection<FlightDefinition> flightDefinitions,
            FlightQuery flightQuery)
        {
            if (flightQuery.EarliestDeparture is null || flightQuery.LatestDeparture is null)
            {
                return Array.Empty<Flight>();
            }

            var flights = new List<Flight>();
            var earliestDeparture = flightQuery.EarliestDeparture.Value;
            var latestDeparture = flightQuery.LatestDeparture.Value;
            var currentDate = earliestDeparture.Date;
            var endDate = latestDeparture.Date;

            while (currentDate <= endDate)
            {
                foreach (var flightDefinition in flightDefinitions)
                {
                    if (!OccursOn(flightDefinition.Frequency, currentDate.DayOfWeek))
                    {
                        continue;
                    }

                    var departureTime = currentDate.Add(flightDefinition.DepartureTime.ToTimeSpan());

                    if (departureTime < earliestDeparture || departureTime > latestDeparture)
                    {
                        continue;
                    }

                    flights.Add(CreateFlight(flightDefinition, departureTime));
                }

                currentDate = currentDate.AddDays(1);
            }

            return flights
                .OrderBy(flight => flight.DepartureTime)
                .ThenBy(flight => flight.FlightGUID, StringComparer.Ordinal)
                .ToList();
        }

        private static Flight CreateFlight(FlightDefinition flightDefinition, DateTime departureTime)
        {
            var arrivalDate = flightDefinition.ArrivalTime >= flightDefinition.DepartureTime
                ? departureTime.Date
                : departureTime.Date.AddDays(1);
            var arrivalTime = arrivalDate.Add(flightDefinition.ArrivalTime.ToTimeSpan());

            return new Flight
            {
                FlightGUID = CreateFlightGuid(flightDefinition.Id, departureTime.Date),
                DepartureTime = departureTime,
                ArrivalTime = arrivalTime,
                Duration = FormatDuration(flightDefinition.DurationMinutes),
                DepartureAirport = flightDefinition.DepartureAirport,
                ArrivalAirport = flightDefinition.ArrivalAirport,
                TouristPrice = flightDefinition.TouristPrice,
                FirstClassPrice = flightDefinition.FirstClassPrice,
                CarryOnPrice = flightDefinition.CarryOnPrice,
                CheckedPrice = flightDefinition.CheckedPrice
            };
        }

        private static bool OccursOn(FlightFrequency frequency, DayOfWeek dayOfWeek)
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

        private static FlightFrequency CreateFrequency(DayOfWeek dayOfWeek)
        {
            return new FlightFrequency
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

        private static string CreateFlightGuid(int flightDefinitionId, DateTime departureDate)
        {
            var bytes = FlightGuidNamespace.ToByteArray();
            var idBytes = BitConverter.GetBytes(flightDefinitionId);
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
