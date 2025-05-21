using Microsoft.AspNetCore.Components;
using MudBlazor;
using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Models.EnergySupplier.Enums;

namespace MyHome.Web.Components;

public partial class EnergyConsumptionCharts
{
    [Parameter]
    public IEnumerable<EnergyConsumptionEntry>? EnergyConsumptions { get; set; }

    private int Index = -1; //default value cannot be 0 -> first selectedindex is 0.

    private readonly ChartOptions _options = new()
    {
        YAxisTicks = 1,
        LineStrokeWidth = 3,
    };
    private readonly List<TimeSeriesChartSeries> _series = [];

    protected override void OnParametersSet()
    {
        InitializeCharts();
    }

    private void InitializeCharts()
    {
        if (EnergyConsumptions is null)
            return;

        var priceChart = new TimeSeriesChartSeries
        {
            Index = 0,
            Name = "Elpris (SEK/kWh)",
            Data = EnergyConsumptions.Select(p => new TimeSeriesChartSeries.TimeValue(p.PriceDetails.StartsAt.LocalDateTime, (double)p.PriceDetails.PriceTotal)).ToList(),
            IsVisible = true,
            LineDisplayType = LineDisplayType.Line
        };
        var priceLevelChart = new TimeSeriesChartSeries
        {
            Index = 1,
            Name = "Prisnivå (0 = låg, 5 = hög)",
            Data = EnergyConsumptions.Where(e => e.PriceDetails.LevelInternal != EnergyPriceLevel.Unknown).Select(p => new TimeSeriesChartSeries.TimeValue(p.PriceDetails.StartsAt.LocalDateTime, (double)(p.PriceDetails.LevelInternal) / 2)).ToList(),
            IsVisible = true,
            LineDisplayType = LineDisplayType.Line
        };
        var consumptionChart = new TimeSeriesChartSeries
        {
            Index = 2,
            Name = "Elförbrukning (kWh)",
            Data = GetConsumptionChartData(EnergyConsumptions),
            IsVisible = true,
            LineDisplayType = LineDisplayType.Line
        };

        _series.Clear();

        _series.Add(priceChart);
        _series.Add(priceLevelChart);
        _series.Add(consumptionChart);
    }

    private static List<TimeSeriesChartSeries.TimeValue> GetConsumptionChartData(IEnumerable<EnergyConsumptionEntry> prices) =>
        prices
            .Where(p => p.Consumption != default)
            .Select(p => new TimeSeriesChartSeries.TimeValue(p.PriceDetails.StartsAt.LocalDateTime, (double)p.Consumption))
            .ToList();
}