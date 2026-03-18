namespace listView_Corsega;

public partial class CompletedPage : ContentPage
{
    public CompletedPage()
    {
        InitializeComponent();
        completedCV.ItemsSource = ToDoStore.Completed;
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
