namespace InternLoop.Helpers;

/// <summary>
/// Agent helper implementation that wraps the GitHub Copilot CLI
/// </summary>
public class CopilotAgentHelper : IAgentHelper
{
    public string Name => "Copilot";

    public Task<CliResult> RunAsync(string prompt, string? model = null, string? workingDirectory = null)
    {
        return CopilotCliHelper.RunAsync(prompt, model, workingDirectory);
    }
}
