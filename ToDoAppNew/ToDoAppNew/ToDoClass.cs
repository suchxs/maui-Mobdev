namespace ToDoMaui_Listview;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class ToDoClass : INotifyPropertyChanged
{
    public ToDoClass()
    {
    }

    int _id { get; set; }
    string _title { get; set; } = string.Empty;
    string _detail { get; set; } = string.Empty;

    public int id
    {
        get { return _id; }
        set { _id = value; OnPropertyChanged(nameof(id)); }
    }

    public string title
    {
        get { return _title; }
        set { _title = value; OnPropertyChanged(nameof(title)); }
    }

    public string detail
    {
        get { return _detail; }
        set { _detail = value; OnPropertyChanged(nameof(detail)); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}