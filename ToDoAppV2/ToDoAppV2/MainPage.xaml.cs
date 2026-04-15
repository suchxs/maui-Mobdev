using ToDoMaui_Listview;

namespace listView_Corsega;

public partial class MainPage : ContentPage
{
    private bool _isLoading;

    public MainPage()
    {
        InitializeComponent();
        todoCV.ItemsSource = ToDoStore.Todos;
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

    private async void AddToDoItem(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AddTodoPage));
    }

    private async void OpenEditPage(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is null || !int.TryParse(e.Parameter.ToString(), out var id))
        {
            return;
        }

        await Shell.Current.GoToAsync($"{nameof(EditTodoPage)}?id={id}");
    }

    private async void DeleteToDoItem(object? sender, EventArgs e)
    {
        if (sender is not Element deleteElement)
        {
            return;
        }

        if (!int.TryParse(deleteElement.ClassId, out var id))
        {
            return;
        }

        var confirm = await DisplayAlertAsync("Delete task", "Delete this item?", "Delete", "Cancel");
        if (!confirm)
        {
            return;
        }

        var result = await ToDoStore.DeleteAsync(id);
        if (!result.Success)
        {
            await DisplayAlertAsync("Delete failed", result.Message, "OK");
        }
    }

    private async void CompleteToDoItem(object? sender, EventArgs e)
    {
        if (sender is not Element completeElement)
        {
            return;
        }

        if (!int.TryParse(completeElement.ClassId, out var id))
        {
            return;
        }

        var result = await ToDoStore.MarkCompletedAsync(id);
        if (!result.Success)
        {
            await DisplayAlertAsync("Status update failed", result.Message, "OK");
        }
    }
}