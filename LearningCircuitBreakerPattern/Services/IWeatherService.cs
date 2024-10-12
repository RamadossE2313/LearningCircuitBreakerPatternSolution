namespace LearningCircuitBreakerPattern.Services
{
    public interface IWeatherService
    {
        Task<IEnumerable<WeatherForecast>> GetWeatherForecasts();
    }
}
