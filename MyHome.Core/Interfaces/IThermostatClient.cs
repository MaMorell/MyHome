namespace MyHome.Core.Interfaces;

public interface IThermostatClient
{
    Task<double> GetSetTemperatureAsync();
    Task UpdateSetTemperatureAsync(int temperature);
}