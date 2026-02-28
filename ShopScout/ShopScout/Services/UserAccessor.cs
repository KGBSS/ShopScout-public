using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using ShopScout.Data;
using ShopScout.SharedLib.Models;
using ShopScout.SharedLib.Services;
using System;
using System.Linq.Expressions;
using System.Security.Claims;

namespace ShopScout.Services;

public class UserAccessor : IUserAccessor
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly NavigationManager _navigationManager;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public UserAccessor(
        UserManager<ApplicationUser> userManager,
        NavigationManager navigationManager,
        AuthenticationStateProvider authenticationStateProvider,
        IDbContextFactory<ApplicationDbContext> contextFactory
        )
    {
        _userManager = userManager;
        _navigationManager = navigationManager;
        _authenticationStateProvider = authenticationStateProvider;
        _contextFactory = contextFactory;
    }

    public async Task<ApplicationUser?> GetCurrentUserAsync<T>(Expression<Func<ApplicationUser, T>> includes)
    {
        var user = await GetAuthenticatedUserClaimAsync();
        if (user == null) return null;

        using var context = await _contextFactory.CreateDbContextAsync();
        var userId = _userManager.GetUserId(user);

        IQueryable<ApplicationUser> query = context.Users;

        query = query.Include(includes);

        return await query.FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<ApplicationUser?> GetCurrentUserAsync(
        params Expression<Func<ApplicationUser, object>>[] includes)
    {
        var user = await GetAuthenticatedUserClaimAsync();
        if (user == null) return null;

        using var context = await _contextFactory.CreateDbContextAsync();
        var userId = _userManager.GetUserId(user);

        IQueryable<ApplicationUser> query = context.Users;

        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        return await query.FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<ApplicationUser?> GetCurrentUserAsync(
        Func<IQueryable<ApplicationUser>, IIncludableQueryable<ApplicationUser, object>> includeFunc)
    {
        var user = await GetAuthenticatedUserClaimAsync();
        if (user == null) return null;

        using var context = await _contextFactory.CreateDbContextAsync();
        var userId = _userManager.GetUserId(user);

        IQueryable<ApplicationUser> query = context.Users;
        query = includeFunc(query);

        return await query.FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<string?> GetCurrentUserIdAsync()
    {
        var user = await GetAuthenticatedUserClaimAsync();
        return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private async Task<ClaimsPrincipal?> GetAuthenticatedUserClaimAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        return user.Identity?.IsAuthenticated == true ? user : null;
    }

    private async Task<ApplicationUser?> GetAuthenticatedUserAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        return user.Identity?.IsAuthenticated == true ? await _userManager.GetUserAsync(user) : null;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        return authState.User.Identity?.IsAuthenticated ?? false;
    }

    public async Task<bool> IsInRoleAsync(string role)
    {
        var user = await GetAuthenticatedUserAsync();
        if (user == null)
            return false;

        return await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<string?> GetCurrentUserEmailAsync()
    {
        var user = await GetAuthenticatedUserAsync();
        return user?.Email;
    }

    public async Task<string?> GetCurrentUserNameAsync()
    {
        var user = await GetAuthenticatedUserAsync();
        return user?.UserName;
    }

    public async Task<ApplicationUser?> RequireAuthenticatedUserAsync()
    {
        var user = await GetAuthenticatedUserAsync();
        if (user == null)
        {
            _navigationManager.NavigateTo("/Account/Login", true);
            return null;
        }
        return user;
    }

    public async Task AddToCollectionAsync<T>(
    ApplicationUser user,
    Expression<Func<ApplicationUser, ICollection<T>>> navigationProperty,
    T itemToAdd) where T : class
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        var existingUser = await _context.Users
            .Include(navigationProperty)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        if (existingUser == null)
            return;

        // Get the collection
        var compiled = navigationProperty.Compile();
        var collection = compiled(existingUser);

        // Get the Id property
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty == null)
            throw new InvalidOperationException($"Type {typeof(T).Name} must have an Id property");

        var itemId = idProperty.GetValue(itemToAdd);

        // Check if already exists
        var alreadyExists = collection.Any(e => Equals(idProperty.GetValue(e), itemId));

        if (!alreadyExists)
        {
            // Get tracked entity
            var trackedItem = await _context.FindAsync<T>(itemId);
            if (trackedItem != null)
            {
                collection.Add(trackedItem);
                await _context.SaveChangesAsync();
            }
        }
    }

    public async Task RemoveFromCollectionAsync<T>(
        ApplicationUser user,
        Expression<Func<ApplicationUser, ICollection<T>>> navigationProperty,
        T itemToRemove) where T : class
    {
        using var _context = await _contextFactory.CreateDbContextAsync();

        var existingUser = await _context.Users
            .Include(navigationProperty)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        if (existingUser == null)
            return;

        var compiled = navigationProperty.Compile();
        var collection = compiled(existingUser);

        var idProperty = typeof(T).GetProperty("Id");
        var itemId = idProperty?.GetValue(itemToRemove);

        var itemInCollection = collection.FirstOrDefault(e => Equals(idProperty?.GetValue(e), itemId));

        if (itemInCollection != null)
        {
            collection.Remove(itemInCollection);
            await _context.SaveChangesAsync();
        }
    }
}