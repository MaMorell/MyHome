using MyHome.ApiService.HostedServices.Services;
using MyHome.ApiService.Services;
using MyHome.Core;
using MyHome.Core.Interfaces;
using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Options;
using MyHome.Core.Repositories;
using MyHome.Core.Repositories.EnergySupplier;
using MyHome.Core.Repositories.HeatPump;
using System.Net.Http.Headers;
using Tibber.Sdk;

namespace MyHome.ApiService.Extensions;

public static class WebApplicationBuilderExtensions
{
    public static IServiceCollection RegisterLocalDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient();
        services.AddWifiSocketClients(configuration);

        services.AddScoped<EnergyPriceService>();
        services.AddScoped<HeatResulatorService>();
        services.AddScoped<IEnergyRepository, EnergyRepository>();
        services.AddScoped<HeatpumpClient>();
        services.AddSingleton<IObserver<RealTimeMeasurement>, EnergyConsumptionObserver>();
        services.AddSingleton<IRepository<EnergyMeasurement>, InMemoryRepository<EnergyMeasurement>>();

        services.AddScoped(s =>
        {
            var accessToken = configuration["TibberApiClient:AccessToken"];
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new KeyNotFoundException("TibberApiClient:AccessToken is missing in appsettings.json");
            }
            var userAgent = new ProductInfoHeaderValue("My-home-automation-system", "1.2");
            return new TibberApiClient(accessToken, userAgent);
        });

        services.Configure<UpLinkCredentialsOptions>(
            configuration.GetSection(UpLinkCredentialsOptions.UpLinkCredentials));

        return services;
    }
}
