namespace listView_Corsega;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
    }

    private async void OnSignOutClicked(object? sender, EventArgs e)
    {
        var confirm = await DisplayAlertAsync("Sign out", "Do you want to sign out?", "Sign out", "Cancel");
        if (!confirm)
        {
            return;
        }

        LocalAuthService.ClearCurrentUser();
        ToDoStore.SetCurrentUser(null);

        await Shell.Current.GoToAsync("//Auth/SignInPage");
    }
}
