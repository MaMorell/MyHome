namespace MyHome.Core.Options;

public class ExternalClientOptions<T> where T : class
{
    public string Name { get; set; } = string.Empty;
    public string BaseAddress { get; set; } = string.Empty;
    public string? ClientIdentifier { get; set; }
    public string? ClientSecret { get; set; }
}
