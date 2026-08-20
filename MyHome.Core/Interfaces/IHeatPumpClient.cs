using MyHome.Core.Models.Integrations.HeatPump;

namespace MyHome.Core.Interfaces;

public interface IHeatPumpClient
{
    Task UpdateComfortMode(ComfortMode value, CancellationToken cancellationToken);
    Task UpdateHeat(int value, CancellationToken cancellationToken);
    Task UpdateOpMode(OpMode value, CancellationToken cancellationToken);
}