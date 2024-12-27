public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiClient<T>(this IServiceCollection services) where T : class
    {
        services.AddHttpClient<T>(client =>
        {
            client.BaseAddress = new("https+http://apiservice");
        });

        return services;
    }
}