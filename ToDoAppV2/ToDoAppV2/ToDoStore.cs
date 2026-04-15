using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Maui.Storage;
using ToDoMaui_Listview;

namespace listView_Corsega;

public static class ToDoStore
{
    private const string TasksPreferencePrefix = "todoapp_tasks_v1_";

    private static int _nextId = 1;
    private static string? _currentUserEmail;

    private sealed class StoredTaskState
    {
        public int NextId { get; set; } = 1;
        public List<ToDoClass> Todos { get; set; } = [];
        public List<ToDoClass> Completed { get; set; } = [];
    }

    public static ObservableCollection<ToDoClass> Todos { get; } = [];

    public static ObservableCollection<ToDoClass> Completed { get; } = [];

    public static void SetCurrentUser(string? email)
    {
        _currentUserEmail = NormalizeEmail(email);
        LoadCurrentUserState();
    }

    public static ToDoClass Add(string title, string detail = "")
    {
        var item = new ToDoClass
        {
            id = _nextId++,
            title = title.Trim(),
            detail = detail.Trim()
        };

        Todos.Add(item);
        SaveCurrentUserState();
        return item;
    }

    public static void Delete(int id)
    {
        var inTodo = Todos.FirstOrDefault(t => t.id == id);
        if (inTodo is not null)
        {
            Todos.Remove(inTodo);
            SaveCurrentUserState();
            return;
        }

        var inCompleted = Completed.FirstOrDefault(t => t.id == id);
        if (inCompleted is not null)
        {
            Completed.Remove(inCompleted);
            SaveCurrentUserState();
        }
    }

    public static void MarkCompleted(int id)
    {
        var item = Todos.FirstOrDefault(t => t.id == id);
        if (item is null)
        {
            return;
        }

        Todos.Remove(item);
        Completed.Add(item);
        SaveCurrentUserState();
    }

    public static void MarkIncomplete(int id)
    {
        var item = Completed.FirstOrDefault(t => t.id == id);
        if (item is null)
        {
            return;
        }

        Completed.Remove(item);
        Todos.Add(item);
        SaveCurrentUserState();
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

    public static bool Update(int id, string title, string detail)
    {
        var item = Find(id);
        if (item is null)
        {
            return false;
        }

        item.title = title.Trim();
        item.detail = detail.Trim();
        SaveCurrentUserState();
        return true;
    }

    private static void LoadCurrentUserState()
    {
        _nextId = 1;
        Todos.Clear();
        Completed.Clear();

        if (string.IsNullOrWhiteSpace(_currentUserEmail))
        {
            return;
        }

        var json = Preferences.Default.Get(BuildTasksKey(_currentUserEmail), string.Empty);
        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        try
        {
            var state = JsonSerializer.Deserialize<StoredTaskState>(json);
            if (state is null)
            {
                return;
            }

            foreach (var item in state.Todos)
            {
                Todos.Add(item);
            }

            foreach (var item in state.Completed)
            {
                Completed.Add(item);
            }

            var maxId = Todos.Concat(Completed).Select(t => t.id).DefaultIfEmpty(0).Max();
            _nextId = Math.Max(state.NextId, maxId + 1);
        }
        catch
        {
            // Recover from malformed local task JSON by starting fresh for this account.
            _nextId = 1;
            Todos.Clear();
            Completed.Clear();
        }
    }

    private static void SaveCurrentUserState()
    {
        if (string.IsNullOrWhiteSpace(_currentUserEmail))
        {
            return;
        }

        var state = new StoredTaskState
        {
            NextId = _nextId,
            Todos = [.. Todos],
            Completed = [.. Completed]
        };

        var json = JsonSerializer.Serialize(state);
        Preferences.Default.Set(BuildTasksKey(_currentUserEmail), json);
    }

    private static string BuildTasksKey(string normalizedEmail)
    {
        return $"{TasksPreferencePrefix}{normalizedEmail}";
    }

    private static string? NormalizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        return email.Trim().ToLowerInvariant();
    }

}
