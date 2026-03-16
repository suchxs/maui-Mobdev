using System.Collections.ObjectModel;
using ToDoMaui_Listview;

namespace listView_Corsega;

public partial class MainPage : ContentPage
{
    private readonly ObservableCollection<ToDoClass> _todos = [];
    private readonly HashSet<int> _animatedItemIds = [];
    private ToDoClass? _selectedToDo;
    private int _nextId = 1;

    public MainPage()
    {
        InitializeComponent();
        todoCV.ItemsSource = _todos;
    }

    private async void AddToDoItem(object? sender, EventArgs e)
    {
        var title = titleEntry.Text?.Trim();
        var detail = detailsEditor.Text?.Trim();

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(detail))
        {
            await DisplayAlertAsync("Missing info", "Please provide a title and details.", "OK");
            return;
        }

        var newItem = new ToDoClass
        {
            id = _nextId++,
            title = title,
            detail = detail
        };

        _todos.Add(newItem);
        todoCV.ScrollTo(newItem, position: ScrollToPosition.End, animate: true);

        ClearForm();
    }

    private async void EditToDoItem(object? sender, EventArgs e)
    {
        if (_selectedToDo is null)
        {
            return;
        }

        var title = titleEntry.Text?.Trim();
        var detail = detailsEditor.Text?.Trim();

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(detail))
        {
            await DisplayAlertAsync("Missing info", "Please provide a title and details.", "OK");
            return;
        }

        _selectedToDo.title = title;
        _selectedToDo.detail = detail;

        todoCV.SelectedItem = null;
        ResetEditState();
        ClearForm();
    }

    private void CancelEdit(object? sender, EventArgs e)
    {
        todoCV.SelectedItem = null;
        ResetEditState();
        ClearForm();
    }

    private async void SwipeDeleteToDoItem(object? sender, EventArgs e)
    {
        if (sender is not SwipeItem swipeItem)
        {
            return;
        }

        if (!int.TryParse(swipeItem.CommandParameter?.ToString(), out var id))
        {
            return;
        }

        await DeleteToDoByIdAsync(id);
    }

    private async void DeleteToDoItem(object? sender, EventArgs e)
    {
        if (sender is not Button deleteButton)
        {
            return;
        }

        if (!int.TryParse(deleteButton.ClassId, out var id))
        {
            return;
        }

        await DeleteToDoByIdAsync(id);
    }

    private async Task DeleteToDoByIdAsync(int id)
    {
        var item = _todos.FirstOrDefault(t => t.id == id);
        if (item is null)
        {
            return;
        }

        var confirm = await DisplayAlertAsync("Delete to-do", "Delete this item?", "Delete", "Cancel");
        if (!confirm)
        {
            return;
        }

        _animatedItemIds.Remove(item.id);
        _todos.Remove(item);

        if (_selectedToDo?.id == item.id)
        {
            todoCV.SelectedItem = null;
            ResetEditState();
            ClearForm();
        }
    }

    private async void TodoItem_Loaded(object? sender, EventArgs e)
    {
        if (sender is not Border itemBorder || itemBorder.BindingContext is not ToDoClass todoItem)
        {
            return;
        }

        if (!_animatedItemIds.Add(todoItem.id))
        {
            return;
        }

        itemBorder.Opacity = 0;
        itemBorder.Scale = 0.98;
        itemBorder.TranslationY = 8;

        await Task.WhenAll(
            itemBorder.FadeToAsync(1, 200, Easing.CubicOut),
            itemBorder.ScaleToAsync(1, 200, Easing.CubicOut),
            itemBorder.TranslateToAsync(0, 0, 200, Easing.CubicOut)
        );
    }

    private void TodoCV_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _selectedToDo = e.CurrentSelection.FirstOrDefault() as ToDoClass;

        if (_selectedToDo is null)
        {
            ResetEditState();
            return;
        }

        titleEntry.Text = _selectedToDo.title;
        detailsEditor.Text = _selectedToDo.detail;

        addBtn.IsVisible = false;
        editBtn.IsVisible = true;
        cancelBtn.IsVisible = true;
    }

    private void ResetEditState()
    {
        _selectedToDo = null;
        addBtn.IsVisible = true;
        editBtn.IsVisible = false;
        cancelBtn.IsVisible = false;
    }

    private void ClearForm()
    {
        titleEntry.Text = string.Empty;
        detailsEditor.Text = string.Empty;
        titleEntry.Focus();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ResetEditState();
    }

}