namespace listView_Corsega;

public partial class AddTodoPage : ContentPage
{
    public AddTodoPage()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        var title = TitleEntry.Text?.Trim();
        var details = DetailsEditor.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(title))
        {
            await DisplayAlertAsync("Missing title", "Please enter a title for your note.", "OK");
            return;
        }

        ToDoStore.Add(title, details);
        await Shell.Current.GoToAsync("..");
    }
}
