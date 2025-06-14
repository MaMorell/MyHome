using AutoFixture;
using Moq;
using MyHome.Core.Interfaces;
using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Models.EnergySupplier.Enums;
using MyHome.Core.Models.Entities.Constants;
using MyHome.Core.Models.Entities.Profiles;
using MyHome.Core.Models.PriceCalculations;
using MyHome.Core.PriceCalculations;
using Shouldly;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MyHome.Core.Tests;

public class PriceLevelGeneratorTests
{
    private readonly Mock<IEnergySupplierRepository> _energySupplierRepository;
    private readonly Mock<IRepository<PriceThearsholdsProfile>> _priceThearsholdsRepository;
    private readonly PriceLevelGenerator _sut;
    private readonly IFixture _fixture = new Fixture();
    private List<EnergyPrice> _pricesToday = [];
    private List<EnergyPrice> _pricesTomorrow = [];

    public PriceLevelGeneratorTests()
    {
        _energySupplierRepository = new Mock<IEnergySupplierRepository>();
        SetupEnergyPriceMocks();

        _priceThearsholdsRepository = new Mock<IRepository<PriceThearsholdsProfile>>();
        _priceThearsholdsRepository.Setup(p => p.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(new PriceThearsholdsProfile());

        _sut = new PriceLevelGenerator(_priceThearsholdsRepository.Object, _energySupplierRepository.Object);
    }

    private void SetupEnergyPriceMocks()
    {
        _pricesToday = Enumerable.Range(0, 24)
            .Select(i => new EnergyPrice
            {
                Level = _fixture.Create<EnergyPriceLevel>(),
                Total = _fixture.Create<decimal>(),
                StartsAt = GetDateTimeNow(i),
            })
            .ToList();
        _pricesTomorrow = Enumerable.Range(0, 24)
            .Select(i => new EnergyPrice
            {
                Level = _fixture.Create<EnergyPriceLevel>(),
                Total = _fixture.Create<decimal>(),
                StartsAt = GetDateTimeNow(24 + i),
            })
            .ToList();
        var pricesTodayAndTomorrow = _pricesToday
            .Concat(_pricesTomorrow)
            .ToList();

        _energySupplierRepository
            .Setup(r => r.GetEnergyPrices(EnergyPriceRange.Today))
            .ReturnsAsync(_pricesToday);
        _energySupplierRepository
            .Setup(r => r.GetEnergyPrices(EnergyPriceRange.Tomorrow))
            .ReturnsAsync(_pricesTomorrow);
        _energySupplierRepository
            .Setup(r => r.GetEnergyPrices(EnergyPriceRange.TodayAndTomorrow))
            .ReturnsAsync(pricesTodayAndTomorrow);
    }

    [Fact]
    public async Task CreateAsync_ShoulFetchPriceThearsholdsProfile()
    {
        await _sut.CreateAsync(EnergyPriceRange.Tomorrow);

        _priceThearsholdsRepository.Verify(r => r.GetByIdAsync(EntityIdConstants.PriceThearsholdsId));
    }

    [Theory]
    [InlineData(EnergyPriceRange.Today)]
    [InlineData(EnergyPriceRange.Tomorrow)]
    [InlineData(EnergyPriceRange.TodayAndTomorrow)]
    public async Task CreateAsync_ShoulCallGetEnergyPrices(EnergyPriceRange priceRange)
    {
        _energySupplierRepository
            .Setup(r => r.GetEnergyPrices(priceRange))
            .ReturnsAsync([]);

        await _sut.CreateAsync(priceRange);

        _energySupplierRepository.Verify(r => r.GetEnergyPrices(priceRange));
    }

    [Theory]
    [InlineData(EnergyPriceRange.Today, 24)]
    [InlineData(EnergyPriceRange.Tomorrow, 24)]
    [InlineData(EnergyPriceRange.TodayAndTomorrow, 48)]
    public async Task CreateAsync_ShouldReturnCorrectNumberOfEntities(EnergyPriceRange priceRange, int expectedEntitiesCount)
    {
        var result = await _sut.CreateAsync(priceRange);

        result.Count().ShouldBe(expectedEntitiesCount);
    }

    [Theory]
    [InlineData(EnergyPriceRange.Today, 0)]
    [InlineData(EnergyPriceRange.Tomorrow, 24)]
    [InlineData(EnergyPriceRange.TodayAndTomorrow, 0)]
    public async Task CreateAsync_ShouldSetCorrectStartsAtDates(EnergyPriceRange priceRange, int hoursFromToday)
    {
        var expectedStartsAt = GetDateTimeNow(addHours: hoursFromToday);

        var result = await _sut.CreateAsync(priceRange);

        var i = 0;
        foreach (var item in result.OrderBy(p => p.StartsAt))
        {
            item.StartsAt.ShouldBe(expectedStartsAt.AddHours(i));
            item.PriceTotal.ShouldBeGreaterThanOrEqualTo(0);

            i++;
        }
    }

    [Fact]
    public async Task CreateAsync_WithExpensivePrices_ShoulSetCorrectInternalPriceLevel()
    {
        // Arrange
        var profile = new PriceThearsholdsProfile()
        {
            InternalPriceLevelRange = 4,
            Extreme = 2.0m,
            VeryExpensive = 2.0m,
            Expensive = 1.5m,
            Cheap = 0.1m,
            VeryCheap = 0.1m,
        };
        _priceThearsholdsRepository.Setup(p => p.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(profile);

        var prices = new List<EnergyPrice>()
        {
            new() { StartsAt = GetDateTimeNow(0), Total = 15, Level = EnergyPriceLevel.Expensive },
            new() { StartsAt = GetDateTimeNow(1), Total = 15, Level = EnergyPriceLevel.Expensive },
            new() { StartsAt = GetDateTimeNow(2), Total = 5, Level = EnergyPriceLevel.Cheap },
            new() { StartsAt = GetDateTimeNow(3), Total = 5, Level = EnergyPriceLevel.Cheap },
        };
        _energySupplierRepository
            .Setup(r => r.GetEnergyPrices(It.IsAny<EnergyPriceRange>()))
            .ReturnsAsync(prices);

        // Act
        var result = await _sut.CreateAsync(EnergyPriceRange.Today);

        // Assert
        result.Count().ShouldBe(prices.Count);
        result.ElementAt(0).LevelInternal.ShouldBe(EnergyPriceLevel.Expensive);
        result.ElementAt(1).LevelInternal.ShouldBe(EnergyPriceLevel.Unknown);
        result.ElementAt(2).LevelInternal.ShouldBe(EnergyPriceLevel.Unknown);
        result.ElementAt(3).LevelInternal.ShouldBe(EnergyPriceLevel.Unknown);
    }

    [Fact]
    public async Task CreateAsync_WithCheapPrices_ShoulSetCorrectInternalPriceLevel()
    {
        // Arrange
        var profile = new PriceThearsholdsProfile()
        {
            InternalPriceLevelRange = 4,
            Extreme = 2.0m,
            VeryExpensive = 2.0m,
            Expensive = 2.0m,
            Cheap = 0.5m,
            VeryCheap = 0.1m,
        };
        _priceThearsholdsRepository.Setup(p => p.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(profile);

        var prices = new List<EnergyPrice>()
        {
            new() { StartsAt = GetDateTimeNow(0), Total = 5, Level = EnergyPriceLevel.Cheap },
            new() { StartsAt = GetDateTimeNow(1), Total = 5, Level = EnergyPriceLevel.Cheap },
            new() { StartsAt = GetDateTimeNow(2), Total = 15, Level = EnergyPriceLevel.Expensive },
            new() { StartsAt = GetDateTimeNow(3), Total = 15, Level = EnergyPriceLevel.Expensive },
        };
        _energySupplierRepository
            .Setup(r => r.GetEnergyPrices(It.IsAny<EnergyPriceRange>()))
            .ReturnsAsync(prices);

        // Act
        var result = await _sut.CreateAsync(EnergyPriceRange.Today);

        // Assert
        result.Count().ShouldBe(prices.Count);
        result.ElementAt(0).LevelInternal.ShouldBe(EnergyPriceLevel.Cheap);
        result.ElementAt(1).LevelInternal.ShouldBe(EnergyPriceLevel.Unknown);
        result.ElementAt(2).LevelInternal.ShouldBe(EnergyPriceLevel.Unknown);
        result.ElementAt(3).LevelInternal.ShouldBe(EnergyPriceLevel.Unknown);
    }

    [Fact]
    public async Task CreateAsync_ShouldUseHoursForCalculaingRelativePriceLevel()
    {
        // Arrange
        var profile = new PriceThearsholdsProfile()
        {
            InternalPriceLevelRange = 2,
            Extreme = 4m,
            VeryExpensive = 3m,
            Expensive = 2m,
            Cheap = 1m,
            VeryCheap = 0.1m,
        };
        _priceThearsholdsRepository.Setup(p => p.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(profile);

        var prices = new List<EnergyPrice>()
        {
            new() { StartsAt = GetDateTimeNow(0), Total = 5, Level = EnergyPriceLevel.Cheap },
            new() { StartsAt = GetDateTimeNow(1), Total = 5, Level = EnergyPriceLevel.Cheap },
            new() { StartsAt = GetDateTimeNow(2), Total = 1, Level = EnergyPriceLevel.Cheap },
        };
        _energySupplierRepository
            .Setup(r => r.GetEnergyPrices(It.IsAny<EnergyPriceRange>()))
            .ReturnsAsync(prices);

        // Act
        var result = await _sut.CreateAsync(EnergyPriceRange.Today);

        // Assert
        result.Count().ShouldBe(prices.Count);
        result.ElementAt(0).LevelInternal.ShouldBe(EnergyPriceLevel.Cheap);
        result.ElementAt(1).LevelInternal.ShouldBe(EnergyPriceLevel.Normal);
        result.ElementAt(2).LevelInternal.ShouldBe(EnergyPriceLevel.Unknown);
    }

    private static DateTimeOffset GetDateTimeNow(int addHours = 0)
    {
        var today = DateTime.Today.AddHours(addHours);
        return new DateTimeOffset(today);
    }
}