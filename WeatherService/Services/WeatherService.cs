using Newtonsoft.Json.Linq;
using RestSharp;
using System.Collections.Concurrent;
using WeatherService.Models;

namespace WeatherStationService
{
    public class WeatherService
    {
        private readonly string[] _apiKeys;
        private readonly int _maxRequestsPerHour;
        private static readonly ConcurrentDictionary<string, (int Count, DateTime FirstRequestTime)> _requestCounts = new ConcurrentDictionary<string, (int, DateTime)>();
        public WeatherService(IConfiguration configuration)
        {
            _apiKeys = configuration.GetSection("ApiKeys").Get<string[]>();
            _maxRequestsPerHour = configuration.GetValue<int>("RateLimit:MaxRequestsPerHour");
        }
        public async Task<GetWeatherResponse> GetWeatherAsync(string city, string country, string apiKey)
        {
            var weather_response = new GetWeatherResponse();
            try
            {
                // Check if the API key is valid
                if (!_apiKeys.Contains(apiKey))
                {
                    weather_response.Description = "Invalid API Key.";
                    return weather_response;
                }

                // Check if the rate limit has been exceeded
                DateTime? retryAfter = CanRequest(apiKey);
                if (retryAfter != null)
                {
                    weather_response.Description = $"Rate limit exceeded. You can retry after {retryAfter} UTC.";
                    return weather_response;
                }
                // Call OpenWeatherMap API
                var client = new RestClient("https://api.openweathermap.org");
                var request = new RestRequest("data/2.5/weather")
                    .AddParameter("q", $"{city},{country}")
                    .AddParameter("appid", "8b7535b42fe1c551f18028f64e8688f7");  // OpenWeatherMap API key

                var response = await client.ExecuteGetAsync(request);
                Console.WriteLine(DateTime.UtcNow);
                var json = JObject.Parse(response.Content);

                if (response.IsSuccessful)
                {
                    var description = json.SelectToken("weather[0].description")?.ToString();
                    var iconID = json.SelectToken("weather[0].icon")?.ToString();
                    weather_response.IconId = iconID;
                    weather_response.Description = $"{description}" ?? "No weather description available.";
                    return weather_response;
                }
                else 
                {
                    var ErrorMessage = json.SelectToken("message")?.ToString();
                    weather_response.Description = $"Failed : {ErrorMessage}" ?? "Failed to fetch weather data.";
                    return weather_response ;
                }

            }
            catch (Newtonsoft.Json.JsonReaderException ex)
            {
                // Handle JSON parsing errors (e.g., if OpenWeatherMap returns malformed JSON)
                weather_response.Description = $"Error parsing the weather data: {ex.Message}. Please try again later.";
                return weather_response;
            }
            catch (Exception ex)
            {
                // Catch any other exceptions
                weather_response.Description = $"An unexpected error occurred: {ex.Message}. Please try again later.";
                return weather_response;
            }
        }

        private DateTime? CanRequest(string apiKey)
        {
            var now = DateTime.UtcNow;
            // Get the current count and the first request time for this API key
            var (count, firstRequestTime) = _requestCounts.GetOrAdd(apiKey, (0, now));

            if (now.Subtract(firstRequestTime).TotalHours >= 1)
            {
                // Reset the count and first request time after an hour has passed
                _requestCounts[apiKey] = (1, now);
                return null;
            }

            if (count >= _maxRequestsPerHour)
            {
                // Calculate when the user can retry after reaching the rate limit
                DateTime retryAfter = firstRequestTime.AddHours(1);
                return retryAfter;
            }

            // Increment the request count
            _requestCounts[apiKey] = (count + 1, firstRequestTime);
            return null;
        }
    }
}
