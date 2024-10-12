using LearningCircuitBreakerPattern.Services;
using Polly;
using Polly.Extensions.Http;

// Retry policy added when transient error occurs (status codes 5xx, 408), it will retry 2 times after 1st attempt after 2 seconds and 2nd attempt after 4 seconds
static IAsyncPolicy<HttpResponseMessage> RetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.InternalServerError)
        .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2,retryAttempt)), onRetryAsync: async (exception, timeSpan, retryCount, context) =>
        {
            Console.WriteLine($"Retry {retryCount} after {timeSpan.Seconds} seconds due to: {exception.Exception?.Message ?? "Unknown issue"}");
        });
}

// Circut breaker policy added when transient error occurs (status codes 5xx, 408), if continuesly 5 request breaking means, later it will take 2 seconds break and it will not the api at all.
// After 2 seconds if new request comes, it will call the api which is mentioned in the httpclient
static IAsyncPolicy<HttpResponseMessage> CircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5, // Number of exceptions before breaking
            durationOfBreak: TimeSpan.FromSeconds(2), // Duration of the break
            onBreak: (outcome, breakDelay) =>
            {
                Console.WriteLine($"Circuit broken! Breaking for {breakDelay.TotalSeconds} seconds due to: {outcome.Exception?.Message ?? outcome.Result.StatusCode.ToString()}");
            },
            onReset: () =>
            {
                Console.WriteLine("Circuit reset. Requests will flow again.");
            },
            onHalfOpen: () =>
            {
                Console.WriteLine("Circuit in half-open state. Testing if requests can flow.");
            }
        );
}


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<IWeatherService, WeatherService>(options =>
{
    options.BaseAddress = new Uri("https://localhost:7129/weatherforecast");
})
.AddPolicyHandler(RetryPolicy())
.AddPolicyHandler(CircuitBreakerPolicy());

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
