namespace InternLoop.Models;

/// <summary>
/// Application configuration model for persistent settings
/// </summary>
public class AppConfig
{
    /// <summary>
    /// The currently selected agent type ("Cursor" or "Copilot")
    /// </summary>
    public string CurrentAgent { get; set; } = "Cursor";
}
