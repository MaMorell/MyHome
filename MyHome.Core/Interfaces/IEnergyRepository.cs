using System.Collections.ObjectModel;
using Tibber.Sdk;

namespace MyHome.Core.Interfaces;

public interface IEnergyRepository
{
    Task<ReadOnlyCollection<Price>> GetTodaysEnergyPrices();
    Task<ReadOnlyCollection<Price>> GetTomorrowsEnergyPrices();
}