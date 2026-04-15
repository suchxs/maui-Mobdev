namespace listView_Corsega;

public partial class AddTodoPage : ContentPage
{
    private bool _isBusy;

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
        if (_isBusy)
        {
            return;
        }

        var title = TitleEntry.Text?.Trim();
        var details = DetailsEditor.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(title))
        {
            await DisplayAlertAsync("Missing title", "Please enter a title for your note.", "OK");
            return;
        }

        _isBusy = true;
        try
        {
            var result = await ToDoStore.AddAsync(title, details);
            if (!result.Success)
            {
                await DisplayAlertAsync("Add failed", result.Message, "OK");
                return;
            }

            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            _isBusy = false;
        }
    }
}
