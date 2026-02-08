namespace InternLoop.Helpers;

/// <summary>
/// Agent helper implementation that wraps the Cursor CLI
/// </summary>
public class CursorAgentHelper : IAgentHelper
{
    public string Name => "Cursor";

    public Task<CliResult> RunAsync(string prompt, string? model = null, string? workingDirectory = null)
    {
        return CursorCliHelper.RunAsync(
            prompt: prompt,
            model: model,
            mode: null,
            force: true,
            workingDirectory: workingDirectory);
    }
}
