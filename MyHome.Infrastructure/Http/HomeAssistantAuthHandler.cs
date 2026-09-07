 using Microsoft.Extensions.Options;
using MyHome.Infrastructure.Options;
using System.Net.Http.Headers;

namespace MyHome.Infrastructure.Http;

/// <summary>
/// Attaches the Home Assistant long-lived access token as a Bearer token on every
/// outgoing request. Unlike <see cref="EbecoAuthHandler"/>, no token refresh is
/// needed since Home Assistant long-lived access tokens do not expire.
/// </summary>
public class HomeAssistantAuthHandler(IOptions<HomeAssistantOptions> options) : DelegatingHandler
{
    private readonly HomeAssistantOptions _options = options.Value;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);
        return base.SendAsync(request, cancellationToken);
    }
}
