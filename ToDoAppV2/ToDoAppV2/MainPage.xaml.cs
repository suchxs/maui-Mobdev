using ToDoMaui_Listview;

namespace listView_Corsega;

public partial class MainPage : ContentPage
{
    private bool _isLoading;
    private bool _isBusy;

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
        _isBusy = true;
        SetBusy(true, "Syncing...");
        try
        {
            var result = await ToDoStore.RefreshAsync();
            if (!result.Success && ToDoStore.CurrentUserId.HasValue)
            {
                await DisplayAlertAsync("Error", result.Message, "OK");
            }
        }
        finally
        {
            SetBusy(false);
            _isBusy = false;
            _isLoading = false;
        }
    }

    private async void AddToDoItem(object? sender, EventArgs e)
    {
        if (_isBusy)
        {
            return;
        }

        await Shell.Current.GoToAsync(nameof(AddTodoPage));
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

    private async void DeleteToDoItem(object? sender, EventArgs e)
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

        var confirm = await DisplayAlertAsync("Confirm", "Delete this item?", "Delete", "Cancel");
        if (!confirm)
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
            await DisplayAlertAsync("Error", result.Message, "OK");
            return;
        }

        await DisplayAlertAsync("Success", result.Message, "OK");
    }

    private async void CompleteToDoItem(object? sender, EventArgs e)
    {
        if (_isBusy)
        {
            return;
        }

        if (sender is not Element completeElement)
        {
            return;
        }

        if (!int.TryParse(completeElement.ClassId, out var id))
        {
            return;
        }

        _isBusy = true;
        SetBusy(true, "Updating status...");
        var result = await ToDoStore.MarkCompletedAsync(id);
        SetBusy(false);
        _isBusy = false;
        if (!result.Success)
        {
            await DisplayAlertAsync("Error", result.Message, "OK");
            return;
        }

        await DisplayAlertAsync("Success", result.Message, "OK");
    }

    private void SetBusy(bool isBusy, string addButtonBusyText = "+")
    {
        MainLayout.InputTransparent = isBusy;
        AddButton.IsEnabled = !isBusy;
        AddButton.Text = isBusy ? addButtonBusyText : "+";
    }
}