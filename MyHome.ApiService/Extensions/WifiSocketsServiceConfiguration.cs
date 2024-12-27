using Microsoft.Extensions.Options;
using MyHome.Core.Options;
using MyHome.Core.Repositories.WifiSocket;
using System.Configuration;

namespace MyHome.ApiService.Extensions;

public static class RadiatorServiceConfiguration
{
    public static IServiceCollection AddWifiSocketClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<WifiSocketsService>();

        var radiatorConfigs = configuration
            .GetSection("WifiSockets")
            .Get<List<WifiSocketOptions>>() ?? [];

        foreach (var config in radiatorConfigs)
        {
            services.AddScoped(sp =>
            {
                var options = Options.Create(config);
                if (options.Value.Name == Core.Models.WifiSocket.WifiSocketName.Unknown)
                {
                    throw new ConfigurationErrorsException("Invalid WifiSocket name");
                }

                var logger = sp.GetRequiredService<ILogger<WifiSocketClient>>();
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                return new WifiSocketClient(httpClientFactory, options, logger);
            });
        }

        return services;
    }
}
