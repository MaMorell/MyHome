public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiClient<T>(this IServiceCollection services, IConfiguration configuration) where T : class
    {
        var apiServiceBaseUrl = configuration["ApiService:BaseUrl"] ?? throw new KeyNotFoundException("Configuration missing: ApiService:BaseUrl");
        services.AddHttpClient<T>(client =>
        {
            client.BaseAddress = new(apiServiceBaseUrl);
        });

        return services;
    }
}