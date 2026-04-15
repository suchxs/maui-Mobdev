namespace listView_Corsega;

public partial class SignUpPage : ContentPage
{
    public SignUpPage()
    {
        InitializeComponent();
    }

    private async void OnSignUpClicked(object? sender, EventArgs e)
    {
        var username = UsernameEntry.Text?.Trim();
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text?.Trim();
        var confirmPassword = ConfirmPasswordEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(username) ||
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

        if (!LocalAuthService.TrySignUp(username, email, password, out var message))
        {
            await DisplayAlertAsync("Sign up failed", message, "OK");
            return;
        }

        await DisplayAlertAsync("Account created", message, "Continue");
        await Shell.Current.GoToAsync("..");
    }

    private async void OnGoToSignInClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
