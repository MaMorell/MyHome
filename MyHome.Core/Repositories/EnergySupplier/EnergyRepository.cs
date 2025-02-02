using MyHome.Core.Interfaces;
using System.Collections.ObjectModel;
using System.Text.Json;
using Tibber.Sdk;
using MyHome.Core.Extensions;

namespace MyHome.Core.Repositories.EnergySupplier;

public class EnergyRepository(TibberApiClient tibberApiClient) : IEnergyRepository
{
    public Task<ReadOnlyCollection<Price>> GetEnergyPricesForTomorrow() =>
        GetEnergyPrices(PriceType.Tomorrow);

    public Task<ReadOnlyCollection<Price>> GetEnergyPricesForToday() =>
        GetEnergyPrices(PriceType.Today);

    public async Task<ICollection<ConsumptionEntry>> GetConsumptionForToday()
    {
        var entriesToFetch = DateTime.Now.Hour;

        return await GetConsumption(tibberApiClient, entriesToFetch);
    }

    public async Task<ICollection<ConsumptionEntry>> GetTopConsumptionDuringWeekdays(int limit = 3)
    {
        var result = await GetConsumptionByHighestConsumption(tibberApiClient);

        return result
            .Where(x => 
                x.From.HasValue && 
                x.From.Value.Date.IsWeekday() && 
                x.From.Value.DateTime.Hour >= 7 && 
                x.From.Value.DateTime.Hour < 19)
            .Take(limit)
            .ToList();

    }

    public async Task<ICollection<ConsumptionEntry>> GetTopConsumption(int limit = 3)
    {
        var result = await GetConsumptionByHighestConsumption(tibberApiClient);

        return result
            .Take(limit)
            .ToList();
    }

    private async Task<IEnumerable<ConsumptionEntry>> GetConsumptionByHighestConsumption(TibberApiClient tibberApiClient)
    {
        var entriesToFetch = ((DateTime.Now.Day - 1) * 24) + DateTime.Now.Hour;

        var consumption = await GetConsumption(tibberApiClient, entriesToFetch);

        return consumption
            .Where(c => c.From.HasValue && c.From.Value.Hour >= 7 && c.From.Value.Hour <= 19)
            .OrderByDescending(entry => entry.Consumption);
    }

    private async Task<ICollection<ConsumptionEntry>> GetConsumption(TibberApiClient tibberApiClient, int entriesToFetch)
    {
        var homeId = await GetHomeId();
        var query = BuildEnergyConsumptionQuery(homeId, entriesToFetch, EnergyResolution.Hourly);
        var queryResponse = await tibberApiClient.Query(query);

        return queryResponse.Data.Viewer.Home?.Consumption?.Nodes ??
            throw new TibberApiException(GetTibberApiErrorMessage(queryResponse.Data));
    }

    private async Task<ReadOnlyCollection<Price>> GetEnergyPrices(PriceType priceType)
    {
        var homeId = await GetHomeId();
        var query = BuildEnergyPricesQuery(homeId);
        var queryResponse = await tibberApiClient.Query(query);

        var result = GetPricesFromQueryResponse(queryResponse, priceType) ??
            throw new TibberApiException(GetTibberApiErrorMessage(queryResponse.Data));

        return result ?? ReadOnlyCollection<Price>.Empty;
    }

    private async Task<Guid> GetHomeId()
    {
        var basicData = await tibberApiClient.GetBasicData();

        return basicData.Data.Viewer.Homes.FirstOrDefault()?.Id.GetValueOrDefault() ??
            throw new TibberApiException(GetTibberApiErrorMessage(basicData.Data));
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
                                    .WithPriceInfo(priceInfoBuilder)
                            ),
                        homeId
                    )
            );

        return customQueryBuilder.Build();
    }

    private static string BuildEnergyConsumptionQuery(Guid homeId, int entriesToFetch, EnergyResolution energyResolution)
    {
        var customQueryBuilder = new TibberQueryBuilder()
            .WithAllScalarFields()
            .WithViewer(
                new ViewerQueryBuilder()
                    .WithAllScalarFields()
                    .WithHome(
                        new HomeQueryBuilder()
                            .WithAllScalarFields()
                            .WithConsumption(energyResolution, entriesToFetch),
                        homeId
                    )
            );

        return customQueryBuilder.Build();
    }

    private static ReadOnlyCollection<Price>? GetPricesFromQueryResponse(TibberApiQueryResponse result, PriceType priceType)
    {
        var prices = priceType switch
        {
            PriceType.Today => result.Data.Viewer.Home?.CurrentSubscription?.PriceInfo?.Today,
            PriceType.Tomorrow => result.Data.Viewer.Home?.CurrentSubscription?.PriceInfo?.Tomorrow,
            _ => throw new ArgumentOutOfRangeException(nameof(priceType), priceType, null)
        };

        return prices != null ? new ReadOnlyCollection<Price>(prices.ToList()) : null;
    }

    private static string GetTibberApiErrorMessage(object data) => $"Get data from Tibber failed. Some data in the response is missing:\n{JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true })}";

    private enum PriceType
    {
        Today,
        Tomorrow
    }
}
