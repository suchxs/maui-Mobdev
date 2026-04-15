namespace listView_Corsega;

public partial class SignInPage : ContentPage
{
    private bool _isBusy;

    public SignInPage()
    {
        InitializeComponent();
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
        SetBusy(true, "Signing in...");
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
        catch (Exception ex)
        {
            await DisplayAlertAsync("Sign in failed", $"Unexpected error: {ex.Message}", "OK");
        }
        finally
        {
            SetBusy(false);
            _isBusy = false;
        }
    }

    private async void OnGoToSignUpClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SignUpPage));
    }

    private void SetBusy(bool isBusy, string message = "Please wait...")
    {
        BusyMessageLabel.Text = message;
        BusyOverlay.IsVisible = isBusy;
        MainScroll.InputTransparent = isBusy;
    }
}
