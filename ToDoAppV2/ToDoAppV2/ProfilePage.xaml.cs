namespace listView_Corsega;

public partial class ProfilePage : ContentPage
{
    private bool _isBusy;

    public ProfilePage()
    {
        InitializeComponent();
    }

    private async void OnSignOutClicked(object? sender, EventArgs e)
    {
        if (_isBusy)
        {
            return;
        }

        var confirm = await DisplayAlertAsync("Confirm", "Do you want to sign out?", "Sign out", "Cancel");
        if (!confirm)
        {
            return;
        }

        _isBusy = true;
        SignOutButton.IsEnabled = false;
        SignOutButton.Text = "Signing out...";

        LocalAuthService.ClearCurrentUser();
        ToDoStore.SetCurrentUser(null);

        await Shell.Current.GoToAsync("//Auth/SignInPage");

        _isBusy = false;
        SignOutButton.IsEnabled = true;
        SignOutButton.Text = "Sign out";
    }
}
