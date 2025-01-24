using Microsoft.Extensions.Configuration;

public class WeatherServiceTests
{
    private readonly WeatherStationService.WeatherService _weatherService;
    private readonly string validKey = "Api-key-1";
    public WeatherServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            { "ApiKeys:0", "Api-key-1" },
            { "ApiKeys:1", "Api-key-2" },
            { "OpenWeatherMapApiKey", "8b7535b42fe1c551f18028f64e8688f7" },
            { "RateLimit:MaxRequestsPerHour", "5" }
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(initialData: inMemorySettings)
            .Build();
        var httpClient = new HttpClient();
        // Instantiate WeatherService with the actual configuration
        _weatherService = new WeatherStationService.WeatherService(configuration, httpClient);
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
        // Act
        var result = await _weatherService.GetWeatherAsync("InvalidCity", "InvalidCountry", validKey);

        // Assert
        Assert.Equal("Failed : city not found", result.Description);
    }

    [Fact]
    public async Task CanRequest_AfterRateLimitReachedWithinHour_ReturnsRetryTimeAsync()
    {
        var utcNow = DateTime.UtcNow;

        // Simulate first 5 requests
        for (int i = 0; i < 5; i++)
        {
            await _weatherService.CanRequestAsync(validKey, utcNow);
        }

        // Act: Simulate the sixth request within the same hour
        var retryTime = await _weatherService.CanRequestAsync(validKey, utcNow);

        // Assert: Ensure that retry time is returned (Rate limit reached)
        Assert.NotNull(retryTime);
        Assert.Equal(utcNow.AddHours(1).ToString("yyyy-MM-dd HH:mm"), retryTime?.ToString("yyyy-MM-dd HH:mm"));
    }

    [Fact]
    public async Task CanRequest_BeforeRateLimitReached_ReturnsNullAsync()
    {
        // Act: Simulate 2 requests (below the rate limit of 5)
        await _weatherService.CanRequestAsync(validKey, DateTime.UtcNow); // 1st request
        var retryTime = await _weatherService.CanRequestAsync(validKey, DateTime.UtcNow.AddMinutes(5)); // 2nd request

        // Assert: Since we're under the rate limit, it should not return a retry time
        Assert.Null(retryTime);
    }

    [Fact]
    public async Task CanRequest_AfterOneHourReset_ReturnsNullAsync()
    {
        var utcNow = DateTime.UtcNow;

        // Simulate 5 requests
        for (int i = 0; i < 5; i++)
        {
            await _weatherService.CanRequestAsync(validKey, utcNow);
        }

        // Simulate that more than an hour has passed (to reset the count)
        var oneHourLater = utcNow.AddHours(1);

        // Act: Now that an hour has passed, the count should reset, and it should allow a request.
        var retryTime = await _weatherService.CanRequestAsync(validKey, oneHourLater);

        // Assert: After the hour, retry time should be null since the counter is reset
        Assert.Null(retryTime);
    }
}
