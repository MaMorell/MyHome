using MudBlazor;
using MyHome.Core.Models.Entities.Constants;
using MyHome.Web.ExternalClients;

namespace MyHome.Web.Components;

public partial class SensorDataChart
{
    private readonly SensorDataClient _sensorDataClient;
    private readonly List<TimeSeriesChartSeries> _series = [];
    private int _index = -1; //default value cannot be 0 -> first selectedindex is 0.
    private readonly ChartOptions _options = new()
    {
        YAxisTicks = 5,
        LineStrokeWidth = 3,
    };
    public SensorDataChart(SensorDataClient sensorDataClient)
    {
        _sensorDataClient = sensorDataClient;
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _series.Clear();

        var data = await _sensorDataClient.GetSensorData(SensorNameConstants.HumiditySensorBasement);

        var basementHumidityChart = new TimeSeriesChartSeries
        {
            Index = 0,
            Name = "Krypgrund - Luftfuktighet",
            Data = data.Select(d => new TimeSeriesChartSeries.TimeValue(d.Timestamp, (double)d.Humidity)).ToList(),
            IsVisible = true,
            LineDisplayType = LineDisplayType.Line
        };
        var basementTempratureChart = new TimeSeriesChartSeries
        {
            Index = 1,
            Name = "Krypgrund - Tempratur",
            Data = data.Select(d => new TimeSeriesChartSeries.TimeValue(d.Timestamp, (double)d.Temperature)).ToList(),
            IsVisible = true,
            LineDisplayType = LineDisplayType.Line
        };

        _series.Add(basementHumidityChart);
        _series.Add(basementTempratureChart);
    }
}