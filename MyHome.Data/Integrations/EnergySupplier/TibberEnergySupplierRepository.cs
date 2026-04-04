using Microsoft.Extensions.Caching.Memory;
using MyHome.Core.Interfaces;
using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Models.EnergySupplier.Enums;
using MyHome.Data.Extensions;
using System.Collections.ObjectModel;
using System.Text.Json;
using Tibber.Sdk;

namespace MyHome.Data.Integrations.EnergySupplier;

public class TibberEnergySupplierRepository(TibberApiClient tibberApiClient, IMemoryCache memoryCache) : IEnergySupplierRepository
{
    public async Task<ICollection<EnergyPrice>> GetEnergyPrices(EnergyPriceRange priceType)
    {
        string cacheKey = $"EnergyPrices_{priceType}";

        var cachedPrices = await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpiration = DateTime.Today.AddDays(1);

            var homeId = await GetHomeId();
            var query = BuildEnergyPricesQuery(homeId);
            var queryResponse = await tibberApiClient.Query(query);

            var result = GetPricesFromQueryResponse(queryResponse, priceType) ??
                throw new TibberApiException(GetTibberApiErrorMessage(queryResponse.Data));

            return result.ToEnergyPrices() ?? ReadOnlyCollection<EnergyPrice>.Empty;
        });

        return cachedPrices ?? ReadOnlyCollection<EnergyPrice>.Empty;
    }

    public async Task<ICollection<EnergyConsumptionEntry>> GetConsumption(int lastHours)
    {
        var homeId = await GetHomeId();
        var query = BuildEnergyConsumptionQuery(homeId, lastHours, EnergyResolution.Hourly);
        var queryResponse = await tibberApiClient.Query(query);

        var result = queryResponse.Data.Viewer.Home?.Consumption?.Nodes ??
            throw new TibberApiException(GetTibberApiErrorMessage(queryResponse.Data));

        return result.ToEnergyConsumptionEntries();
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

    private static string BuildEnergyConsumptionQuery(Guid homeId, int lastEntries, EnergyResolution energyResolution)
    {
        var customQueryBuilder = new TibberQueryBuilder()
            .WithAllScalarFields()
            .WithViewer(
                new ViewerQueryBuilder()
                    .WithAllScalarFields()
                    .WithHome(
                        new HomeQueryBuilder()
                            .WithAllScalarFields()
                            .WithConsumption(energyResolution, lastEntries),
                        homeId
                    )
            );

        return customQueryBuilder.Build();
    }

    private static ReadOnlyCollection<Price>? GetPricesFromQueryResponse(TibberApiQueryResponse result, EnergyPriceRange priceType)
    {
        var pricesToday = result.Data.Viewer.Home?.CurrentSubscription?.PriceInfo?.Today;
        var pricesTomorrow = result.Data.Viewer.Home?.CurrentSubscription?.PriceInfo?.Tomorrow;

        var prices = priceType switch
        {
            EnergyPriceRange.Today => pricesToday?.ToList(),
            EnergyPriceRange.Tomorrow => pricesTomorrow?.ToList(),
            EnergyPriceRange.TodayAndTomorrow => CombinePrices(pricesToday, pricesTomorrow),
            _ => throw new ArgumentOutOfRangeException(nameof(priceType), priceType, null)
        };

        return prices != null
            ? new ReadOnlyCollection<Price>(prices)
            : null;
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