using Microsoft.ApplicationInsights.AspNetCore.Extensions;

namespace TaxBeacon.API.Extensions.Logging;

public static class LoggingExtensions
{
    public static void AddLogging(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
        if (!string.IsNullOrEmpty(connectionString))
        {
            // Add Application Insights Logging
            var options = new ApplicationInsightsServiceOptions
            {
                ConnectionString = connectionString
            };
            builder.Services.AddApplicationInsightsTelemetry(options);
            builder
                .Logging
                .AddApplicationInsights(configureTelemetryConfiguration: opt => opt.ConnectionString = connectionString,
                    configureApplicationInsightsLoggerOptions: (options) => { });
        }

    }
}