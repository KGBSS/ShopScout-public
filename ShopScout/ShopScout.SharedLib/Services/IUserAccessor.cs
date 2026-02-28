using Microsoft.EntityFrameworkCore.Query;
using ShopScout.SharedLib.Models;
using System.Linq.Expressions;

namespace ShopScout.SharedLib.Services;

public interface IUserAccessor
{
    /// <summary>
    /// Gets the currently authenticated user with optional related entities.
    /// </summary>
    /// <param name="includes">Optional navigation properties to include in the query.</param>
    /// <returns>The current ApplicationUser with specified includes, or null if not authenticated.</returns>
    Task<ApplicationUser?> GetCurrentUserAsync<T>(Expression<Func<ApplicationUser, T>> includes);

    /// <summary>
    /// Gets the currently authenticated user with optional related entities.
    /// </summary>
    /// <param name="includes">Optional navigation properties to include in the query.</param>
    /// <returns>The current ApplicationUser with specified includes, or null if not authenticated.</returns>
    Task<ApplicationUser?> GetCurrentUserAsync(params Expression<Func<ApplicationUser, object>>[] includes);

    /// <summary>
    /// Gets the currently authenticated user with multiple related entities and supports chaining.
    /// </summary>
    Task<ApplicationUser?> GetCurrentUserAsync(
        Func<IQueryable<ApplicationUser>, IIncludableQueryable<ApplicationUser, object>> includeFunc);

    /// <summary>
    /// Gets the ID of the currently authenticated user.
    /// </summary>
    /// <returns>The current user's ID or null if not authenticated</returns>
    Task<string?> GetCurrentUserIdAsync();

    /// <summary>
    /// Checks if the current user is authenticated.
    /// </summary>
    /// <returns>True if authenticated, false otherwise</returns>
    Task<bool> IsAuthenticatedAsync();

    /// <summary>
    /// Adds an item to a many-to-many collection of the user and saves changes to the database.
    /// </summary>
    /// <typeparam name="T">The type of entity in the collection (must have an Id property)</typeparam>
    /// <param name="user">The user to update</param>
    /// <param name="navigationProperty">Expression pointing to the collection property (e.g., u => u.FavoriteProducts)</param>
    /// <param name="itemToAdd">The item to add to the collection</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task AddToCollectionAsync<T>(
        ApplicationUser user,
        Expression<Func<ApplicationUser, ICollection<T>>> navigationProperty,
        T itemToAdd) where T : class;

    /// <summary>
    /// Removes an item from a many-to-many collection of the user and saves changes to the database.
    /// </summary>
    /// <typeparam name="T">The type of entity in the collection (must have an Id property)</typeparam>
    /// <param name="user">The user to update</param>
    /// <param name="navigationProperty">Expression pointing to the collection property (e.g., u => u.FavoriteProducts)</param>
    /// <param name="itemToRemove">The item to remove from the collection</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task RemoveFromCollectionAsync<T>(
        ApplicationUser user,
        Expression<Func<ApplicationUser, ICollection<T>>> navigationProperty,
        T itemToRemove) where T : class;

    /// <summary>
    /// Checks if the current user has a specific role.
    /// </summary>
    /// <param name="role">The role name to check</param>
    /// <returns>True if user has the role, false otherwise</returns>
    Task<bool> IsInRoleAsync(string role);

    /// <summary>
    /// Gets the email of the currently authenticated user.
    /// </summary>
    /// <returns>The current user's email or null if not authenticated</returns>
    Task<string?> GetCurrentUserEmailAsync();

    /// <summary>
    /// Gets the username of the currently authenticated user.
    /// </summary>
    /// <returns>The current user's username or null if not authenticated</returns>
    Task<string?> GetCurrentUserNameAsync();

    /// <summary>
    /// Requires the user to be authenticated, redirects to login if not.
    /// </summary>
    /// <returns>The authenticated user or null if redirected</returns>
    Task<ApplicationUser?> RequireAuthenticatedUserAsync();
}