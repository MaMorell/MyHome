using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MyHome.Core.Interfaces;
using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Models.EnergySupplier.Enums;
using MyHome.Core.Models.Entities.Profiles;
using MyHome.Core.Models.Integrations.HeatPump;
using MyHome.Core.Models.PriceCalculations;
using MyHome.Core.PriceCalculations;
using MyHome.Core.Services;
using System.Text.Json;

namespace MyHome.Core.Tests;

public class AutomationPreviewTests
{
    private static readonly DateTimeOffset MondayMidday = new(2026, 1, 5, 11, 1, 0, TimeSpan.FromHours(1));

    [Theory]
    [InlineData(EnergyPriceLevel.VeryCheap, 2, 26, 28, 14, 63)]
    [InlineData(EnergyPriceLevel.Cheap, 1, 25, 27, 13, 64)]
    [InlineData(EnergyPriceLevel.Normal, 0, 24, 26, 12, 65)]
    [InlineData(EnergyPriceLevel.Expensive, -2, 22, 24, 10, 67)]
    [InlineData(EnergyPriceLevel.VeryExpensive, -4, 20, 22, 8, 69)]
    public void PriceLevelsChangeAllNumericDeviceTargets(
        EnergyPriceLevel level, int offset, int bathZero, int bathOne, int garage, int humidity)
    {
        var time = new DateTimeOffset(2026, 1, 4, 5, 0, 0, TimeSpan.FromHours(1));
        var price = new EnergyPriceDetails { StartsAt = time, PriceTotal = 0.1m, LevelInternal = level };

        var targets = DeviceTargetsPlanner.Create(price, [price], new DeviceSettingsProfile(), time);

        Assert.Equal(offset, targets.HeatOffset);
        Assert.Equal(bathZero, targets.ThermostatBathZeroTemperature);
        Assert.Equal(bathOne, targets.ThermostatBathOneTemperature);
        Assert.Equal(garage, targets.ThermostatGarageTemperature);
        Assert.Equal(humidity, targets.DehumidifierTargetHumidity);
    }

    [Fact]
    public void TimeRulesModifyComfortThermostatsAndOperationMode()
    {
        var price = new EnergyPriceDetails
        {
            StartsAt = MondayMidday.AddMinutes(-1),
            PriceTotal = 0.1m,
            LevelInternal = EnergyPriceLevel.VeryCheap
        };

        var targets = DeviceTargetsPlanner.Create(price, [price], new DeviceSettingsProfile(), MondayMidday);

        Assert.Equal(ComfortMode.Economy, targets.ComfortMode);
        Assert.Equal(OpMode.Manual, targets.OpMode);
        Assert.Equal(24, targets.ThermostatBathZeroTemperature);
        Assert.Equal(26, targets.ThermostatBathOneTemperature);
    }

    [Fact]
    public void MidnightRulesLowerBathroomTargetsWithoutChangingGarageTarget()
    {
        var time = new DateTimeOffset(2026, 1, 4, 1, 0, 0, TimeSpan.FromHours(1));
        var price = new EnergyPriceDetails
        {
            StartsAt = time,
            PriceTotal = 0.1m,
            LevelInternal = EnergyPriceLevel.Cheap
        };

        var targets = DeviceTargetsPlanner.Create(price, [price], new DeviceSettingsProfile(), time);

        Assert.Equal(ComfortMode.Economy, targets.ComfortMode);
        Assert.Equal(OpMode.Auto, targets.OpMode);
        Assert.Equal(21, targets.ThermostatBathZeroTemperature);
        Assert.Equal(23, targets.ThermostatBathOneTemperature);
        Assert.Equal(13, targets.ThermostatGarageTemperature);
    }

