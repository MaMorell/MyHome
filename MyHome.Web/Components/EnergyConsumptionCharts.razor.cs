using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Charts;
using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Models.EnergySupplier.Enums;

namespace MyHome.Web.Components;

public partial class EnergyConsumptionCharts
{
    [Parameter]
    public IEnumerable<EnergyConsumptionEntry>? EnergyConsumptions { get; set; }

    private int _index = -1; //default value cannot be 0 -> first selectedindex is 0.

    private readonly TimeSeriesChartOptions _options = new()
    {
        YAxisLines = false,
        YAxisTicks = 1,
        MaxNumYAxisTicks = 10,
        YAxisRequireZeroPoint = true,
        XAxisLines = false,
        LineStrokeWidth = 2,
        ShowDataMarkers = false,
        TooltipTimeLabelFormat = "yyyy MMM dd HH:mm:ss",
        TimeLabelSpacing = TimeSpan.FromHours(6),
        XAxisTitle = "Time",
        YAxisTitle = "Values",
    };
    private readonly List<ChartSeries<double>> _series = [];

    protected override void OnParametersSet()
    {
        InitializeCharts();
    }

    private void InitializeCharts()
    {
        if (EnergyConsumptions is null)
            return;

        var priceLevelChart = new ChartSeries<double>
        {
            Name = "Prisnivå (0 = låg, 5 = hög)",
            Data = EnergyConsumptions
                .Where(e => e.PriceDetails.LevelInternal != EnergyPriceLevel.Unknown)
                .Select(p => new TimeValue<double>(p.PriceDetails.StartsAt.LocalDateTime, (double)p.PriceDetails.LevelInternal))
                .ToList(),
            
        };
        var priceChart = new ChartSeries<double>
        {
            Name = "Pris (SEK/kWh)",
            Data = EnergyConsumptions
                .Select(p => new TimeValue<double>(p.PriceDetails.StartsAt.LocalDateTime, (double)p.PriceDetails.PriceTotal))
                .ToList(),
        };
        var consumptionChart = new ChartSeries<double>
        {
            Name = "Förbrukning (kWh)",
            Data = EnergyConsumptions
                .Where(p => p.Consumption != default)
                .Select(p => new TimeValue<double>(p.PriceDetails.StartsAt.LocalDateTime, (double)p.Consumption))
                .ToList(),
        };
        var costChart = new ChartSeries<double>
        {
            Name = "Kostnad (SEK/kWh)",
            Data = EnergyConsumptions
                .Where(e => e.Cost != default)
                .Select(p => new TimeValue<double>(p.PriceDetails.StartsAt.LocalDateTime, (double)p.Cost))
                .ToList(),
        };

        _series.Clear();

        _series.Add(priceChart);
        _series.Add(priceLevelChart);
        _series.Add(consumptionChart);
        _series.Add(costChart);
    }
}