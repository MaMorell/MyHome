namespace MyHome.Core.Models.Interfaces;

public interface IDeviceProfile<T>
{
    T Baseline { get; set; }
    T Economic { get; set; }
    T Enhanced { get; set; }
    T ExtremeSavings { get; set; }
    T MaxSavings { get; set; }
    T Moderate { get; set; }
}