using System.Text.Json.Serialization;

namespace MyHome.Core.Models.WifiSocket;

public class SetTemprature
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "Normal";
    [JsonPropertyName("value")]
    public int Value { get; set; }
}
