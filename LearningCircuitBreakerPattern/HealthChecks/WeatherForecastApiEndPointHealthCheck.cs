using LearningCircuitBreakerPattern.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace LearningCircuitBreakerPattern.HealthChecks
{
    public class WeatherForecastApiEndPointHealthCheck : IHealthCheck
    {
        private readonly IWeatherService _weatherService;

        public WeatherForecastApiEndPointHealthCheck(IWeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var weatherForecasts = await _weatherService.GetWeatherForecasts();
                if (weatherForecasts != null && weatherForecasts.Any())
                {
                    return HealthCheckResult.Healthy("WeatherForecast service is working.");
                }
                return HealthCheckResult.Degraded("WeatherForecast service returned no data.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("WeatherForecast service is unavailable.", ex);
            }
        }
    }
}
