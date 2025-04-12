using System.Text.Json.Serialization;

namespace MyHome.Core.Models.Integrations.WifiSocket;

public class SetTempratureResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}
