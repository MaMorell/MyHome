using Microsoft.Extensions.Options;
using MyHome.Core.Http;
using MyHome.Core.Options;
using MyHome.Core.Repositories;
using MyHome.Core.Repositories.WifiSocket;

namespace MyHome.ApiService.Extensions;

public static class RadiatorServiceConfiguration
{
    public static IServiceCollection AddWifiSocketServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<WifiSocketsService>();
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
