namespace listView_Corsega;

public partial class CompletedPage : ContentPage
{
    public CompletedPage()
    {
        InitializeComponent();
        completedCV.ItemsSource = ToDoStore.Completed;
    }

    private async void OpenEditPage(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is null || !int.TryParse(e.Parameter.ToString(), out var id))
        {
            return;
        }

        await Shell.Current.GoToAsync($"{nameof(EditTodoPage)}?id={id}");
    }

    private void DeleteCompletedItem(object? sender, EventArgs e)
    {
        if (sender is not Element deleteElement)
        {
            return;
        }

        if (!int.TryParse(deleteElement.ClassId, out var id))
        {
            return;
        }

        ToDoStore.Delete(id);
    }
}
