namespace MyHome.Data.Http;

public class EbecoTokenResponse
{
    public EbecoTokenResult? Result { get; set; }
    public bool Success { get; set; }
}

public class EbecoTokenResult
{
    public string? AccessToken { get; set; }
    public int ExpireInSeconds { get; set; }
    public long UserId { get; set; }
}