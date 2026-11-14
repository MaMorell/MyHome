using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Models.EnergySupplier.Enums;

namespace MyHome.Core.Interfaces;

public interface IEnergySupplierRepository
{
    Task<ICollection<EnergyPrice>> GetEnergyPrices(EnergyPriceRange range);

    /// <summary>
    /// Returns the measured energy consumption for every hour of <paramref name="day"/> that
    /// has already happened. Hours without data yet (e.g. a future day, or hours later today)
    /// are omitted.
    /// </summary>
    Task<ICollection<EnergyConsumptionHour>> GetEnergyConsumption(DateOnly day);
}