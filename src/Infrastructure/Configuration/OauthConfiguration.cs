namespace Infrastructure.Configuration;

public class OauthConfiguration
{
    /// <summary>
    ///     Client id of the OAuth2 application.
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    ///     Client secret of the OAuth2 application.
    /// </summary>
    public required string ClientSecret { get; init; }

    /// <summary>
    ///     Redirect URI used for authentication.
    /// </summary>
    public required string RedirectUri { get; init; }

    /// <summary>
    ///     Scopes used for authentication.
    /// </summary>
    public required string[] Scopes { get; init; }

    /// <summary>
    ///     Endpoints used for authentication.
    /// </summary>
    public required OauthEndpointConfiguration Endpoints { get; init; }
}