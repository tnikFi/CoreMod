﻿using System.Net.Http.Headers;
using System.Text;
using Infrastructure.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("[controller]")]
public class LogoutController : ControllerBase
{
    private readonly AuthConfiguration _authConfiguration;
    private readonly HttpClient _httpClient = new();

    public LogoutController(AuthConfiguration authConfiguration)
    {
        _authConfiguration = authConfiguration;
    }

    /// <summary>
    ///     Revokes the user's access token. This endpoint will revoke the OAuth2 access token used to initially
    ///     authenticate the user, but will not automatically invalidate the JWT used for authentication with the API.
    /// </summary>
    /// <param name="token">Access token to revoke.</param>
    /// <param name="tokenTypeHint"></param>
    /// <param name="postLogoutRedirectUri"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Logout(string token, [FromQuery(Name = "token_type_hint")] string tokenTypeHint,
        [FromQuery(Name = "post_logout_redirect_uri")]
        string postLogoutRedirectUri)
    {
        var form = new Dictionary<string, string>
        {
            {"token", token},
            {"token_type_hint", tokenTypeHint}
        };
        var credentials =
            Encoding.UTF8.GetBytes($"{_authConfiguration.Oauth.ClientId}:{_authConfiguration.Oauth.ClientSecret}");
        var request = new HttpRequestMessage(HttpMethod.Post, _authConfiguration.Oauth.Endpoints.Revoke)
        {
            Content = new FormUrlEncodedContent(form),
            Headers =
            {
                Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(credentials))
            }
        };
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return Redirect(postLogoutRedirectUri);
    }
}