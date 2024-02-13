using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Todo.Web.Client.Server.Authentication;

namespace Todo.Web.Client.Server.Components.Pages;

[AllowAnonymous]
public class LoginModel : PageModel
{
    public string ReturnUrl { get; set; }

    private readonly AuthClient _authClient;

    public LoginModel(AuthClient authClient)
    {
        _authClient = authClient;
    }
    
    public async Task<IActionResult> OnPost(string username, string password)
    {
        string returnUrl = Url.Content("~/");
        try
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
        catch
        {
            
        }
        
        var token = await _authClient.GetTokenAsync(new UserInfo { Username = username, Password = password });

        if (token is not null)
        {
            await SignIn(new UserInfo { Username = username, Password = password }, token);
        }
        
        return LocalRedirect(returnUrl);
    }
    
    
    private async Task SignIn(UserInfo userInfo, string token)
    {
        await SignIn(userInfo.Username, userInfo.Username, token, providerName: null);
    }

    private async Task SignIn(string userId, string userName, string token, string? providerName)
    {
        var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));
        identity.AddClaim(new Claim(ClaimTypes.Name, userName));

        var properties = new AuthenticationProperties();

        // Store the external provider name so we can do remote sign out
        if (providerName is not null)
        {
            properties.SetExternalProvider(providerName);
        }

        var tokens = new[]
        {
            new AuthenticationToken { Name = TokenNames.AccessToken, Value = token }
        };

        properties.StoreTokens(tokens);

        var httpContext = HttpContext ?? throw new InvalidOperationException("HttpContext is null");
        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), properties);
        httpContext.Session.SetString(TokenNames.AccessToken, token);
    }
}