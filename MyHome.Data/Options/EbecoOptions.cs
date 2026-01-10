namespace MyHome.Data.Options;

public class EbecoOptions
{
    public required Uri BaseAddress { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
}
