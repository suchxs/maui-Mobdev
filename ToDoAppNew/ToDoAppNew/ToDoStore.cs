using System.Collections.ObjectModel;
using ToDoMaui_Listview;

namespace listView_Corsega;

public static class ToDoStore
{
    private static int _nextId = 1;

    public static ObservableCollection<ToDoClass> Todos { get; } = [];

    public static ObservableCollection<ToDoClass> Completed { get; } = [];

    public static ToDoClass Add(string title, string detail = "")
    {
        var item = new ToDoClass
        {
            id = _nextId++,
            title = title.Trim(),
            detail = detail.Trim()
        };

        Todos.Add(item);
        return item;
    }

    public static void Delete(int id)
    {
        var inTodo = Todos.FirstOrDefault(t => t.id == id);
        if (inTodo is not null)
        {
            Todos.Remove(inTodo);
            return;
        }

        var inCompleted = Completed.FirstOrDefault(t => t.id == id);
        if (inCompleted is not null)
        {
            Completed.Remove(inCompleted);
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
    }

}
