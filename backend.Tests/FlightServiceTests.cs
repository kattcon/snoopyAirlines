using Microsoft.Extensions.Configuration;
using Xunit;
using Moq;
using SnoopyAirlines.Services;
using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.View;
using SnoopyAirlines.Repositories;

namespace SnoopyAirlines.Tests
{
    public class FlightServiceTests
    {
        private readonly Mock<FlightRepository> _mockRepository;
        private readonly FlightService _flightService;

        public FlightServiceTests()
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=TestDb;Trusted_Connection=True;"
                })
                .Build();

            _mockRepository = new Mock<FlightRepository>(configuration);
            _flightService = new FlightService(_mockRepository.Object);
        }

        /// <summary>
        /// Test 1: Verifica que se retornen vuelos directos cuando no se solicitan escalas
        /// </summary>
        [Fact]
        public async Task SearchFlights_WithoutStopovers_ReturnOnlyDirectFlights()
        {
            // Arrange
            var query = new FlightQuery
            {
                Origin = "SJO",
                Destination = "MIA",
                EarliestDeparture = new DateTime(2026, 6, 15, 6, 0, 0),
                LatestDeparture = new DateTime(2026, 6, 15, 22, 0, 0),
                QuantityOfPassengers = 2,
                IncludeStopovers = false
            };

            var flightDefinition = new FlightDefinition
            {
                Id = 1,
                DepartureAirport = new FlightDefinitionAirport { Code = "SJO", Name = "San José", City = "San José" },
                ArrivalAirport = new FlightDefinitionAirport { Code = "MIA", Name = "Miami", City = "Miami" },
                DepartureTime = new TimeOnly(8, 0),
                ArrivalTime = new TimeOnly(10, 30),
                DurationMinutes = 150,
                Frequency = new FlightFrequency { Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = false, Sunday = false },
                PriceEconomyClass = 299,
                PriceFirstClass = 599,
                CarryOnPrice = 0,
                CheckedPrice = 25
            };

            _mockRepository
                .Setup(r => r.GetFlightDefinitionsAsync(It.IsAny<FlightDefinitionQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { flightDefinition });

            // Act
            var results = await _flightService.SearchFlightsAsync(query, CancellationToken.None);

            // Assert
            Assert.NotEmpty(results);
            Assert.All(results, flight => Assert.False(flight.HasStopover));
            Assert.True(results.All(f => f.DepartureAirport.Code == "SJO" && f.ArrivalAirport.Code == "MIA"));
        }

        /// <summary>
        /// Test 2: Verifica que las escalas menores a 1 hora sean filtradas (restricción mínima)
        /// </summary>
        [Fact]
        public async Task SearchFlights_WithStopoverLessThanOneHour_StopoverIsFiltered()
        {
            // Arrange
            var query = new FlightQuery
            {
                Origin = "SJO",
                Destination = "LAX",
                EarliestDeparture = new DateTime(2026, 6, 15, 6, 0, 0),
                LatestDeparture = new DateTime(2026, 6, 15, 22, 0, 0),
                QuantityOfPassengers = 1,
                IncludeStopovers = true
            };

            var firstLegDef = new FlightDefinition
            {
                Id = 1,
                DepartureAirport = new FlightDefinitionAirport { Code = "SJO", Name = "San José", City = "San José" },
                ArrivalAirport = new FlightDefinitionAirport { Code = "MIA", Name = "Miami", City = "Miami" },
                DepartureTime = new TimeOnly(8, 0),
                ArrivalTime = new TimeOnly(10, 30),
                DurationMinutes = 150,
                Frequency = new FlightFrequency { Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = false, Sunday = false },
                PriceEconomyClass = 299,
                PriceFirstClass = 599,
                CarryOnPrice = 0,
                CheckedPrice = 25
            };

            var secondLegDef = new FlightDefinition
            {
                Id = 2,
                DepartureAirport = new FlightDefinitionAirport { Code = "MIA", Name = "Miami", City = "Miami" },
                ArrivalAirport = new FlightDefinitionAirport { Code = "LAX", Name = "Los Ángeles", City = "Los Ángeles" },
                DepartureTime = new TimeOnly(11, 0), // Solo 30 minutos después de la llegada (10:30)
                ArrivalTime = new TimeOnly(12, 30),
                DurationMinutes = 90,
                Frequency = new FlightFrequency { Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = false, Sunday = false },
                PriceEconomyClass = 249,
                PriceFirstClass = 499,
                CarryOnPrice = 0,
                CheckedPrice = 25
            };

            _mockRepository
                .Setup(r => r.GetFlightDefinitionsAsync(
                    It.Is<FlightDefinitionQuery>(q => q.Origin == "SJO"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { firstLegDef });

            _mockRepository
                .Setup(r => r.GetFlightDefinitionsAsync(
                    It.Is<FlightDefinitionQuery>(q => q.Destination == "LAX"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { secondLegDef });

            // Act
            var results = await _flightService.SearchFlightsAsync(query, CancellationToken.None);

            // Assert
            // No debe haber vuelos con escala de 30 minutos (menor a 1 hora)
            var stopoverFlights = results.Where(f => f.HasStopover && f.StopoverAirport?.Code == "MIA").ToList();
            Assert.DoesNotContain(stopoverFlights, flight => flight.StopoverDuration != null && flight.StopoverDuration.Contains("00:30"));
        }

        /// <summary>
        /// Test 3: Verifica que las escalas mayores a 12 horas sean filtradas (restricción máxima)
        /// </summary>
        [Fact]
        public async Task SearchFlights_WithStopoverMoreThanTwelveHours_StopoverIsFiltered()
        {
            // Arrange
            var query = new FlightQuery
            {
                Origin = "SJO",
                Destination = "CDG",
                EarliestDeparture = new DateTime(2026, 6, 15, 6, 0, 0),
                LatestDeparture = new DateTime(2026, 6, 16, 22, 0, 0),
                QuantityOfPassengers = 2,
                IncludeStopovers = true
            };

            var firstLegDef = new FlightDefinition
            {
                Id = 3,
                DepartureAirport = new FlightDefinitionAirport { Code = "SJO", Name = "San José", City = "San José" },
                ArrivalAirport = new FlightDefinitionAirport { Code = "BOG", Name = "Bogotá", City = "Bogotá" },
                DepartureTime = new TimeOnly(8, 0),
                ArrivalTime = new TimeOnly(11, 0),
                DurationMinutes = 180,
                Frequency = new FlightFrequency { Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = true, Sunday = true },
                PriceEconomyClass = 199,
                PriceFirstClass = 399,
                CarryOnPrice = 0,
                CheckedPrice = 25
            };

            var secondLegDef = new FlightDefinition
            {
                Id = 4,
                DepartureAirport = new FlightDefinitionAirport { Code = "BOG", Name = "Bogotá", City = "Bogotá" },
                ArrivalAirport = new FlightDefinitionAirport { Code = "CDG", Name = "París", City = "París" },
                DepartureTime = new TimeOnly(23, 30), // 12:30 horas después de la llegada (11:00 + 12:30)
                ArrivalTime = new TimeOnly(15, 0), // Día siguiente
                DurationMinutes = 840,
                Frequency = new FlightFrequency { Monday = true, Tuesday = true, Wednesday = true, Thursday = true, Friday = true, Saturday = true, Sunday = true },
                PriceEconomyClass = 649,
                PriceFirstClass = 1249,
                CarryOnPrice = 0,
                CheckedPrice = 25
            };

            _mockRepository
                .Setup(r => r.GetFlightDefinitionsAsync(
                    It.Is<FlightDefinitionQuery>(q => q.Origin == "SJO"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { firstLegDef });

            _mockRepository
                .Setup(r => r.GetFlightDefinitionsAsync(
                    It.Is<FlightDefinitionQuery>(q => q.Destination == "CDG"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { secondLegDef });

            // Act
            var results = await _flightService.SearchFlightsAsync(query, CancellationToken.None);

            // Assert
            // No debe haber vuelos con escala de 750 minutos (12:30 horas, mayor a 12 horas)
            var stopoverFlights = results.Where(f => f.HasStopover && f.StopoverAirport?.Code == "BOG").ToList();
            Assert.DoesNotContain(stopoverFlights, flight => flight.StopoverDuration != null && flight.StopoverDuration.Contains("12:30"));
        }
    }
}
