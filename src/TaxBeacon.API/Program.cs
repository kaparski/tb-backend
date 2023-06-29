using Microsoft.IdentityModel.Logging;
using TaxBeacon.Accounts;
using TaxBeacon.API;
using TaxBeacon.API.Extensions.Logging;
using TaxBeacon.API.Extensions.SwaggerServices;
using TaxBeacon.API.Middlewares;
using TaxBeacon.Common;
using TaxBeacon.Email;
using TaxBeacon.Administration;

var builder = WebApplication.CreateBuilder(args);

builder.AddLogging();

builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddAdministrationServices();
builder.Services.AddCommonServices();
builder.Services.AddAccountsServices();
builder.Services.AddEmailServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUi();
    IdentityModelEventSource.ShowPII = true;
}

app.UseMiddleware<ExceptionMiddleware>();

// app.UseHttpsRedirection();
app.UseCors("DefaultCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapDefaultControllerRoute();

app.Run();
