using System.Collections.ObjectModel;
using Tibber.Sdk;

namespace MyHome.Core.Interfaces;

public interface IEnergyRepository
{
    Task<ICollection<ConsumptionEntry>> GetConsumptionForToday();
    Task<ReadOnlyCollection<Price>> GetEnergyPricesForToday();
    Task<ReadOnlyCollection<Price>> GetEnergyPricesForTomorrow();
    Task<ICollection<ConsumptionEntry>> GetTopConsumption(int limit = 3);
    Task<ICollection<ConsumptionEntry>> GetTopConsumptionDuringWeekdays(int limit = 3);
}