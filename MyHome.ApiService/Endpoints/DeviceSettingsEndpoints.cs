using Microsoft.AspNetCore.Mvc;
using MyHome.Core.Interfaces;
using MyHome.Core.Models.Entities.Profiles;

public static class DeviceSettingsEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/profiles/device-settings/{id}", async (
            [FromServices] IRepository<DeviceSettingsProfile> repository,
            Guid id) =>
        {
            return await repository.GetByIdAsync(id);
        });

        app.MapPut("/profiles/device-settings/{id}", async (
            [FromServices] IRepository<DeviceSettingsProfile> repository,
            Guid id,
            [FromBody] DeviceSettingsProfile profile) =>
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