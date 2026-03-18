namespace listView_Corsega;

[QueryProperty(nameof(TaskId), "id")]
public partial class EditTodoPage : ContentPage
{
    private int _taskId;
    private bool _isCompleted;

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
        var title = TitleEntry.Text?.Trim();
        var details = DetailsEditor.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(title))
        {
            await DisplayAlertAsync("Missing title", "Please enter a title for your task.", "OK");
            return;
        }

        if (!ToDoStore.Update(_taskId, title, details))
        {
            await DisplayAlertAsync("Task not found", "This task no longer exists.", "OK");
            await Shell.Current.GoToAsync("..");
            return;
        }

        await DisplayAlertAsync("Updated", "Task updated successfully.", "OK");
    }

    private async void OnToggleStatusClicked(object? sender, EventArgs e)
    {
        if (_isCompleted)
        {
            ToDoStore.MarkIncomplete(_taskId);
        }
        else
        {
            ToDoStore.MarkCompleted(_taskId);
        }

        await Shell.Current.GoToAsync("..");
    }

    private async void OnDeleteClicked(object? sender, EventArgs e)
    {
        var confirm = await DisplayAlertAsync("Delete task", "Delete this item?", "Delete", "Cancel");
        if (!confirm)
        {
            return;
        }

        ToDoStore.Delete(_taskId);
        await Shell.Current.GoToAsync("..");
    }
}