    [Fact]
    public async Task PreviewAppliesLiveOverrideOnlyToCurrentPeriodWithoutWritingDevices()
    {
        var (service, heatPump, dehumidifier, bathZero, bathOne, garage) = CreateService(3m);

        var preview = await service.GetPreviewAsync(MondayMidday);

        Assert.Equal(MondayMidday, preview.GeneratedAt);
        Assert.Equal(ConsumptionOverrideStatus.Applied, preview.CurrentConsumptionOverride);
        Assert.Equal(3m, preview.AccumulatedConsumptionCurrentHour);
        Assert.Equal(24, preview.Baseline.ThermostatBathZeroTemperature);
        Assert.Equal(65, preview.Baseline.DehumidifierTargetHumidity);
        Assert.True(preview.Periods.Count > 1);
        Assert.True(preview.Periods[0].IsCurrentPeriod);
        Assert.Equal(-4, preview.Periods[0].Targets.HeatOffset);
        Assert.Equal(20, preview.Periods[0].Targets.ThermostatBathZeroTemperature);
        Assert.Equal(69, preview.Periods[0].Targets.DehumidifierTargetHumidity);
        Assert.False(preview.Periods[1].IsCurrentPeriod);
        Assert.Equal(0, preview.Periods[1].Targets.HeatOffset);
        Assert.Equal(22, preview.Periods[1].Targets.ThermostatBathZeroTemperature);
        Assert.Equal(ComfortMode.Economy, preview.Periods[1].Targets.ComfortMode);
        Assert.Equal(OpMode.Manual, preview.Periods[1].Targets.OpMode);
        var json = JsonSerializer.Serialize(preview, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var response = JsonSerializer.Deserialize<AutomationPreview>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.Equal(ConsumptionOverrideStatus.Applied, response!.CurrentConsumptionOverride);
        Assert.Equal(3m, response.AccumulatedConsumptionCurrentHour);
        Assert.Equal(preview.Periods.Count, response.Periods.Count);
        Assert.Equal(preview.Periods[0].Targets, response.Periods[0].Targets);
        heatPump.VerifyNoOtherCalls();
        dehumidifier.VerifyNoOtherCalls();
        bathZero.VerifyNoOtherCalls();
        bathOne.VerifyNoOtherCalls();
        garage.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PreviewReportsUnavailableConsumptionInsteadOfClaimingOverrideApplied()
    {
        var (service, _, _, _, _, _) = CreateService(null);

        var preview = await service.GetPreviewAsync(MondayMidday);

        Assert.Equal(ConsumptionOverrideStatus.Unavailable, preview.CurrentConsumptionOverride);
        Assert.Null(preview.AccumulatedConsumptionCurrentHour);
        Assert.Equal(0, preview.Periods[0].Targets.HeatOffset);
        Assert.Equal(22, preview.Periods[0].Targets.ThermostatBathZeroTemperature);
    }

    private static (
        HouseAutomationService Service,
        Mock<IHeatPumpClient> HeatPump,
        Mock<IDehumidifierClient> Dehumidifier,
        Mock<IThermostatClient> BathZero,
        Mock<IThermostatClient> BathOne,
        Mock<IThermostatClient> Garage) CreateService(decimal? accumulatedConsumption)
    {
        var profile = new DeviceSettingsProfile();
        var profileRepository = new Mock<IRepository<DeviceSettingsProfile>>();
        profileRepository.Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(profile);

        var thresholdRepository = new Mock<IRepository<PriceThearsholdsProfile>>();
        thresholdRepository.Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new PriceThearsholdsProfile { InternalPriceLevelRangeInHours = 1 });

        ICollection<EnergyPrice> prices = Enumerable.Range(0, 8)
            .Select(i => new EnergyPrice
            {
                StartsAt = MondayMidday.AddMinutes(-1 + i * 15),
                Total = 1m,
                Level = EnergyPriceLevel.Normal
            })
            .ToList();
        var supplier = new Mock<IEnergySupplierRepository>();
        supplier.Setup(repository => repository.GetEnergyPrices(EnergyPriceRange.TodayAndTomorrow))
            .ReturnsAsync(prices);

        var homeAssistant = new Mock<IHomeAssistantEnergyClient>();
        if (accumulatedConsumption.HasValue)
        {
            homeAssistant.Setup(client => client.GetAccumulatedConsumptionCurrentHourAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(accumulatedConsumption.Value);
        }
        else
        {
            homeAssistant.Setup(client => client.GetAccumulatedConsumptionCurrentHourAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new HttpRequestException("Sensor unavailable"));
        }

        var heatPump = new Mock<IHeatPumpClient>(MockBehavior.Strict);
        var dehumidifier = new Mock<IDehumidifierClient>(MockBehavior.Strict);
        var bathZero = new Mock<IThermostatClient>(MockBehavior.Strict);
        var bathOne = new Mock<IThermostatClient>(MockBehavior.Strict);
        var garage = new Mock<IThermostatClient>(MockBehavior.Strict);
        var service = new HouseAutomationService(
            new PriceLevelGenerator(thresholdRepository.Object, supplier.Object),
            heatPump.Object, dehumidifier.Object, bathOne.Object, bathZero.Object, garage.Object,
            profileRepository.Object, homeAssistant.Object, NullLogger<HouseAutomationService>.Instance);
        return (service, heatPump, dehumidifier, bathZero, bathOne, garage);
    }
}
