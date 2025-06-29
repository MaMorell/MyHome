namespace MyHome.Core.Interfaces;

public interface ISmartHubClient
{
    Task StartAsync();
    Task StopAsync();
}
