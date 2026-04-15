using ToDoMaui_Listview;

namespace listView_Corsega;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        todoCV.ItemsSource = ToDoStore.Todos;
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

        ToDoStore.Delete(id);
    }

    private void CompleteToDoItem(object? sender, EventArgs e)
    {
        if (sender is not Element completeElement)
        {
            return;
        }

        if (!int.TryParse(completeElement.ClassId, out var id))
        {
            return;
        }

        ToDoStore.MarkCompleted(id);
    }
}