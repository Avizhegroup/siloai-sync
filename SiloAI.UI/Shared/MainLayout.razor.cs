using Microsoft.AspNetCore.Components.Routing;

namespace SiloAI.UI.Shared;

public partial class MainLayout
{
    public TelerikNotification Notification { get; set; }

    private bool _menuOpen;

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        _menuOpen = false;
        InvokeAsync(StateHasChanged);
    }

    private void ToggleMenu() => _menuOpen = !_menuOpen;
    private void CloseMenu() => _menuOpen = false;

    private async Task Logout()
    {
        await AuthService.Logout();
        NavigationManager.NavigateTo("/account/login", forceLoad: true);
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }
}
