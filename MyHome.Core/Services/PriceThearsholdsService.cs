using MyHome.Core.Interfaces;
using MyHome.Core.Models.Entities.Profiles;

namespace MyHome.Core.Services;

public class PriceThearsholdsService(IRepository<PriceThearsholdsProfile> repository)
{
    private readonly IRepository<PriceThearsholdsProfile> _repository = repository;

    public async Task<PriceThearsholdsProfile> GetThearsholdsProfile()
    {
        var profiles = await _repository.GetAllAsync();
        if (profiles != null && profiles.Any())
        {
            return profiles.First();
        }

        var profile = new PriceThearsholdsProfile();
        await _repository.UpsertAsync(profile);

        return profile;
    }

    public Task UpsertThearsholdsProfile(PriceThearsholdsProfile profile) => _repository.UpsertAsync(profile);
}
