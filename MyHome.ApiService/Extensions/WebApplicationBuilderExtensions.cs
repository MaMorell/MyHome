using Microsoft.Extensions.Options;
using MyHome.ApiService.HostedServices.Services;
using MyHome.Core.Interfaces;
using MyHome.Core.Models.Audit;
using MyHome.Core.Models.Entities;
using MyHome.Core.Models.Entities.Profiles;
using MyHome.Core.Options;
using MyHome.Core.PriceCalculations;
using MyHome.Core.Services;
using MyHome.Data.Http;
using MyHome.Data.Integrations.EnergySupplier;
using MyHome.Data.Integrations.FloorHeating;
using MyHome.Data.Integrations.HeatPump;
using MyHome.Data.Integrations.WifiSocket;
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

        services.AddScoped<IRepository<DeviceSettingsProfile>, FileRepository<DeviceSettingsProfile>>();
        services.AddScoped<IRepository<PriceThearsholdsProfile>, FileRepository<PriceThearsholdsProfile>>();
        services.AddScoped<IEnergySupplierRepository, EnergyRepository>();

        services.AddScoped<EnergySupplierService>();
        services.AddScoped<HeatRegulatorService>();
        services.AddScoped<IWifiSocketsService, WifiSocketsService>();

        services.AddScoped<DeviceSettingsFactory>();
        services.AddScoped<PriceLevelGenerator>();

        services.AddSingleton<IObserver<RealTimeMeasurement>, EnergyConsumptionObserver>();
        services.AddScoped<EnergyConsumptionListener>();

        services.AddTibberClient(configuration);

        services.Configure<FloorHeaterOptions>(configuration.GetSection(FloorHeaterOptions.ConfigurationSection));
        services.AddScoped<IFloorHeaterClient, FloorHeaterClient>();

        services.AddScoped<IHeatPumpClient, NibeClient>();
        services.AddMyUplinkClient(configuration);

        services.AddWifiSocketClients(configuration);

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

    private static IServiceCollection AddWifiSocketClients(this IServiceCollection services, IConfiguration configuration)
    {
        var radiatorConfigs = configuration
            .GetSection("WifiSockets")
            .Get<List<WifiSocketOptions>>() ?? [];

        foreach (var config in radiatorConfigs)
        {
            services.AddHttpClient(config.Name, client =>
            {
                client.BaseAddress = new Uri(config.BaseAddress);
            });
        }

        foreach (var config in radiatorConfigs)
        {
            services.AddScoped(sp =>
            {
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var auditRepository = sp.GetRequiredService<IRepository<AuditEvent>>();
                var options = Options.Create(config);
                var httpClientLogger = sp.GetRequiredService<ILogger<AuditedHttpClient<WifiSocketOptions>>>();
                var auditedHttpClient = new AuditedHttpClient<WifiSocketOptions>(httpClientFactory, auditRepository, options, httpClientLogger);

                var wifiSocketLogger = sp.GetRequiredService<ILogger<WifiSocketClient>>();
                return new WifiSocketClient(auditedHttpClient, options, wifiSocketLogger);
            });
        }

        return services;
    }
}