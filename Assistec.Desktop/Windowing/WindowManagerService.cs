namespace Assistec.Desktop.Windowing;

public class WindowManagerService
{
    private readonly List<AppWindowInstance> _windows = new();

    public IReadOnlyList<AppWindowInstance> Windows => _windows;

    public event Action? OnChange;

    public void Open<TComponent>(string title, string icon) where TComponent : Microsoft.AspNetCore.Components.IComponent
    {
        var existing = _windows.FirstOrDefault(w => w.ComponentType == typeof(TComponent));
        if (existing is not null)
        {
            existing.IsMinimized = false;
            BringToFront(existing);
            NotifyChange();
            return;
        }

        var window = new AppWindowInstance
        {
            Title = title,
            Icon = icon,
            ComponentType = typeof(TComponent)
        };

        _windows.Add(window);
        NotifyChange();
    }

    public void Minimize(Guid id)
    {
        var window = _windows.FirstOrDefault(w => w.Id == id);
        if (window is not null)
        {
            window.IsMinimized = true;
            NotifyChange();
        }
    }

    public void Restore(Guid id)
    {
        var window = _windows.FirstOrDefault(w => w.Id == id);
        if (window is not null)
        {
            window.IsMinimized = false;
            BringToFront(window);
            NotifyChange();
        }
    }

    public void Close(Guid id)
    {
        var window = _windows.FirstOrDefault(w => w.Id == id);
        if (window is not null)
        {
            _windows.Remove(window);
            NotifyChange();
        }
    }

    private void BringToFront(AppWindowInstance window)
    {
        _windows.Remove(window);
        _windows.Add(window);
    }

    public void Navigate(Guid windowId, Type componentType, Dictionary<string, object>? parameters = null, string? title = null)
{
    var window = _windows.FirstOrDefault(w => w.Id == windowId);
    if (window is null) return;

    window.ComponentType = componentType;
    window.Parameters = parameters ?? new();
    if (title is not null) window.Title = title;

    NotifyChange();
}

    private void NotifyChange() => OnChange?.Invoke();
}

