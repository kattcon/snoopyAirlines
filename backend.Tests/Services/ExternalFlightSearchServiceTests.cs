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
        public async Task SearchConnectionsAsync_AcceptsNonGuidPartnerFlightUuid()
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

        private sealed class StubHttpMessageHandler : HttpMessageHandler
        {
            private readonly string _json;

            public StubHttpMessageHandler(string json)
            {
                _json = json;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(_json, Encoding.UTF8, "application/json")
                });
            }
        }
    }
}
