namespace listView_Corsega;

public partial class SignUpPage : ContentPage
{
    private bool _isBusy;

    public SignUpPage()
    {
        InitializeComponent();
    }

    private async void OnSignUpClicked(object? sender, EventArgs e)
    {
        if (_isBusy)
        {
            return;
        }

        var firstName = FirstNameEntry.Text?.Trim();
        var lastName = LastNameEntry.Text?.Trim();
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text?.Trim();
        var confirmPassword = ConfirmPasswordEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(confirmPassword))
        {
            await DisplayAlertAsync("Missing details", "Please complete all fields.", "OK");
            return;
        }

        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
        {
            await DisplayAlertAsync("Password mismatch", "Password and confirm password must match.", "OK");
            return;
        }

        _isBusy = true;
        SetBusy(true, "Creating account...");
        try
        {
            var result = await ToDoApiClient.SignUpAsync(firstName, lastName, email, password, confirmPassword);
            if (!result.Success)
            {
                await DisplayAlertAsync("Sign up failed", result.Message, "OK");
                return;
            }

            await DisplayAlertAsync("Account created", result.Message, "Continue");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Sign up failed", $"Unexpected error: {ex.Message}", "OK");
        }
        finally
        {
            SetBusy(false);
            _isBusy = false;
        }
    }

    private async void OnGoToSignInClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private void SetBusy(bool isBusy, string message = "Please wait...")
    {
        BusyMessageLabel.Text = message;
        BusyOverlay.IsVisible = isBusy;
        MainScroll.InputTransparent = isBusy;
    }
}
