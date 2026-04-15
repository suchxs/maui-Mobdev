using Microsoft.Extensions.DependencyInjection;

namespace listView_Corsega;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        ToDoStore.SetCurrentUser(LocalAuthService.CurrentUser);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}