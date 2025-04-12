using MyHome.Core.Models.EnergySupplier;
using MyHome.Core.Models.EnergySupplier.Enums;

namespace MyHome.Core.Interfaces;

public interface IEnergyRepository
{
    Task<ICollection<EnergyPrice>> GetEnergyPrices(PriceType priceType);
    Task<ICollection<EnergyConsumptionEntry>> GetConsumptionForToday();
    Task<ICollection<EnergyConsumptionEntry>> GetTopConsumption(int limit = 3);
    Task<ICollection<EnergyConsumptionEntry>> GetTopConsumptionDuringWeekdays(int limit = 3);
}