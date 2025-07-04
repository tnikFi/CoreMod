namespace Infrastructure.Configuration;

public class AuthConfiguration
{
    /// <summary>
    ///     OAuth2 configuration.
    /// </summary>
    public required OauthConfiguration Oauth { get; init; }

    /// <summary>
    ///     JWT configuration.
    /// </summary>
    public required JwtConfiguration Jwt { get; init; }
    
    /// <summary>
    ///     Allowed logout redirect URIs.
    ///     If a logout request is made with a redirect URI that is not in this list,
    ///     the first allowed redirect URI will be used instead.
    /// </summary>
    public required string[] AllowedLogoutRedirects { get; init; }
}