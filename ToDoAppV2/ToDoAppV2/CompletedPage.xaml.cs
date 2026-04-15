namespace listView_Corsega;

public partial class CompletedPage : ContentPage
{
    private bool _isLoading;
    private bool _isBusy;

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
        _isBusy = true;
        SetBusy(true, "Syncing tasks...");
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
            SetBusy(false);
            _isBusy = false;
            _isLoading = false;
        }
    }

    private async void OpenEditPage(object? sender, TappedEventArgs e)
    {
        if (_isBusy)
        {
            return;
        }

        if (e.Parameter is null || !int.TryParse(e.Parameter.ToString(), out var id))
        {
            return;
        }

        await Shell.Current.GoToAsync($"{nameof(EditTodoPage)}?id={id}");
    }

    private async void DeleteCompletedItem(object? sender, EventArgs e)
    {
        if (_isBusy)
        {
            return;
        }

        if (sender is not Element deleteElement)
        {
            return;
        }

        if (!int.TryParse(deleteElement.ClassId, out var id))
        {
            return;
        }

        _isBusy = true;
        SetBusy(true, "Deleting task...");
        var result = await ToDoStore.DeleteAsync(id);
        SetBusy(false);
        _isBusy = false;
        if (!result.Success)
        {
            await DisplayAlertAsync("Delete failed", result.Message, "OK");
            return;
        }

        await DisplayAlertAsync("Deleted", result.Message, "OK");
    }

    private void SetBusy(bool isBusy, string message = "Please wait...")
    {
        BusyMessageLabel.Text = message;
        BusyOverlay.IsVisible = isBusy;
        MainLayout.InputTransparent = isBusy;
    }
}
