using Microsoft.ApplicationInsights.AspNetCore.Extensions;

namespace TaxBeacon.API.Extensions.Logging
{
    public static class LoggingExtensions
    {
        public static void AddLogging(this WebApplicationBuilder builder)
        {
            var connestionString = builder.Configuration["ApplicationInsights:ConnectionString"];
            // Add Application Insights Logging
            var options = new ApplicationInsightsServiceOptions
            {
                ConnectionString = connestionString
            };
            builder.Services.AddApplicationInsightsTelemetry(options);
            builder
                .Logging
                .AddApplicationInsights(configureTelemetryConfiguration: opt => opt.ConnectionString = connestionString,
                                        configureApplicationInsightsLoggerOptions: (options) => { });

        }
    }
}
