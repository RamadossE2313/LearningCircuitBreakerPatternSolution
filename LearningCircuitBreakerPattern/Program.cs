using LearningCircuitBreakerPattern.HealthChecks;
using LearningCircuitBreakerPattern.Services;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Polly;
using Polly.Extensions.Http;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Azure.Cosmos;

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

// health check end point result format
static Task WriteResponse(HttpContext context, HealthReport healthReport)
{
    context.Response.ContentType = "application/json; charset=utf-8";

    var options = new JsonWriterOptions { Indented = true };

    using var memoryStream = new MemoryStream();
    using (var jsonWriter = new Utf8JsonWriter(memoryStream, options))
    {
        jsonWriter.WriteStartObject();
        jsonWriter.WriteString("status", healthReport.Status.ToString());
        jsonWriter.WriteStartObject("results");

        foreach (var healthReportEntry in healthReport.Entries)
        {
            jsonWriter.WriteStartObject(healthReportEntry.Key);
            jsonWriter.WriteString("status",
                healthReportEntry.Value.Status.ToString());
            jsonWriter.WriteString("description",
                healthReportEntry.Value.Description);
            jsonWriter.WriteStartObject("data");

            foreach (var item in healthReportEntry.Value.Data)
            {
                jsonWriter.WritePropertyName(item.Key);

                JsonSerializer.Serialize(jsonWriter, item.Value,
                    item.Value?.GetType() ?? typeof(object));
            }

            jsonWriter.WriteEndObject();
            jsonWriter.WriteEndObject();
        }

        jsonWriter.WriteEndObject();
        jsonWriter.WriteEndObject();
    }

    return context.Response.WriteAsync(
        Encoding.UTF8.GetString(memoryStream.ToArray()));
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// adding retry and circuit breaker policy
builder.Services.AddHttpClient<IWeatherService, WeatherService>(options =>
{
    options.BaseAddress = new Uri("https://localhost:7129/weatherforecast");
})
.AddPolicyHandler(RetryPolicy())
.AddPolicyHandler(CircuitBreakerPolicy());

string cosmosDbConnectionString = "AccountEndpoint=https://test.documents.azure.com:443/;AccountKey=123324324234;Database=employee;";

// health check
builder.Services.AddHealthChecks()
    // added custom health check for the api
    .AddCheck<WeatherForecastApiEndPointHealthCheck>("WeatherForcastApi")
    // added health check for cosmosdb
    .AddAzureCosmosDB(svc => new CosmosClient(cosmosDbConnectionString), name: "cosmodb");

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    ResponseWriter = WriteResponse
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
