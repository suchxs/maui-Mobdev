namespace listView_Corsega;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(SignUpPage), typeof(SignUpPage));
        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(CompletedPage), typeof(CompletedPage));
        Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
        Routing.RegisterRoute(nameof(AddTodoPage), typeof(AddTodoPage));
        Routing.RegisterRoute(nameof(EditTodoPage), typeof(EditTodoPage));
    }
}