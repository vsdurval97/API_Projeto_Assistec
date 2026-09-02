namespace Assistec.Desktop.Windowing;

public class AppWindowInstance
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public Type ComponentType { get; set; } = null!;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public bool IsMinimized { get; set; }
}