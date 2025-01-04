using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Components.Chart.Models;
using MyHome.Core.Models.EnergySupplier;
using MyHome.Web.ExternalClients;

namespace MyHome.Web.Components.Pages;
public partial class EnergyCharts
{
    [Inject]
    private EnergySupplierClient EnergySupplierClient { get; set; } = default!;

    private bool _loading;
    private int Index = -1; //default value cannot be 0 -> first selectedindex is 0.

    private readonly ChartOptions _options = new()
    {
        YAxisTicks = 1,
        LineStrokeWidth = 3,
    };
    private readonly List<TimeSeriesChartSeries> _series = [];

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _loading = true;

            var prices = await GetPrices();

            var _priceChart = new TimeSeriesChartSeries
            {
                Index = 0,
                Name = "Elpris (SEK/kWh)",
                Data = prices.Select(p => new TimeSeriesChartSeries.TimeValue(p.Time, (double)p.Price)).ToList(),
                IsVisible = true,
                Type = TimeSeriesDiplayType.Line
            };
            var _priceLevelChart = new TimeSeriesChartSeries
            {
                Index = 1,
                Name = "Prisnivå (0 = låg, 5 = hög)",
                Data = prices.Select(p => new TimeSeriesChartSeries.TimeValue(p.Time, (double)p.RelativePriceLevel)).ToList(),
                IsVisible = true,
                Type = TimeSeriesDiplayType.Line
            };
            var _consumptionChart = new TimeSeriesChartSeries
            {
                Index = 2,
                Name = "Elförbrukning (kWh)",
                Data = GetConsumptionChartData(prices),
                IsVisible = true,
                Type = TimeSeriesDiplayType.Line,
            };

            _series.Add(_priceChart);
            _series.Add(_priceLevelChart);
            _series.Add(_consumptionChart);

            StateHasChanged();
        }
        finally
        {
            _loading = false;
        }
    }

    private static List<TimeSeriesChartSeries.TimeValue> GetConsumptionChartData(IEnumerable<EnergyPrice> prices) =>
        prices
            .Where(p => p.Consumption.HasValue)
            .Select(p => new TimeSeriesChartSeries.TimeValue(p.Time, p.Consumption.HasValue ? (double)p.Consumption : 0))
            .ToList();

    private async Task<IEnumerable<EnergyPrice>> GetPrices()
    {
        var pricesTask = EnergySupplierClient.GetEnergyPricesAsync();
        var waitTask = Task.Delay(500);

        await Task.WhenAll(pricesTask, waitTask);

        return await pricesTask;
    }
}