using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using TestTasks.WeatherFromAPI;
using Xunit;

public class WeatherManagerTests
{
    private HttpClient CreateMockHttpClient(string responseContent)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(responseContent) // Fresh instance per call
            });

        return new HttpClient(handlerMock.Object);
    }

    [Fact]
    public async Task CompareWeather_InvalidCity_ThrowsArgumentException()
    {
        var mockHttp = CreateMockHttpClient("[]");
        var manager = new WeatherManager(mockHttp);

        await Assert.ThrowsAsync<ArgumentException>(() => manager.CompareWeather("invalidcity", "kyiv,ua", 3));
    }

    [Fact]
    public async Task CompareWeather_InvalidDayCount_ThrowsArgumentException()
    {
        var mockHttp = CreateMockHttpClient("{\"lat\":50.45,\"lon\":30.52}");
        var manager = new WeatherManager(mockHttp);

        await Assert.ThrowsAsync<ArgumentException>(() => manager.CompareWeather("kyiv,ua", "london,gb", 10));
    }

    [Fact]
    public async Task CompareWeather_ValidInput_ReturnsResult()
    {
        var manager = new WeatherManager();

        var result = await manager.CompareWeather("kyiv,ua", "london,gb", 1);

        Assert.Equal("kyiv,ua", result.CityA);
        Assert.Equal("london,gb", result.CityB);
    }
}
