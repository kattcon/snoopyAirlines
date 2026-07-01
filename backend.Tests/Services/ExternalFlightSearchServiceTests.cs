using System.Globalization;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using SnoopyAirlines.Domain;
using SnoopyAirlines.Domain.Airlines;
using SnoopyAirlines.Repositories;
using SnoopyAirlines.Services.PartnerAirlines;
using Xunit;

namespace SnoopyAirlines.Tests
{
    public class ExternalFlightSearchServiceTests
    {
        [Fact]
        public async Task TestSearchConnectionsAsyncWhenQueryIsInvalidReturnsEmptyAndSkipsRepositories()
        {
            var partnerAirlineRepository = new Mock<IPartnerAirlineRepository>();
            var flightRepository = new Mock<IFlightRepository>();
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var service = new ExternalFlightSearchService(
                flightRepository.Object,
                partnerAirlineRepository.Object,
                httpClientFactory.Object,
                Mock.Of<ILogger<ExternalFlightSearchService>>());

            var results = await service.SearchConnectionsAsync(
                new ExternalFlightSearchQuery
                {
                    Destination = " ",
                    QuantityOfPassengers = 1,
                    MaxTimeWindow = TimeSpan.FromHours(4),
                    Legs =
                    [
                        new ExternalFlightSearchQueryLeg(
                            "MIA",
                            new DateTime(2026, 6, 15, 10, 0, 0))
                    ]
                },
                CancellationToken.None);

            Assert.Empty(results);
            partnerAirlineRepository.Verify(
                repository => repository.GetAllAsync(It.IsAny<CancellationToken>()),
                Times.Never);
            flightRepository.Verify(
                repository => repository.MaterializeExternalFlightAsync(
                    It.IsAny<ExternalFlight>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
            httpClientFactory.Verify(
                factory => factory.CreateClient(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task TestSearchConnectionsAsyncWhenPartnerAirlineExistsSendsExpectedPartnerSearchQuery()
        {
            var earliestDeparture = new DateTime(2026, 6, 15, 8, 0, 0);
            var latestLegDeparture = new DateTime(2026, 6, 15, 12, 0, 0);
            var partnerAirline = new PartnerAirline
            {
                Id = 7,
                Name = "Partner",
                Host = "https://partner.example",
                ApiKey = "api-key"
            };
            var partnerAirlineRepository = new Mock<IPartnerAirlineRepository>();
            var flightRepository = new Mock<IFlightRepository>();
            var httpClientFactory = new Mock<IHttpClientFactory>();
            Uri? capturedRequestUri = null;

            partnerAirlineRepository
                .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([partnerAirline]);
            httpClientFactory
                .Setup(factory => factory.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(new StubHttpMessageHandler(
                    """
                    {
                      "flights": []
                    }
                    """,
                    onRequest: request => capturedRequestUri = request.RequestUri)));
            var service = new ExternalFlightSearchService(
                flightRepository.Object,
                partnerAirlineRepository.Object,
                httpClientFactory.Object,
                Mock.Of<ILogger<ExternalFlightSearchService>>());

            var results = await service.SearchConnectionsAsync(
                new ExternalFlightSearchQuery
                {
                    Destination = "LAX",
                    QuantityOfPassengers = 3,
                    MaxTimeWindow = TimeSpan.FromHours(4),
                    Legs =
                    [
                        new ExternalFlightSearchQueryLeg("MIA", latestLegDeparture),
                        new ExternalFlightSearchQueryLeg("BOG", earliestDeparture)
                    ]
                },
                CancellationToken.None);

            Assert.Empty(results);
            Assert.NotNull(capturedRequestUri);
            Assert.Equal("/api/external", capturedRequestUri!.AbsolutePath);
            Assert.Contains("destination=LAX", capturedRequestUri.Query);
            Assert.Contains(
                $"earliestDeparture={EncodeDateTime(earliestDeparture)}",
                capturedRequestUri.Query);
            Assert.Contains(
                $"latestDeparture={EncodeDateTime(latestLegDeparture.Add(TimeSpan.FromHours(4)))}",
                capturedRequestUri.Query);
            Assert.Contains("quantityOfPassengers=3", capturedRequestUri.Query);
            Assert.Contains("apiKey=api-key", capturedRequestUri.Query);
            flightRepository.Verify(
                repository => repository.MaterializeExternalFlightAsync(
                    It.IsAny<ExternalFlight>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task TestSearchConnectionsAsyncWhenPartnerSearchFailsReturnsEmpty()
        {
            var partnerAirline = new PartnerAirline
            {
                Id = 7,
                Name = "Partner",
                Host = "https://partner.example",
                ApiKey = "api-key"
            };
            var partnerAirlineRepository = new Mock<IPartnerAirlineRepository>();
            var flightRepository = new Mock<IFlightRepository>();
            var httpClientFactory = new Mock<IHttpClientFactory>();

            partnerAirlineRepository
                .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([partnerAirline]);
            httpClientFactory
                .Setup(factory => factory.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(new StubHttpMessageHandler(
                    "{}",
                    HttpStatusCode.InternalServerError)));
            var service = new ExternalFlightSearchService(
                flightRepository.Object,
                partnerAirlineRepository.Object,
                httpClientFactory.Object,
                Mock.Of<ILogger<ExternalFlightSearchService>>());

            var results = await service.SearchConnectionsAsync(
                new ExternalFlightSearchQuery
                {
                    Destination = "LAX",
                    QuantityOfPassengers = 1,
                    MaxTimeWindow = TimeSpan.FromHours(4),
                    Legs =
                    [
                        new ExternalFlightSearchQueryLeg(
                            "MIA",
                            new DateTime(2026, 6, 15, 10, 0, 0))
                    ]
                },
                CancellationToken.None);

            Assert.Empty(results);
            flightRepository.Verify(
                repository => repository.MaterializeExternalFlightAsync(
                    It.IsAny<ExternalFlight>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task TestSearchConnectionsAsyncWhenPartnerFlightUuidIsNotGuidAcceptsUuid()
        {
            var generatedFlightGuid = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var partnerAirline = new PartnerAirline
            {
                Id = 7,
                Name = "Partner",
                Host = "https://partner.example/api/external",
                ApiKey = "api-key"
            };
            var partnerAirlineRepository = new Mock<IPartnerAirlineRepository>();
            var flightRepository = new Mock<IFlightRepository>();
            var httpClientFactory = new Mock<IHttpClientFactory>();
            ExternalFlight? capturedFlight = null;
            Guid capturedGuidBeforeMaterialization = Guid.Empty;

            partnerAirlineRepository
                .Setup(repository => repository.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([partnerAirline]);
            flightRepository
                .Setup(repository => repository.MaterializeExternalFlightAsync(
                    It.IsAny<ExternalFlight>(),
                    It.IsAny<CancellationToken>()))
                .Callback<ExternalFlight, CancellationToken>((flight, _) =>
                {
                    capturedFlight = flight;
                    capturedGuidBeforeMaterialization = flight.Guid;
                })
                .ReturnsAsync(generatedFlightGuid);
            httpClientFactory
                .Setup(factory => factory.CreateClient(It.IsAny<string>()))
                .Returns(new HttpClient(new StubHttpMessageHandler(
                    """
                    {
                      "flights": [
                        {
                          "flightUuid": "AIRLINE-ABC-42",
                          "routeId": 15,
                          "departureTime": "2026-06-15T12:00:00",
                          "arrivalTime": "2026-06-15T15:30:00",
                          "duration": "03:30",
                          "departureAirport": {
                            "code": "MIA",
                            "name": "Miami International Airport",
                            "city": "Miami"
                          },
                          "arrivalAirport": {
                            "code": "LAX",
                            "name": "Los Angeles International Airport",
                            "city": "Los Angeles"
                          },
                          "touristPrice": 250,
                          "firstClassPrice": 600,
                          "carryOnPrice": 10,
                          "checkedPrice": 35
                        }
                      ]
                    }
                    """)));
            var service = new ExternalFlightSearchService(
                flightRepository.Object,
                partnerAirlineRepository.Object,
                httpClientFactory.Object,
                Mock.Of<ILogger<ExternalFlightSearchService>>());

            var results = await service.SearchConnectionsAsync(
                new ExternalFlightSearchQuery
                {
                    Destination = "LAX",
                    QuantityOfPassengers = 1,
                    MaxTimeWindow = TimeSpan.FromHours(4),
                    Legs =
                    [
                        new ExternalFlightSearchQueryLeg(
                            "MIA",
                            new DateTime(2026, 6, 15, 10, 0, 0))
                    ]
                },
                CancellationToken.None);

            var result = Assert.Single(results);
            Assert.Equal(generatedFlightGuid, result.Guid);
            Assert.Equal("AIRLINE-ABC-42", result.ExternalFlightUuid);
            Assert.NotNull(capturedFlight);
            Assert.Equal("AIRLINE-ABC-42", capturedFlight!.ExternalFlightUuid);
            Assert.Equal(Guid.Empty, capturedGuidBeforeMaterialization);
        }

        private static string EncodeDateTime(DateTime dateTime)
        {
            return Uri.EscapeDataString(dateTime.ToString("O", CultureInfo.InvariantCulture));
        }

        private sealed class StubHttpMessageHandler : HttpMessageHandler
        {
            private readonly string _json;
            private readonly HttpStatusCode _statusCode;
            private readonly Action<HttpRequestMessage>? _onRequest;

            public StubHttpMessageHandler(
                string json,
                HttpStatusCode statusCode = HttpStatusCode.OK,
                Action<HttpRequestMessage>? onRequest = null)
            {
                _json = json;
                _statusCode = statusCode;
                _onRequest = onRequest;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                _onRequest?.Invoke(request);

                return Task.FromResult(new HttpResponseMessage(_statusCode)
                {
                    Content = new StringContent(_json, Encoding.UTF8, "application/json")
                });
            }
        }
    }
}
