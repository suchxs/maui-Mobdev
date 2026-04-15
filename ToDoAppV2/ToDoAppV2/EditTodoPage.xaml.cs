namespace listView_Corsega;

[QueryProperty(nameof(TaskId), "id")]
public partial class EditTodoPage : ContentPage
{
    private int _taskId;
    private bool _isCompleted;
    private bool _isBusy;

    public string? TaskId
    {
        set
        {
            if (!int.TryParse(value, out var parsedId))
            {
                return;
            }

            _taskId = parsedId;
            LoadTask();
        }
    }

    public EditTodoPage()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private void LoadTask()
    {
        if (_taskId <= 0)
        {
            return;
        }

        var item = ToDoStore.Find(_taskId);
        if (item is null)
        {
            _ = DisplayAlertAsync("Task not found", "This task no longer exists.", "OK");
            _ = Shell.Current.GoToAsync("..");
            return;
        }

        TitleEntry.Text = item.title;
        DetailsEditor.Text = item.detail;
        _isCompleted = ToDoStore.IsCompleted(_taskId);

        if (_isCompleted)
        {
            StatusButton.Text = "Incomplete";
            StatusButton.BackgroundColor = Color.FromArgb("#E8EDF7");
            StatusButton.TextColor = Color.FromArgb("#1F2937");
            return;
        }

        StatusButton.Text = "Complete";
        StatusButton.BackgroundColor = Color.FromArgb("#5B3FD6");
        StatusButton.TextColor = Colors.White;
    }

    private async void OnUpdateClicked(object? sender, EventArgs e)
    {
        if (_isBusy)
        {
            return;
        }

        var title = TitleEntry.Text?.Trim();
        var details = DetailsEditor.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(title))
        {
            await DisplayAlertAsync("Missing title", "Please enter a title for your task.", "OK");
            return;
        }

        _isBusy = true;
        try
        {
            var result = await ToDoStore.UpdateAsync(_taskId, title, details);
            if (!result.Success)
            {
                await DisplayAlertAsync("Update failed", result.Message, "OK");
                return;
            }

            await DisplayAlertAsync("Updated", "Task updated successfully.", "OK");
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async void OnToggleStatusClicked(object? sender, EventArgs e)
    {
        if (_isBusy)
        {
            return;
        }

        _isBusy = true;
        try
        {
            (bool Success, string Message) result;
            if (_isCompleted)
            {
                result = await ToDoStore.MarkIncompleteAsync(_taskId);
            }
            else
            {
                result = await ToDoStore.MarkCompletedAsync(_taskId);
            }

            if (!result.Success)
            {
                await DisplayAlertAsync("Status update failed", result.Message, "OK");
                return;
            }

            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (_isBusy)
        {
            return;
        }

        var confirm = await DisplayAlertAsync("Delete task", "Delete this item?", "Delete", "Cancel");
        if (!confirm)
        {
            return;
        }

        _isBusy = true;
        try
        {
            var result = await ToDoStore.DeleteAsync(_taskId);
            if (!result.Success)
            {
                await DisplayAlertAsync("Delete failed", result.Message, "OK");
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
