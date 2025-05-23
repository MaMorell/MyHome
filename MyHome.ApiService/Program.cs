using Microsoft.AspNetCore.Mvc;
using MyHome.ApiService.Constants;
using MyHome.ApiService.Extensions;
using MyHome.Core.Interfaces;
using MyHome.Core.Models.Audit;
using MyHome.Core.Models.Entities;
using MyHome.Core.Models.Entities.Profiles;
using MyHome.Core.Services;
using MyHome.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);


// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddMemoryCache();

builder.Services.RegisterLocalDependencies(builder.Configuration);
//builder.Services.AddHostedService<HeatRegulatorHost>();
//builder.Services.AddHostedService<EnergyConsumptionWatcherHost>();

var app = builder.Build();


// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("energysupplier/energyprice", async ([FromServices] EnergySupplierService energyPriceService) =>
{
    return await energyPriceService.GetFutureEnergyPricesAsync();
})
.WithName("GetEnergyPrices");

app.MapGet("energysupplier/energymeasurement", async ([FromServices] IRepository<EnergyMeasurement> repository) =>
{
    return await repository.GetByIdAsync(MyHomeConstants.MyTibberHomeId);
})
.WithName("GetLastEnergyMeasurement");

app.MapGet("/energysupplier/consumption/currentmonth/top", async (
    [FromServices] EnergySupplierService energyPriceService,
    [FromQuery] int limit = 3,
    [FromQuery] bool onlyDuringWeekdays = true) =>
{
    return await energyPriceService.GetTopConumptionAsync(limit, onlyDuringWeekdays);
})
.WithName("GetTopMonthlyConsumption");

app.MapGet("/wifisocket/{name}/status", async ([FromServices] IWifiSocketsService service, string name) =>
{
    return await service.GetStatus(name);
})
.WithName("GetWifiSocketStatus");

app.MapGet("/auditevents", async ([FromServices] IRepository<AuditEvent> repository, [FromQuery] int count) =>
{
    var result = await repository.GetAllAsync();
    if (result is null)
    {
        return [];
    }

    return result.OrderByDescending(r => r.Timestamp).Take(count);
})
.WithName("GetAuditEvents");

app.MapGet("/profiles/pricethearsholds", async ([FromServices] PriceThearsholdsService service) =>
{
    return await service.GetThearsholdsProfile();
})
.WithName("GetThearsholdsProfile");

app.MapPut("/profiles/pricethearsholds", async ([FromServices] IRepository<PriceThearsholdsProfile> repository, [FromBody] PriceThearsholdsProfile profile) =>
{
    await repository.UpsertAsync(profile);
})
.WithName("UpdateThearsholdsProfile");

app.MapDefaultEndpoints();

app.Run();
