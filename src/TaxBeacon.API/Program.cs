using Microsoft.IdentityModel.Logging;
using TaxBeacon.API;
using TaxBeacon.API.Extensions.SwaggerServices;
using TaxBeacon.API.Middlewares;
using TaxBeacon.Common;
using TaxBeacon.UserManagement;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddUserManagementServices();
builder.Services.AddCommonServices();

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
