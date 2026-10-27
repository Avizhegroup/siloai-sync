namespace SiloAI.UI.Pages.Account;

public partial class Login
{
    public bool IsLoading = false;
    public LoginModel Request = new();
    public bool _showPassword;

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
        if (IsLoading)
        {
            return;
        }

        IsLoading = true;

        bool success = await AuthenticationService.Login(Request.Username, Request.Password);

            if (success)
            {
                NavigationManager.NavigateTo("/", forceLoad: true);

                return;
            }

            else
            {

            Notification.Show("نام کاربری یا رمز عبور نامعتبر است.", "error");

            }
        
            IsLoading = false;       
    }
}
