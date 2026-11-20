using Microsoft.Extensions.Caching.Memory;
using MyHome.Core.Interfaces;
using MyHome.Core.Models.EnergySupplier;
using MyHome.Infrastructure.Extensions;
using System.Collections.ObjectModel;
using System.Text.Json;
using Tibber.Sdk;

namespace MyHome.Infrastructure.Integrations.EnergySupplier;

public class TibberEnergySupplierRepository(TibberApiClient tibberApiClient, IMemoryCache memoryCache) : IEnergySupplierRepository
{
    public async Task<ICollection<EnergyPrice>> GetEnergyPrices()
    {
        const string cacheKey = "EnergyPrices";

        var cachedPrices = await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            var homeId = await GetHomeId();
            var query = BuildEnergyPricesQuery(homeId);
            var queryResponse = await tibberApiClient.Query(query);

            var prices = GetPricesFromQueryResponse(queryResponse).ToEnergyPrices();

            entry.AbsoluteExpiration = GetEnergyPricesCacheExpirationTime(queryResponse);

            return prices;
        });

        return cachedPrices ?? ReadOnlyCollection<EnergyPrice>.Empty;
    }

    private static DateTime GetEnergyPricesCacheExpirationTime(TibberApiQueryResponse queryResponse)
    {
        var now = DateTime.Now;

        var pricePublicationTime = now.Date.AddHours(13);
        
        var hasTomorrowPrices = queryResponse.Data.Viewer.Home?.CurrentSubscription?.PriceInfo?.Tomorrow?.Any() == true;

        return hasTomorrowPrices
            ? pricePublicationTime.AddDays(1)
            : now < pricePublicationTime ? pricePublicationTime : now.AddMinutes(5);
    }

    public async Task<ICollection<EnergyConsumptionHour>> GetEnergyConsumption(DateOnly day)
    {
        string cacheKey = $"EnergyConsumption_{day:yyyy-MM-dd}";

        var cachedConsumption = await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            var isToday = day == DateOnly.FromDateTime(DateTime.Now);
            entry.AbsoluteExpirationRelativeToNow = isToday ? TimeSpan.FromMinutes(10) : TimeSpan.FromDays(1);

            var homeId = await GetHomeId();
            var query = BuildEnergyConsumptionQuery(homeId);
            var queryResponse = await tibberApiClient.Query(query);

            var nodes = queryResponse.Data.Viewer.Home?.Consumption?.Nodes ??
                throw new TibberApiException(GetTibberApiErrorMessage(queryResponse.Data));

            return nodes
                .Where(node => node.From.HasValue && DateOnly.FromDateTime(node.From.Value.DateTime) == day)
                .ToEnergyConsumptionHours();
        });

        return cachedConsumption ?? ReadOnlyCollection<EnergyConsumptionHour>.Empty;
    }

    private async Task<Guid> GetHomeId()
    {
        return await memoryCache.GetOrCreateAsync("TibberHomeId", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30);

            var lightQuery = "query { viewer { homes { id } } }";
            var queryResponse = await tibberApiClient.Query(lightQuery);

            return queryResponse.Data.Viewer.Homes.FirstOrDefault()?.Id.GetValueOrDefault() ??
                throw new TibberApiException("Failed to fetch Home ID. Response was empty.");
        });
    }

    private static string BuildEnergyPricesQuery(Guid homeId)
    {
        var priceInfoBuilder = new PriceInfoQueryBuilder()
            .WithToday(new PriceQueryBuilder().WithAllScalarFields())
            .WithTomorrow(new PriceQueryBuilder().WithAllScalarFields());

        var customQueryBuilder = new TibberQueryBuilder()
            .WithAllScalarFields()
            .WithViewer(
                new ViewerQueryBuilder()
                    .WithAllScalarFields()
                    .WithHome(
                        new HomeQueryBuilder()
                            .WithAllScalarFields()
                            .WithCurrentSubscription(
                                new SubscriptionQueryBuilder()
                                    .WithAllScalarFields()
                                    .WithPriceInfo(priceInfoBuilder, resolution: PriceInfoResolution.QuarterHourly)
                            ),
                        homeId
                    )
            );

        return customQueryBuilder.Build();
    }

    private static string BuildEnergyConsumptionQuery(Guid homeId)
    {
        var consumptionNodesBuilder = new ConsumptionEntryQueryBuilder()
            .WithFrom()
            .WithTo()
            .WithConsumption();

        var consumptionConnectionBuilder = new HomeConsumptionConnectionQueryBuilder()
            .WithNodes(consumptionNodesBuilder);

        var customQueryBuilder = new TibberQueryBuilder()
            .WithAllScalarFields()
            .WithViewer(
                new ViewerQueryBuilder()
                    .WithAllScalarFields()
                    .WithHome(
                        new HomeQueryBuilder()
                            .WithAllScalarFields()
                            .WithConsumption(consumptionConnectionBuilder, resolution: EnergyResolution.Hourly, last: 72),
                        homeId
                    )
            );

        return customQueryBuilder.Build();
    }

    private static ReadOnlyCollection<Price> GetPricesFromQueryResponse(TibberApiQueryResponse result)
    {
        var pricesToday = result.Data.Viewer.Home?.CurrentSubscription?.PriceInfo?.Today;
        var pricesTomorrow = result.Data.Viewer.Home?.CurrentSubscription?.PriceInfo?.Tomorrow;

        if (pricesToday?.Any() != true)
        {
            throw new TibberApiException(GetTibberApiErrorMessage(result.Data));
        }

        return new ReadOnlyCollection<Price>(CombinePrices(pricesToday, pricesTomorrow));
    }

    private static List<Price> CombinePrices(params IEnumerable<IEnumerable<Price>?> prices)
    {
        var allPrices = new List<Price>();
        foreach (var price in prices)
        {
            if (price != null)
            {
                allPrices.AddRange(price);
            }
        }

        return allPrices;
    }

    private static string GetTibberApiErrorMessage(object data) => $"Get data from Tibber failed. Some data in the response is missing:\n{JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true })}";
}