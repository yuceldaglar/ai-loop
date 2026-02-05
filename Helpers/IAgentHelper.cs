namespace InternLoop.Helpers;

/// <summary>
/// Common interface for AI agent helpers (Cursor CLI, Copilot CLI, etc.)
/// </summary>
public interface IAgentHelper
{
    /// <summary>
    /// Gets the display name of the agent
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Runs the agent with the specified prompt
    /// </summary>
    /// <param name="prompt">The prompt to send to the agent</param>
    /// <param name="model">Optional model to use</param>
    /// <returns>A CliResult containing the response or error information</returns>
    Task<CliResult> RunAsync(string prompt, string? model = null);
}
