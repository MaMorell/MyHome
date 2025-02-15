using MyHome.ApiService.HostedServices.Services;
using MyHome.Core.Http;
using MyHome.Core.Interfaces;
using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Options;
using MyHome.Core.Repositories;
using MyHome.Core.Repositories.EnergySupplier;
using MyHome.Core.Repositories.FloorHeating;
using MyHome.Core.Repositories.HeatPump;
using MyHome.Core.Services;
using System.Net.Http.Headers;
using Tibber.Sdk;

namespace MyHome.ApiService.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static IServiceCollection RegisterLocalDependencies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IRepository<AuditEvent>, InMemoryRepository<AuditEvent>>();

        services
            .AddWifiSocketServices(configuration)
            .AddEnergySupplierServices(configuration)
            .AddHeatPumpServices(configuration)
            .AddFloorHeatServices(configuration);

        return services;
    }

    private static IServiceCollection AddEnergySupplierServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<EnergySupplierService>();
        services.AddScoped<IEnergyRepository, EnergyRepository>();
        services.AddSingleton<IObserver<RealTimeMeasurement>, EnergyConsumptionObserver>();
        services.AddSingleton<IRepository<EnergyMeasurement>, InMemoryRepository<EnergyMeasurement>>();

        services.AddTibberClient(configuration);

        return services;
    }

    private static IServiceCollection AddFloorHeatServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FloorHeaterOptions>(configuration.GetSection(FloorHeaterOptions.ConfigurationSection));
        services.AddScoped<FloorHeaterRepository>();

        return services;
    }

    private static IServiceCollection AddTibberClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped(serviceProvider =>
        {
            var accessToken = configuration["TibberApiClient:AccessToken"]
                ?? throw new KeyNotFoundException("TibberApiClient:AccessToken is missing in appsettings.json");

            var userAgent = new ProductInfoHeaderValue("My-home-automation-system", "1.2");

            return new TibberApiClient(accessToken, userAgent);
        });

        return services;
    }

    private static IServiceCollection AddHeatPumpServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<HeatRegulatorService>();
        services.AddScoped<NibeClient>();
        services.AddMyUplinkClient(configuration);
        return services;
    }

    private static IServiceCollection AddMyUplinkClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddTransient<OAuthHandler<MyUplinkOptions>>();

        var myUplinkOptionsSection = configuration.GetSection(MyUplinkOptions.UplinkOptions);
        services.Configure<ExternalClientOptions<MyUplinkOptions>>(myUplinkOptionsSection);

        var options = myUplinkOptionsSection.Get<MyUplinkOptions>() ?? throw new InvalidOperationException($"Failed to get {nameof(MyUplinkOptions)}");
        services.AddHttpClient(options.Name, client =>
        {
            client.BaseAddress = new Uri(options.BaseAddress);
        }).AddHttpMessageHandler<OAuthHandler<MyUplinkOptions>>();

        services.AddScoped<AuditedHttpClient<MyUplinkOptions>>();

        return services;
    }
}