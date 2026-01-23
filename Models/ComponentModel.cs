namespace InternLoop.Models;

public class ComponentModel
{
	public DevelopmentStatus DevelopmentStatus { get; set; } = DevelopmentStatus.NotStarted;
	public string ComponentName { get; set; } = string.Empty;
    public string ComponentDescription { get; set; } = string.Empty;
    public string ComponentDetailedDesign { get; set; } = string.Empty;
    public List<string> Dependencies { get; set; } = new();
}

public enum DevelopmentStatus
{
	NotStarted = 0,
	InProgress = 1,
	Completed = 2
}
