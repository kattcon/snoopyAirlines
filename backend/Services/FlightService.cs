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
            var directFlights = CreateFlights(flightDefinitions, flightQuery);

            if (!flightQuery.IncludeStopovers)
            {
                return directFlights;
            }

            var connectingFlights = await CreateConnectingFlightsAsync(flightQuery, cancellationToken);

            return directFlights
                .Concat(connectingFlights)
                .OrderBy(f => f.DepartureTime)
                .ThenBy(f => f.FlightGUID, StringComparer.Ordinal)
                .ToList();
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
            FlightQuery flightQuery,
            CancellationToken cancellationToken)
        {
            const int MinStopoverMinutes = 60;     // 1 hora
            const int MaxStopoverMinutes = 720;    // 12 horas

            var firstLegQuery = new FlightDefinitionQuery
            {
                Origin = flightQuery.Origin
            };

            var secondLegQuery = new FlightDefinitionQuery
            {
                Destination = flightQuery.Destination
            };

            var firstDefs = await _flightRepository.GetFlightDefinitionsAsync(firstLegQuery, cancellationToken);
            var secondDefs = await _flightRepository.GetFlightDefinitionsAsync(secondLegQuery, cancellationToken);

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

                            var totalDurationMinutes = (int)(arr2 - dep1).TotalMinutes;

                            results.Add(new FlightResponse
                            {
                                FlightGUID = CreateConnectingFlightGuid(first.Id, second.Id, dep1.Date),
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
                                CarryOnPrice = first.CarryOnPrice + second.CarryOnPrice,
                                CheckedPrice = first.CheckedPrice + second.CheckedPrice
                            });
                        }
                    }
                }

                currentDate = currentDate.AddDays(1);
            }

            return results;
        }

        private static string FormatDuration(int durationMinutes)
        {
            var hours = durationMinutes / 60;
            var minutes = durationMinutes % 60;

            return $"{hours:00}:{minutes:00}";
        }
    }
}
