using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using ShopScout.Components.Account.Pages;
using ShopScout.Components.Admin;
using ShopScout.SharedLib.Models;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;

namespace ShopScout.Services;

public class UserService
{
    private readonly UserManager<ApplicationUser> UserManager;
    private readonly RoleManager<IdentityRole> RoleManager;
    private readonly ILogger<UserService> Logger;
    private readonly SignInManager<ApplicationUser> SignInManager;
    private readonly NavigationManager NavigationManager;
    private readonly IEmailSender _emailSender;
    public UserService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<UserService> logger,
        RoleManager<IdentityRole> roleManager,
        NavigationManager navigationManager,
        IEmailSender emailSender)
    {
        UserManager = userManager;
        Logger = logger;
        NavigationManager = navigationManager;
        SignInManager = signInManager;
        RoleManager = roleManager;
        _emailSender = emailSender;
    }

    /// <summary>
    /// Sets a unique username for the user based on their email address.
    /// Ensures the username is at least 4 characters and adds a numeric suffix if the username is already taken.
    /// </summary>
    /// <param name="user">The user to set the username for</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task SetUniqueUserName(ApplicationUser user)
    {
        var username = user.Email.Split('@')[0];
        if (username.Length < 4)
        {
            username = username.PadRight(4, '0');
        }
        
        user.UserName = username;

        int i = 1;
        while (await UserManager.FindByNameAsync(user.UserName) != null)
        {
            user.UserName = $"{username}{i++}";
        }
    }

    /// <summary>
    /// Creates a new user account with email and password authentication.
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <param name="password">The user's password</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task CreateUserAsync(string email, string password)
    {
        var user = await CreateApplicationUser(email);
        if (user is null)
        {
            RedirectToRegisterWithError();
            return;
        }
        var result = await UserManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await CompleteUserRegistration(user);
            await SignInManager.SignInAsync(user, isPersistent: true);

            var callbackUrl = await GetCallbackUrl(user);
            await _emailSender.SendWelcomeWithEmailConfirmation(email, HtmlEncoder.Default.Encode(callbackUrl));

            NavigationManager.NavigateTo("/Account/Welcome");
            return;
        }

        RedirectToRegisterWithError();
    }

    private async Task<string> GetCallbackUrl(ApplicationUser user)
    {
        var userId = await UserManager.GetUserIdAsync(user);
        var code = await UserManager.GenerateChangeEmailTokenAsync(user, user.Email);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        var callbackUrl = NavigationManager.GetUriWithQueryParameters(
            NavigationManager.ToAbsoluteUri("Account/ConfirmEmailChange").AbsoluteUri,
            new Dictionary<string, object?> { ["userId"] = userId, ["email"] = user.Email, ["code"] = code });

        return callbackUrl;
    }

    /// <summary>
    /// Creates a new user account using external login provider authentication.
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <param name="externalLoginInfo">External login information which the user used to register</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task CreateUserAsync(string email, ExternalLoginInfo externalLoginInfo)
    {
        var user = await CreateApplicationUser(email);
        if (user is null)
        {
            RedirectToRegisterWithError();
            return;
        }

        user.EmailConfirmed = true;
        var result = await UserManager.CreateAsync(user);

        if (!result.Succeeded)
        {
            RedirectToRegisterWithError();
            return;
        }

        await CompleteUserRegistration(user);

        result = await UserManager.AddLoginAsync(user, externalLoginInfo);
        if (result.Succeeded)
        {
            await SignInManager.SignInAsync(user, isPersistent: true, externalLoginInfo.LoginProvider);
            await _emailSender.SendWelcomeWithoutEmailConfirmation(user.Email);
            NavigationManager.NavigateTo("/Account/Welcome");
            return;
        }

        RedirectToRegisterWithError();
    }

    /// <summary>
    /// Creates a new ApplicationUser instance with default values.
    /// </summary>
    /// <param name="email">The user's email address</param>
    /// <returns>A new ApplicationUser instance</returns>
    private async Task<ApplicationUser> CreateApplicationUser(string email)
    {
        var existingUser = await UserManager.FindByEmailAsync(email);
        if (existingUser is not null) return null;
        var user = new ApplicationUser
        {
            Email = email,
            RegistrationDate = DateTime.Today,
            LastLoginTime = DateTime.Now
        };

        await SetUniqueUserName(user);
        return user;
    }

    /// <summary>
    /// Completes the user registration process by setting username, assigning role, and logging the event.
    /// </summary>
    /// <param name="user">The user to complete registration for</param>
    /// <param name="logMessage">The message to log upon successful registration</param>
    /// <returns>A task representing the asynchronous operation</returns>
    private async Task CompleteUserRegistration(ApplicationUser user)
    {
        await UserManager.AddToRoleAsync(user, "USER");
        string logMessage;
        if (await UserManager.HasPasswordAsync(user))
        {
            logMessage = "User created an account using Google.";
            
        }
        else
            logMessage = "User created a new account with password.";
        Logger.LogInformation(logMessage);
    }

    /// <summary>
    /// Redirects to the registration page with a generic error message.
    /// </summary>
    private void RedirectToRegisterWithError()
    {
        var errorMessage = WebUtility.UrlEncode("err: valami hiba történt");
        NavigationManager.NavigateTo($"/Account/Register?errorMessage={errorMessage}", true);
    }

    public async Task<List<ApplicationUser>> GetAllUsersAsync()
    {
        return await UserManager.Users.ToListAsync();
    }

    public async Task<List<string>> GetAllRolesAsync()
    {
        return await RoleManager.Roles.Select(x => x.Name).ToListAsync();
    }

    public async Task<bool> UpdateUserAsync(ApplicationUser user, List<string> roles)
    {
        var dbUser = await UserManager.FindByIdAsync(user.Id);
        if (dbUser == null)
        {
            return false;
        }

        dbUser.UserName = user.UserName;
        dbUser.Email = user.Email;
        dbUser.LanguageCode = user.LanguageCode;
        dbUser.IsAnonymous = user.IsAnonymous;

        var result = await UserManager.UpdateAsync(dbUser);
        if (!result.Succeeded)
        {
            return false;
        }

        var currentRoles = await UserManager.GetRolesAsync(dbUser);

        var rolesToAdd = roles.Except(currentRoles).ToList();
        var rolesToRemove = currentRoles.Except(roles).ToList();

        if (rolesToAdd.Any())
        {
            await UserManager.AddToRolesAsync(dbUser, rolesToAdd);
        }

        if (rolesToRemove.Any())
        {
            await UserManager.RemoveFromRolesAsync(dbUser, rolesToRemove);
        }

        return true;
    }

    public async Task<bool> ToggleBanUserAsync(string UserId)
    {
        var dbUser = await UserManager.FindByIdAsync(UserId);
        if (dbUser == null)
        {
            return false;
        }

        dbUser.IsBanned = !dbUser.IsBanned;

        var result = await UserManager.UpdateAsync(dbUser);
        return result.Succeeded;
    }

    public async Task<Dictionary<string, IList<string>>> GetAllUserRolesAsync(List<ApplicationUser> Users)
    {
        Dictionary<string, IList<string>> dict = new();
        foreach (var user in Users)
        {
            dict.Add(user.Id, await UserManager.GetRolesAsync(user));
        }

        return dict;
    }
}