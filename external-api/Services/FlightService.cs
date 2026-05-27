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
                Destination = flightQuery.Destination,
                QuantityOfPassengers = flightQuery.QuantityOfPassengers,
                ArrivalWindows = CreateArrivalWindows(flightQuery)
            };
        }

        private static IReadOnlyCollection<FlightDefinitionArrivalWindow> CreateArrivalWindows(FlightQuery flightQuery)
        {
            if (flightQuery.EarliestArrival is null || flightQuery.LatestArrival is null)
            {
                return Array.Empty<FlightDefinitionArrivalWindow>();
            }

            var earliestArrival = flightQuery.EarliestArrival.Value;
            var latestArrival = flightQuery.LatestArrival.Value;

            var windows = new List<FlightDefinitionArrivalWindow>();
            var currentDate = earliestArrival.Date;
            var endDate = latestArrival.Date;

            while (currentDate <= endDate)
            {
                windows.Add(new FlightDefinitionArrivalWindow
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
            IReadOnlyCollection<FlightDefinition> flightDefinitions,
            FlightQuery flightQuery)
        {
            if (flightQuery.EarliestArrival is null || flightQuery.LatestArrival is null)
            {
                return Array.Empty<Flight>();
            }

            var flights = new List<Flight>();
            var earliestArrival = flightQuery.EarliestArrival.Value;
            var latestArrival = flightQuery.LatestArrival.Value;
            var currentDate = earliestArrival.Date;
            var endDate = latestArrival.Date;

            while (currentDate <= endDate)
            {
                foreach (var flightDefinition in flightDefinitions)
                {
                    var departureDate = ArrivesNextDay(flightDefinition)
                        ? currentDate.AddDays(-1)
                        : currentDate;

                    if (!OccursOn(flightDefinition.Frequency, departureDate.DayOfWeek))
                    {
                        continue;
                    }

                    var arrivalTime = currentDate.Add(flightDefinition.ArrivalTime.ToTimeSpan());

                    if (arrivalTime < earliestArrival || arrivalTime > latestArrival)
                    {
                        continue;
                    }

                    var departureTime = departureDate.Add(flightDefinition.DepartureTime.ToTimeSpan());
                    flights.Add(CreateFlight(flightDefinition, departureTime));
                }

                currentDate = currentDate.AddDays(1);
            }

            return flights
                .OrderBy(flight => flight.ArrivalTime)
                .ThenBy(flight => flight.DepartureTime)
                .ThenBy(flight => flight.FlightGUID, StringComparer.Ordinal)
                .ToList();
        }

        private static Flight CreateFlight(FlightDefinition flightDefinition, DateTime departureTime)
        {
            var arrivalDate = ArrivesNextDay(flightDefinition)
                ? departureTime.Date.AddDays(1)
                : departureTime.Date;
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

        private static bool ArrivesNextDay(FlightDefinition flightDefinition)
        {
            return flightDefinition.ArrivalTime < flightDefinition.DepartureTime;
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
