using Xunit;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

public class WeatherServiceTests
{
    private readonly WeatherStationService.WeatherService _weatherService;

    public WeatherServiceTests()
    {
        // Build configuration from appsettings.json (using the default path)
        var configuration = new ConfigurationBuilder()
            .SetBasePath(System.IO.Directory.GetCurrentDirectory()) // Set the base path to the current directory
            .AddJsonFile("appsettings.json") // Load the appsettings.json file
            .Build();

        // Instantiate WeatherService with the actual configuration
        _weatherService = new WeatherStationService.WeatherService(configuration);
    }

    [Fact]
    public async Task GetWeatherAsync_InvalidApiKey_ReturnsInvalidApiKeyMessage()
    {
        // Act
        var result = await _weatherService.GetWeatherAsync("New York", "US", "invalid-key");

        // Assert
        Assert.Equal("Invalid API Key.", result.Description);
    }

    [Fact]
    public async Task GetWeatherAsync_ExceedRateLimit_ReturnsRateLimitExceededMessage()
    {
        // Arrange
        var validKey = "Api-key-1";

        // Simulate exceeding the rate limit by making multiple requests
        for (int i = 0; i < 6; i++)
        {
            await _weatherService.GetWeatherAsync("New York", "US", validKey);
        }

        // Act
        var result = await _weatherService.GetWeatherAsync("New York", "US", validKey);

        // Assert
        Assert.Contains("Rate limit exceeded.", result.Description);
    }

    [Fact]
    public async Task GetWeatherAsync_ValidRequest_ReturnsWeatherDescription()
    {
        // Arrange
        var validKey = "Api-key-1";

        // Act
        var result = await _weatherService.GetWeatherAsync("London", "UK", validKey);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual("Invalid API Key.", result.Description);
        Assert.NotEqual("Rate limit exceeded.", result.Description);
    }

    [Fact]
    public async Task GetWeatherAsync_FailedApiCall_ReturnsErrorMessage()
    {
        // Arrange
        var validKey = "Api-key-2";

        // Act
        var result = await _weatherService.GetWeatherAsync("InvalidCity", "InvalidCountry", validKey);

        // Assert
        Assert.Equal("Failed : city not found", result.Description);
    }
}
