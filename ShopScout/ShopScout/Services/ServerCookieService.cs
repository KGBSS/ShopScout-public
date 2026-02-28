using Microsoft.JSInterop;

public class ServerCookieService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IJSRuntime _jsRuntime;

    public ServerCookieService(IHttpContextAccessor httpContextAccessor, IJSRuntime jsRuntime)
    {
        _httpContextAccessor = httpContextAccessor;
        _jsRuntime = jsRuntime;
    }

    // Use this for setting cookies during initial page load
    public void SetSharedCookie(string key, object value, string domain = ".shopscout.me", int? expireDays = null)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) return;

        var options = new CookieOptions
        {
            Domain = domain,
            Path = "/",
            IsEssential = true,
            HttpOnly = false,
            Secure = true,
            SameSite = SameSiteMode.Lax
        };

        if (expireDays.HasValue)
            options.Expires = DateTime.Now.AddDays(expireDays.Value);

        context.Response.Cookies.Append(key, value.ToString(), options);
    }

    // Use this for setting cookies in event handlers (after response started)
    public async Task SetSharedCookieAsync(string key, object value, string domain = ".shopscout.me", int expireDays = 365)
    {
        var script = $"document.cookie = '{key}={value}; domain={domain}; path=/; expires={GetExpiresString(expireDays)}; SameSite=Lax; Secure'";

        await _jsRuntime.InvokeVoidAsync("eval", script);
    }

    private string GetExpiresString(int days)
    {
        return DateTime.Now.AddDays(days).ToUniversalTime().ToString("R");
    }

    public string GetCookie(string key)
    {
        var context = _httpContextAccessor.HttpContext;
        return context?.Request.Cookies[key];
    }

    public async Task<string> GetCookieAsync(string key)
    {
        return await _jsRuntime.InvokeAsync<string>("eval",
            $@"(() => {{
                const value = `; ${{document.cookie}}`;
                const parts = value.split(`; {key}=`);
                return parts.length === 2 ? parts.pop().split(';').shift() : '';
            }})()");
    }

    public async Task<bool> GetCookieBoolAsync(string key, bool defaultValue = false)
    {
        var value = await GetCookieAsync(key);
        if (string.IsNullOrEmpty(value))
            return defaultValue;

        return bool.TryParse(value, out var result) ? result : defaultValue;
    }

    public async Task DeleteCookieAsync(string key, string domain = ".shopscout.me")
    {
        await _jsRuntime.InvokeVoidAsync("eval",
            $"document.cookie = '{key}=; domain={domain}; path=/; expires=Thu, 01 Jan 1970 00:00:00 UTC'");
    }
}