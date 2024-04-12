namespace Infrastructure.Configuration;

public class JwtConfiguration
{
    /// <summary>
    ///     Key used for signing JWT tokens.
    /// </summary>
    public required string SigningKey { get; init; }

    /// <summary>
    ///     Audience used by the JWT middleware.
    /// </summary>
    public required string Audience { get; init; }

    /// <summary>
    ///     Issuer used by the JWT middleware.
    /// </summary>
    public required string Issuer { get; init; }
}