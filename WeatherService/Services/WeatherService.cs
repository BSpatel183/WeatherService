using Newtonsoft.Json.Linq;
using RestSharp;
using System.Collections.Concurrent;

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
        public async Task<string> GetWeatherAsync(string city, string country, string apiKey)
        {
            try
            {
                // Check if the API key is valid
                if (!_apiKeys.Contains(apiKey))
                {
                    return "Invalid API Key.";
                }

                // Check if the rate limit has been exceeded
                DateTime? retryAfter = CanRequest(apiKey);
                if (retryAfter != null)
                {
                    return $"Rate limit exceeded. You can retry after {retryAfter} UTC.";
                }
                    // Call OpenWeatherMap API
                    var client = new RestClient("https://api.openweathermap.org");
                var request = new RestRequest("data/2.5/weather")
                    .AddParameter("q", $"{city},{country}")
                    .AddParameter("appid", "8b7535b42fe1c551f18028f64e8688f7");  // OpenWeatherMap API key

                var response = await client.ExecuteGetAsync(request);

                if (response.IsSuccessful)
                {
                    Console.WriteLine(DateTime.UtcNow);
                    var json = JObject.Parse(response.Content);
                    var description = json.SelectToken("weather[0].description")?.ToString();
                    return description ?? "No weather description available.";
                }

                return "Failed to fetch weather data.";
            }
            catch (Newtonsoft.Json.JsonReaderException ex)
            {
                // Handle JSON parsing errors (e.g., if OpenWeatherMap returns malformed JSON)
                return $"Error parsing the weather data: {ex.Message}. Please try again later.";
            }
            catch (Exception ex)
            {
                // Catch any other exceptions
                return $"An unexpected error occurred: {ex.Message}. Please try again later.";
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
