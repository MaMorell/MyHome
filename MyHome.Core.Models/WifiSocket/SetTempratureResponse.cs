using System.Text.Json.Serialization;

namespace MyHome.Core.Models.WifiSocket;

public class SetTempratureResponse
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}
