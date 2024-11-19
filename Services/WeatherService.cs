using Newtonsoft.Json.Linq;
using RestSharp;

public class WeatherService
{
    public async Task<string> GetWeatherAsync(string city, string country, string apiKey)
    {
        try
        {
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
}
