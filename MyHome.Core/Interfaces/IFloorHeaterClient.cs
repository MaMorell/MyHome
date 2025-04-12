namespace MyHome.Core.Interfaces;

public interface IFloorHeaterClient
{
    Task<double> GetSetTemperatureAsync();
    Task UpdateSetTemperatureAsync(int temperature);
}