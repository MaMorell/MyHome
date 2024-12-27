using Microsoft.AspNetCore.Mvc;
using MyHome.ApiService.Constants;
using MyHome.ApiService.Extensions;
using MyHome.ApiService.HostedServices;
using MyHome.ApiService.Services;
using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Models.WifiSocket;
using MyHome.Core.Repositories;
using MyHome.Core.Repositories.WifiSocket;
using MyHome.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.RegisterLocalDependencies(builder.Configuration);

//builder.Services.AddHostedService<HeatRegulatorHost>();
//builder.Services.AddHostedService<EnergyConsumptionHost>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("energysupplier/energyprice", async ([FromServices] EnergyPriceService energyPriceService) =>
{
    return await energyPriceService.GetEnergyPriceAsync();
})
.WithName("GetEnergyPrices");

app.MapGet("energysupplier/energymeasurement", async ([FromServices] IRepository<EnergyMeasurement> repository) =>
{
    return await repository.GetByIdAsync(MyHomeConstants.MyTibberHomeId);
})
.WithName("GetLastEnergyMeasurement");

app.MapGet("/wifisocket/{name}/status", async ([FromServices] WifiSocketsService service, WifiSocketName name) =>
{
    return await service.GetStatus(name);
})
.WithName("GetWifiSocketStatus");

app.MapDefaultEndpoints();

app.Run();