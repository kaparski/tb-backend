using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;

namespace TaxBeacon.API.Extensions.SwaggerServices;

public static class SwaggerServiceExtension
{
    internal static void AddSwagger(this IServiceCollection services) =>
        services.AddSwaggerGen(x =>
        {
            x.SwaggerDoc("v1",
                new OpenApiInfo
                {
                    Title = "TaxBeacon API",
                    Description = "TaxBeacon API Description",
                    Version = "v1",
                });
            x.DescribeAllParametersInCamelCase();
            x.SchemaFilter<EnumSchemaFilter>();
            var commentsFileName = Assembly.GetExecutingAssembly().GetName().Name + ".xml";
            x.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, commentsFileName));
        });

    internal static void UseSwaggerUi(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(x =>
        {
            x.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            x.DefaultModelExpandDepth(4);
            x.DefaultModelRendering(ModelRendering.Example);
            x.DisplayOperationId();
            x.DisplayRequestDuration();
            x.DocExpansion(DocExpansion.None);
            x.EnableDeepLinking();
            x.EnableFilter();
            x.ShowExtensions();
        });
    }
}
