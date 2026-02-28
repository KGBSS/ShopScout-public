using Microsoft.AspNetCore.Identity.UI.Services;
using ShopScout.Data.EmailTemplates;
using ShopScout.SharedLib.Models;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ShopScout.Services
{
    public sealed class EmailSender : IEmailSender
    {
        private readonly HttpClient _client;
        private readonly Creds _credentials;
        private string? _accessToken;
        private string? _refreshToken;
        private DateTime _tokenExpiry;

        private record Creds(string Username, string Password);

        public EmailSender()
        {
            _client = new HttpClient()
            {
                BaseAddress = new Uri("https://shopscout.hu", UriKind.Absolute)
            };
            try
            {
                _credentials = GetCredentials();
            }
            catch { }
        }

        private async Task<bool> EnsureAuthenticatedAsync()
        {
            if (_accessToken != null && DateTime.UtcNow < _tokenExpiry)
            {
                return true;
            }

            if (_refreshToken != null)
            {
                if (await RefreshTokenAsync())
                {
                    return true;
                }
            }

            return await AuthenticateAsync();
        }

        private async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            if (!await EnsureAuthenticatedAsync()) return;
            await SendAsync(email, subject, htmlMessage);
        }

        private async Task SendAsync(string email, string subject, string htmlMessage)
        {
            var emailData = new
            {
                to = email,
                subject = subject,
                messageHTML = htmlMessage
            };

            // Serialize with custom options to prevent escaping
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(emailData, options);
            Console.WriteLine("Sending JSON:");
            Console.WriteLine(json);

            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync(EmailApiEndpoints.SEND_MAIL, content);

            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine($"Response: {responseBody}");

        }

        private async Task<bool> AuthenticateAsync()
        {
            var response = await _client.PostAsJsonAsync(EmailApiEndpoints.AUTH, _credentials);

            if (!response.IsSuccessStatusCode) return false;

            await ParseResponse(response);
            return true;
        }

        private async Task<bool> RefreshTokenAsync()
        {
            var response = await _client.PostAsJsonAsync(EmailApiEndpoints.REFRESH,
                new { Token = _refreshToken });

            if (!response.IsSuccessStatusCode)
            {
                _refreshToken = null;
                return false;
            }

            await ParseResponse(response);
            return true;
        }

        private async Task ParseResponse(HttpResponseMessage response)
        {
            var jsonString = await response.Content.ReadAsStringAsync();
            using var result = JsonDocument.Parse(jsonString);

            _accessToken = result.RootElement.GetProperty("accessToken").GetString();
            _refreshToken = result.RootElement.GetProperty("refreshToken").GetString();

            var expiryString = result.RootElement.GetProperty("accessTokenExpiration").GetString();
            _tokenExpiry = DateTime.Parse(expiryString, null, System.Globalization.DateTimeStyles.RoundtripKind);

            _client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
        }

        private static Creds GetCredentials()
        {
            var username = Environment.GetEnvironmentVariable("EMAIL_USER")
                ?? throw new InvalidOperationException("EMAIL_USER not set");
            var password = Environment.GetEnvironmentVariable("EMAIL_PASS")
                ?? throw new InvalidOperationException("EMAIL_PASS not set");

            return new Creds(username, password);
        }

        public async Task SendConfirmationLinkAsync(string email, string confirmationLink) =>
            await SendEmailAsync(email, "Email megerősítése", EmailTemplates.GetEmailConfirmation(confirmationLink));

        public async Task SendPasswordResetLinkAsync(string email, string resetLink) =>
            await SendEmailAsync(email, "Elfelejtett jelszó", EmailTemplates.GetPasswordReset(resetLink));

        public async Task SendWelcomeWithEmailConfirmation(string email, string confirmationLink) =>
            await SendEmailAsync(email, "Üdv a ShopScout-nál!", EmailTemplates.GetWelcomeWithEmailConfirmation(confirmationLink));

        public async Task SendWelcomeWithoutEmailConfirmation(string email) =>
            await SendEmailAsync(email, "Üdv a ShopScout-nál!", EmailTemplates.GetWelcomeWithoutEmailConfirmation());

        internal static class EmailApiEndpoints
        {
            private const string BASE_URL = "https://mail5010.site4now.net/api/v1";

            public const string AUTH = BASE_URL + "/auth/authenticate-user";
            public const string REFRESH = BASE_URL + "/auth/refresh-token";
            public const string SEND_MAIL = BASE_URL + "/mail/message-put";
        }
    }
}