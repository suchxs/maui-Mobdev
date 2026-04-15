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
        if (_isBusy)
        {
            return;
        }

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
            await DisplayAlertAsync("Error", "Please enter a title for your note.", "OK");
            return;
        }

        _isBusy = true;
        SetBusy(true);
        try
        {
            var result = await ToDoStore.AddAsync(title, details);
            if (!result.Success)
            {
                await DisplayAlertAsync("Error", result.Message, "OK");
                return;
            }

            await DisplayAlertAsync("Success", result.Message, "OK");
            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            SetBusy(false);
            _isBusy = false;
        }
    }

    private void SetBusy(bool isBusy)
    {
        MainLayout.InputTransparent = isBusy;
        AddButton.IsEnabled = !isBusy;
        AddButton.Text = isBusy ? "Adding..." : "Add";
    }
}
