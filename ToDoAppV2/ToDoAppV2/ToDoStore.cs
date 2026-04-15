using System.Collections.ObjectModel;
using ToDoMaui_Listview;

namespace listView_Corsega;

public static class ToDoStore
{
    private static AuthUser? _currentUser;

    public static ObservableCollection<ToDoClass> Todos { get; } = [];

    public static ObservableCollection<ToDoClass> Completed { get; } = [];

    public static int? CurrentUserId => _currentUser?.Id;

    public static void SetCurrentUser(AuthUser? user)
    {
        _currentUser = user;
        Todos.Clear();
        Completed.Clear();
    }

    public static async Task<(bool Success, string Message)> RefreshAsync()
    {
        if (_currentUser is null || _currentUser.Id <= 0)
        {
            Todos.Clear();
            Completed.Clear();
            return (false, "Please sign in first.");
        }

        var activeResult = await ToDoApiClient.GetItemsAsync(_currentUser.Id, "active");
        var inactiveResult = await ToDoApiClient.GetItemsAsync(_currentUser.Id, "inactive");

        if (!activeResult.Success)
        {
            return (false, activeResult.Message);
        }

        if (!inactiveResult.Success)
        {
            return (false, inactiveResult.Message);
        }

        ReplaceItems(Todos, activeResult.Items.OrderByDescending(t => t.id));
        ReplaceItems(Completed, inactiveResult.Items.OrderByDescending(t => t.id));
        return (true, "Success");
    }

    public static async Task<(bool Success, string Message)> AddAsync(string title, string detail = "")
    {
        if (_currentUser is null || _currentUser.Id <= 0)
        {
            return (false, "Please sign in first.");
        }

        var result = await ToDoApiClient.AddItemAsync(title.Trim(), detail.Trim(), _currentUser.Id);
        if (!result.Success)
        {
            return (false, result.Message);
        }

        await RefreshAsync();
        return (true, result.Message);
    }

    public static async Task<(bool Success, string Message)> DeleteAsync(int id)
    {
        var result = await ToDoApiClient.DeleteItemAsync(id);
        if (!result.Success)
        {
            return (false, result.Message);
        }

        var inTodo = Todos.FirstOrDefault(t => t.id == id);
        if (inTodo is not null)
        {
            Todos.Remove(inTodo);
        }

        var inCompleted = Completed.FirstOrDefault(t => t.id == id);
        if (inCompleted is not null)
        {
            Completed.Remove(inCompleted);
        }

        return (true, result.Message);
    }

    public static async Task<(bool Success, string Message)> MarkCompletedAsync(int id)
    {
        var result = await ToDoApiClient.UpdateItemStatusAsync(id, "inactive");
        if (!result.Success)
        {
            return (false, result.Message);
        }

        await RefreshAsync();
        return (true, result.Message);
    }

    public static async Task<(bool Success, string Message)> MarkIncompleteAsync(int id)
    {
        var result = await ToDoApiClient.UpdateItemStatusAsync(id, "active");
        if (!result.Success)
        {
            return (false, result.Message);
        }

        await RefreshAsync();
        return (true, result.Message);
    }

    public static ToDoClass? Find(int id)
    {
        return Todos.FirstOrDefault(t => t.id == id)
            ?? Completed.FirstOrDefault(t => t.id == id);
    }

    public static bool IsCompleted(int id)
    {
        return Completed.Any(t => t.id == id);
    }

    public static async Task<(bool Success, string Message)> UpdateAsync(int id, string title, string detail)
    {
        var result = await ToDoApiClient.UpdateItemAsync(id, title.Trim(), detail.Trim());
        if (!result.Success)
        {
            return (false, result.Message);
        }

        await RefreshAsync();
        return (true, result.Message);
    }

    private static void ReplaceItems(ObservableCollection<ToDoClass> target, IEnumerable<ToDoClass> source)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(item);
        }
    }
}
