using MyHome.Core.Models.Integrations.HeatPump;

namespace MyHome.Core.Interfaces;

public interface IHeatPumpClient
{
    Task<ComfortMode> GetComfortMode(CancellationToken cancellationToken);
    Task<double> GetCurrentOutdoorTemp(CancellationToken cancellationToken);
    Task<double> GetExhaustAirTemp(CancellationToken cancellationToken);
    Task<OpMode> GetOpMode(CancellationToken cancellationToken);
    Task UpdateComfortMode(ComfortMode value, CancellationToken cancellationToken);
    Task UpdateHeat(int value, CancellationToken cancellationToken);
    Task UpdateIncreasedVentilation(IncreasedVentilationValue value, CancellationToken cancellationToken);
    Task UpdateOpMode(OpMode value, CancellationToken cancellationToken);
}