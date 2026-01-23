namespace InternLoop.Models;

public class PlanModel
{
    public string ApplicationDescription { get; set; } = string.Empty;
    public List<string> ArchitecturalDesicions { get; set; } = new();
    public List<ComponentModel> Components { get; set; } = new();
}
