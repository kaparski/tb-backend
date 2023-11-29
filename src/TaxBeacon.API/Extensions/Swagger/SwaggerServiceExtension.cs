using Ardalis.SmartEnum;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;

namespace TaxBeacon.API.Extensions.Swagger;

public static class SwaggerServiceExtension
{
    internal static void AddSwagger(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration
            .GetSection("SwaggerAzureAd")
            .Get<SwaggerAzureAdOptions>();

        services.AddSwaggerGen(x =>
        {
            x.SwaggerDoc("v1",
                new OpenApiInfo
                {
                    Title = "TaxBeacon API",
                    Description = "TaxBeacon API Description",
                    Version = "v1"
                });
            x.AddSecurityDefinition(name: "OAuth2", securityScheme: new OpenApiSecurityScheme
            {
                Name = "OAuth2 Authorization",
                Description = "B2C Authorization",
                Type = SecuritySchemeType.OAuth2,
                In = ParameterLocation.Header,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri(settings?.AuthorizationUrl ?? string.Empty),
                        TokenUrl = new Uri(settings?.TokenUrl ?? string.Empty)
                    },
                }
            });
            x.AddSecurityDefinition(name: "Bearer", securityScheme: new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter the Bearer Authorization string as following: `Bearer <Generated-JWT-Token>`",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            x.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Name = "OAuth2",
                        Reference = new OpenApiReference
                        {
                            Id = "OAuth2", Type = ReferenceType.SecurityScheme
                        },
                    },
                    settings?.ApiScopes ?? Array.Empty<string>()
                },
                {
                    new OpenApiSecurityScheme
                    {
                        Name = "ApiKey",
                        In = ParameterLocation.Header,
                        Reference = new OpenApiReference
                        {
                            Id = "Bearer", Type = ReferenceType.SecurityScheme
                        }
                    },
                    new List<string>()
                }
            });
            x.DescribeAllParametersInCamelCase();
            x.SupportNonNullableReferenceTypes();
            var commentsFileName = Assembly.GetExecutingAssembly().GetName().Name + ".xml";
            x.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, commentsFileName));
            x.SchemaFilter<SmartEnumSchemaFilter>();
        });
    }

    internal static void UseSwaggerUi(this IApplicationBuilder app, IConfiguration configuration)
    {
        var settings = configuration
            .GetSection("SwaggerAzureAd")
            .Get<SwaggerAzureAdOptions>();
        app.UseSwagger();
        app.UseSwaggerUI(x =>
        {
            x.OAuthClientId(settings?.ClientId);
            x.OAuthScopes(settings?.ApiScopes);
            x.OAuthUsePkce();
            x.OAuthScopeSeparator(" ");
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

public sealed class SmartEnumSchemaFilter: ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (!IsTypeDerivedFromGenericType(context.Type, typeof(SmartEnum<>)))
        {
            return;
        }

        var smartEnumList = context.Type
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(x => x.FieldType == context.Type)
            .Select(x => x.GetValue(null));

        // See https://swagger.io/docs/specification/data-models/enums/
        schema.Type = "string";
        schema.Enum = smartEnumList.Select(x => new OpenApiString(x?.ToString())).ToList<IOpenApiAny>();
        schema.Properties = null;
    }

    private static bool IsTypeDerivedFromGenericType(Type? typeToCheck, Type genericType)
    {
        if (typeToCheck is null || typeToCheck == typeof(object))
        {
            return false;
        }

        return typeToCheck.IsGenericType
               && typeToCheck.GetGenericTypeDefinition() == genericType
               || IsTypeDerivedFromGenericType(typeToCheck.BaseType, genericType);
    }
}

