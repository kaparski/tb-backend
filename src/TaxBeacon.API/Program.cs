using OwaspHeaders.Core.Extensions;
using TaxBeacon.Accounts;
using TaxBeacon.Administration;
using TaxBeacon.API;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Extensions;
using TaxBeacon.API.Extensions.Swagger;
using TaxBeacon.API.Extensions.KeyVault;
using TaxBeacon.API.FeatureManagement;
using TaxBeacon.API.Middlewares;
using TaxBeacon.Common;
using TaxBeacon.DocumentManagement;
using TaxBeacon.Email;

var builder = WebApplication.CreateBuilder(args);

builder.AddLogging();

if (!builder.Environment.IsDevelopment())
{
    builder.Configuration.AddKeyVault();
}

builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddAdministrationServices();
builder.Services.AddCommonServices();
builder.Services.AddAccountsServices();
builder.Services.AddEmailServices();
builder.Services.AddDocumentManagementServices();
builder.Services.AddResponseCaching();

var app = builder.Build();

app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsCloudDevelopment())
{
    app.UseSwaggerUi(app.Configuration);

    app
        .MapGet("/environment", [HasPermissions(TaxBeacon.Common.Permissions.QualityAssurance.Full)] () =>
            builder.Configuration.GetDebugView())
        .WithName("GetEnvironmentVariables")
        .WithTags("Development")
        .RequireAuthorization();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseCors("DefaultCorsPolicy");
app.UseResponseCaching();
app.UseAuthentication();
app.UseAuthorization();

var config = SecureHeadersMiddlewareExtensions.BuildDefaultConfiguration();
config.UseCacheControl = false;
app.UseSecureHeadersMiddleware(config);

app.UseMiddleware<FeatureFlagMiddleware>();
app.MapControllers();
app.MapDefaultControllerRoute();

app.Run();
