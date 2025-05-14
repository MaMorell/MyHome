using Moq;
using MyHome.Core.Interfaces;
using MyHome.Core.Models.Entities.Constants;
using MyHome.Core.Models.Entities.Profiles;
using Shouldly;

namespace MyHome.Core.Tests;

public class PriceLevelGeneratorTests
{
    private readonly Mock<IEnergySupplierRepository> _energySupplierRepository;
    private readonly Mock<IRepository<PriceThearsholdsProfile>> _priceThearsholdsRepository;
    private readonly PriceLevelGenerator _sut;

    public PriceLevelGeneratorTests()
    {
        _energySupplierRepository = new Mock<IEnergySupplierRepository>();
        _priceThearsholdsRepository = new Mock<IRepository<PriceThearsholdsProfile>>();
        _sut = new PriceLevelGenerator(_priceThearsholdsRepository.Object, _energySupplierRepository.Object);
    }

    [Fact]
    public async Task CreateAsync_WithZeroEntries_ShouldReturnEmptyList()
    {
        var result = await _sut.CreateAsync(DateTime.Now.AddDays(-1), 0);

        result.ShouldNotBeNull();
        result.ShouldBeEmpty();

        _priceThearsholdsRepository.Verify(r => r.GetByIdAsync(EntityIdConstants.PriceThearsholdsId), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WithCurrentDate_ShouldReturnEmptyList()
    {
        var result = await _sut.CreateAsync(DateTime.Now, 24);

        result.ShouldNotBeNull();
        result.ShouldBeEmpty();

        _priceThearsholdsRepository.Verify(r => r.GetByIdAsync(EntityIdConstants.PriceThearsholdsId), Times.Never);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task CreateAsync_WitMultipleEntries_ShouldReturnListWithCorrectCount(int entries)
    {
        var result = await _sut.CreateAsync(DateTime.Now.AddDays(-1), entries);

        result.ShouldNotBeNull();
        result.Count().ShouldBe(entries);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task CreateAsync_WitMultipleEntries_ShouldReturnEntriesWithCorrectDate(int entries)
    {
        var fromDate = GetDateTimeNow(-1);

        var results = await _sut.CreateAsync(fromDate, entries);

        for (int i = 0; i < entries; i++)
        {
            results.ElementAt(i).StartsAt.ShouldBe(fromDate.AddHours(i));
        }
    }

    [Fact]
    public async Task CreateAsync_WithMoreEntriesThenTime_ShouldLimitResultToAvailableTime()
    {
        var now = GetDateTimeNow();
        var fromDate = GetDateTimeNow(-1);
        var expectedEntriesCount = (now - fromDate).TotalHours;

        var result = await _sut.CreateAsync(fromDate, 48);

        result.Count().ShouldBe((int)expectedEntriesCount);
    }

    [Fact]
    public async Task CreateAsync_ShoulFetchPriceThearsholdsProfile()
    {
        await _sut.CreateAsync(DateTime.Now.AddDays(-1), 1);

        _priceThearsholdsRepository.Verify(r => r.GetByIdAsync(EntityIdConstants.PriceThearsholdsId));
    }

    [Fact]
    public async Task CreateAsync_ShoulFetchPrices()
    {
        await _sut.CreateAsync(DateTime.Now.AddDays(-1), 1);

        _energySupplierRepository.Verify(r => r.GetEnergyPrices(EntityIdConstants.PriceThearsholdsId));
    }

    private static DateTime GetDateTimeNow(int addHours = 0)
    {
        var yesterday = DateTime.Now.AddDays(addHours);
        var fromDate = new DateTime(yesterday.Year, yesterday.Month, yesterday.Day, yesterday.Hour, 0, 0);
        return fromDate;
    }
}

internal class PriceLevelGenerator
{
    private readonly IRepository<PriceThearsholdsProfile> _priceThearsholdsRepository;

    public PriceLevelGenerator(IRepository<PriceThearsholdsProfile> priceThearsholdsRepository, IEnergySupplierRepository @object)
    {
        _priceThearsholdsRepository = priceThearsholdsRepository;
    }

    public async Task<IEnumerable<EnergyPriceLevel>> CreateAsync(DateTime fromDate, int entries)
    {
        var now = GetDateTimeNow();
        if (entries == 0 || fromDate >= now)
        {
            return [];
        }

        var profile = await _priceThearsholdsRepository.GetByIdAsync(EntityIdConstants.PriceThearsholdsId);

        return CreatePriceLevels(fromDate, entries);
    }

    private static IEnumerable<EnergyPriceLevel> CreatePriceLevels(DateTime fromDate, int entries)
    {
        var hoursToNow = (GetDateTimeNow() - fromDate).TotalHours;
        var entriesToGet = entries > hoursToNow ? hoursToNow : entries;

        var result = new List<EnergyPriceLevel>();
        for (var i = 0; i < entriesToGet; i++)
        {
            var priceLevel = new EnergyPriceLevel()
            {
                StartsAt = fromDate.AddHours(i),
            };
            result.Add(priceLevel);
        }

        return result;
    }

    private static DateTime GetDateTimeNow()
    {
        var now = DateTime.Now;
        var nowWithoutMinutes = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
        return nowWithoutMinutes;
    }
}

public record EnergyPriceLevel
{
    public decimal Total { get; set; }
    public DateTimeOffset StartsAt { get; init; }
    public PriceLevel LevelExternal { get; set; }
    public PriceLevel LevelInternal { get; set; }

}

public enum PriceLevel
{
    Unknown,
    Normal,
    VeryCheap,
    Cheap,
    Expensive,
    VeryExpensive,
    Extreme
}

public enum EnergyPriceRange
{
    Unspecified,
    Today,
    Tomorrow,
    All
}