namespace listView_Corsega;

public partial class SignInPage : ContentPage
{
    public SignInPage()
    {
        InitializeComponent();
    }

    private async void OnSignInClicked(object? sender, EventArgs e)
    {
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlertAsync("Missing details", "Please enter your email and password.", "OK");
            return;
        }

        await Shell.Current.GoToAsync(nameof(MainPage));
    }

    private async void OnGoToSignUpClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(SignUpPage));
    }
}
