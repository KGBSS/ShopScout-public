//using ShopScout.Data.EmailTemplates;
//using ShopScout.SharedLib.Models;
//using ShopScout.Services;

//namespace ShopScout.Components.Account;

//internal sealed class IdentityEmailSender : IEmailSender
//{
//    public IdentityEmailSender()
//    {
//    }

//    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
//        _emailSender.SendEmailAsync(email, "Confirm your email", EmailTemplates.GetEmailConfirmation(confirmationLink));

//    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
//        _emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

//    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
//        _emailSender.SendEmailAsync(email, "Reset your password", $"Please reset your password using the following code: {resetCode}");
//}