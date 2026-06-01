using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Repositories;

namespace SnoopyAirlines.Services
{
    public class FlightService
    {
        private static readonly Guid FlightGuidNamespace = new("9e991ddc-7c58-4e29-a379-503fc594c1e2");
        private readonly FlightRepository _flightRepository;

        public FlightService(FlightRepository flightRepository)
        {
            _flightRepository = flightRepository;
        }

        public Task<IReadOnlyCollection<Flight>> GetFlightsAsync(CancellationToken cancellationToken)
        {
            return _flightRepository.GetAllAsync(cancellationToken);
        }

        public Task<IReadOnlyCollection<Flight>> SearchFlightsAsync(
            int? departureAirportId,
            int? arrivalAirportId,
            DateOnly? departureDate,
            CancellationToken cancellationToken)
        {
            return _flightRepository.SearchAsync(departureAirportId, arrivalAirportId, departureDate, cancellationToken);
        }

        public async Task<IReadOnlyCollection<FlightResponse>> SearchFlightsAsync(
            FlightQuery flightQuery,
            CancellationToken cancellationToken)
        {
            var repositoryQuery = ToFlightDefinitionQuery(flightQuery);
            var flightDefinitions = await _flightRepository.GetFlightDefinitionsAsync(repositoryQuery, cancellationToken);
            return CreateFlights(flightDefinitions, flightQuery);
        }

        public Task<Flight> SaveFlightAsync(Flight flight, CancellationToken cancellationToken)
        {
            return _flightRepository.SaveAsync(flight, cancellationToken);
        }

        private static FlightDefinitionQuery ToFlightDefinitionQuery(FlightQuery flightQuery)
        {
            return new FlightDefinitionQuery
            {
                Origin = flightQuery.Origin,
                Destination = flightQuery.Destination,
                QuantityOfPassengers = flightQuery.QuantityOfPassengers,
                IncludeStopovers = flightQuery.IncludeStopovers,
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

        private static IReadOnlyCollection<FlightResponse> CreateFlights(
            IReadOnlyCollection<FlightDefinition> flightDefinitions,
            FlightQuery flightQuery)
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
                foreach (var definition in flightDefinitions)
                {
                    if (!OccursOn(definition.Frequency, currentDate.DayOfWeek))
                    {
                        continue;
                    }

                    var departureTime = currentDate.Add(definition.DepartureTime.ToTimeSpan());

                    if (departureTime < earliestDeparture || departureTime > latestDeparture)
                    {
                        continue;
                    }

                    flights.Add(CreateFlightResponse(definition, departureTime));
                }

                currentDate = currentDate.AddDays(1);
            }

            return flights
                .OrderBy(flight => flight.DepartureTime)
                .ThenBy(flight => flight.FlightGUID, StringComparer.Ordinal)
                .ToList();
        }

        private static FlightResponse CreateFlightResponse(FlightDefinition definition, DateTime departureTime)
        {
            var arrivalDate = definition.ArrivalTime >= definition.DepartureTime
                ? departureTime.Date
                : departureTime.Date.AddDays(1);

            return new FlightResponse
            {
                FlightGUID = CreateFlightGuid(definition.Id, departureTime.Date),
                DepartureTime = departureTime,
                ArrivalTime = arrivalDate.Add(definition.ArrivalTime.ToTimeSpan()),
                Duration = FormatDuration(definition.DurationMinutes),
                DepartureAirport = new AirportResponse
                {
                    Code = definition.DepartureAirport.Code,
                    Name = definition.DepartureAirport.Name,
                    City = definition.DepartureAirport.City
                },
                ArrivalAirport = new AirportResponse
                {
                    Code = definition.ArrivalAirport.Code,
                    Name = definition.ArrivalAirport.Name,
                    City = definition.ArrivalAirport.City
                },
                TouristPrice = definition.PriceEconomyClass,
                FirstClassPrice = definition.PriceFirstClass,
                CarryOnPrice = definition.CarryOnPrice,
                CheckedPrice = definition.CheckedPrice
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
