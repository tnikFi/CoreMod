using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Discord.Rest;
using Infrastructure.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Web.Models;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TokenController : ControllerBase
{
    private readonly AuthConfiguration _authConfiguration;
    private readonly HttpClient _httpClient;

    public TokenController(AuthConfiguration authConfiguration)
    {
        _authConfiguration = authConfiguration;
        _httpClient = new HttpClient();
    }

    /// <summary>
    ///     Get the token using PKCE.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="codeVerifier"></param>
    /// <returns></returns>
    [HttpPost(Name = "token")]
    public async Task<ActionResult<object>> GetToken([FromForm] string code,
        [FromForm(Name = "code_verifier")] string codeVerifier)
    {
        var request = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            {"client_id", _authConfiguration.Oauth.ClientId},
            {"client_secret", _authConfiguration.Oauth.ClientSecret},
            {"grant_type", "authorization_code"},
            {"code", code},
            {"redirect_uri", _authConfiguration.Oauth.RedirectUri},
            {"code_verifier", codeVerifier}
        });

        var response = await _httpClient.PostAsync(_authConfiguration.Oauth.Endpoints.Token, request);

        // Deserialize the response content into a PkceResponse object.
        var responseContent = await response.Content.ReadFromJsonAsync<PkceResponse>();

        // Request user information from the OAuth2 provider.
        var userRequest = new HttpRequestMessage(HttpMethod.Get, _authConfiguration.Oauth.Endpoints.UserInfo);
        userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", responseContent.AccessToken);
        var userResponse = await _httpClient.SendAsync(userRequest);
        userResponse.EnsureSuccessStatusCode();
        var user = await userResponse.Content.ReadFromJsonAsync<RestUser>() ?? throw new Exception("User not found.");

        // Add claims for the jwt token.
        var permClaims = new List<Claim>();
        permClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
        permClaims.Add(new Claim("userId", user.Id.ToString()));
        permClaims.Add(new Claim("userName", user.GlobalName));
        permClaims.Add(new Claim("avatar", user.GetAvatarUrl()));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authConfiguration.Jwt.SigningKey));
        var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _authConfiguration.Jwt.Issuer,
            _authConfiguration.Jwt.Audience,
            permClaims,
            expires: DateTime.Now.AddDays(1),
            signingCredentials: signingCredentials
        );

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        // Add the token to the response.
        responseContent.IdToken = jwt;

        return responseContent;
    }
}