using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Models.EnergySupplier.Enums;

namespace MyHome.Core.Interfaces;

public interface IEnergySupplierRepository
{
    Task<ICollection<EnergyPrice>> GetEnergyPrices(EnergyPriceRange range);
    Task<ICollection<EnergyConsumptionEntry>> GetConsumptionForToday();
    Task<ICollection<EnergyConsumptionEntry>> GetTopConsumption(int limit = 3);
    Task<ICollection<EnergyConsumptionEntry>> GetTopConsumptionDuringWeekdays(int limit = 3);
}