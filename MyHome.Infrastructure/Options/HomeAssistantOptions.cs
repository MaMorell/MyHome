using System.ComponentModel.DataAnnotations;

namespace MyHome.Infrastructure.Options;

public class HomeAssistantOptions
{
    public const string ConfigurationSection = "HomeAssistant";
    [Url]
    public required Uri BaseAddress { get; init; }
    [Required]
    public required string AccessToken { get; init; }
}
