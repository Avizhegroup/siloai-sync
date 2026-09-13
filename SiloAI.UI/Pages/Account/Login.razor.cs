using System.ComponentModel.DataAnnotations;

namespace SiloAI.UI.Pages.Account;

public partial class Login
{
    public bool IsLoading;
    public LoginModel Request = new();

    [Inject] public IAiAuthenticationService AuthenticationService { get; set; }
    [Inject] public NavigationManager NavigationManager { get; set; }
    [Inject] public AuthenticationStateProvider AuthStateProvider { get; set; }

    [CascadingParameter] public TelerikNotification Notification { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var state = await AuthStateProvider.GetAuthenticationStateAsync();

        if (state.User.Identity?.IsAuthenticated == true)
        {
            NavigationManager.NavigateTo("/", forceLoad: false);
        }

#if DEBUG
        Request = new()
        {
            Username = "admin",
            Password = "Admin@123",
        };
#endif
    }

    private async Task OnLoginSubmit()
    {
        IsLoading = true;

        bool success = await AuthenticationService.Login(Request.Username, Request.Password);

        if (success)
        {
            NavigationManager.NavigateTo("/", forceLoad: true);
        }
        else
        {
            Notification.Show("نام کاربری یا رمز عبور نامعتبر است.", "error");
        }

        IsLoading = false;
    }

    public sealed class LoginModel
    {
        [Required(ErrorMessage = "نام کاربری الزامی است.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "رمز عبور الزامی است.")]
        public string Password { get; set; } = string.Empty;
    }
}
