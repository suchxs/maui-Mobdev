namespace listView_Corsega;

public partial class CompletedPage : ContentPage
{
    private bool _isLoading;

    public CompletedPage()
    {
        InitializeComponent();
        completedCV.ItemsSource = ToDoStore.Completed;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isLoading)
        {
            return;
        }

        _isLoading = true;
        try
        {
            var result = await ToDoStore.RefreshAsync();
            if (!result.Success && ToDoStore.CurrentUserId.HasValue)
            {
                await DisplayAlertAsync("Sync failed", result.Message, "OK");
            }
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async void OpenEditPage(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is null || !int.TryParse(e.Parameter.ToString(), out var id))
        {
            return;
        }

        await Shell.Current.GoToAsync($"{nameof(EditTodoPage)}?id={id}");
    }

    private async void DeleteCompletedItem(object? sender, EventArgs e)
    {
        if (sender is not Element deleteElement)
        {
            return;
        }

        if (!int.TryParse(deleteElement.ClassId, out var id))
        {
            return;
        }

        var result = await ToDoStore.DeleteAsync(id);
        if (!result.Success)
        {
            await DisplayAlertAsync("Delete failed", result.Message, "OK");
        }
    }
}
