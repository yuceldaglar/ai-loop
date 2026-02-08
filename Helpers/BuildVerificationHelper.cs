using System.Text.RegularExpressions;

namespace InternLoop.Helpers;

/// <summary>
/// Helper for build verification after agent-implemented code.
/// Agent decides if project is buildable, runs build, returns OK or ERROR.
/// Loops up to 5 times on ERROR asking agent to fix.
/// </summary>
public static class BuildVerificationHelper
{
	public const string BuildVerificationPromptSuffix = """

		After implementing, if this project is buildable (e.g. has .csproj, package.json, or similar), run the appropriate build command. If the build succeeds, respond with exactly: OK
		If the build fails, respond with exactly: ERROR:
		followed by the complete build output (stdout and stderr). If the project is not buildable, respond with: OK
		""";

	private const string ErrorPrefix = "ERROR:";
	private const int MaxRetries = 5;

	/// <summary>
	/// Parses the agent's response for OK or ERROR.
	/// </summary>
	/// <param name="output">Raw output from the agent</param>
	/// <returns>(success: true, errorOutput: null) if OK; (success: false, errorOutput: extracted text) if ERROR or malformed</returns>
	public static (bool Success, string? ErrorOutput) ParseAgentBuildResult(string? output)
	{
		if (string.IsNullOrWhiteSpace(output))
		{
			return (false, "Agent returned empty output");
		}

		var normalized = output.Trim();

		// Check for ERROR first (takes precedence)
		var errorIndex = normalized.IndexOf(ErrorPrefix, StringComparison.OrdinalIgnoreCase);
		if (errorIndex >= 0)
		{
			var errorOutput = normalized[(errorIndex + ErrorPrefix.Length)..].Trim();
			return (false, string.IsNullOrWhiteSpace(errorOutput) ? output : errorOutput);
		}

		// Check for OK as standalone word (avoid matching "look", "book", etc.)
		if (Regex.IsMatch(normalized, @"\bOK\b", RegexOptions.IgnoreCase))
		{
			return (true, null);
		}

		// Malformed: treat as ERROR, use full output in retry
		return (false, output);
	}

	/// <summary>
	/// Builds the fix prompt when build fails.
	/// </summary>
	public static string GetFixPrompt(string errorOutput) => $"""
		The build failed with the following output:
		{errorOutput}

		Please fix the build errors and try again. After fixing, run the build command again. Respond with OK if the build succeeds, or ERROR: followed by the full build output if it still fails.
		""";

	/// <summary>
	/// Runs the agent with build verification. On ERROR, loops up to 5 times with fix prompts.
	/// </summary>
	/// <param name="initialPrompt">The full prompt including build verification suffix</param>
	/// <param name="workingDirectory">Working directory for the agent (project root). Uses current directory if null.</param>
	/// <returns>(success: true, null) if build OK; (success: false, finalError) if failed after 5 attempts</returns>
	public static async Task<(bool Success, string? FinalError)> ExecuteWithBuildVerificationAsync(string initialPrompt, string? workingDirectory = null)
	{
		var agent = AgentManager.Instance.CurrentAgent;
		var currentPrompt = initialPrompt;
		string? lastError = null;

		for (var attempt = 1; attempt <= MaxRetries; attempt++)
		{
			var result = await agent.RunAsync(currentPrompt, workingDirectory: workingDirectory);

			if (!result.Success)
			{
				var msg = result.ErrorMessage ?? $"{agent.Name} exited with code {result.ExitCode}";
				Console.WriteLine(msg);
				if (!string.IsNullOrWhiteSpace(result.Output))
					Console.WriteLine(result.Output);
				if (!string.IsNullOrWhiteSpace(result.ErrorOutput))
					Console.Error.WriteLine(result.ErrorOutput);
				return (false, msg);
			}

			var combinedOutput = string.Join("\n",
				Enumerable.Empty<string>()
					.Append(result.Output)
					.Append(result.ErrorOutput)
					.Where(s => !string.IsNullOrWhiteSpace(s)));

			if (!string.IsNullOrWhiteSpace(result.Output))
				Console.WriteLine(result.Output);
			if (!string.IsNullOrWhiteSpace(result.ErrorOutput))
				Console.Error.WriteLine(result.ErrorOutput);

			var (success, errorOutput) = ParseAgentBuildResult(combinedOutput);

			if (success)
			{
				Console.WriteLine("Build OK");
				return (true, null);
			}

			lastError = errorOutput;

			if (attempt < MaxRetries)
			{
				Console.WriteLine($"Build failed (attempt {attempt}/{MaxRetries}). Asking agent to fix...");
				currentPrompt = GetFixPrompt(lastError ?? combinedOutput);
			}
			else
			{
				Console.WriteLine($"Build ERROR after {MaxRetries} attempts");
				if (!string.IsNullOrWhiteSpace(lastError))
					Console.WriteLine(lastError);
				return (false, lastError);
			}
		}

		return (false, lastError);
	}
}
