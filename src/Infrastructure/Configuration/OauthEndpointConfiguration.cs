namespace Infrastructure.Configuration;

public class OauthEndpointConfiguration
{
    /// <summary>
    ///     Endpoint used for getting the access token using the authorization code.
    /// </summary>
    public required string Token { get; init; }

    /// <summary>
    ///     Endpoint used for getting the user info of the authenticated user.
    /// </summary>
    public required string UserInfo { get; init; }
}