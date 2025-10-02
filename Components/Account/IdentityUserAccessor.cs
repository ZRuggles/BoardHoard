using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using BoardHoard.Data;

namespace BoardHoard.Components.Account;

public class IdentityUserAccessor
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityUserAccessor(AuthenticationStateProvider authStateProvider,
                                UserManager<ApplicationUser> userManager)
    {
        _authStateProvider = authStateProvider;
        _userManager = userManager;
    }

    public async Task<ApplicationUser> GetRequiredUserAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var principal = authState.User;

        if (principal?.Identity is null || !principal.Identity.IsAuthenticated)
            throw new InvalidOperationException("No authenticated user is available.");

        var user = await _userManager.GetUserAsync(principal);
        if (user is null)
            throw new InvalidOperationException("Authenticated user not found in UserManager.");

        return user;
    }

    public async Task<string> GetRequiredUserIdAsync()
    {
        var user = await GetRequiredUserAsync();
        return user.Id!;
    }
}
