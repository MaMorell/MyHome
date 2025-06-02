using Microsoft.AspNetCore.Mvc;
using MyHome.Core.Interfaces;
using MyHome.Core.Models.Entities.Profiles;

public static class PriceThearsholdsEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/profiles/price-thearsholds/{id}", async (
            [FromServices] IRepository<PriceThearsholdsProfile> repository,
            Guid id) =>
        {
            return await repository.GetByIdAsync(id);
        });

        app.MapPut("/profiles/price-thearsholds/{id}", async (
            [FromServices] IRepository<PriceThearsholdsProfile> repository,
            Guid id,
            [FromBody] PriceThearsholdsProfile profile) =>
        {
            if (id != profile.Id)
            {
                return Results.BadRequest("ID in URL must match ID in body");
            }

            await repository.UpsertAsync(profile);
            return Results.NoContent();
        });
    }
}
