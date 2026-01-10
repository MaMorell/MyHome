using Microsoft.AspNetCore.Routing;
using MyHome.ApiService.HostedServices.Services;
using MyHome.Core.Interfaces;
using MyHome.Core.Models.Audit;
using MyHome.Core.Models.Entities;
using MyHome.Core.Models.Entities.Profiles;
using MyHome.Core.PriceCalculations;
using MyHome.Core.Services;
using MyHome.Data;
using MyHome.Data.Http;
using MyHome.Data.Integrations.EnergySupplier;
using MyHome.Data.Integrations.HeatPump;
using MyHome.Data.Integrations.Thermostats;
using MyHome.Data.Options;
using MyHome.Data.Repositories;
using System.Net.Http.Headers;
using Tibber.Sdk;

namespace MyHome.ApiService.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static IServiceCollection RegisterLocalDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IRepository<AuditEvent>, InMemoryRepository<AuditEvent>>();
        services.AddSingleton<IRepository<EnergyMeasurement>, InMemoryRepository<EnergyMeasurement>>();
        services.AddSingleton<IRepository<SensorData>, InMemoryRepository<SensorData>>();
        services.AddScoped<IRepository<DeviceSettingsProfile>, FileRepository<DeviceSettingsProfile>>();
        services.AddScoped<IRepository<PriceThearsholdsProfile>, FileRepository<PriceThearsholdsProfile>>();
        services.AddScoped<IEnergySupplierRepository, TibberEnergySupplierRepository>();

        services.AddScoped<EnergySupplierService>();
        services.AddScoped<HouseAutomationService>();
        services.AddSingleton<ISmartHubClient, MqttClient>();

        services.AddScoped<DeviceSettingsFactory>();
        services.AddScoped<PriceLevelGenerator>();

        services.AddSingleton<IObserver<RealTimeMeasurement>, EnergyConsumptionObserver>();
        services.AddScoped<EnergyConsumptionListener>();


        services.Configure<ThermostatTuyaOptions>(configuration.GetSection(ThermostatTuyaOptions.ConfigurationSection));
        services.AddKeyedScoped<IThermostatClient, TuyaThermostatClient>("tuya");

        services.AddEbecoClient(configuration);
        services.AddMyUplinkClient(configuration);
        services.AddTibberClient(configuration);

        return services;
    }

    private static IServiceCollection AddTibberClient(this IServiceCollection services, IConfiguration configuration)
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

    private static IServiceCollection AddMyUplinkClient(this IServiceCollection services, IConfiguration configuration)
    {
        var upLinkOptionsSection = configuration.GetSection("UpLinkOptions");
        var upLinkOptions = upLinkOptionsSection.Get<HeatPumpClientOptions>() ?? throw new InvalidOperationException($"Failed to get {nameof(HeatPumpClientOptions)}");
        services.Configure<HeatPumpClientOptions>(upLinkOptionsSection);

        services.AddTransient<OAuthHandler>();
        services
            .AddHttpClient<IHeatPumpClient, NibeClient>(httpClient => httpClient.BaseAddress = upLinkOptions.BaseAddress)
            .AddHttpMessageHandler<OAuthHandler>();


        return services;
    }

    private static IServiceCollection AddEbecoClient(this IServiceCollection services, IConfiguration configuration)
    {
        var ebecoOptionsSection = configuration.GetSection("ThermostatEbeco");
        var ebecoOptions = ebecoOptionsSection.Get<EbecoOptions>() ?? throw new InvalidOperationException($"Failed to get {nameof(EbecoOptions)}");
        services.Configure<EbecoOptions>(ebecoOptionsSection);
        services.AddTransient<EbecoAuthHandler>();

        // First: Register HttpClient with the concrete type
        services
            .AddHttpClient<EbecoConnectClient>(httpClient =>
                httpClient.BaseAddress = ebecoOptions.BaseAddress)
            .AddHttpMessageHandler<EbecoAuthHandler>();

        services.AddKeyedScoped<IThermostatClient>("ebeco", (sp, key) =>
            sp.GetRequiredService<EbecoConnectClient>());

        return services;
    }
}