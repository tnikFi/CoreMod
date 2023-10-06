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
}