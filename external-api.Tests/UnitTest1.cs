using System.Collections;
using System.Linq;
using System.Reflection;
using SnoopyAirlines.External.Controllers;
using SnoopyAirlines.External.Services;

namespace external_api.Tests;

public class ExternalApiTests
{
    [Fact]
    public void TryCreateFlightQuery_ValidParameters_ReturnsTrueAndNormalizesCodes()
    {
        var method = typeof(FlightController).GetMethod(
            "TryCreateFlightQuery",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        object?[] args = new object?[]
        {
            "sjo",
            "mad",
            "2026-05-27T06:00",
            "2026-05-27T22:00",
            "2",
            null,
            null
        };

        var result = (bool)method!.Invoke(null, args)!;

        Assert.True(result);

        var flightQuery = args[5];
        Assert.NotNull(flightQuery);
        Assert.Equal("SJO", flightQuery!.GetType().GetProperty("Origin")!.GetValue(flightQuery));
        Assert.Equal("MAD", flightQuery!.GetType().GetProperty("Destination")!.GetValue(flightQuery));
        Assert.Equal(2, flightQuery!.GetType().GetProperty("QuantityOfPassengers")!.GetValue(flightQuery));
    }

    [Fact]
    public void FormatDuration_WhenDurationIsTwoHoursAndFiveMinutes_ReturnsZeroPaddedString()
    {
        var method = typeof(FlightService).GetMethod(
            "FormatDuration",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        var formatted = (string)method!.Invoke(null, new object[] { 125 })!;

        Assert.Equal("02:05", formatted);
    }
}
