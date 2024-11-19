using Microsoft.AspNetCore.Mvc;

[Route("api/weather")]
[ApiController]
public class WeatherController : ControllerBase
{
    private readonly WeatherService _weatherService;

    public WeatherController(WeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    [HttpGet]
    public async Task<IActionResult> GetWeather(string city, string country, string apiKey)
    {
        try
        {
            // Call the weather service to fetch the weather data
            var result = await _weatherService.GetWeatherAsync(city, country, apiKey);

            // Check if the result contains a rate limit exceeded or invalid API key message
            if (result.Contains("Failed"))
            {
                return StatusCode(500, result);
            }
            else if (result.Contains("Invalid API Key."))
            {
                return BadRequest(result);
            }
            else if (result.Contains("Rate limit exceeded."))
            {
                return StatusCode(429, "Rate limit exceeded. You can retry later.");
            }
            else
            {
                return Ok(result);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            // Return a generic error message to the client
            return StatusCode(500, $"An unexpected error occurred. Please try again later. -> {ex.Message}");
        }
    }
}
