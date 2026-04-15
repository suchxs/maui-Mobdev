namespace listView_Corsega;

public partial class SignInPage : ContentPage
{
    private bool _isBusy;

    public SignInPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isBusy)
        {
            return;
        }

        var currentUser = LocalAuthService.CurrentUser;
        if (currentUser is null)
        {
            return;
        }

        _isBusy = true;
        try
        {
            ToDoStore.SetCurrentUser(currentUser);
            var refreshResult = await ToDoStore.RefreshAsync();
            if (!refreshResult.Success)
            {
                LocalAuthService.ClearCurrentUser();
                ToDoStore.SetCurrentUser(null);
                return;
            }

            await Shell.Current.GoToAsync("//MainPage");
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async void OnSignInClicked(object? sender, EventArgs e)
    {
        if (_isBusy)
        {
            return;
        }

        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlertAsync("Missing details", "Please enter your email and password.", "OK");
            return;
        }

        _isBusy = true;
        try
        {
            var result = await ToDoApiClient.SignInAsync(email, password);
            if (!result.Success || result.User is null)
            {
                await DisplayAlertAsync("Sign in failed", result.Message, "OK");
                return;
            }

            LocalAuthService.SetCurrentUser(result.User);
            ToDoStore.SetCurrentUser(result.User);

            var refreshResult = await ToDoStore.RefreshAsync();
            if (!refreshResult.Success)
            {
                await DisplayAlertAsync("Warning", refreshResult.Message, "OK");
            }

            await Shell.Current.GoToAsync("//MainPage");
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async void OnGoToSignUpClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SignUpPage));
    }
}
