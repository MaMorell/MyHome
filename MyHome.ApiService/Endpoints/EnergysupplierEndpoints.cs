using Microsoft.AspNetCore.Mvc;
using MyHome.Core.Services;

public static class EnergysupplierEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("energysupplier/energyprice", async ([FromServices] EnergySupplierService energyPriceService) =>
        {
            return await energyPriceService.GetFutureEnergyPrices();
        });
    }
}
