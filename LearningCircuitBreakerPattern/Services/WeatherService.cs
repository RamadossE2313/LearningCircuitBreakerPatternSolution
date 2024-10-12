
using System.Text.Json;

namespace LearningCircuitBreakerPattern.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public WeatherService(HttpClient httpClient)
        {
           _httpClient = httpClient;
            _jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<IEnumerable<WeatherForecast>> GetWeatherForecasts()
        {
            var response = await _httpClient.SendAsync(new HttpRequestMessage());
            response.EnsureSuccessStatusCode();
            var weatherForecasts = await response.Content.ReadFromJsonAsync<IEnumerable<WeatherForecast>>(_jsonSerializerOptions);

            if (weatherForecasts == null)
            {
                return new List<WeatherForecast>();
            }

            return weatherForecasts;
        }
    }
}
