namespace ShopScout.Services;

/// <summary>
/// Provides state management and notification for the account navigation bar, including its title and back navigation
/// link.
/// </summary>
public class AccountNavbarService
{
    public string Title { get; private set; } = "Default Title";
    public string BackHref { get; private set; } = "/";

    public event Action? OnChange;

    public void SetNavBar(string title, string backHref = "/")
    {
        Title = title;
        BackHref = backHref;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}