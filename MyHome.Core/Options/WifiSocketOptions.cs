using MyHome.Core.Models.WifiSocket;

namespace MyHome.Core.Options;

public class WifiSocketOptions
{
    public WifiSocketName Name { get; set; }
    public string BaseAddress { get; set; } = string.Empty;
}