using Microsoft.AspNetCore.Mvc;
using MyHome.Core.Services;

public static class EnergysupplierEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/energysupplier/consumption/top", async (
            [FromServices] EnergySupplierService energyPriceService,
            [FromQuery] int limit = 3,
            [FromQuery] bool onlyDuringWeekdays = true) =>
        {
            return await energyPriceService.GetTopConsumptionThisMonth(limit, onlyDuringWeekdays);
        });

        app.MapGet("energysupplier/consumption/daily", async ([FromServices] EnergySupplierService service, [FromQuery] int lastDays) =>
        {
            return await service.GetConsumptionByDays(lastDays);
        });

        app.MapGet("energysupplier/energyprice", async ([FromServices] EnergySupplierService energyPriceService) =>
        {
            return await energyPriceService.GetFutureEnergyPrices();
        });
    }
}
