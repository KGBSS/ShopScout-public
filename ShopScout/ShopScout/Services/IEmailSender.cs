using ShopScout.SharedLib.Models;

namespace ShopScout.Services;

public interface IEmailSender
{
    
    /// <summary>
    /// Asynchronously sends an email containing a confirmation link to the specified address.
    /// </summary>
    /// <param name="email">The email address to which the confirmation link will be sent. Cannot be null or empty.</param>
    /// <param name="confirmationLink">The URL to include in the confirmation email. Must be a valid, absolute URI.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    Task SendConfirmationLinkAsync(string email, string confirmationLink);

    /// <summary>
    /// Sends a password reset link to the specified email address asynchronously.
    /// </summary>
    /// <param name="email">The email address of the user to receive the password reset link. Cannot be null or empty.</param>
    /// <param name="resetLink">The URL to include in the password reset email. Must be a valid, absolute URI.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendPasswordResetLinkAsync(string email, string resetLink);

    /// <summary>
    /// Sends a welcome email to the specified recipient with a confirmation link.
    /// </summary>
    /// <param name="email">The email address of the recipient. Cannot be null or empty.</param>
    /// <param name="confirmationLink">The URL that the recipient can use to confirm their email address. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendWelcomeWithEmailConfirmation(string email, string confirmationLink);

    /// <summary>
    /// Sends a welcome message to the specified email address without email confirmation.
    /// </summary>
    /// <param name="email">The email address to which the welcome message will be sent. Cannot be null or empty.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SendWelcomeWithoutEmailConfirmation(string email);
}